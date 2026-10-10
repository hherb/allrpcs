using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped by default; controlled Master capture uses explicit actions in an ordinary Main instance.</summary>
public partial class CaskCoverHandlingAudioOutputCheck : Node2D
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly string[] Cases = ["default",
        "quiet",
        "master-muted",
        "effects-muted",
        "missing-stream",
        "master-half",
        "effects-half",
        "mono",
        "captions-off",
        "missing-effects-bus",
        "out-of-reach",
        "unsupported-segment",
        "duplicate-request",
        "stale-target",
        "overlay-rejection",
        "focus-loss",
        "restore-cancellation",
        "target-moved-cancellation",
        "concealed-or-substituted-target",
        "a04-a05-water-caption-coexistence"];
    private readonly List<object> timeline = [];
    private Node main = null!;
    private OpeningRegionCaskCoverHandlingCoordinator source = null!;
    private OpeningRegionCaskCoverAudioController audio = null!;
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
        source = main.GetNode<OpeningRegionCaskCoverHandlingCoordinator>("OpeningRegionCaskCoverHandlingCoordinator");
        audio = main.GetNode<OpeningRegionCaskCoverAudioController>("OpeningRegionCaskCoverAudioController");
        settings = main.GetNode<AudioSettingsController>("AudioSettingsController"); player = audio.GetNode<AudioStreamPlayer>("CaskCover");
        automated = OS.GetEnvironment("ALLRPCS_CASK_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Master evidence requires Movie Maker."); }
        source.Automatic = automated; audio.Automatic = automated;
        StopBackground();
        GetWindow().Size = new(320, 180);
        if (!automated)
        {
            Label label = new() { Text = "Cask cover output proof · stopped", Position = new(12, 12) }; AddChild(label);
            Button play = new() { Text = "Play diagnostic cover passage", Position = new(12, 48) }; AddChild(play); play.Pressed += audio.DiagnosticPlay;
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
                GD.Print("PASS: cask output standalone starts stopped; zero commits/presentations/playback.");
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
                    File.WriteAllText(OS.GetEnvironment("ALLRPCS_CASK_TIMELINE"), JsonSerializer.Serialize(timeline, Options));
                    string decode = OS.GetEnvironment("ALLRPCS_CASK_DECODE_PATH");
                    if (decode.Length > 0) { File.WriteAllBytes(decode, GD.Load<AudioStreamWav>("res://assets/audio/cask-cover/A02-C01.wav").Data); }
                    GD.Print("PASS: 20 actual cask-cover Master-output cases through live commit owner."); GetTree().Quit(); return;
                }
                started = frame; Configure();
            }
            int local = frame - started; string name = Cases[index];
            if (local == 30)
            {
                admitted = source.TryHandle();
                if (name == "a04-a05-water-caption-coexistence")
                { var material = main.GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController"); material.PlayCue("A04"); material.PlayCue("A05"); main.GetNode<MaterialSoundCaptionPanel>("MaterialSoundCaptionPanel").SetWaterContact("Water contact"); }
            }
            if (local == 50 && name == "target-moved-cancellation")
            {
                if (!main.GetNode<TemporaryCharacterObjectSabotageMovePresentationFlowController>("TemporaryCharacterObjectSabotageMovePresentationFlowController").TryApply())
                { throw new InvalidOperationException("Output move fixture failed."); }
            }
            if (local == 50 && name == "focus-loss") { source._Notification((int)NotificationApplicationFocusOut); }
            if (local == 50 && name == "restore-cancellation")
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
        var moveState = main.GetNode<CharacterObjectSabotageMoveController>("CharacterObjectSabotageMoveController");
        var moveSource = main.GetNode<TemporaryCharacterObjectSabotageMoveSource>("TemporaryCharacterObjectSabotageMoveSource");
        if (moveState.Placements!.Entries.Single().Location.Identity.Id.Value == "abandoned-wagon")
        {
            var perception = new Allrpcs.Domain.Perception.CharacterPerceptionSnapshot(
                main.GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource").Character.Identity.Id,
                [new(moveSource.Candidate.Destination.Occurrence.Target.Identity.Id, Allrpcs.Domain.Perception.VisibilityState.Identified)]);
            if (!main.GetNode<TemporaryCharacterObjectSabotageMovePresentationFlowController>("TemporaryCharacterObjectSabotageMovePresentationFlowController").TryRecover(perception))
            { throw new InvalidOperationException("Output fixture move recovery failed."); }
        }
        main.GetNode<GreyboxInteractionTarget>("InteractionCandidates/WaterCask").Rotation = 0;
        main.GetNode<Polygon2D>("InteractionCandidates/WaterCask/SubstituteBody").Visible = false;
        main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = true;
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
        player.Stream = name == "missing-stream" ? null : GD.Load<AudioStreamWav>("res://assets/audio/cask-cover/A02-C01.wav");
        main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Close(false);
        main.GetNode<CanvasLayer>("DialogueOverlay").Visible = false; main.GetNode<CanvasLayer>("SettingsOverlay").Visible = false;
        main.GetNode<Seeker>("Seeker").GlobalPosition = name == "out-of-reach" ? new(2132, 900) : new(2168, 900);
        source._Process(0);
        if (initial is null)
        {
            main.GetNode<CaskCoverHandlingRuntimeSmokeCheck>("CaskCoverHandlingRuntimeSmokeCheck").ArrangeSave();
            initial = main.GetNode<SaveSnapshotCaptureController>("SaveSnapshotCaptureController").Capture()
                ?? throw new InvalidOperationException("Missing complete output fixture save.");
        }
        priorCommits = source.CommitCount; priorPresentations = audio.PresentationCount; admitted = false;
        if (name == "unsupported-segment") { main.GetNode<GreyboxNavigation>("GreyboxNavigation").Enabled = false; }
        if (name == "stale-target") { main.GetNode<GreyboxInteractionTarget>("InteractionCandidates/WaterCask").Rotation = 0.1f; }
        if (name == "concealed-or-substituted-target") { main.GetNode<Polygon2D>("InteractionCandidates/WaterCask/SubstituteBody").Visible = true; }
        if (name == "duplicate-request")
        { if (!source.TryHandle()) { throw new InvalidOperationException("Duplicate setup contact failed."); } audio.Cancel(); }
        if (name == "overlay-rejection") { main.GetNode<AudioSettingsPanel>("AudioSettingsPanel").Open(); }
        if (name == "missing-effects-bus") { AudioServer.SetBusName(AudioServer.GetBusIndex("Effects"), "MissingEffects"); }
    }
    private void FinishCase()
    {
        string name = Cases[index];
        bool expected = name is not ("out-of-reach" or "unsupported-segment" or "stale-target" or "concealed-or-substituted-target" or "duplicate-request" or "overlay-rejection");
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
            cancel_at = name is "focus-loss" or "restore-cancellation" or "target-moved-cancellation" ? 50 / 60.0 : (double?)null
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
