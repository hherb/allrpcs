using System.Text.Json;
using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped laboratory that renders the installed contact players through the existing Master mixer.</summary>
public partial class FootstepAudioRuntimeProof : Node
{
    private static readonly JsonSerializerOptions TimelineOptions = new() { WriteIndented = true };
    private sealed record Step(string Name, FootstepMaterial Material, int Variant, string Mode, double Duration);
    private readonly List<Step> steps = [];
    private readonly List<object> timeline = [];
    private OpeningRegionFootstepAudioController footsteps = null!;
    private OpeningRegionMaterialAudioController material = null!;
    private AudioSettingsController settings = null!;
    private Label status = null!;
    private bool automated;
    private int frame;
    private int index = -1;
    private int started;

    /// <inheritdoc />
    public override void _Ready()
    {
        footsteps = GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController");
        material = GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController");
        settings = GetNode<AudioSettingsController>("AudioSettingsController");
        VBoxContainer controls = new(); AddChild(controls); controls.Position = new(12, 12);
        status = new() { Text = "Footstep proof · stopped" }; controls.AddChild(status);
        foreach (FootstepMaterial surface in Enum.GetValues<FootstepMaterial>())
        {
            Button button = new() { Text = surface.ToString() }; controls.AddChild(button);
            button.Pressed += () => footsteps.PresentContact(new(surface, FootstepLeg.Left, 1, default));
            for (int variant = 1; variant <= 6; variant++) { steps.Add(new($"{surface}-{variant:00}", surface, variant, "sample", 1.5)); }
            foreach (string mode in new[] { "normal", "half", "zero", "mute", "master-zero", "master-mute", "mono", "reduced", "ambience-zero" })
            {
                steps.Add(new($"{surface}-{mode}", surface, 1, mode, 2));
            }
        }
        foreach (string mode in new[] { "overlap", "rate", "material-change", "stop", "mute-unmute", "mix", "missing" })
        {
            steps.Add(new(mode, FootstepMaterial.Earth, 1, mode, 4));
        }
        Button stop = new() { Text = "Stop" }; controls.AddChild(stop); stop.Pressed += Stop;
        automated = OS.GetEnvironment("ALLRPCS_FOOTSTEP_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Contact output proof requires Movie Maker."); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated) { return; }
        try { AdvanceProof(); }
        catch (Exception error) { GD.PrintErr(error); GetTree().Quit(1); }
    }

    private void AdvanceProof()
    {
        if (index < 0 || frame - started >= (int)(steps[index].Duration * 60))
        {
            index++;
            if (index == steps.Count)
            {
                Stop();
                File.WriteAllText(OS.GetEnvironment("ALLRPCS_FOOTSTEP_TIMELINE"), JsonSerializer.Serialize(timeline, TimelineOptions));
                GD.Print($"Footstep output proof completed {steps.Count} cases through installed players and Master.");
                GetTree().Quit(); return;
            }
            started = frame;
            Stop(); settings.Ducking.Clear(); settings.RestoreDefaults();
            Step step = steps[index];
            AudioPreferences p = settings.Preferences;
            if (step.Mode == "half") { p = p.WithChannel(AudioCategory.Effects, 0.5, false); }
            if (step.Mode == "zero") { p = p.WithChannel(AudioCategory.Effects, 0, false); }
            if (step.Mode == "mute") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
            if (step.Mode == "master-zero") { p = p.WithChannel(AudioCategory.Master, 0, false); }
            if (step.Mode == "master-mute") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (step.Mode == "ambience-zero") { p = p.WithChannel(AudioCategory.Ambience, 0, false); }
            settings.SetPreferences(new(p.Channels, step.Mode == "reduced" ? AudioMixPreset.ReducedRange : AudioMixPreset.Default,
                step.Mode == "mono", true));
            status.Text = step.Name;
            timeline.Add(new
            {
                name = step.Name,
                material = step.Material.ToString(),
                variant = step.Variant,
                mode = step.Mode,
                start = frame / 60.0,
                duration = step.Duration
            });
        }
        Step current = steps[index];
        int local = frame - started;
        if (local == 12 && current.Mode != "missing") { Contact(current.Material, current.Variant); }
        if (current.Mode == "missing" && local == 12)
        {
            footsteps.RemoveDiagnosticResources(); Contact(current.Material, current.Variant);
        }
        if (current.Mode is "overlap" or "material-change" && local == 34)
        {
            Contact(current.Mode == "material-change" ? FootstepMaterial.Timber : current.Material, 2);
        }
        if (current.Mode == "rate" && local > 12 && local <= 144 && (local - 12) % 11 == 0)
        {
            Contact((FootstepMaterial)(((local - 12) / 11) % 4), 1 + (((local - 12) / 11) % 6));
        }
        if (current.Mode == "stop" && local == 25) { footsteps.ResetPresentation(); }
        if (current.Mode == "mute-unmute" && local == 25) { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Effects, 1, true)); }
        if (current.Mode == "mute-unmute" && local == 36) { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Effects, 1, false)); }
        if (current.Mode == "mix" && local == 12)
        {
            material.Present(new(2, "B03", "Rain in leaves", 1)); material.PlayCue("A04"); material.PlayCue("A05");
        }
        if (current.Mode != "mix" && settings.Ducking.Gain(AudioCategory.Ambience) != 1)
        {
            throw new InvalidOperationException("Footstep acquired a foreground lease.");
        }
        if (footsteps.GetChildren().Cast<AudioStreamPlayer>().Count(p => p.Playing) > 2)
        {
            throw new InvalidOperationException("Unbounded contact voice count.");
        }
        frame++;
    }

    private void Contact(FootstepMaterial surface, int variant) => footsteps.PresentContact(new(surface, FootstepLeg.Left, variant, default));
    private void Stop() { footsteps.ResetPresentation(); material.StopPlayback(); }
}
