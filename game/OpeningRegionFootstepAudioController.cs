using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns Seeker-only footstep tails and captions from post-collision displacement.</summary>
public partial class OpeningRegionFootstepAudioController : Node
{
    private readonly FootstepContactCadence cadence = new();
    private readonly Dictionary<string, AudioStreamWav> streams = [];
    private readonly HashSet<string> reported = [];
    private readonly List<AudioStreamPlayer> players = [];
    private readonly long[] age = new long[2];
    private AudioSettingsController settings = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private OpeningRegionFootstepMaterialSource? materials;
    private Seeker? seeker;
    private SaveSnapshotRestorationController? restoration;
    private bool inactive;
    private long sequence;

    /// <summary>Ordinary Main observes Seeker; standalone proof starts without motion subscriptions.</summary>
    [Export] public bool Automatic { get; set; } = true;
    /// <summary>Number of eligible contacts observed, independent of output settings.</summary>
    internal long ContactCount { get; private set; }
    /// <summary>Last modeled contact for diagnostics, without persisted world state.</summary>
    internal FootstepContact? LastContact { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        for (int index = 0; index < 2; index++)
        {
            AudioStreamPlayer player = new() { Name = $"Contact{index}", Bus = "Effects" };
            AddChild(player); players.Add(player);
        }
        foreach (FootstepMaterial material in Enum.GetValues<FootstepMaterial>())
        {
            for (int variant = 1; variant <= 6; variant++)
            {
                string id = $"{OpeningRegionFootstepMaterialProjection.ClipId(material)}-{variant:00}";
                string path = $"res://assets/audio/footsteps/{id}.wav";
                if (ResourceLoader.Exists(path) && GD.Load<AudioStreamWav>(path) is { } stream
                    && stream.MixRate == 48000 && stream.Stereo && Math.Abs(stream.GetLength() - 0.5) < 0.001
                    && stream.LoopMode == AudioStreamWav.LoopModeEnum.Disabled) { streams.Add(id, stream); }
                else { Report(id, $"Missing or malformed footstep {id}; movement and captions remain available."); }
            }
        }
        settings.Changed += OnSettings;
        if (Automatic)
        {
            seeker = GetNode<Seeker>("../Seeker");
            materials = GetNode<OpeningRegionFootstepMaterialSource>("../OpeningRegionFootstepMaterialSource");
            restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            seeker.MotionCompleted += OnMotion;
            restoration.PresentationReset += ResetPresentation;
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        bool stopped = GetTree().Paused || (Automatic && seeker is not null
            && (!seeker.CanProcess() || !seeker.IsPhysicsProcessing()));
        if (stopped && !inactive) { ResetPresentation(); }
        inactive = stopped;
    }

    /// <summary>Cancels only this controller's cadence, tails and caption ownership.</summary>
    internal void ResetPresentation()
    {
        cadence.Reset();
        StopTails();
        captions.ClearFootstep();
        LastContact = null;
    }

    /// <summary>Plays one bounded window through the same installed players in disposable proofs.</summary>
    internal void PresentContact(FootstepContact contact)
    {
        string? parent = OpeningRegionFootstepMaterialProjection.ClipId(contact.Material);
        if (parent is null || contact.Variant < 1 || contact.Variant > 6) { ResetPresentation(); return; }
        ContactCount++;
        LastContact = contact;
        captions.SetFootstep(OpeningRegionFootstepMaterialProjection.Caption(contact.Material));
        if (!OpeningRegionMaterialAudioProjection.EffectsAudible(settings.Preferences)) { return; }
        string id = $"{parent}-{contact.Variant:00}";
        if (!streams.TryGetValue(id, out AudioStreamWav? stream)) { return; }
        int slot = players.FindIndex(p => !p.Playing);
        if (slot < 0) { slot = age[0] <= age[1] ? 0 : 1; }
        AudioStreamPlayer player = players[slot];
        player.Stop(); player.Stream = stream; player.VolumeLinear = 1; player.Play();
        age[slot] = ++sequence;
    }

    /// <summary>Removes loaded resources in an enabled missing-playback diagnostic only.</summary>
    internal void RemoveDiagnosticResources() { streams.Clear(); StopTails(); }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (seeker is not null) { seeker.MotionCompleted -= OnMotion; }
        if (restoration is not null) { restoration.PresentationReset -= ResetPresentation; }
        if (settings is not null) { settings.Changed -= OnSettings; }
        foreach (AudioStreamPlayer player in players) { player.Stop(); }
        if (captions is not null && GodotObject.IsInstanceValid(captions) && captions.IsInsideTree()) { captions.ClearFootstep(); }
        streams.Clear();
        LastContact = null;
        cadence.Reset();
    }

    private void OnMotion(FootstepMotionSample sample)
    {
        try
        {
            if (GetTree().Paused || materials is null || seeker is null || !seeker.CanProcess())
            {
                ResetPresentation(); return;
            }
            FootstepCadenceResult result = cadence.Advance(sample, materials.Select);
            if (result.Cancelled) { StopTails(); captions.ClearFootstep(); LastContact = null; }
            if (result.Contact is { } contact) { PresentContact(contact); }
        }
        catch (Exception error)
        {
            ResetPresentation();
            Report("observer", $"Footstep presentation unavailable: {error.GetType().Name}; movement remains available.");
        }
    }

    private void OnSettings()
    {
        if (!OpeningRegionMaterialAudioProjection.EffectsAudible(settings.Preferences)) { StopTails(); }
    }
    private void StopTails() { foreach (AudioStreamPlayer player in players) { player.Stop(); } }
    private void Report(string key, string message) { if (reported.Add(key)) { GD.PrintErr(message); } }
}
