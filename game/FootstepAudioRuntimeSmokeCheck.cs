using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Exercises real Seeker physics, transient cancellation and mode-independent save state.</summary>
public partial class FootstepAudioRuntimeSmokeCheck : Node
{
    private static readonly JsonSerializerOptions EvidenceOptions = new() { WriteIndented = true, IncludeFields = true };
    private Seeker seeker = null!;
    private OpeningRegionFootstepAudioController audio = null!;
    private OpeningRegionFootstepMaterialSource materials = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private readonly List<object> evidence = [];

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_FOOTSTEP_MAIN_CHECK") != "1") { return; }
        try
        {
            await Frame(); await Frame();
            seeker = GetNode<Seeker>("../Seeker");
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            audio = GetNode<OpeningRegionFootstepAudioController>("../OpeningRegionFootstepAudioController");
            materials = GetNode<OpeningRegionFootstepMaterialSource>("../OpeningRegionFootstepMaterialSource");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
            AudioSettingsController settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            settings.RestoreDefaults();
            string mode = OS.GetEnvironment("ALLRPCS_FOOTSTEP_MODE");
            if (mode == "muted") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Effects, 0, true)); }
            if (mode == "missing") { audio.RemoveDiagnosticResources(); }
            if (mode == "captions-off")
            {
                settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, settings.Preferences.Mono, false));
            }
            for (int wait = 0; wait < 5000 && materials.Select(new(2200, 900)) is null; wait++) { await Ticks(1); }
            WorldTimeSnapshot initialTime = GetNode<WorldTimeController>("../WorldTimeController").WorldTime;
            foreach ((Vector2 position, FootstepMaterial? expected) in new (Vector2, FootstepMaterial?)[]
            {
                (new(2200, 900), FootstepMaterial.Earth), (new(1800, 500), FootstepMaterial.Timber),
                (new(2500, 1275), FootstepMaterial.Gravel), (new(690, 325), FootstepMaterial.Leaves),
                (new(825, 250), FootstepMaterial.Leaves), (new(2640, 1450), null), (new(1500, 220), null),
                (new(3000, 100), null)
            })
            {
                Require(materials.Select(new(position.X, position.Y)) == expected, $"Wrong live material at {position}: {materials.Status}.");
            }
            OpeningRegionProjectionSource projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
            foreach (OpeningRegionAnchor anchor in projection.Projection.Anchors)
            {
                Vector2 position = projection.ToGlobal(new(anchor.Position.X, anchor.Position.Y));
                evidence.Add(new { anchor = anchor.Location.Identity.Id.Value, material = materials.Select(new(position.X, position.Y))?.ToString() });
            }
            foreach (OpeningRegionProjectedConnection connection in projection.Projection.Connections)
            {
                System.Numerics.Vector2 midpoint = (connection.FirstAnchor.Position + connection.SecondAnchor.Position) / 2;
                Vector2 position = projection.ToGlobal(new(midpoint.X, midpoint.Y));
                evidence.Add(new
                {
                    route = connection.FirstAnchor.Location.Identity.Id.Value + "/" + connection.SecondAnchor.Location.Identity.Id.Value,
                    material = materials.Select(new(position.X, position.Y))?.ToString()
                });
            }
            bool injected = false;
            seeker.MotionCompleted += _ => { if (!injected) { injected = true; throw new InvalidOperationException("expected isolated observer probe"); } };
            int observed = 0;
            seeker.MotionCompleted += _ => observed++;
            foreach ((Vector2 start, Vector2 direction, FootstepMaterial expected) in new[]
            {
                (new Vector2(2150, 900), Vector2.Left, FootstepMaterial.Earth),
                (new Vector2(700, 325.80645f), new Vector2(620, 50).Normalized(), FootstepMaterial.Leaves),
                (new Vector2(2440, 1200), new Vector2(440, 550).Normalized(), FootstepMaterial.Gravel),
                (new Vector2(1776, 596), new Vector2(100, -400).Normalized(), FootstepMaterial.Timber)
            })
            {
                Reposition(start);
                long before = audio.ContactCount;
                await Move(direction, expected == FootstepMaterial.Gravel ? 24 : 26);
                Require(audio.ContactCount > before, $"Real movement did not produce {expected}.");
                Require(audio.LastContact?.Material == expected, $"Real contact used wrong {expected} material.");
                Require(audio.GetChildCount() == 2 && audio.GetChildren().All(n => n is AudioStreamPlayer), "Unbounded footstep players.");
                Require(mode is "missing" or "muted" || audio.GetChildren().Cast<AudioStreamPlayer>().Any(p => p.Playing), "Audible movement did not play contact.");
                Label line = captions.GetNode<Label>("Panel/Lines/Footstep");
                Require(line.Text == (mode == "captions-off" ? string.Empty : OpeningRegionFootstepMaterialProjection.Caption(expected)), "Caption did not follow gain-independent contact.");
                evidence.Add(new { movement = expected.ToString(), start, end = seeker.GlobalPosition, contacts = audio.ContactCount - before });
                await Capture("contact-" + expected);
                ReleaseInput();
                await Ticks(2);
                Require(audio.GetChildren().Cast<AudioStreamPlayer>().All(p => !p.Playing) && line.Text.Length == 0, "Stop retained tails/caption.");
            }
            Require(observed > 0, "Failing observer prevented later motion notification.");
            Reposition(new(1700, 900));
            await Move(Vector2.Left, 50);
            long blockedBefore = audio.ContactCount;
            Vector2 blockedPosition = seeker.GlobalPosition;
            await Move(Vector2.Left, 40);
            Require(seeker.GlobalPosition.DistanceTo(blockedPosition) < 0.1f && audio.ContactCount == blockedBefore, "Wall pressure created steps.");
            ReleaseInput();
            Reposition(new(2150, 900));
            await Move(Vector2.Left, 24);
            seeker.SetPhysicsProcess(false);
            await Frame(); await Frame();
            Require(audio.GetChildren().Cast<AudioStreamPlayer>().All(p => !p.Playing), "Disable retained footstep tails.");
            ReleaseInput();
            seeker.SetPhysicsProcess(true);
            Reposition(new(2150, 900));
            await Move(Vector2.Left, 24);
            GetTree().Paused = true;
            await Frame(); await Frame();
            Require(audio.GetChildren().Cast<AudioStreamPlayer>().All(p => !p.Playing), "Pause retained footstep tails.");
            GetTree().Paused = false;
            ReleaseInput();
            ArrangeSave();
            SaveSnapshotCaptureController capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot snapshot = capture.Capture() ?? throw new InvalidOperationException("Missing save fixture.");
            SaveSnapshotRestorationController restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            int resets = 0; restore.PresentationReset += () => resets++;
            Reposition(new(2150, 900));
            await Move(Vector2.Left, 24);
            Require(restore.TryRestore(snapshot), "Valid load rejected.");
            Require(resets == 1 && audio.LastContact is null
                && audio.GetChildren().Cast<AudioStreamPlayer>().All(p => !p.Playing), "Successful load retained cue/history.");
            WorldEventId unknown = new("unknown-footstep-probe");
            SaveSnapshot rejected = new(snapshot.SchemaVersion, snapshot.Discovery,
                new WorldEventSaveState(new WorldEventLogSnapshot([unknown]), new WorldTimeSnapshot(unknown)),
                snapshot.CharacterMemory, snapshot.FirstPatient, snapshot.Journal, snapshot.Map, snapshot.InvestigationRoutes, snapshot.FirstPatientRecovery);
            Require(!restore.TryRestore(rejected) && resets == 1, "Failed load reset presentation.");
            ReleaseInput();
            await Ticks(2);
            Require(GetNode<WorldTimeController>("../WorldTimeController").WorldTime.CurrentEventId == initialTime.CurrentEventId, "Footsteps changed world time.");
            foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900) })
            {
                GetWindow().Size = size;
                captions.SetAction("Grinding", 8);
                Reposition(new(2150, 900));
                await Move(Vector2.Left, 24);
                Require(captions.GetNode<Label>("Panel/Lines/Footstep").Text ==
                    (mode == "captions-off" ? string.Empty : "Footsteps on earth"), "Three-line contact view lost its footstep caption.");
                Require(GetViewport().GetVisibleRect().Encloses(captions.GetNode<Control>("Panel").GetGlobalRect()), "Three-line caption surface exceeds viewport.");
                await Capture($"captions-{size.X}");
                ReleaseInput();
                AudioSettingsPanel panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
                panel.Open(); await Frame();
                Button entry = (Button)panel.FindChild("SourceCredits", true, false);
                entry.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
                MaterialAudioCreditsPanel credits = panel.GetNode<MaterialAudioCreditsPanel>("MaterialAudioCreditsPanel");
                string text = ((RichTextLabel)credits.FindChild("Credits", true, false)).Text;
                Require(text.Contains("Dominik_W", StringComparison.Ordinal) && text.Contains("florianreichelt", StringComparison.Ordinal)
                    && text.Contains("kurt", StringComparison.Ordinal), "Missing retained or footstep credits.");
                await Capture($"credits-{size.X}");
                RichTextLabel notices = (RichTextLabel)credits.FindChild("Credits", true, false);
                notices.ScrollToLine(Math.Max(0, notices.GetLineCount() - 16));
                await Frame();
                await Capture($"credits-footsteps-{size.X}");
                panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
                Require(entry.HasFocus() && panel.Visible && !credits.Visible, "Credits failed return focus.");
                panel.Close(false);
            }
            audio.ResetPresentation();
            string output = OS.GetEnvironment("ALLRPCS_FOOTSTEP_RESULT");
            if (output.Length > 0)
            {
                SaveSnapshot current = capture.Capture()!;
                File.WriteAllText(output, SaveSnapshotJsonSerializer.Serialize(current));
                File.WriteAllText(output + ".movement.json", JsonSerializer.Serialize(evidence, EvidenceOptions));
            }
            captions.SetAction("Grinding", 8);
            audio.PresentContact(new(FootstepMaterial.Timber, FootstepLeg.Left, 1, default));
            long unrelated = settings.Ducking.Begin(AudioForeground.Reading);
            audio._ExitTree();
            Require(captions.GetNode<Label>("Panel/Lines/Footstep").Text.Length == 0
                && audio.GetChildren().Cast<AudioStreamPlayer>().All(p => !p.Playing), "Teardown retained footstep ownership.");
            Require(settings.Ducking.End(unrelated), "Footstep teardown ended another producer's lease.");
            audio.QueueFree(); await Frame();
            GD.Print("Footstep Main checks passed: actual four-material movement, wall pressure, observer isolation, stop/pause/disable/load, captions, credits and save parity fixture.");
            GetTree().Quit();
        }
        catch (Exception error) { ReleaseInput(); GetTree().Paused = false; GD.PrintErr(error); GetTree().Quit(1); }
    }

    private void Reposition(Vector2 position) { ReleaseInput(); seeker.GlobalPosition = position; seeker.Velocity = Vector2.Zero; audio.ResetPresentation(); }
    private async Task Move(Vector2 direction, int ticks)
    {
        ReleaseInput();
        if (direction.X != 0) { Input.ActionPress(direction.X < 0 ? "move_left" : "move_right", Math.Abs(direction.X)); }
        if (direction.Y != 0) { Input.ActionPress(direction.Y < 0 ? "move_up" : "move_down", Math.Abs(direction.Y)); }
        await Ticks(ticks);
    }
    private async Task Ticks(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); } }
    private static void ReleaseInput() { foreach (string action in new[] { "move_left", "move_right", "move_up", "move_down" }) { Input.ActionRelease(action); } }
    private SignalAwaiter Frame() => ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private void ArrangeSave()
    {
        JournalController journal = GetNode<JournalController>("../JournalController");
        if (journal.Journal is null) { Require(GetNode<RepresentativeJournalCompositionCoordinator>("../RepresentativeJournalCompositionCoordinator").TryComposeAndInitialize(), "Journal fixture failed."); }
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        if (map.Map is null) { MapSaveStateRestorationProjection.Restore(new MapSaveState(GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id, [], [], [])).InstallInto(map); }
        OpeningRegionInvestigationRouteProgressController routes = GetNode<OpeningRegionInvestigationRouteProgressController>("../OpeningRegionInvestigationRouteProgressController");
        if (routes.Progress is null) { Require(SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routes, map, GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource")), "Route fixture failed."); }
    }
    private async Task Capture(string name)
    {
        string directory = OS.GetEnvironment("ALLRPCS_FOOTSTEP_VIEWS");
        if (directory.Length == 0 || DisplayServer.GetName() == "headless") { return; }
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Directory.CreateDirectory(directory);
        Require(GetViewport().GetTexture().GetImage().SavePng(Path.Combine(directory, name + ".png")) == Error.Ok, "View capture failed.");
    }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
}
