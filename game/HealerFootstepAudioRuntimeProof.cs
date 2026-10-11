using System.Reflection;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Stopped-by-default diagnostic; explicit Movie Maker captures the installed spatial players and Master.</summary>
public partial class HealerFootstepAudioRuntimeProof : Node2D
{
    private sealed record Case(string Group, string Name, FootstepMaterial Material, int Variant);
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly List<Case> cases = [];
    private readonly List<object> timeline = [];
    private Node main = null!;
    private OpeningRegionHealerFootstepAudioController audio = null!;
    private AudioSettingsController settings = null!;
    private AudioListener2D listener = null!;
    private SaveSnapshot initial = null!;
    private FootstepContact contact = null!;
    private Camera2D camera = null!;
    private bool automated;
    private int frame, started, index = -1;
    private long prior;
    private bool sourceSupported, listenerSupported;
    private string originalBus = "Effects";

    /// <inheritdoc />
    public override void _Ready()
    {
        main = GetNode<Node>("Main"); main.SetProcess(false);
        main.GetNode<Node>("TemporaryCharacterTurnCompletionController").SetProcess(false);
        main.GetNode<Healer>("Healer").SetPhysicsProcess(false); main.GetNode<Healer>("Healer").Visible = true;
        audio = main.GetNode<OpeningRegionHealerFootstepAudioController>("OpeningRegionHealerFootstepAudioController");
        audio.Automatic = false; audio.ResetAfterLoad();
        settings = main.GetNode<AudioSettingsController>("AudioSettingsController");
        listener = main.GetNode<AudioListener2D>("OpeningRegionPositionalAudioController/SeekerListener");
        camera = main.GetNode<Camera2D>("Seeker/FollowCamera"); camera.PositionSmoothingEnabled = false;
        automated = OS.GetEnvironment("ALLRPCS_HEALER_FOOTSTEP_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Actual Master evidence requires Movie Maker."); }
        StopBackground(); GetWindow().Size = new(320, 180);
        string[] groups = ["earth-six-variants", "gravel-six-variants", "leaves-six-variants", "timber-six-variants", "distance-zero", "distance-72", "distance-96", "distance-191", "distance-192", "distance-193", "left-bearing", "right-bearing", "mono", "zoom-08", "zoom-2", "camera-offset", "effects-half", "master-half", "effects-zero", "effects-muted", "master-zero", "master-muted", "quiet-range", "other-bus-zero", "missing-stream", "missing-effects-bus", "captions-off", "stationary-wait-arrival", "wall-pressure", "unsupported-segment", "interior-source", "interior-listener", "duplicate-foreign-motion", "teleport-invalid-delta", "listener-stale-replaced", "source-hidden-moved-replaced", "pause-focus-loss", "reading-overlay", "successful-load", "failed-load", "mute-unmute-no-replay", "two-healer-tails", "retained-seeker-action-reply-overlap"];
        foreach (string group in groups)
        {
            if (group.EndsWith("six-variants", StringComparison.Ordinal))
            {
                FootstepMaterial material = group.StartsWith("gravel", StringComparison.Ordinal) ? FootstepMaterial.Gravel
                    : group.StartsWith("leaves", StringComparison.Ordinal) ? FootstepMaterial.Leaves
                    : group.StartsWith("timber", StringComparison.Ordinal) ? FootstepMaterial.Timber : FootstepMaterial.Earth;
                for (int i = 1; i <= 6; i++) { cases.Add(new(group, group + "-" + i, material, i)); }
            }
            else
            {
                string[] parts = group switch
                {
                    "stationary-wait-arrival" => ["stationary", "wait", "arrival"],
                    "duplicate-foreign-motion" => ["duplicate", "foreign"],
                    "teleport-invalid-delta" => ["teleport", "invalid-delta"],
                    "listener-stale-replaced" => ["stale", "replaced"],
                    "source-hidden-moved-replaced" => ["hidden", "moved", "replaced"],
                    "pause-focus-loss" => ["pause", "focus-loss"],
                    _ => [group]
                };
                // Balanced unchanged timber preserves a raw-channel pan comparison;
                // accepted earth PCM has a stronger original right channel.
                foreach (string part in parts) { cases.Add(new(group, part, group is "left-bearing" or "right-bearing" ? FootstepMaterial.Timber : FootstepMaterial.Earth, 1)); }
            }
        }
        if (!automated)
        {
            Label label = new() { Text = "Healer positional footstep proof · stopped", Position = new(12, 12) }; AddChild(label);
            Button play = new() { Text = "Play diagnostic exterior contact", Position = new(12, 48) }; AddChild(play);
            play.Pressed += () => audio.DiagnosticContact(new(FootstepMaterial.Earth, FootstepLeg.Left, 1, new(1700, 900)), listener);
            Button stop = new() { Text = "Stop", Position = new(12, 90) }; AddChild(stop); stop.Pressed += audio.ResetAfterLoad;
        }
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated)
        {
            if (frame++ == 10) { if (audio.PresentationCount != 0 || Playing()) { throw new InvalidOperationException("Stopped proof started audio."); } GD.Print("PASS: Healer output proof starts stopped."); }
            return;
        }
        try
        {
            if (frame < 12) { frame++; return; }
            if (index < 0 || frame - started >= 120)
            {
                if (index >= 0) { FinishCase(); }
                if (++index == cases.Count)
                {
                    RestoreBus(); audio.ResetAfterLoad();
                    File.WriteAllText(OS.GetEnvironment("ALLRPCS_HEALER_FOOTSTEP_TIMELINE"), JsonSerializer.Serialize(timeline, Options));
                    GD.Print($"PASS: Healer actual Master: 43 groups/{cases.Count} independently measured cases."); GetTree().Quit(); return;
                }
                started = frame; Configure();
            }
            int local = frame - started; Case c = cases[index];
            if (local == 20) { Trigger(); }
            if (local == 24 && c.Group == "two-healer-tails") { audio.DiagnosticContact(contact with { Variant = 2 }, listener); }
            if (local == 26 && c.Group is "pause-focus-loss" or "successful-load" or "reading-overlay")
            {
                if (c.Name == "focus-loss") { audio._Notification((int)NotificationApplicationFocusOut); }
                else if (c.Group == "successful-load")
                { if (!main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(initial)) { throw new InvalidOperationException("Output restore failed."); } }
                else { audio.CancelMotion(); }
            }
            if (local == 26 && c.Group == "mute-unmute-no-replay") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Effects, 1, true)); }
            if (local == 35 && c.Group == "mute-unmute-no-replay") { settings.RestoreDefaults(); }
            frame++;
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private void Configure()
    {
        RestoreBus(); audio.ResetAfterLoad(); audio.RestoreDiagnosticResources(); audio._Notification((int)NotificationApplicationFocusIn);
        StopBackground(); settings.RestoreDefaults();
        if (initial is null)
        {
            main.GetNode<HandToolTapRuntimeSmokeCheck>("HandToolTapRuntimeSmokeCheck").ArrangeSave();
            initial = main.GetNode<SaveSnapshotCaptureController>("SaveSnapshotCaptureController").Capture()!;
        }
        Case c = cases[index]; NVector origin = c.Material switch
        {
            FootstepMaterial.Gravel => new(2528, 1280),
            FootstepMaterial.Leaves => new(825, 250),
            FootstepMaterial.Timber => new(1800, 500),
            _ => new(1700, 900)
        };
        contact = new(c.Material, FootstepLeg.Left, c.Variant, origin);
        Vector2 listenerPosition = new(origin.X, origin.Y);
        if (c.Group != "distance-zero" && c.Group.StartsWith("distance-", StringComparison.Ordinal)) { listenerPosition.Y += float.Parse(c.Group[9..], System.Globalization.CultureInfo.InvariantCulture); }
        if (c.Group == "left-bearing") { listenerPosition.X += 96; }
        if (c.Group == "right-bearing") { listenerPosition.X -= 96; }
        main.GetNode<Seeker>("Seeker").GlobalPosition = listenerPosition;
        listener.GlobalPosition = listenerPosition; camera.Zoom = Vector2.One; camera.Offset = Vector2.Zero;
        if (c.Group == "zoom-08") { camera.Zoom = new(0.8f, 0.8f); }
        if (c.Group == "zoom-2") { camera.Zoom = new(2, 2); }
        if (c.Group == "camera-offset") { camera.Offset = new(100, 0); }
        camera.ResetSmoothing(); camera.ForceUpdateScroll();
        sourceSupported = listenerSupported = true;
        AudioPreferences p = settings.Preferences;
        if (c.Group == "effects-half") { p = p.WithChannel(AudioCategory.Effects, 0.5, false); }
        if (c.Group == "master-half") { p = p.WithChannel(AudioCategory.Master, 0.5, false); }
        if (c.Group == "effects-zero") { p = p.WithChannel(AudioCategory.Effects, 0, false); }
        if (c.Group == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
        if (c.Group == "master-zero") { p = p.WithChannel(AudioCategory.Master, 0, false); }
        if (c.Group == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
        if (c.Group == "other-bus-zero") { p = p.WithChannel(AudioCategory.Ambience, 0, false); }
        if (c.Group == "quiet-range") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
        if (c.Group == "mono") { p = new(p.Channels, p.Preset, true); }
        if (c.Group == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
        settings.SetPreferences(p);
        if (c.Group == "missing-stream") { audio.RemoveDiagnosticResources(); }
        if (c.Group == "missing-effects-bus") { AudioServer.SetBusName(AudioServer.GetBusIndex("Effects"), "MissingEffects"); originalBus = "MissingEffects"; }
        if (c.Group == "unsupported-segment") { sourceSupported = false; }
        if (c.Group == "interior-source") { sourceSupported = OpeningRegionFootstepMaterialProjection.Select(new(2600, 1400), true) is not null; }
        if (c.Group == "interior-listener") { listenerSupported = OpeningRegionFootstepMaterialProjection.Select(new(1500, 200), true) is not null; }
        prior = audio.PresentationCount;
    }
    private void Trigger()
    {
        Case c = cases[index];
        camera.ForceUpdateScroll();
        GD.Print($"OUTPUT {c.Group}/{c.Name}: camera={camera.GetScreenCenterPosition()} listener={listener.GlobalPosition} canvas={GetViewport().CanvasTransform}");
        if (c.Group is "stationary-wait-arrival" or "teleport-invalid-delta" or "wall-pressure")
        {
            FootstepContactCadence cadence = new(); cadence.Advance(new(1, contact.Position, contact.Position, 0.1, 180), _ => FootstepMaterial.Earth);
            var result = cadence.Advance(new(2, contact.Position, c.Name == "teleport" ? contact.Position + new NVector(200, 0) : contact.Position,
                c.Name == "invalid-delta" ? double.NaN : 0.1, 180), _ => FootstepMaterial.Earth);
            if (result.Contact is not null) { throw new InvalidOperationException("Invalid/stationary motion emitted contact."); }
            return;
        }
        if (c.Group == "duplicate-foreign-motion")
        {
            var body = main.GetNode<Healer>("Healer"); var sample = body.LastMotion;
            if (sample is not null) { typeof(OpeningRegionHealerFootstepAudioController).GetMethod("OnMotion", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(audio, [c.Name == "duplicate" ? sample : sample with { Tick = sample.Tick + 1 }]); }
            return;
        }
        if (c.Group is "listener-stale-replaced" or "source-hidden-moved-replaced") { RejectChangedBinding(c); return; }
        audio.DiagnosticContact(contact, listener, sourceSupported, listenerSupported);
        if (c.Group == "failed-load")
        {
            long before = audio.PresentationCount;
            WorldEventId unknown = new("unknown-healer-output-event");
            SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
                initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
            if (main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(rejected)
                || audio.PresentationCount != before || !Playing()) { throw new InvalidOperationException("Failed restore changed cue."); }
        }
        if (c.Group == "retained-seeker-action-reply-overlap")
        {
            main.GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController").PresentContact(contact);
            var material = main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController"); material.PlayCue("A04"); material.PlayCue("A05");
            main.GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController").DiagnosticReply(listener.GlobalPosition, listener.GlobalPosition, 192);
        }
    }
    private void RejectChangedBinding(Case c)
    {
        bool Invoke(string method) => (bool)typeof(OpeningRegionHealerFootstepAudioController)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(audio, null)!;
        Healer body = main.GetNode<Healer>("Healer"); body.SetPhysicsProcess(true);
        if (!Invoke("Bindings") || !Invoke("ListenerValid")) { throw new InvalidOperationException("Normal proof binding unavailable."); }
        if (c.Group == "listener-stale-replaced")
        {
            if (c.Name == "stale")
            {
                Vector2 p = listener.GlobalPosition; listener.GlobalPosition += new Vector2(10, 0);
                if (Invoke("ListenerValid")) { throw new InvalidOperationException("Stale listener admitted."); }
                listener.GlobalPosition = p;
            }
            else
            {
                Node parent = listener.GetParent(); parent.RemoveChild(listener);
                AudioListener2D replacement = new() { Name = "SeekerListener", TopLevel = true }; parent.AddChild(replacement);
                replacement.GlobalPosition = listener.GlobalPosition; replacement.MakeCurrent();
                if (Invoke("ListenerValid")) { throw new InvalidOperationException("Replaced listener admitted."); }
                parent.RemoveChild(replacement); replacement.QueueFree(); parent.AddChild(listener); listener.MakeCurrent();
            }
        }
        else if (c.Name == "hidden")
        {
            body.Visible = false; if (Invoke("Bindings")) { throw new InvalidOperationException("Hidden body admitted."); }
            body.Visible = true;
        }
        else if (c.Name == "moved")
        {
            var projection = main.GetNode<Node2D>("OpeningRegionProjectionSource"); projection.Position += Vector2.One;
            if (Invoke("Bindings")) { throw new InvalidOperationException("Moved source admitted."); }
            projection.Position -= Vector2.One;
        }
        else
        {
            Node parent = body.GetParent(); parent.RemoveChild(body); Healer replacement = (Healer)body.Duplicate();
            replacement.Name = "Healer"; replacement.ProcessMode = ProcessModeEnum.Disabled; parent.AddChild(replacement);
            if (Invoke("Bindings")) { throw new InvalidOperationException("Value-equal replaced source admitted."); }
            parent.RemoveChild(replacement); replacement.Free(); parent.AddChild(body);
        }
        body.SetPhysicsProcess(false);
    }
    private void FinishCase()
    {
        Case c = cases[index]; long presentations = audio.PresentationCount - prior;
        bool rejected = c.Group is "distance-192" or "distance-193" or "stationary-wait-arrival" or "wall-pressure" or "unsupported-segment" or "interior-source" or "interior-listener" or "duplicate-foreign-motion" or "teleport-invalid-delta" or "listener-stale-replaced" or "source-hidden-moved-replaced";
        if (presentations != (rejected ? 0 : c.Group == "two-healer-tails" ? 2 : 1)) { throw new InvalidOperationException("Wrong controlled presentation: " + c.Name); }
        timeline.Add(new
        {
            group = c.Group,
            name = c.Name,
            material = c.Material.ToString(),
            variant = c.Variant,
            start = started / 60.0,
            duration = 2,
            trigger = 20 / 60.0,
            presentations,
            sourceSupported,
            listenerSupported,
            cancel_at = c.Group is "pause-focus-loss" or "successful-load" or "reading-overlay" or "mute-unmute-no-replay" ? 26 / 60.0 : (double?)null
        });
    }
    private void RestoreBus() { if (originalBus == "MissingEffects") { AudioServer.SetBusName(AudioServer.GetBusIndex(originalBus), "Effects"); originalBus = "Effects"; } }
    private bool Playing() => audio.GetChildren().OfType<AudioStreamPlayer2D>().Any(p => p.Playing);
    private void StopBackground()
    {
        var material = main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController"); material.StopPlayback(); material.SetProcess(false);
        var positional = main.GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController"); positional.StopPlayback(); positional.SetProcess(false); positional.Automatic = false;
        var feet = main.GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController"); feet.ResetPresentation(); feet.SetProcess(false);
        var music = main.GetNode<OpeningRegionMusicAudioController>("OpeningRegionMusicAudioController"); music.ResetPresentation(); music.GetNode<AudioStreamPlayer>("Music").Stop(); music.SetProcess(false);
    }
}
