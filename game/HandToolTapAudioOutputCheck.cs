using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped by default; controlled Master capture uses explicit actions in an ordinary Main instance.</summary>
public partial class HandToolTapAudioOutputCheck : Node2D
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly string[] Cases = ["default", "quiet", "master-muted", "effects-muted", "missing-stream", "master-half", "effects-half", "mono", "captions-off", "missing-effects-bus", "arrival-without-activation", "out-of-reach", "unsupported-segment", "duplicate-request", "stale-or-replaced-site", "overlay-rejection", "focus-loss", "restore-cancellation", "hidden-moved-or-replaced-source", "retained-caption-coexistence"];
    private readonly List<object> timeline = [];
    private Node main = null!;
    private OpeningRegionHandToolTapCoordinator source = null!;
    private OpeningRegionHandToolTapAudioController audio = null!;
    private AudioSettingsController settings = null!;
    private AudioStreamPlayer player = null!;
    private SaveSnapshot? initial;
    private long priorCommits, priorPresentations;
    private bool automated, admitted;
    private int frame, started, index = -1;

    /// <inheritdoc />
    public override void _Ready()
    {
        main = GetNode<Node>("Main"); main.SetProcess(false);
        main.GetNode<Node>("TemporaryCharacterTurnCompletionController").SetProcess(false);
        main.GetNode<Healer>("Healer").SetPhysicsProcess(false);
        source = main.GetNode<OpeningRegionHandToolTapCoordinator>("OpeningRegionHandToolTapCoordinator");
        audio = main.GetNode<OpeningRegionHandToolTapAudioController>("OpeningRegionHandToolTapAudioController");
        settings = main.GetNode<AudioSettingsController>("AudioSettingsController"); player = audio.GetNode<AudioStreamPlayer>("HandToolTap");
        automated = OS.GetEnvironment("ALLRPCS_TOOL_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Master evidence requires Movie Maker."); }
        source.Automatic = automated; audio.Automatic = automated;
        StopBackground();
        GetWindow().Size = new(320, 180);
        if (!automated)
        {
            Label label = new() { Text = "Wooden mallet output proof · stopped", Position = new(12, 12) }; AddChild(label);
            Button play = new() { Text = "Play diagnostic mallet contact", Position = new(12, 48) }; AddChild(play); play.Pressed += audio.DiagnosticPlay;
            Button stop = new() { Text = "Stop", Position = new(12, 90) }; AddChild(stop); stop.Pressed += audio.Cancel;
        }
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated)
        {
            if (frame++ == 10)
            {
                if (source.CommitCount != 0 || audio.PresentationCount != 0 || player.Playing)
                { throw new InvalidOperationException("Stopped output proof produced live feedback."); }
                GD.Print("PASS: hand-tool output standalone starts stopped; zero commits/presentations/playback.");
            }
            return;
        }
        try
        {
            if (frame < 8) { frame++; return; }
            if (index < 0 || frame - started >= 360)
            {
                if (index >= 0) { FinishCase(); }
                index++;
                if (index == Cases.Length)
                {
                    audio.Cancel();
                    File.WriteAllText(OS.GetEnvironment("ALLRPCS_TOOL_TIMELINE"), JsonSerializer.Serialize(timeline, Options));
                    string decode = OS.GetEnvironment("ALLRPCS_TOOL_DECODE_PATH");
                    if (decode.Length > 0) { File.WriteAllBytes(decode, GD.Load<AudioStreamWav>("res://assets/audio/hand-tool/A03-T01.wav").Data); }
                    GD.Print("PASS: 20 actual hand-tool Master-output cases through live commit owner."); GetTree().Quit(); return;
                }
                started = frame; Configure();
            }
            int local = frame - started; string name = Cases[index];
            if (local == 20) { GetWindow().GrabFocus(); }
            if (local == 30)
            {
                admitted = name != "arrival-without-activation" && source.TryTap();
                if (name == "retained-caption-coexistence")
                {
                    var material = main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController");
                    material.PlayCue("A04"); material.PlayCue("A05");
                    var captions = main.GetNode<MaterialSoundCaptionPanel>("MaterialSoundCaptionPanel");
                    captions.SetWaterContact("Water contact"); captions.SetCaskCover("Container and cloth handling");
                    if (captions.GetNode<Label>("Panel/Lines/HandToolTap").Text != "Wooden mallet contact"
                        || captions.GetNode<Label>("Panel/Lines/CaskCover").Text.Length == 0)
                    { throw new InvalidOperationException("Retained caption ownership collided."); }
                }
            }
            if (local == 38 && name == "focus-loss") { source._Notification((int)NotificationApplicationFocusOut); }
            if (local == 38 && name == "restore-cancellation")
            {
                if (!main.GetNode<SaveSnapshotRestorationController>("SaveSnapshotRestorationController").TryRestore(initial!))
                { throw new InvalidOperationException("Controlled successful restore failed."); }
            }
            frame++;
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private void Configure()
    {
        source._Notification((int)NotificationApplicationFocusIn); source.ResetPresentation(); StopBackground();
        var marker = main.GetNode<Marker2D>("OpeningRegionProjectionSource/village-edge");
        marker.Position = new(1700, 900); marker.Visible = true;
        main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = true;
        GetWindow().GrabFocus();
        settings.RestoreDefaults();
        string name = Cases[index];
        AudioPreferences p = settings.Preferences;
        if (name == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange, false, true); }
        if (name == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
        if (name == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
        if (name == "master-half") { p = p.WithChannel(AudioCategory.Master, 0.5, false); }
        if (name == "effects-half") { p = p.WithChannel(AudioCategory.Effects, 0.5, false); }
        if (name == "mono") { p = new(p.Channels, p.Preset, true, true); }
        if (name == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
        settings.SetPreferences(p);
        int bus = AudioServer.GetBusIndex("MissingEffects"); if (bus >= 0) { AudioServer.SetBusName(bus, "Effects"); }
        player.Stream = name == "missing-stream" ? null : GD.Load<AudioStreamWav>("res://assets/audio/hand-tool/A03-T01.wav");
        main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Close(false);
        main.GetNode<CanvasLayer>("DialogueOverlay").Visible = false; main.GetNode<CanvasLayer>("SettingsOverlay").Visible = false;
        main.GetNode<Seeker>("Seeker").GlobalPosition = name == "out-of-reach" ? new(1768, 900) : new(1732, 900);
        source._Process(0);
        if (initial is null)
        {
            main.GetNode<HandToolTapRuntimeSmokeCheck>("HandToolTapRuntimeSmokeCheck").ArrangeSave();
            initial = main.GetNode<SaveSnapshotCaptureController>("SaveSnapshotCaptureController").Capture()
                ?? throw new InvalidOperationException("Missing complete output fixture save.");
        }
        priorCommits = source.CommitCount; priorPresentations = audio.PresentationCount; admitted = false;
        if (name == "unsupported-segment") { main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = false; }
        if (name == "stale-or-replaced-site") { marker.Position += Vector2.One; }
        if (name == "hidden-moved-or-replaced-source") { marker.Visible = false; }
        if (name == "duplicate-request")
        { if (!source.TryTap()) { throw new InvalidOperationException("Duplicate setup contact failed."); } audio.Cancel(); }
        if (name == "overlay-rejection") { main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Open(); }
        if (name == "missing-effects-bus") { AudioServer.SetBusName(AudioServer.GetBusIndex("Effects"), "MissingEffects"); }
    }
    private void FinishCase()
    {
        string name = Cases[index];
        bool expected = name is not ("out-of-reach" or "unsupported-segment" or "stale-or-replaced-site" or "hidden-moved-or-replaced-source" or "duplicate-request" or "overlay-rejection" or "arrival-without-activation");
        if (admitted != expected) { throw new InvalidOperationException("Wrong action admission: " + name); }
        timeline.Add(new
        {
            name,
            start = started / 60.0,
            duration = 6,
            trigger = 0.5,
            admitted,
            support_condition = name == "unsupported-segment" ? "Current navigation disabled; interior segment support independently unit-tested" : "Retained ready navigation",
            commits = source.CommitCount - priorCommits,
            presentations = audio.PresentationCount - priorPresentations,
            caption_enabled = name != "captions-off",
            cancel_at = name is "focus-loss" or "restore-cancellation" ? 38 / 60.0 : (double?)null
        });
    }
    private void StopBackground()
    {
        var material = main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController"); material.StopPlayback(); material.SetProcess(false);
        var positional = main.GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController"); positional.StopPlayback(); positional.SetProcess(false);
        var feet = main.GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController"); feet.ResetPresentation(); feet.SetProcess(false);
        var music = main.GetNode<OpeningRegionMusicAudioController>("OpeningRegionMusicAudioController"); music.ResetPresentation(); music.GetNode<AudioStreamPlayer>("Music").Stop(); music.SetProcess(false);
    }
}
