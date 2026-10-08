using System.Text.Json;
using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped-by-default proof exercising installed material players and the existing mixer.</summary>
public partial class MaterialAudioRuntimeProof : Node
{
    private static readonly JsonSerializerOptions TimelineOptions = new() { WriteIndented = true };
    private sealed record Step(string Name, string Clip, double Duration, string Variant = "normal");
    private readonly List<Step> steps = [];
    private readonly List<object> timeline = [];
    private OpeningRegionMaterialAudioController material = null!;
    private AudioSettingsController settings = null!;
    private Label status = null!;
    private bool automated;
    private int index = -1;
    private int frame;
    private int stepStart;
    private long extraLease;

    /// <inheritdoc />
    public override void _Ready()
    {
        material = GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController");
        settings = GetNode<AudioSettingsController>("AudioSettingsController");
        VBoxContainer controls = new();
        AddChild(controls);
        controls.Position = new Vector2(20, 20);
        status = new() { Text = "Material proof · stopped" };
        controls.AddChild(status);
        foreach (string id in new[] { "B01", "B02", "B03", "B04", "A04", "A05" })
        {
            Button button = new() { Text = id };
            controls.AddChild(button);
            button.Pressed += () => Start(new Step(id, id, 34));
        }
        Button stop = new() { Text = "Stop" };
        controls.AddChild(stop);
        stop.Pressed += material.StopPlayback;
        foreach (string id in new[] { "B01", "B02", "B03", "B04" }) { steps.Add(new(id + "-loop", id, 34)); }
        foreach (string id in new[] { "B01", "A04", "A05" })
        {
            foreach (string variant in new[] { "normal", "half", "zero", "mute", "master-zero", "master-mute", "mono", "reduced" })
            {
                steps.Add(new(id + "-" + variant, id, 10, variant));
            }
        }
        steps.Add(new("revealed", "B03", 10, "revealed"));
        steps.Add(new("crossfade", "B01", 10, "crossfade"));
        steps.Add(new("retarget", "B01", 10, "retarget"));
        steps.Add(new("treatment-release", "A05", 12, "release"));
        steps.Add(new("treatment-reset", "A05", 10, "reset"));
        steps.Add(new("treatment-muted-release", "A05", 10, "mute-release"));
        steps.Add(new("treatment-missing", "A05", 10, "missing"));
        steps.Add(new("bed-muted-phase", "B01", 10, "phase"));
        automated = OS.GetEnvironment("ALLRPCS_MATERIAL_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Output proof requires Movie Maker."); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated) { return; }
        if (index < 0 || frame - stepStart >= (int)(steps[index].Duration * 60))
        {
            index++;
            if (index == steps.Count)
            {
                material.StopPlayback();
                string output = OS.GetEnvironment("ALLRPCS_MATERIAL_TIMELINE");
                File.WriteAllText(output, JsonSerializer.Serialize(timeline, TimelineOptions));
                GD.Print($"Material output proof completed {steps.Count} installed-player cases; analyze final WAV.");
                GetTree().Quit();
                return;
            }
            stepStart = frame;
            Start(steps[index]);
            timeline.Add(new
            {
                name = steps[index].Name,
                clip = steps[index].Clip,
                start = frame / 60.0,
                duration = steps[index].Duration,
                variant = steps[index].Variant
            });
        }
        Step step = steps[index];
        int local = frame - stepStart;
        if (local == 180 && step.Variant is "crossfade" or "retarget")
        {
            material.Present(new(1, "B02", "Running water and rain", 1));
        }
        if (local == 200 && step.Variant == "retarget") { material.Present(new(3, "B04", "Wind, rain and timber creak", 1)); }
        if (local == 180 && step.Variant == "reset") { material.ResetPresentation(); }
        if (local == 180 && step.Variant is "mute-release" or "phase")
        {
            settings.SetPreferences(settings.Preferences.WithChannel(
                step.Variant == "phase" ? AudioCategory.Ambience : AudioCategory.Effects, 1, true));
        }
        if (local == 240 && step.Variant is "mute-release" or "phase")
        {
            settings.SetPreferences(settings.Preferences.WithChannel(
                step.Variant == "phase" ? AudioCategory.Ambience : AudioCategory.Effects, 1, false));
        }
        if (local == 540 && step.Variant == "release" && settings.Ducking.Gain(AudioCategory.Ambience) < 0.999999)
        {
            throw new InvalidOperationException("Finished treatment retained foreground attenuation.");
        }
        if (local == 290 && step.Variant is "reset" or "mute-release"
            && settings.Ducking.Gain(AudioCategory.Ambience) < 0.999999)
        {
            throw new InvalidOperationException("Cancelled treatment retained foreground attenuation.");
        }
        frame++;
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (settings is not null && extraLease != 0) { settings.Ducking.End(extraLease); }
    }

    private void Start(Step step)
    {
        material.StopPlayback();
        settings.Ducking.Clear();
        if (extraLease != 0) { settings.Ducking.End(extraLease); extraLease = 0; }
        settings.RestoreDefaults();
        AudioCategory category = step.Clip.StartsWith('B') ? AudioCategory.Ambience : AudioCategory.Effects;
        AudioPreferences preferences = settings.Preferences;
        if (step.Variant == "half") { preferences = preferences.WithChannel(category, 0.5, false); }
        if (step.Variant == "zero") { preferences = preferences.WithChannel(category, 0, false); }
        if (step.Variant == "mute") { preferences = preferences.WithChannel(category, 1, true); }
        if (step.Variant == "master-zero") { preferences = preferences.WithChannel(AudioCategory.Master, 0, false); }
        if (step.Variant == "master-mute") { preferences = preferences.WithChannel(AudioCategory.Master, 1, true); }
        settings.SetPreferences(new(preferences.Channels, step.Variant == "reduced" ? AudioMixPreset.ReducedRange : AudioMixPreset.Default,
            step.Variant == "mono", true));
        status.Text = step.Name;
        if (step.Clip.StartsWith('B'))
        {
            int bed = int.Parse(step.Clip.AsSpan(1), System.Globalization.CultureInfo.InvariantCulture) - 1;
            material.Present(new(bed, step.Clip, step.Name, step.Variant == "revealed"
                ? OpeningRegionMaterialAudioProjection.QuieterWoodlandGain : 1));
        }
        else
        {
            AudioStreamPlayer player = material.GetNode<AudioStreamPlayer>(step.Clip);
            AudioStream? stream = player.Stream;
            if (step.Variant == "missing") { player.Stream = null; }
            material.PlayCue(step.Clip);
            if (step.Variant == "missing")
            {
                if (player.Playing || settings.Ducking.Gain(AudioCategory.Ambience) != 1)
                {
                    throw new InvalidOperationException("Missing cue started playback or acquired foreground.");
                }
                player.Stream = stream;
            }
        }
    }
}
