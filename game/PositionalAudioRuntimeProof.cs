using System.Text.Json;
using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped-by-default actual Master-output proof with explicit listener/source positions.</summary>
public partial class PositionalAudioRuntimeProof : Node2D
{
    private sealed record Step(string Name, string Kind, Vector2 Offset, string Variant = "normal", double Duration = 3);
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, IncludeFields = true };
    private readonly List<Step> steps = [];
    private readonly List<object> timeline = [];
    private OpeningRegionPositionalAudioController positional = null!;
    private OpeningRegionMaterialAudioController material = null!;
    private OpeningRegionFootstepAudioController footsteps = null!;
    private AudioSettingsController settings = null!;
    private bool automated;
    private int index = -1;
    private int frame;
    private int start;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        positional = GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController");
        material = GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController");
        footsteps = GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController");
        settings = GetNode<AudioSettingsController>("AudioSettingsController");
        status = new() { Position = new(24, 24), Text = "Positional output proof · stopped" }; AddChild(status);
        Button water = new() { Position = new(24, 65), Text = "Waterway" }; AddChild(water);
        water.Pressed += () => positional.DiagnosticWater(new(500, 360), new(700, 360), 1);
        Button reply = new() { Position = new(24, 105), Text = "Reply indication" }; AddChild(reply);
        reply.Pressed += () => positional.DiagnosticReply(new(500, 360), new(580, 360), 192);
        Button stop = new() { Position = new(24, 145), Text = "Stop all" }; AddChild(stop);
        stop.Pressed += Stop;
        foreach (string kind in new[] { "water", "reply" })
        {
            foreach (string variant in new[] { "normal", "half", "zero", "mute", "master-zero", "master-mute", "mono", "reduced", "missing", "other-bus-zero", "cancel", "mute-unmute" })
            { steps.Add(new(kind + "-" + variant, kind, Vector2.Zero, variant)); }
            float range = kind == "water" ? 768 : 192;
            steps.Add(new(kind + "-half-distance", kind, new(0, range / 2)));
            steps.Add(new(kind + "-boundary", kind, new(0, range)));
            steps.Add(new(kind + "-outside", kind, new(0, range + 1)));
            steps.Add(new(kind + "-left", kind, new(-range / 2, 0)));
            steps.Add(new(kind + "-right", kind, new(range / 2, 0)));
            steps.Add(new(kind + "-above", kind, new(0, -range / 2)));
            steps.Add(new(kind + "-zoom2", kind, new(0, range / 2), "zoom2"));
        }
        steps.Add(new("whisper-boundary", "reply", new(48, 0), "whisper"));
        steps.Add(new("whisper-half", "reply", new(24, 0), "whisper"));
        steps.Add(new("water-loop", "water", Vector2.Zero, "normal", 34));
        steps.Add(new("mixed-material", "water", Vector2.Zero, "mix", 10));
        steps.Add(new("water-crossfade", "water", Vector2.Zero, "crossfade", 5));
        steps.Add(new("water-retarget", "water", Vector2.Zero, "retarget", 5));
        automated = OS.GetEnvironment("ALLRPCS_POSITIONAL_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Actual output proof needs Movie Maker."); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated) { return; }
        if (index < 0 || frame - start >= steps[index].Duration * 60)
        {
            index++;
            if (index == steps.Count)
            {
                Stop(); File.WriteAllText(OS.GetEnvironment("ALLRPCS_POSITIONAL_TIMELINE"), JsonSerializer.Serialize(timeline, Options));
                GD.Print($"PASS: {steps.Count} actual positional Master-output cases rendered."); GetTree().Quit(); return;
            }
            start = frame; Configure(steps[index]);
            timeline.Add(new
            {
                name = steps[index].Name,
                kind = steps[index].Kind,
                offset = steps[index].Offset,
                variant = steps[index].Variant,
                start = frame / 60.0,
                duration = steps[index].Duration
            });
        }
        Step step = steps[index]; int local = frame - start;
        if (local == 30)
        {
            Vector2 origin = new(640, 360);
            AudioStreamPlayer2D player = positional.GetNode<AudioStreamPlayer2D>(step.Kind == "water" ? "B02" : "RecordedReply");
            AudioStream? stream = player.Stream;
            if (step.Variant == "missing") { player.Stream = null; }
            if (step.Kind == "water") { positional.DiagnosticWater(origin, origin + step.Offset, 1); }
            else { positional.DiagnosticReply(origin, origin + step.Offset, step.Variant == "whisper" ? 48 : 192); }
            if (step.Variant == "missing") { player.Stream = stream; positional.StopPlayback(); }
            if (step.Variant == "mix")
            {
                material.PlayCue("A04"); material.PlayCue("A05");
                footsteps.PresentContact(new(FootstepMaterial.Earth, FootstepLeg.Left, 1, default));
                positional.DiagnosticReply(origin, origin + new Vector2(80, 0), 192);
            }
        }
        if (local == 31 && step.Variant == "cancel") { positional.StopPlayback(); }
        if (local == 60 && step.Variant == "mute-unmute")
        {
            settings.SetPreferences(settings.Preferences.WithChannel(step.Kind == "water" ? AudioCategory.Ambience : AudioCategory.Dialogue, 1, false));
        }
        if (local == 90 && step.Variant is "crossfade" or "retarget")
        { material.Present(new(0, "B01", "Sheltered rain", 1)); }
        if (local == 110 && step.Variant == "retarget")
        { material.Present(new(3, "B04", "Wind, rain and timber creak", 1)); }
        frame++;
    }

    private void Configure(Step step)
    {
        Stop(); settings.Ducking.Clear(); settings.RestoreDefaults(); status.Text = step.Name;
        material.SetProcess(step.Variant is "crossfade" or "retarget" or "mix");
        AudioCategory category = step.Kind == "water" ? AudioCategory.Ambience : AudioCategory.Dialogue;
        AudioPreferences prefs = settings.Preferences;
        if (step.Variant == "half") { prefs = prefs.WithChannel(category, 0.5, false); }
        if (step.Variant == "zero") { prefs = prefs.WithChannel(category, 0, false); }
        if (step.Variant is "mute" or "mute-unmute") { prefs = prefs.WithChannel(category, 1, true); }
        if (step.Variant == "master-zero") { prefs = prefs.WithChannel(AudioCategory.Master, 0, false); }
        if (step.Variant == "master-mute") { prefs = prefs.WithChannel(AudioCategory.Master, 1, true); }
        if (step.Variant == "other-bus-zero") { prefs = prefs.WithChannel(step.Kind == "water" ? AudioCategory.Effects : AudioCategory.Ambience, 0, true); }
        settings.SetPreferences(new(prefs.Channels, step.Variant == "reduced" ? AudioMixPreset.ReducedRange : AudioMixPreset.Default, step.Variant == "mono", true));
        GetViewport().CanvasTransform = new Transform2D(0, new Vector2(step.Variant == "zoom2" ? 2 : 1, step.Variant == "zoom2" ? 2 : 1), 0, Vector2.Zero);
        if (step.Variant is "crossfade" or "retarget" or "mix") { material.Present(new(1, "B02", "Running water and rain", 1)); }
    }
    private void Stop() { material.StopPlayback(); positional.StopPlayback(); footsteps.ResetPresentation(); }
}
