using System.Text.Json;
using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped laboratory of the installed bed/contact players and final Master output.</summary>
public partial class InteriorAudioRuntimeProof : Node
{
    private static readonly JsonSerializerOptions EvidenceOptions = new() { WriteIndented = true };
    private static readonly string[] ContactParents = ["W01", "W04"];
    private sealed record Step(string Name, InteriorAudioRoom Room, double Blend, int Bed, string Mode, int Seconds = 4);
    private readonly List<Step> steps = [];
    private readonly List<object> timeline = [];
    private OpeningRegionInteriorAudioSource interior = null!;
    private OpeningRegionMaterialAudioController material = null!;
    private OpeningRegionPositionalAudioController positional = null!;
    private OpeningRegionFootstepAudioController footsteps = null!;
    private OpeningRegionMusicAudioController music = null!;
    private AudioSettingsController settings = null!;
    private bool automated;
    private int frame;
    private int index = -1;
    private int started;

    /// <inheritdoc />
    public override void _Ready()
    {
        interior = GetNode<OpeningRegionInteriorAudioSource>("OpeningRegionInteriorAudioSource");
        material = GetNode<OpeningRegionMaterialAudioController>("OpeningRegionMaterialAudioController");
        positional = GetNode<OpeningRegionPositionalAudioController>("OpeningRegionPositionalAudioController");
        footsteps = GetNode<OpeningRegionFootstepAudioController>("OpeningRegionFootstepAudioController");
        music = GetNode<OpeningRegionMusicAudioController>("OpeningRegionMusicAudioController");
        settings = GetNode<AudioSettingsController>("AudioSettingsController");
        GetWindow().Size = new(320, 180);
        Label label = new() { Text = "Interior audio proof · stopped", Position = new(12, 12) }; AddChild(label);
        Button play = new() { Text = "Sickhouse ambience", Position = new(12, 48) }; AddChild(play);
        play.Pressed += () => { Stop(); Target(InteriorAudioRoom.Sickhouse, 1, 0.25); StartBed(2); };
        Button stop = new() { Text = "Stop", Position = new(12, 90) }; AddChild(stop); stop.Pressed += Stop;
        foreach (InteriorAudioRoom room in new[] { InteriorAudioRoom.Sickhouse, InteriorAudioRoom.Laundry })
        {
            for (int bed = 0; bed < 4; bed++)
            {
                foreach (double blend in new[] { 0.0, 0.5, 1.0 })
                { steps.Add(new($"{room}-B0{bed + 1}-{blend:0.0}", room, blend, bed, "bed")); }
            }
        }
        foreach (InteriorAudioRoom room in new[] { InteriorAudioRoom.Sickhouse, InteriorAudioRoom.Laundry })
        { steps.Add(new(room + "-doorway", room, 0, 2, "doorway", 5)); }
        steps.Add(new("reset", InteriorAudioRoom.Sickhouse, 1, 2, "reset"));
        steps.Add(new("owner-loss", InteriorAudioRoom.Sickhouse, 1, 2, "owner-loss"));
        steps.Add(new("timber-contact", InteriorAudioRoom.Sickhouse, 1, -1, "contact", 2));
        steps.Add(new("earth-contact", InteriorAudioRoom.Laundry, 1, -1, "contact", 2));
        steps.Add(new("unsupported-wall", InteriorAudioRoom.None, 0, -1, "wall", 2));
        steps.Add(new("foreground-overlap", InteriorAudioRoom.Sickhouse, 1, 2, "mix", 28));
        foreach (string bus in new[] { "Ambience", "Effects", "Music", "Master" })
        { steps.Add(new("mute-" + bus, InteriorAudioRoom.Sickhouse, 1, 2, "mute-" + bus, 6)); }
        steps.Add(new("mono", InteriorAudioRoom.Laundry, 1, 2, "mono", 6));
        steps.Add(new("mix-default", InteriorAudioRoom.Sickhouse, 1, 2, "mix", 28));
        steps.Add(new("mix-reduced", InteriorAudioRoom.Laundry, 1, 2, "reduced", 28));
        automated = OS.GetEnvironment("ALLRPCS_INTERIOR_OUTPUT_CHECK") == "1";
        if (automated && !OS.HasFeature("movie")) { throw new InvalidOperationException("Actual Master output requires Movie Maker."); }
        string decode = OS.GetEnvironment("ALLRPCS_INTERIOR_DECODE_PATH");
        if (decode.Length > 0)
        {
            Directory.CreateDirectory(decode);
            foreach (string id in Enumerable.Range(1, 4).Select(i => $"B0{i}")
                .Concat(ContactParents.SelectMany(parent => Enumerable.Range(1, 6).Select(i => $"{parent}-{i:00}"))))
            {
                string folder = id.StartsWith('B') ? "material" : "footsteps";
                File.WriteAllBytes(Path.Combine(decode, id + ".pcm16"), GD.Load<AudioStreamWav>($"res://assets/audio/{folder}/{id}.wav").Data);
            }
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated) { return; }
        try { AdvanceProof(); }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private void AdvanceProof()
    {
        if (index < 0 || frame - started >= steps[index].Seconds * 60)
        {
            index++;
            if (index == steps.Count)
            {
                Stop();
                File.WriteAllText(OS.GetEnvironment("ALLRPCS_INTERIOR_TIMELINE"), JsonSerializer.Serialize(timeline, EvidenceOptions));
                GD.Print("PASS: 39 actual interior Master-output cases through installed players."); GetTree().Quit(); return;
            }
            started = frame;
            Configure(steps[index]);
            Step step = steps[index];
            timeline.Add(new
            {
                name = step.Name,
                room = step.Room.ToString(),
                blend = step.Blend,
                bed = step.Bed,
                mode = step.Mode,
                start = frame / 60.0,
                duration = step.Seconds
            });
        }
        Step current = steps[index]; int local = frame - started;
        if (current.Mode == "doorway")
        {
            double blend = local < 60 ? 0 : local < 150 ? 1 : local < 210 ? (local % 2 == 0 ? 0.49 : 0.51) : 0;
            Target(current.Room, blend, 1 / 60.0);
        }
        if (local == 120 && current.Mode == "reset") { interior.ResetPresentation(); }
        if (local == 120 && current.Mode == "owner-loss")
        { RemoveChild(interior); interior.ResetPresentation(); }
        if (local == 12 && (current.Mode == "contact" || current.Mode == "mute-Effects" || current.Mode == "mute-Master"))
        {
            FootstepMaterial? floor = OpeningRegionInteriorAudioProjection.MaterialAt(Layout(),
                current.Room == InteriorAudioRoom.Sickhouse ? new(2640, 1450) : new(1500, 220), true);
            if (floor is { } surface) { footsteps.PresentContact(new(surface, FootstepLeg.Left, 1, default)); }
        }
        if (local == 12 && current.Mode == "wall"
            && OpeningRegionInteriorAudioProjection.MaterialAt(Layout(), new(2480, 1400), true) is not null)
        { throw new InvalidOperationException("Wall acquired an interior contact."); }
        if (current.Mode is "mix" or "reduced" && local == 12)
        { Mixed(); }
        if (local == 120 && current.Mode.StartsWith("mute-", StringComparison.Ordinal))
        {
            if (current.Mode == "mute-Ambience") { footsteps.PresentContact(new(FootstepMaterial.Timber, FootstepLeg.Left, 1, default)); }
            else { StartBed(2); }
        }
        material._Process(0); positional._Process(0);
        if (current.Mode is "mix" or "reduced" or "mono") { music.DiagnosticAdvance(1 / 60.0); }
        frame++;
    }
    private void Configure(Step step)
    {
        if (!interior.IsInsideTree()) { AddChild(interior); }
        Stop(); settings.Ducking.Clear(); settings.RestoreDefaults();
        AudioPreferences p = settings.Preferences;
        if (step.Mode.StartsWith("mute-", StringComparison.Ordinal))
        { p = p.WithChannel(Enum.Parse<AudioCategory>(step.Mode[5..]), 1, true); }
        settings.SetPreferences(new(p.Channels, step.Mode == "reduced" ? AudioMixPreset.ReducedRange : AudioMixPreset.Default, step.Mode == "mono", true));
        Target(step.Room, step.Blend, 0.25); Target(step.Room, step.Blend, 0.25); Target(step.Room, step.Blend, 0.25);
        if (step.Bed >= 0 && step.Mode is not ("mute-Effects" or "mute-Music")) { StartBed(step.Bed); }
        if (step.Mode is "mute-Music" or "mute-Master")
        {
            AudioStreamPlayer score = music.GetNode<AudioStreamPlayer>("Music");
            score.Stream = GD.Load<AudioStreamWav>("res://assets/audio/music/M02.wav");
            score.VolumeLinear = 1; score.Play();
        }
        if (step.Mode == "mono")
        {
            music.DiagnosticTreatment();
            for (int i = 0; i < 80; i++) { music.DiagnosticAdvance(0.25); }
            footsteps.PresentContact(new(FootstepMaterial.Timber, FootstepLeg.Left, 1, default));
        }
    }
    private void Mixed()
    {
        material.PlayCue("A05"); positional.DiagnosticReply(new(1800, 500), new(1800, 500), 192);
        footsteps.PresentContact(new(FootstepMaterial.Timber, FootstepLeg.Left, 1, default));
        music.DiagnosticTreatment();
        for (int i = 0; i < 80; i++) { music.DiagnosticAdvance(0.25); }
    }
    private void StartBed(int bed)
    {
        material.Present(new(bed, $"B0{bed + 1}", "Controlled bed", 1));
        material._Process(0.25); material._Process(0.25); material._Process(0.25);
        if (bed == 1) { positional.DiagnosticWater(new(1800, 500), new(1800, 500), 1); }
    }
    private void Target(InteriorAudioRoom room, double blend, double delta) =>
        interior.DiagnosticPresent(new(room, blend, room == InteriorAudioRoom.Sickhouse ? FootstepMaterial.Timber : FootstepMaterial.Earth), delta);
    private void Stop()
    {
        material.StopPlayback(); positional.StopPlayback(); footsteps.ResetPresentation(); music.ResetPresentation();
        for (int i = 0; i < 720; i++) { music.DiagnosticAdvance(0.25); }
        music.GetNode<AudioStreamPlayer>("Music").Stop(); interior.ResetPresentation();
    }
    private static InteriorAudioLayout Layout() => new(new(2528, 1310), 64, new(1420, 240.8f), new(1580, 294.66666f), 96);
}
