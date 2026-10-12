using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped native Master fixture; exercises only the installed clearing player and exact clock.</summary>
public partial class RainAudioRuntimeProof : Node2D
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly string[] Cases = ["distance-0",
        "distance-64",
        "distance-128",
        "distance-192",
        "distance-255",
        "distance-256",
        "distance-257",
        "left",
        "right",
        "mono",
        "zoom-08",
        "zoom-2",
        "camera-offset",
        "quiet",
        "ambience-half",
        "master-half",
        "other-bus-zero",
        "ambience-zero",
        "ambience-muted",
        "master-zero",
        "master-muted",
        "missing-stream",
        "missing-bus",
        "captions-off",
        "foreground-duck",
        "foreground-recovery",
        "enter-range-mid",
        "leave-range-mid",
        "pause",
        "focus-loss",
        "reading-overlay",
        "text-entry",
        "load-success",
        "load-failure",
        "teleport",
        "listener-stale",
        "listener-replaced",
        "listener-unsupported",
        "source-hidden",
        "source-moved",
        "source-replaced",
        "clearing-area-drift",
        "map-disabled",
        "invalid-delta",
        "mute-unmute",
        "retained-overlap",
        "source-unsupported",
        "source-disabled",
        "source-nonfinite",
        "map-drift",
        "journal-overlay",
        "map-overlay",
        "dialogue-overlay",
        "customization-overlay",
        "settings-overlay",
        "missing-stream-repair",
        "ambience-unmute",
        "master-unmute"];
    private readonly List<object> timeline = [];
    private Node main = null!;
    private OpeningRegionRainAudioController audio = null!;
    private AudioSettingsController settings = null!;
    private Seeker seeker = null!;
    private AudioListener2D listener = null!;
    private Camera2D camera = null!;
    private OpeningRegionWoodlandClearingSceneSource clearing = null!;
    private Polygon2D area = null!;
    private Vector2[] points = [];
    private Vector2[] navVertices = [];
    private SaveSnapshot initial = null!;
    private Node? removed;
    private Node? replacement;
    private int clearingIndex;
    private long duck;
    private int frame, started, index = -1;
    private long prior;
    private bool automated;
    private bool missingBus;
    private LineEdit? editor;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_RAIN_PACK") is { Length: > 0 } pack)
        { CheckPackage(pack); return; }
        ProcessMode = ProcessModeEnum.Always;
        main = GetNode<Node>("Main"); main.SetProcess(false);
        main.GetNode<Node>("TemporaryCharacterTurnCompletionController").SetProcess(false);
        audio = main.GetNode<OpeningRegionRainAudioController>("OpeningRegionRainAudioController"); audio.Automatic = false;
        settings = main.GetNode<AudioSettingsController>("AudioSettingsController");
        seeker = main.GetNode<Seeker>("Seeker"); seeker.SetPhysicsProcess(false);
        main.GetNode<Healer>("Healer").SetPhysicsProcess(false);
        listener = main.GetNode<AudioListener2D>("OpeningRegionPositionalAudioController/SeekerListener");
        camera = main.GetNode<Camera2D>("Seeker/FollowCamera"); camera.PositionSmoothingEnabled = false;
        clearing = main.GetNode<OpeningRegionWoodlandClearingSceneSource>("OpeningRegionWoodlandClearingSceneSource");
        area = clearing.GetNode<Polygon2D>("ClearingArea"); points = area.Polygon; clearingIndex = clearing.GetIndex();
        main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Close(false);
        main.GetNode<CanvasLayer>("DialogueOverlay").Visible = false;
        main.GetNode<CanvasLayer>("SettingsOverlay").Visible = false;
        StopBackground(); GetWindow().Size = new(320, 180); GetWindow().GrabFocus();
        automated = OS.GetEnvironment("ALLRPCS_RAIN_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Movie Maker required for native Master evidence."); }
        if (!automated) { AddChild(new Label { Text = "Rain proof · stopped", Position = new(12, 12) }); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!automated)
        {
            if (++frame == 10) { Require(audio.PresentationCount == 0 && !Playing(), "Stopped fixture emitted a passage."); GD.Print("PASS: rain native proof starts stopped."); }
            return;
        }
        try
        {
            if (frame < 12) { frame++; return; }
            if (index < 0 || frame - started >= 300)
            {
                if (index >= 0) { FinishCase(); }
                if (++index == Cases.Length)
                {
                    Restore(); audio.ResetAfterLoad();
                    File.WriteAllText(OS.GetEnvironment("ALLRPCS_RAIN_TIMELINE"), JsonSerializer.Serialize(timeline, Options));
                    GD.Print($"PASS: rain native Master: {Cases.Length} independent cases."); GetTree().Quit(); return;
                }
                started = frame; Configure();
            }
            int local = frame - started; string name = Cases[index];
            if (local == 20)
            {
                bool Guard(string method) => (bool)typeof(OpeningRegionRainAudioController).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(audio, null)!;
                GD.Print($"CASE {name}: active={Guard("Active")} bindings={Guard("Bindings")} listener={Guard("ListenerValid")} focus={GetWindow().HasFocus()} gui={GetViewport().GuiGetFocusOwner()?.Name}");
                for (int t = 0; t < 200; t++) { audio.DiagnosticAdvance(0.1, name != "source-unsupported", name != "listener-unsupported"); }
                if (name == "retained-overlap")
                {
                    main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController").PlayCue("A04");
                    var bed = main.GetNode<AudioStreamPlayer>("OpeningRegionMaterialAudioController/B04");
                    bed.VolumeLinear = 1; bed.Play();
                    main.GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController").DiagnosticReply(listener.GlobalPosition, listener.GlobalPosition, 192);
                    main.GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController").PresentContact(
                        new(FootstepMaterial.Earth, FootstepLeg.Left, 1, new(1000, 350)));
                }
                if (name == "load-failure") { Require(!main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(Rejected()), "Failed load accepted."); }
            }
            else if (local > 20) { audio.DiagnosticAdvance(1.0 / 60); }
            if (local == 50) { Transition(name); }
            if (local == 270 && name == "retained-overlap") { StopBackground(); }
            if (local == 70)
            {
                if (name == "pause") { GetTree().Paused = false; }
                if (name == "focus-loss") { audio._Notification((int)NotificationApplicationFocusIn); }
                if (name is "mute-unmute") { settings.RestoreDefaults(); }
                if (name == "reading-overlay") { main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Close(false); }
                if (name == "text-entry") { editor!.ReleaseFocus(); editor.QueueFree(); editor = null; }
            }
            frame++;
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private void Configure()
    {
        Restore(); audio.ResetAfterLoad(); audio.DiagnosticResources(true); settings.RestoreDefaults(); StopBackground();
        if (initial is null)
        { main.GetNode<HandToolTapRuntimeSmokeCheck>("HandToolTapRuntimeSmokeCheck").ArrangeSave(); initial = main.GetNode<SaveSnapshotCaptureController>("SaveSnapshotCaptureController").Capture()!; }
        string name = Cases[index]; Vector2 position = new(1000, 350);
        if (name.StartsWith("distance-", StringComparison.Ordinal)) { position.Y += float.Parse(name[9..], System.Globalization.CultureInfo.InvariantCulture); }
        if (name == "left") { position.X += 128; }
        if (name == "right") { position.X -= 128; }
        if (name == "enter-range-mid") { position.Y += 257; }
        if (name == "leave-range-mid") { position.Y += 192; }
        seeker.GlobalPosition = position; listener.GlobalPosition = position;
        camera.Zoom = name == "zoom-08" ? new(0.8f, 0.8f) : name == "zoom-2" ? new(2, 2) : Vector2.One;
        camera.Offset = name == "camera-offset" ? new(100, 0) : Vector2.Zero;
        camera.ResetSmoothing(); camera.ForceUpdateScroll();
        AudioPreferences p = settings.Preferences;
        if (name == "mono") { p = new(p.Channels, p.Preset, true); }
        if (name == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
        if (name == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
        if (name == "ambience-half") { p = p.WithChannel(AudioCategory.Ambience, 0.5, false); }
        if (name == "master-half") { p = p.WithChannel(AudioCategory.Master, 0.5, false); }
        if (name == "other-bus-zero") { p = p.WithChannel(AudioCategory.Effects, 0, false); }
        if (name == "ambience-zero") { p = p.WithChannel(AudioCategory.Ambience, 0, false); }
        if (name == "master-zero") { p = p.WithChannel(AudioCategory.Master, 0, false); }
        if (name is "ambience-muted" or "ambience-unmute") { p = p.WithChannel(AudioCategory.Ambience, 1, true); }
        if (name is "master-muted" or "master-unmute") { p = p.WithChannel(AudioCategory.Master, 1, true); }
        settings.SetPreferences(p);
        if (name is "missing-stream" or "missing-stream-repair") { audio.DiagnosticResources(false); }
        if (name == "missing-bus") { AudioServer.SetBusName(AudioServer.GetBusIndex("Ambience"), "MissingAmbience"); missingBus = true; }
        if (name == "foreground-duck") { duck = settings.Ducking.Begin(AudioForeground.Speech); }
        if (name == "foreground-recovery") { duck = settings.Ducking.Begin(AudioForeground.ConsequentialCue); }
        if (name == "listener-stale") { listener.GlobalPosition += new Vector2(5, 0); }
        if (name == "listener-replaced")
        {
            Node parent = listener.GetParent(); parent.RemoveChild(listener); removed = listener;
            var fresh = new AudioListener2D { Name = "SeekerListener", TopLevel = true }; replacement = fresh; parent.AddChild(fresh); fresh.GlobalPosition = position; fresh.MakeCurrent();
        }
        if (name == "source-disabled") { clearing.ProcessMode = ProcessModeEnum.Disabled; }
        if (name == "source-nonfinite") { clearing.Position = new(float.NaN, 0); }
        var nav = main.GetNode<GreyboxNavigation>("GreyboxNavigation"); navVertices = nav.NavigationPolygon.GetVertices();
        if (name == "map-drift") { Vector2[] drift = (Vector2[])navVertices.Clone(); drift[0] += Vector2.One; nav.NavigationPolygon.SetVertices(drift); }
        if (name == "source-hidden") { clearing.Visible = false; }
        if (name == "source-moved") { clearing.Position += Vector2.One; }
        if (name == "clearing-area-drift") { area.Polygon = [Vector2.Zero, Vector2.One]; }
        if (name == "source-replaced")
        {
            Node parent = clearing.GetParent(); parent.RemoveChild(clearing); removed = clearing;
            Node2D fresh = new() { Name = "OpeningRegionWoodlandClearingSceneSource" }; replacement = fresh; parent.AddChild(fresh); parent.MoveChild(fresh, clearingIndex);
        }
        if (name == "map-disabled") { main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = false; }
        GetWindow().GrabFocus();
        prior = audio.PresentationCount;
    }

    private void Transition(string name)
    {
        if (name == "foreground-recovery") { settings.Ducking.End(duck); duck = 0; }
        if (name == "enter-range-mid") { seeker.GlobalPosition = new(1000, 350); listener.GlobalPosition = seeker.GlobalPosition; }
        if (name is "leave-range-mid" or "teleport") { seeker.GlobalPosition += new Vector2(0, name == "teleport" ? 97 : 65); listener.GlobalPosition = seeker.GlobalPosition; audio.DiagnosticAdvance(0.01); audio._Process(0.01); }
        if (name is "ambience-unmute" or "master-unmute") { settings.RestoreDefaults(); }
        if (name == "missing-stream-repair") { audio.DiagnosticResources(true); }
        string? overlay = name switch { "journal-overlay" => "JournalSectionPanel", "map-overlay" => "SeekerMapPanel", "dialogue-overlay" => "DialogueOverlay", "settings-overlay" => "SettingsOverlay", _ => null };
        if (overlay is not null) { main.GetNode<CanvasLayer>(overlay).Visible = true; audio._Process(0.01); }
        if (name == "customization-overlay") { main.GetNode<Control>("SeekerCustomizationOverlay/Panel").Visible = true; audio._Process(0.01); }
        if (name == "pause") { GetTree().Paused = true; audio._Process(0.01); }
        if (name == "focus-loss") { audio._Notification((int)NotificationApplicationFocusOut); }
        if (name == "reading-overlay") { main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Open(); audio._Process(0.01); }
        if (name == "text-entry") { editor = new LineEdit(); AddChild(editor); editor.GrabFocus(); audio._Process(0.01); }
        if (name == "load-success") { Require(main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(initial), "Valid load failed."); }
        if (name == "invalid-delta") { audio._Process(double.NaN); }
        if (name == "mute-unmute") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Ambience, 1, true)); }
    }

    private void FinishCase()
    {
        string name = Cases[index]; long presentations = audio.PresentationCount - prior;
        bool rejected = name is "distance-256" or "distance-257" or "enter-range-mid" or "listener-stale" or "listener-replaced"
            or "listener-unsupported" or "source-hidden" or "source-moved" or "source-replaced" or "clearing-area-drift" or "map-disabled" or "source-unsupported" or "source-disabled" or "source-nonfinite" or "map-drift";
        Require(presentations == (rejected ? 0 : 1), "Wrong presentation count: " + name + "=" + presentations);
        bool cancelled = name is "leave-range-mid" or "pause" or "focus-loss" or "reading-overlay" or "text-entry" or "load-success" or "teleport" or "invalid-delta" or "mute-unmute" or "journal-overlay" or "map-overlay" or "dialogue-overlay" or "settings-overlay" or "customization-overlay";
        timeline.Add(new
        {
            name,
            start = started / 60.0,
            duration = 5,
            trigger = 20 / 60.0,
            presentations,
            cancel_at = cancelled ? 50 / 60.0 : (double?)null
        });
    }

    private SaveSnapshot Rejected()
    {
        WorldEventId unknown = new("unknown-rain-output-event");
        return new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
            initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
    }
    private void Restore()
    {
        GetTree().Paused = false; audio._Notification((int)NotificationApplicationFocusIn);
        if (missingBus) { AudioServer.SetBusName(AudioServer.GetBusIndex("MissingAmbience"), "Ambience"); missingBus = false; }
        if (duck != 0) { settings.Ducking.End(duck); duck = 0; }
        settings.Ducking.Clear();
        if (removed is not null)
        {
            Node parent = replacement!.GetParent(); parent.RemoveChild(replacement); replacement.Free(); parent.AddChild(removed);
            if (removed == clearing) { parent.MoveChild(clearing, clearingIndex); }
            if (removed == listener) { listener.MakeCurrent(); }
            removed = replacement = null;
        }
        clearing.ProcessMode = ProcessModeEnum.Inherit;
        if (navVertices.Length > 0) { main.GetNode<GreyboxNavigation>("GreyboxNavigation").NavigationPolygon.SetVertices(navVertices); }
        foreach (string overlay in new[] { "JournalSectionPanel", "SeekerMapPanel", "DialogueOverlay", "SettingsOverlay" }) { main.GetNode<CanvasLayer>(overlay).Visible = false; }
        main.GetNode<Control>("SeekerCustomizationOverlay/Panel").Visible = false;
        clearing.Visible = true; clearing.Position = Vector2.Zero; area.Polygon = points;
        main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = true;
        main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Close(false);
        editor?.QueueFree(); editor = null;
    }
    private void StopBackground()
    {
        var material = main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController"); material.StopPlayback(); material.SetProcess(false);
        var positional = main.GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController"); positional.StopPlayback(); positional.SetProcess(false); positional.SetPhysicsProcess(false); positional.Automatic = false;
        var feet = main.GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController"); feet.ResetPresentation(); feet.SetProcess(false);
        var healer = main.GetNode<OpeningRegionHealerFootstepAudioController>("OpeningRegionHealerFootstepAudioController"); healer.Automatic = false; healer.CancelMotion();
        var foliage = main.GetNode<OpeningRegionFoliageAudioController>("OpeningRegionFoliageAudioController"); foliage.Automatic = false; foliage.CancelAndRebase(); foliage.SetProcess(false); foliage.SetPhysicsProcess(false);
        var wind = main.GetNode<OpeningRegionWindAudioController>("OpeningRegionWindAudioController");
        wind.Automatic = false; wind.CancelAndRebase(); wind.SetProcess(false); wind.SetPhysicsProcess(false);
        var music = main.GetNode<OpeningRegionMusicAudioController>("OpeningRegionMusicAudioController"); music.ResetPresentation(); music.GetNode<AudioStreamPlayer>("Music").Stop(); music.SetProcess(false);
    }
    private void CheckPackage(string pack)
    {
        try
        {
            Require(ProjectSettings.LoadResourcePack(pack, true), "Mounted resource archive failed.");
            var native = ResourceLoader.Load<AudioStreamWav>("res://assets/audio/rain/R01.wav", "", ResourceLoader.CacheMode.Ignore);
            byte[] expected = File.ReadAllBytes(OS.GetEnvironment("ALLRPCS_RAIN_EXPECTED_PCM"));
            File.WriteAllBytes("/private/tmp/rain-native-pcm16.bin", native.Data);
            GD.Print($"PACK rate={native.MixRate} stereo={native.Stereo} length={native.Data.Length} expected={expected.Length} format={native.Format} loop={native.LoopMode}");
            Require(native.MixRate == 48000 && native.Stereo && native.Data.SequenceEqual(expected)
                && native.LoopMode == AudioStreamWav.LoopModeEnum.Disabled, "Mounted native decoding differs.");
            string credits = Godot.FileAccess.GetFileAsString("res://assets/audio/rain/CREDITS.txt");
            Require(credits.Contains("Kresiek The Furry", StringComparison.Ordinal) && credits.Contains("CC0-1.0", StringComparison.Ordinal), "Packaged attribution missing.");
            File.WriteAllText(OS.GetEnvironment("ALLRPCS_RAIN_PACK_REPORT"), JsonSerializer.Serialize(new
            {
                status = "passed",
                exact = true,
                streams = 1,
                sample_rate = native.MixRate,
                frames = native.Data.Length / 4,
                conversion = "signed PCM24 arithmetic right shift by 8; little-endian PCM16; every mounted byte equal",
                native_pcm16_sha256 = Convert.ToHexString(SHA256.HashData(native.Data)).ToLowerInvariant(),
                credits_included = true
            }, Options));
            GD.Print("PASS: rain mounted resource archive: exact native PCM16 and source attribution."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private bool Playing() => audio.GetChildren().OfType<AudioStreamPlayer2D>().Any(p => p.Playing);
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
}
