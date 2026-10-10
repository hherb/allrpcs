using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Finite live input, reach, lifecycle, UI and complete-save checks for the new action.</summary>
public partial class WaterContactRuntimeSmokeCheck : Node
{
    private OpeningRegionStreamWaterContactCoordinator source = null!;
    private OpeningRegionWaterContactAudioController audio = null!;
    private StreamWaterContactPanel panel = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private Seeker seeker = null!;
    private readonly List<string> checks = [];
    private string mode = string.Empty;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_WATER_MAIN_CHECK") != "1")
        {
            if (HasNode("OpeningRegionWaterContactAudioController"))
            {
                Label label = new() { Text = "Water contact proof · stopped", Position = new(16, 16) }; AddChild(label);
                Button play = new() { Text = "Play accepted splash (diagnostic)", Position = new(16, 56) }; AddChild(play);
                Button stop = new() { Text = "Stop", Position = new(16, 100) }; AddChild(stop);
                var local = GetNode<OpeningRegionWaterContactAudioController>("OpeningRegionWaterContactAudioController");
                play.Pressed += local.DiagnosticPlay; stop.Pressed += local.Cancel;
            }
            return;
        }
        try
        {
            for (int i = 0; i < 8; i++) { await Frame(); }
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            source = GetNode<OpeningRegionStreamWaterContactCoordinator>("../OpeningRegionStreamWaterContactCoordinator");
            audio = GetNode<OpeningRegionWaterContactAudioController>("../OpeningRegionWaterContactAudioController");
            panel = GetNode<StreamWaterContactPanel>("../StreamWaterContactPanel");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel"); seeker = GetNode<Seeker>("../Seeker");
            mode = OS.GetEnvironment("ALLRPCS_WATER_MODE");
            AudioSettingsController settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            settings.RestoreDefaults(); AudioPreferences p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange, false, true); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
            settings.SetPreferences(p);
            if (mode == "missing-stream") { audio.GetNode<AudioStreamPlayer>("WaterContact").Stream = null; }
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); await Frame(); }
            await Place(new(1800, 500));
            Require(source.CommitCount == 0 && !source.TryContact(), "Startup or out-of-reach crossing acquired a commit.");
            await Move("move_left", 11); await Move("move_up", 4); await Frame();
            Require(seeker.GlobalPosition.DistanceTo(new(1756, 484)) <= 5 && source.CanOffer,
                "Ordinary motion could not reach approved example: " + seeker.GlobalPosition);
            Require(source.CommitCount == 0, "Walking/eligibility emitted water contact.");
            Vector2 reached = seeker.GlobalPosition;
            await Move("move_down", 4); await Move("move_right", 11);
            Require(seeker.GlobalPosition.DistanceTo(new(1800, 500)) <= 5, "Ordinary route return blocked.");
            await Place(reached);
            checks.Add("ordinary collision-resolved movement to supported bridge-side example and route return; no walking/standing/startup contact");
            ArrangeSave();
            SaveSnapshotCaptureController capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("Missing complete save root.");
            string before = SaveSnapshotJsonSerializer.Serialize(initial);
            if (mode == "no-action") { Finish(before); return; }
            if (mode == "views") { await Views(settings); Finish(before); return; }
            Button button = (Button)panel.FindChild("TouchWater", true, false);
            button.EmitSignal(BaseButton.SignalName.Pressed);
            Require(source.CommitCount == 1 && audio.PresentationCount == 1, "Explicit button did not commit/present exactly once.");
            Require(captions.GetNode<Label>("Panel/Lines/WaterContact").Text == "Water contact", "Missing independent caption.");
            Require(panel.FindChild("Status", true, false) is Label status && status.Text == StreamWaterContactPanel.StatusText, "Missing own-action status.");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Contact mutated durable state.");
            StreamWaterContactCommit commit = source.LastCommit!;
            Require(!source.TryContact(), "Cooldown admitted duplicate activation."); audio.Receive(commit);
            audio.Receive(new(commit.Request, commit.Sequence));
            Require(audio.PresentationCount == 1, "Repeated/forged notification reached audio.");
            checks.Add("one explicit native button/one exact commit/one caption; copied/repeated commits rejected; complete durable state unchanged");
            double remaining = source.Cooldown;
            GetTree().Paused = true; source._Process(0.1);
            Require(source.Cooldown == remaining && !source.TryContact() && !audio.GetNode<AudioStreamPlayer>("WaterContact").Playing, "Pause did not freeze/cancel.");
            GetTree().Paused = false;
            source._Process(double.NaN); Require(source.Cooldown == remaining, "Invalid time replenished cooldown.");
            source._Process(0.251); Require(source.Cooldown == remaining, "Long frame replenished cooldown.");
            source._Notification((int)NotificationApplicationFocusOut); source._Process(0.1);
            Require(!source.TryContact() && source.Cooldown == remaining, "Focus loss admitted/freed contact.");
            source._Notification((int)NotificationApplicationFocusIn);
            seeker.SetPhysicsProcess(false); Require(!source.TryContact(), "Disabled Seeker admitted contact."); seeker.SetPhysicsProcess(true);
            source.ResetPresentation();
            AudioSettingsPanel settingsPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); settingsPanel.Open();
            Require(!source.TryContact(), "Reading overlay admitted contact."); settingsPanel.Close(false);
            LineEdit input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input"); GetNode<CanvasLayer>("../DialogueOverlay").Visible = true;
            input.GrabFocus(); Require(!source.TryContact(), "Text input admitted contact."); input.ReleaseFocus(); GetNode<CanvasLayer>("../DialogueOverlay").Visible = false;
            Node2D geometry = GetNode<Node2D>("../OpeningRegionStreamLaundrySceneSource"); geometry.Position += Vector2.One;
            Require(!source.TryContact(), "Foreign stream transform admitted contact."); geometry.Position -= Vector2.One;
            Polygon2D water = geometry.GetNode<Polygon2D>("StreamWater"); Vector2[] polygon = water.Polygon;
            water.Polygon = [.. polygon.Select(v => v + Vector2.One)]; Require(!source.TryContact(), "Changed water admitted contact."); water.Polygon = polygon;
            GreyboxNavigation navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
            Vector2[] vertices = navigation.NavigationPolygon.GetVertices(); Vector2[] changedVertices = (Vector2[])vertices.Clone();
            changedVertices[0] += Vector2.One; navigation.NavigationPolygon.SetVertices(changedVertices);
            Require(!source.TryContact(), "Mutated navigation contents admitted contact."); navigation.NavigationPolygon.SetVertices(vertices);
            navigation.Enabled = false; Require(!source.TryContact(), "Disabled navigation admitted contact."); navigation.Enabled = true;
            Rid mapRid = NavigationServer2D.RegionGetMap(navigation.GetRid());
            NavigationServer2D.MapSetActive(mapRid, false); await Frame(); await Frame(); await Frame();
            Require(!source.TryContact(), "Inactive navigation map admitted contact.");
            NavigationServer2D.MapSetActive(mapRid, true); await Frame(); await Frame(); await Frame();
            Require(source.TryContact(), "Contact failed after valid owners returned.");
            SaveSnapshotRestorationController restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            WorldEventId unknown = new("unknown-water-probe");
            SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
                initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
            StreamWaterContactCommit last = source.LastCommit!; double cooldown = source.Cooldown;
            Require(!restore.TryRestore(rejected) && ReferenceEquals(last, source.LastCommit) && source.Cooldown == cooldown, "Failed restore changed contact state.");
            Require(restore.TryRestore(initial) && source.LastCommit is null && source.Cooldown == 0, "Successful restore retained action token.");
            Require(!audio.GetNode<AudioStreamPlayer>("WaterContact").Playing && captions.GetNode<Label>("Panel/Lines/WaterContact").Text == string.Empty, "Restore replayed/retained feedback.");
            checks.Add("pause/focus/invalid time/disabled input, overlays/text focus, changed transform/water/navigation and failed/successful full restore");
            await Place(reached); Require(source.TryContact(), "Missing-bus fixture failed to commit.");
            source.ResetPresentation(); int effects = AudioServer.GetBusIndex("Effects"); AudioServer.SetBusName(effects, "MissingEffects");
            Require(source.TryContact() && !audio.GetNode<AudioStreamPlayer>("WaterContact").Playing, "Missing Effects fell through to Master.");
            Require(captions.GetNode<Label>("Panel/Lines/WaterContact").Text == "Water contact", "Missing bus removed caption.");
            AudioServer.SetBusName(effects, "Effects"); source.ResetPresentation();
            Require(source.TryContact(), "Lifecycle final contact failed."); seeker.GlobalPosition = new(1800, 500); source._Process(0);
            Require(!audio.GetNode<AudioStreamPlayer>("WaterContact").Playing, "Departure did not stop contact.");
            checks.Add("missing Effects stream/bus retain commits/captions without fallback; departure cancels owned audio only");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Lifecycle/action checks changed full save.");
            Finish(before);
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    internal void ArrangeSave()
    {
        JournalController journal = GetNode<JournalController>("../JournalController");
        if (journal.Journal is null) { Require(GetNode<RepresentativeJournalCompositionCoordinator>("../RepresentativeJournalCompositionCoordinator").TryComposeAndInitialize(), "Journal fixture failed."); }
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        if (map.Map is null) { MapSaveStateRestorationProjection.Restore(new(GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id, [], [], [])).InstallInto(map); }
        var routes = GetNode<OpeningRegionInvestigationRouteProgressController>("../OpeningRegionInvestigationRouteProgressController");
        if (routes.Progress is null) { Require(SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routes, map, GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource")), "Route fixture failed."); }
    }
    private void Finish(string save)
    {
        string output = OS.GetEnvironment("ALLRPCS_WATER_CHECK_OUTPUT");
        if (output.Length > 0)
        {
            File.WriteAllText(output + ".save.json", save); File.WriteAllText(output, JsonSerializer.Serialize(new
            {
                mode,
                checks,
                commits = source.CommitCount,
                presentations = audio.PresentationCount,
                transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Select(e => e.SpokenStatement?.Text)
            }, Options));
        }
        GD.Print("PASS: water Main: actual reach/input, exact commits, full-save parity and lifecycle."); GetTree().Quit();
    }
    private async Task Place(Vector2 position)
    { seeker.GlobalPosition = position; for (int i = 0; i < 8; i++) { await Frame(); } }
    private async Task Move(string action, int count)
    { Input.ActionPress(action); for (int i = 0; i <= count; i++) { await Physics(); } Input.ActionRelease(action); await Frame(); }
    private async Task Frame() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private async Task Physics() => await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private async Task Capture(string name)
    { await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); Require(GetViewport().GetTexture().GetImage().SavePng(name) == Error.Ok, "Capture failed."); }

    private async Task Views(AudioSettingsController settings)
    {
        string directory = OS.GetEnvironment("ALLRPCS_WATER_VIEWS"); Directory.CreateDirectory(directory);
        // Isolate prior caption writers in this disposable layout fixture; the live water owner stays active.
        Node priorFootsteps = GetNode<Node>("../OpeningRegionFootstepAudioController");
        priorFootsteps.GetParent().RemoveChild(priorFootsteps); priorFootsteps.QueueFree();
        foreach (string owner in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource" })
        { GetNode<Node>("../" + owner).SetProcess(false); }
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); source.ResetPresentation(); await Frame(); await Frame();
            Button button = (Button)panel.FindChild("TouchWater", true, false);
            GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
            for (int tab = 0; tab < 64 && !button.HasFocus(); tab++)
            {
                Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true }); await Frame();
                Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false }); await Frame();
            }
            Require(button.HasFocus(), "Native Tab could not reach contextual button.");
            long prior = source.CommitCount;
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true }); await Frame();
            long count = source.CommitCount; Require(count == prior + 1, "Native keyboard activation failed.");
            for (int tick = 0; tick < 8; tick++) { source._Process(0.25); }
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true, Echo = true }); await Frame();
            Require(source.CommitCount == count, "Held-key echo produced a contact after cooldown.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = false }); await Frame();
            source.ResetPresentation(); await Frame();
            prior = source.CommitCount;
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, Pressed = true }); await Frame();
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, Pressed = false }); await Frame();
            Require(source.CommitCount == prior + 1, "Native Space activation failed.");
            source.ResetPresentation(); await Frame(); await Frame(); prior = source.CommitCount;
            Require(!button.Disabled && source.CanOffer, "Native mouse button not ready.");
            Vector2 buttonCenter = GetViewport().GetFinalTransform() * button.GetGlobalRect().GetCenter();
            Input.ParseInputEvent(new InputEventMouseMotion { Position = buttonCenter, GlobalPosition = buttonCenter }); await Frame();
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = buttonCenter, GlobalPosition = buttonCenter, Pressed = true }); await Frame();
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = buttonCenter, GlobalPosition = buttonCenter, Pressed = false }); await Frame();
            Require(source.CommitCount == prior + 1, "Native mouse activation failed: " + size + ", " + buttonCenter + ", count=" + source.CommitCount + ", disabled=" + button.Disabled + ", offer=" + source.CanOffer);
            captions.SetRegion("Wind, rain and timber creak"); captions.SetAction("Treatment handling", 60);
            captions.SetFootstep("Footsteps on timber"); captions.SetConversation("Conversation · off-screen right"); captions.SetInterior("Outdoor sounds softened");
            await Frame(); await Frame(); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Control box = captions.GetNode<Control>("Panel");
            Require(GetViewport().GetVisibleRect().Encloses(box.GetGlobalRect()), "Six-line panel outside viewport.");
            foreach (Label line in captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>())
            { Require(line.Text.Length > 0 && box.GetGlobalRect().Encloses(line.GetGlobalRect()), "Caption missing/clipped: " + line.Name); }
            Require(GetViewport().GetVisibleRect().Encloses(panel.GetNode<Control>("Panel").GetGlobalRect()), "Button/status outside viewport.");
            await Capture(Path.Combine(directory, "contact-" + size.X + ".png"));
            settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, false, false));
            Require(!captions.Visible && ((Label)panel.FindChild("Status", true, false)).Text == StreamWaterContactPanel.StatusText, "Captions-off removed status.");
            await Capture(Path.Combine(directory, "captions-off-" + size.X + ".png")); settings.RestoreDefaults();
            AudioSettingsPanel preferences = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); preferences.Open(button); await Frame();
            Require(!source.TryContact(), "Audio reading admitted contact."); Button credit = (Button)preferences.FindChild("SourceCredits", true, false);
            credit.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            Require(((RichTextLabel)preferences.FindChild("Credits", true, false)).Text.Contains("Water contact A01-W01", StringComparison.Ordinal), "New notice missing.");
            RichTextLabel creditText = (RichTextLabel)preferences.FindChild("Credits", true, false);
            creditText.GetVScrollBar().Value = creditText.GetVScrollBar().MaxValue; await Frame();
            await Capture(Path.Combine(directory, "credits-" + size.X + ".png")); preferences._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Require(credit.HasFocus(), "Credits keyboard return failed.");
            Button history = (Button)preferences.FindChild("RecentSoundCues", true, false); history.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            await Capture(Path.Combine(directory, "history-" + size.X + ".png")); preferences._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Require(history.HasFocus(), "History keyboard return failed."); preferences.Close(false);
        }
        source.ResetPresentation(); await Frame(); Require(source.TryContact(), "Native focus fixture failed.");
        GetWindow().Mode = Window.ModeEnum.Minimized; await Frame(); await Frame();
        Require(!GetWindow().HasFocus(), "Native window did not lose focus."); source._Process(0.1);
        Require(!audio.GetNode<AudioStreamPlayer>("WaterContact").Playing, "Native focus loss did not stop contact.");
        GetWindow().Mode = Window.ModeEnum.Windowed; GetWindow().GrabFocus();
        checks.Add("1280x720 and 1920x1080: six captions/status, native mouse/Tab/Enter/Space activation/echo guard, captions-off, credits/history return; actual native focus loss");
    }
}
