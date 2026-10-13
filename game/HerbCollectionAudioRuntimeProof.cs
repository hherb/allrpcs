using System.Security.Cryptography;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped native Master fixture for the installed local cue, cancellation and mounted packaging.</summary>
public partial class HerbCollectionAudioRuntimeProof : Node2D
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly string[] Cases = ["default",
        "quiet",
        "mono",
        "effects-half",
        "master-half",
        "effects-zero",
        "master-zero",
        "effects-muted",
        "master-muted",
        "ambience-zero",
        "captions-off",
        "missing-stream",
        "missing-bus",
        "pause",
        "focus-loss",
        "reading-overlay",
        "text-entry",
        "journal-overlay",
        "map-overlay",
        "dialogue-overlay",
        "settings-overlay",
        "customization-overlay",
        "teleport",
        "invalid-delta",
        "source-hidden",
        "source-moved",
        "source-disabled",
        "source-replaced",
        "patch-drift",
        "actor-leaves",
        "listener-range",
        "listener-drift",
        "navigation-disabled",
        "spatial-left",
        "spatial-right",
        "spatial-edge",
        "actor-interior",
        "listener-interior",
        "mute-unmute",
        "missing-stream-repair",
        "load-success",
        "load-failure",
        "foreground",
        "retained-overlap",
        "recipe-overlap"];
    private static readonly string[] OverlayNames = ["SeekerMapPanel", "JournalSectionPanel", "DialogueOverlay", "SettingsOverlay"];
    private Node main = null!;
    private OpeningRegionHerbCollectionAudioController audio = null!;
    private AudioSettingsController settings = null!;
    private OpeningRegionHerbCollectionAudioSource scene = null!;
    private Seeker seeker = null!;
    private SaveSnapshot initial = null!;
    private readonly List<object> timeline = [];
    private int frame, index = -1, started;
    private long prior;
    private bool automated;
    private long duck;
    private LineEdit? editor;
    private Node? replacement;
    private int sceneIndex;
    private Healer healer = null!;
    private readonly List<AudioStreamPlayer> mix = [];
    private int firstStoppedFrame = -1;
    private readonly List<object> stopStates = [];

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; main = GetNode<Node>("Main"); main.SetProcess(false);
        if (OS.GetEnvironment("ALLRPCS_HERB_PACK") is { Length: > 0 } pack) { CheckPackage(pack); return; }
        main.GetNode<Node>("TemporaryCharacterTurnCompletionController").SetProcess(false);
        audio = main.GetNode<OpeningRegionHerbCollectionAudioController>("OpeningRegionHerbCollectionAudioController"); audio.Automatic = false;
        settings = main.GetNode<AudioSettingsController>("AudioSettingsController");
        seeker = main.GetNode<Seeker>("Seeker"); seeker.GlobalPosition = new(920, 350);
        healer = main.GetNode<Healer>("Healer"); healer.CommandWait(); healer.Visible = true; healer.GlobalPosition = new(952, 350);
        scene = main.GetNode<OpeningRegionHerbCollectionAudioSource>("OpeningRegionHerbCollectionAudioSource"); sceneIndex = scene.GetIndex();
        main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Close(false);
        main.GetNode<CanvasLayer>("DialogueOverlay").Visible = false; main.GetNode<CanvasLayer>("SettingsOverlay").Visible = false;
        foreach (var node in main.GetChildren().Where(n => n.Name.ToString().EndsWith("AudioController", StringComparison.Ordinal) && !ReferenceEquals(n, audio)))
        { node.SetProcess(false); node.SetPhysicsProcess(false); }
        main.GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController").SetPhysicsProcess(true);
        GetWindow().Size = new(320, 180); GetWindow().GrabFocus();
        automated = OS.GetEnvironment("ALLRPCS_HERB_OUTPUT_CHECK") == "1";
        if (OS.GetEnvironment("ALLRPCS_HERB_LAST_CASE_CHECK") == "1") { index = Cases.Length - 3; }
        Require(!automated || OS.HasFeature("movie"), "Native Master requires Movie Maker.");
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!automated)
        {
            if (++frame == 15) { Require(audio.PresentationCount == 0 && !Playing(), "Stopped proof emitted audio."); GD.Print("PASS: herb standalone starts stopped."); }
            return;
        }
        try
        {
            if (frame < 15) { frame++; return; }
            if (index < 0 || frame - started >= 300)
            {
                if (index >= 0) { FinishCase(); }
                if (++index == Cases.Length)
                {
                    Restore(); File.WriteAllText(OS.GetEnvironment("ALLRPCS_HERB_TIMELINE"), JsonSerializer.Serialize(timeline, Options));
                    File.WriteAllText(OS.GetEnvironment("ALLRPCS_HERB_TIMELINE") + ".stops.json", JsonSerializer.Serialize(stopStates, Options));
                    GD.Print($"PASS: herb native Master: {timeline.Count} measured cases."); GetTree().Quit(); return;
                }
                started = frame; firstStoppedFrame = -1; Configure();
            }
            int local = frame - started; string name = Cases[index];
            if (local > 22 && local < 200 && firstStoppedFrame < 0 && !Playing())
            {
                firstStoppedFrame = local;
                stopStates.Add(new
                {
                    name,
                    local,
                    delta,
                    focused = GetWindow().HasFocus(),
                    bindings = audio.Bindings(),
                    paused = GetTree().Paused,
                    active = typeof(OpeningRegionHerbCollectionAudioController).GetMethod("Active", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(audio, null)
                });
            }
            if (local == 20)
            {
                prior = audio.PresentationCount; audio.DiagnosticPlay();
                if (name == "recipe-overlap")
                {
                    foreach (string resource in new[] { "remedy-heating/H01", "remedy-steeping/S01", "remedy-cooling/C01" })
                    {
                        var player = new AudioStreamPlayer { Stream = GD.Load<AudioStreamWav>("res://assets/audio/" + resource + ".wav"), Bus = "Effects" };
                        AddChild(player); mix.Add(player); player.Play();
                    }
                }
                if (name == "retained-overlap")
                {
                    main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController").PlayCue("A04");
                    var bed = main.GetNode<AudioStreamPlayer>("OpeningRegionMaterialAudioController/B04"); bed.VolumeLinear = 1; bed.Play();
                }
                if (name == "load-failure") { Require(!main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(Rejected()), "Rejected load accepted."); }
            }
            if (local == 22 && name == "recipe-overlap") { Require(Playing() && mix.All(p => p.Playing), $"Independent recipe output coexistence failed: rustle={Playing()}, presentations={audio.PresentationCount - prior}, streams={string.Join(",", mix.Select(p => p.Playing))}, bindings={audio.Bindings()}, visible={healer.Visible}, actor={healer.GlobalPosition}, listener={seeker.GlobalPosition}."); }
            if (local == 60 && name == "retained-overlap") { main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController").PlayCue("A05"); }
            if (local == 50) { Transition(name); }
            if (local == 70)
            {
                if (name == "pause") { GetTree().Paused = false; }
                if (name == "focus-loss") { audio._Notification((int)NotificationApplicationFocusIn); }
                if (name == "mute-unmute") { settings.RestoreDefaults(); }
                if (name == "missing-stream-repair") { audio.DiagnosticResources(true); }
            }
            if (local == 230 && name == "retained-overlap") { StopPlayers(main); }
            frame++;
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private void Configure()
    {
        Restore();
        if (initial is not null) { Require(main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(initial), "Case restore failed."); }
        audio.Automatic = false;
        main.GetNode<OpeningRegionRemedyHeatingAudioController>("OpeningRegionRemedyHeatingAudioController").Automatic = false;
        main.GetNode<OpeningRegionRemedySteepingAudioController>("OpeningRegionRemedySteepingAudioController").Automatic = false;
        audio.ResetAfterLoad(); audio.DiagnosticResources(true); StopPlayers(main);
        if (initial is null)
        {
            main.GetNode<HandToolTapRuntimeSmokeCheck>("HandToolTapRuntimeSmokeCheck").ArrangeSave();
            seeker.GlobalPosition = new(920, 350); healer.GlobalPosition = new(952, 350); main.GetNode<OpeningRegionMapVisitVerificationCoordinator>("OpeningRegionMapVisitVerificationCoordinator").TryVerifyCurrentVisit();
            initial = main.GetNode<SaveSnapshotCaptureController>("SaveSnapshotCaptureController").Capture()!;
        }
        string name = Cases[index]; settings.RestoreDefaults(); var p = settings.Preferences;
        if (name == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
        if (name == "mono") { p = new(p.Channels, p.Preset, true, true); }
        if (name == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
        if (name == "effects-half") { p = p.WithChannel(AudioCategory.Effects, 0.5, false); }
        if (name == "master-half") { p = p.WithChannel(AudioCategory.Master, 0.5, false); }
        if (name == "effects-zero") { p = p.WithChannel(AudioCategory.Effects, 0, false); }
        if (name == "master-zero") { p = p.WithChannel(AudioCategory.Master, 0, false); }
        if (name == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
        if (name == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
        if (name == "ambience-zero") { p = p.WithChannel(AudioCategory.Ambience, 0, false); }
        settings.SetPreferences(p);
        if (name is "missing-stream" or "missing-stream-repair") { audio.DiagnosticResources(false); }
        if (name == "missing-bus") { AudioServer.SetBusName(3, "AbsentEffects"); }
        if (name == "source-hidden") { main.GetNode<GreyboxInteractionTarget>("InteractionCandidates/HerbPatch").Visible = false; }
        if (name == "source-moved") { main.GetNode<GreyboxInteractionTarget>("InteractionCandidates/HerbPatch").Position += Vector2.One; }
        if (name == "source-disabled") { scene.ProcessMode = ProcessModeEnum.Disabled; }
        if (name == "patch-drift") { main.GetNode<GreyboxInteractionTarget>("InteractionCandidates/HerbPatch").Position += Vector2.One; }
        if (name == "navigation-disabled") { main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = false; }
        if (name == "spatial-left") { seeker.GlobalPosition = new(1016, 350); }
        if (name == "spatial-right") { seeker.GlobalPosition = new(824, 350); }
        if (name == "spatial-edge") { seeker.GlobalPosition = new(729, 350); }
        if (name == "actor-interior") { healer.GlobalPosition = new(2600, 1400); }
        if (name == "listener-interior") { seeker.GlobalPosition = new(2600, 1400); }
        if (name == "source-replaced")
        {
            main.RemoveChild(scene); replacement = new() { Name = "OpeningRegionHerbCollectionAudioSource" };
            main.AddChild(replacement); main.MoveChild(replacement, sceneIndex);
        }
        if (name == "foreground") { duck = settings.Ducking.Begin(AudioForeground.ConsequentialCue); }
        GetWindow().GrabFocus();
    }
    private void Transition(string name)
    {
        if (name == "actor-leaves") { healer.GlobalPosition = new(953, 350); }
        if (name == "listener-range") { seeker.GlobalPosition = new(728, 350); }
        if (name == "listener-drift") { main.GetNode<AudioListener2D>("OpeningRegionPositionalAudioController/SeekerListener").GlobalPosition += new Vector2(2, 0); }
        if (name == "pause") { GetTree().Paused = true; }
        if (name == "focus-loss") { audio._Notification((int)NotificationApplicationFocusOut); }
        if (name == "reading-overlay") { main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Open(); }
        if (name == "text-entry") { editor = new() { Name = "CoolingTextProbe" }; main.AddChild(editor); editor.GrabFocus(); }
        string? overlay = name switch { "journal-overlay" => "JournalSectionPanel", "map-overlay" => "SeekerMapPanel", "dialogue-overlay" => "DialogueOverlay", "settings-overlay" => "SettingsOverlay", _ => null };
        if (overlay is not null) { main.GetNode<CanvasLayer>(overlay).Visible = true; }
        if (name == "customization-overlay") { main.GetNode<Control>("SeekerCustomizationOverlay/Panel").Visible = true; }
        if (name == "teleport") { seeker.GlobalPosition += new Vector2(97, 0); }
        if (name == "invalid-delta") { audio._Process(double.NaN); }
        if (name == "mute-unmute") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Effects, 1, true)); }
        if (name == "load-success") { Require(main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(initial), "Exact load rejected."); }
        audio._Process(1.0 / 60);
    }
    private void FinishCase()
    {
        string name = Cases[index]; Require(!Playing(), "Tail did not expire: " + name);
        timeline.Add(new
        {
            name,
            startFrame = started,
            admitFrame = started + 20,
            endFrame = frame,
            transitionFrame = started + 50,
            presentations = audio.PresentationCount - prior,
            history = audio.RecentCues.Count
        });
    }
    private void Restore()
    {
        foreach (var player in mix) { player.Free(); }
        mix.Clear();
        GetTree().Paused = false; AudioServer.SetBusName(3, "Effects");
        if (replacement is not null) { main.RemoveChild(replacement); replacement.Free(); replacement = null; main.AddChild(scene); main.MoveChild(scene, sceneIndex); }
        scene.ProcessMode = ProcessModeEnum.Inherit; healer.Visible = true;
        var patch = main.GetNode<GreyboxInteractionTarget>("InteractionCandidates/HerbPatch"); patch.Visible = true; patch.Position = new(920, 350);
        main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = true;
        seeker.GlobalPosition = new(920, 350); healer.GlobalPosition = new(952, 350);
        foreach (string name in OverlayNames) { main.GetNode<CanvasLayer>(name).Visible = false; }
        main.GetNode<Control>("SeekerCustomizationOverlay/Panel").Visible = false; main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Close(false);
        if (editor is not null) { editor.ReleaseFocus(); editor.Free(); editor = null; }
        if (duck != 0) { settings.Ducking.End(duck); duck = 0; }
        audio._Notification((int)NotificationApplicationFocusIn);
    }
    private SaveSnapshot Rejected()
    {
        WorldEventId unknown = new("unknown-herb-proof-event");
        return new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
            initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
    }
    private void CheckPackage(string pack)
    {
        try
        {
            Require(ProjectSettings.LoadResourcePack(pack, true), "Mount failed.");
            var native = ResourceLoader.Load<AudioStreamWav>("res://assets/audio/herb-collection/HC01.wav", "", ResourceLoader.CacheMode.Ignore);
            byte[] expected = File.ReadAllBytes(OS.GetEnvironment("ALLRPCS_HERB_EXPECTED_PCM"));
            Require(native.MixRate == 48000 && native.Stereo && native.LoopMode == AudioStreamWav.LoopModeEnum.Disabled && native.Data.SequenceEqual(expected), "Mounted native PCM16 differs.");
            Require(Godot.FileAccess.GetFileAsString("res://assets/audio/herb-collection/CREDITS.txt").Contains("qubodup", StringComparison.Ordinal), "Mounted credits missing.");
            File.WriteAllText(OS.GetEnvironment("ALLRPCS_HERB_PACK_REPORT"), JsonSerializer.Serialize(new { status = "passed", exact = true, frames = native.Data.Length / 4, sample_rate = native.MixRate, sha256 = Convert.ToHexString(SHA256.HashData(native.Data)).ToLowerInvariant() }, Options));
            GD.Print("PASS: herb mounted pack: exact PCM16 and attribution."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private static void StopPlayers(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is AudioStreamPlayer player) { player.Stop(); }
            if (child is AudioStreamPlayer2D spatial) { spatial.Stop(); }
            StopPlayers(child);
        }
    }
    private bool Playing() => audio.GetNode<AudioStreamPlayer2D>("Rustle").Playing;
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
}
