using System.Text.Json;
using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped-by-default proof of actual Music/Master output, gaps and interruption.</summary>
public partial class MusicAudioRuntimeProof : Node2D
{
    private sealed record Step(string Name, string Variant, bool Treatment = false, double Duration = 27);
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly List<Step> steps = [];
    private readonly List<object> timeline = [];
    private OpeningRegionMusicAudioController music = null!;
    private AudioSettingsController settings = null!;
    private OpeningRegionMaterialAudioController material = null!;
    private OpeningRegionPositionalAudioController positional = null!;
    private OpeningRegionFootstepAudioController footsteps = null!;
    private bool automated;
    private int index = -1;
    private int frame;
    private int start;
    private long motionTick;
    private static readonly string[] ClipIds = ["M01", "M02"];

    /// <inheritdoc />
    public override void _Ready()
    {
        music = GetNode<OpeningRegionMusicAudioController>("OpeningRegionMusicAudioController");
        settings = GetNode<AudioSettingsController>("AudioSettingsController");
        material = GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController");
        positional = GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController");
        footsteps = GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController");
        AddChild(new Label { Position = new(24, 24), Text = "Optional music proof · stopped" });
        Button play = new() { Position = new(24, 70), Text = "Quiet exploration passage" }; AddChild(play);
        play.Pressed += () => { Prepare(); Offer(false); Prime(12); };
        Button stop = new() { Position = new(24, 110), Text = "Stop" }; AddChild(stop); stop.Pressed += music.ResetPresentation;
        foreach (string variant in new[] { "normal", "half", "master-half", "zero", "mute", "master-zero", "master-mute", "mono", "reduced", "missing", "missing-bus", "other-bus-zero", "interrupt", "cancel", "mute-unmute", "focus", "long-frame" })
        { steps.Add(new("M01-" + variant, variant)); }
        steps.Add(new("M02-normal", "normal", true));
        steps.Add(new("M02-interrupt", "interrupt", true));
        steps.Add(new("mixed-five-minutes", "mix", false, 300));
        automated = OS.GetEnvironment("ALLRPCS_MUSIC_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Music output requires actual Movie Maker."); }
        byte[][] decoded = ClipIds.Select(id => GD.Load<AudioStreamWav>($"res://assets/audio/music/{id}.wav").Data).ToArray();
        string decodePath = OS.GetEnvironment("ALLRPCS_MUSIC_DECODE_PATH");
        if (decodePath.Length > 0)
        {
            Directory.CreateDirectory(decodePath);
            for (int i = 0; i < 2; i++) { File.WriteAllBytes(Path.Combine(decodePath, $"M0{i + 1}.pcm16"), decoded[i]); }
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated) { return; }
        if (index < 0 || frame - start >= steps[index].Duration * 60)
        {
            RestoreBus(); index++;
            if (index == steps.Count)
            {
                music.ResetPresentation(); material.StopPlayback(); footsteps.ResetPresentation();
                File.WriteAllText(OS.GetEnvironment("ALLRPCS_MUSIC_TIMELINE"), JsonSerializer.Serialize(timeline, Options));
                GD.Print($"PASS: {steps.Count} actual music Master-output cases, including five-minute gaps/mix."); GetTree().Quit(); return;
            }
            start = frame; Configure(steps[index]);
            timeline.Add(new
            {
                name = steps[index].Name,
                variant = steps[index].Variant,
                treatment = steps[index].Treatment,
                start = frame / 60.0,
                duration = steps[index].Duration
            });
        }
        Step step = steps[index]; int local = frame - start;
        if (step.Variant == "mix") { Mixed(local); }
        else
        {
            if (local == 30)
            {
                Offer(step.Treatment); Prime(step.Treatment ? 20 : 12);
            }
            if (local == 600 && step.Variant == "interrupt") { music.DiagnosticForeground(3); }
            if (local == 600 && step.Variant == "cancel") { music.ResetPresentation(); }
            if (local == 600 && step.Variant == "mute-unmute") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Music, 1, true)); }
            if (local == 720 && step.Variant == "mute-unmute") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Music, 1, false)); }
            if (local == 600 && step.Variant == "focus") { music.DiagnosticAdvance(0, focused: false); }
            if (local == 600 && step.Variant == "long-frame") { music.DiagnosticAdvance(0.251); }
            music.DiagnosticAdvance(1 / 60.0);
        }
        frame++;
    }

    private void Prepare()
    {
        music.ResetPresentation(); music.DiagnosticRestoreResources(); material.StopPlayback(); footsteps.ResetPresentation(); settings.RestoreDefaults(); Prime(180);
    }
    private void Configure(Step step)
    {
        Prepare();
        AudioPreferences preferences = settings.Preferences;
        if (step.Variant is "half" or "zero" or "mute") { preferences = preferences.WithChannel(AudioCategory.Music, step.Variant == "half" ? 0.5 : step.Variant == "zero" ? 0 : 1, step.Variant == "mute"); }
        if (step.Variant is "master-half" or "master-zero" or "master-mute") { preferences = preferences.WithChannel(AudioCategory.Master, step.Variant == "master-half" ? 0.5 : step.Variant == "master-zero" ? 0 : 1, step.Variant == "master-mute"); }
        if (step.Variant == "other-bus-zero") { preferences = preferences.WithChannel(AudioCategory.Effects, 0, true); }
        if (step.Variant is "mono" or "reduced") { preferences = new(preferences.Channels, step.Variant == "reduced" ? AudioMixPreset.ReducedRange : AudioMixPreset.Default, step.Variant == "mono", true); }
        settings.SetPreferences(preferences);
        if (step.Variant == "missing") { music.DiagnosticMissing(); }
        if (step.Variant == "missing-bus") { AudioServer.SetBusName(AudioServer.GetBusIndex("Music"), "MissingMusic"); }
        music.DiagnosticRegion("B01");
    }
    private void Offer(bool treatment)
    {
        if (treatment) { music.DiagnosticTreatment(); }
        else
        {
            music.DiagnosticRegion("B03");
            music.DiagnosticMotion(new(++motionTick, default, new(24, 0), 0.1, 240));
        }
    }
    private void Prime(double seconds) { for (int i = 0; i < (int)Math.Round(seconds * 4); i++) { music.DiagnosticAdvance(0.25); } }
    private static void RestoreBus()
    {
        int bus = AudioServer.GetBusIndex("MissingMusic"); if (bus >= 0) { AudioServer.SetBusName(bus, "Music"); }
    }
    private void Mixed(int local)
    {
        if (local == 0) { material.Present(new(0, "B01", "Sheltered rain", 1)); }
        if (local == 30) { Offer(false); }
        if (local == 60 * 60) { material.PlayCue("A04"); music.DiagnosticForeground(8); }
        if (local == 90 * 60) { material.PlayCue("A05"); music.DiagnosticForeground(8); }
        if (local == 120 * 60) { positional.DiagnosticReply(new(640, 360), new(720, 360), 192); music.DiagnosticForeground(3); }
        if (local % 60 == 0 && local < 10 * 60) { footsteps.PresentContact(new(FootstepMaterial.Earth, FootstepLeg.Left, 1, default)); }
        if (local == 225 * 60) { music.DiagnosticTreatment(); }
        music.DiagnosticAdvance(1 / 60.0);
    }
}
