using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Ingredients;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Plays six accepted sounds from existing live owners, without changing world state.</summary>
public partial class OpeningRegionMaterialAudioController : Node
{
    private static readonly string[] BedIds = ["B01", "B02", "B03", "B04"];
    private static readonly string[] CueIds = ["A04", "A05"];
    private readonly MaterialAudioRamp ramp = new();
    private readonly Dictionary<string, AudioStreamPlayer> players = [];
    private readonly HashSet<string> reported = [];
    private AudioSettingsController settings = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private OpeningRegionPositionalAudioController positional = null!;
    private OpeningRegionProjectionSource? projection;
    private OpeningRegionEnvironmentalStateSource? environment;
    private Seeker? seeker;
    private OpeningRegionRemedyPreparationInputCoordinator? preparation;
    private OpeningRegionFirstPatientAdministrationInputCoordinator? administration;
    private SaveSnapshotRestorationController? restoration;
    private MaterialAudioPresentation? current;
    private long treatmentLease;

    /// <summary>Ordinary Main starts live ambience; standalone diagnostics start stopped.</summary>
    [Export] public bool Automatic { get; set; } = true;
    /// <summary>The selected caption-safe presentation, without a world location claim.</summary>
    public MaterialAudioPresentation? Current => current;

    /// <inheritdoc />
    public override void _Ready()
    {
        settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        positional = GetNodeOrNull<OpeningRegionPositionalAudioController>("../OpeningRegionPositionalAudioController")!;
        if (positional is null && !Automatic)
        {
            positional = new() { Name = "OpeningRegionPositionalAudioController", Automatic = false };
            GetParent().CallDeferred(Node.MethodName.AddChild, positional);
        }
        if (positional is null) { throw new InvalidOperationException("Missing ordinary positional owner."); }
        foreach (string id in BedIds.Where(id => id != "B02").Concat(CueIds))
        {
            AudioStreamPlayer player = new() { Name = id, Bus = id.StartsWith('B') ? "Ambience" : "Effects" };
            AddChild(player);
            string path = $"res://assets/audio/material/{id}.wav";
            if (ResourceLoader.Exists(path) && GD.Load<AudioStreamWav>(path) is { } source)
            {
                AudioStreamWav stream = (AudioStreamWav)source.Duplicate();
                if (id.StartsWith('B'))
                {
                    stream.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
                    stream.LoopBegin = 0;
                    stream.LoopEnd = (int)Math.Round(stream.GetLength() * stream.MixRate);
                }
                player.Stream = stream;
            }
            else { Report(id, $"Missing material sound {id}; gameplay remains available."); }
            players.Add(id, player);
        }
        players["A05"].Finished += EndTreatmentLease;
        settings.Changed += OnPreferencesChanged;
        if (Automatic)
        {
            projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
            environment = GetNode<OpeningRegionEnvironmentalStateSource>("../OpeningRegionEnvironmentalStateSource");
            seeker = GetNode<Seeker>("../Seeker");
            preparation = GetNode<OpeningRegionRemedyPreparationInputCoordinator>("../OpeningRegionRemedyPreparationInputCoordinator");
            administration = GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>("../OpeningRegionFirstPatientAdministrationInputCoordinator");
            restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            preparation.GrindingCompleted += OnGrinding;
            administration.TreatmentCompleted += OnTreatment;
            restoration.PresentationReset += ResetPresentation;
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (Automatic && projection is not null && environment is not null && seeker is not null)
        {
            Vector2 position = projection.ToLocal(seeker.GlobalPosition);
            Present(OpeningRegionMaterialAudioProjection.Select(projection.Projection,
                environment.CurrentRegions, new NumericsVector2(position.X, position.Y)));
        }
        ramp.Advance(delta);
        for (int index = 0; index < BedIds.Length; index++)
        {
            double gain = ramp.Gain(index);
            if (index == 1) { positional.PresentWaterway(gain, current?.ClipId == "B02"); continue; }
            AudioStreamPlayer player = players[BedIds[index]];
            player.VolumeLinear = (float)gain;
            if (gain > 0 && !player.Playing && player.Stream is not null) { player.Play(); }
        }
        if (!players["A05"].Playing
            || !OpeningRegionMaterialAudioProjection.EffectsAudible(settings.Preferences)) { EndTreatmentLease(); }
    }

    /// <summary>Retargets presentation only; used by the stopped standalone output proof.</summary>
    internal void Present(MaterialAudioPresentation? presentation)
    {
        if (current == presentation) { return; }
        current = presentation;
        ramp.Retarget(presentation);
        if (presentation?.ClipId != "B02") { captions.SetRegion(presentation?.Caption ?? string.Empty); }
        if (presentation is null && Automatic) { Report("selection", "Material region unavailable; ambience fades to silence."); }
    }

    /// <summary>Plays a committed action passage once; diagnostics never create gameplay commits.</summary>
    internal void PlayCue(string id)
    {
        if (id is not ("A04" or "A05")) { return; }
        AudioStreamPlayer player = players[id];
        captions.SetAction(id == "A04" ? "Grinding" : "Treatment handling", 8);
        if (player.Stream is null)
        {
            if (id == "A05") { EndTreatmentLease(); }
            return;
        }
        player.Stop();
        if (id == "A05") { EndTreatmentLease(); }
        player.VolumeLinear = 1;
        player.Play();
        if (id == "A05" && OpeningRegionMaterialAudioProjection.EffectsAudible(settings.Preferences))
        {
            treatmentLease = settings.Ducking.Begin(AudioForeground.ConsequentialCue);
        }
    }

    /// <summary>Clears transient actions and leases, without replaying restored action history.</summary>
    internal void ResetPresentation()
    {
        players["A04"].Stop();
        players["A05"].Stop();
        EndTreatmentLease();
        captions.Clear();
        current = null;
        ramp.Retarget(null);
        positional.ResetPresentation();
    }

    /// <summary>Stops all diagnostic playback and restores a silent regional target.</summary>
    internal void StopPlayback()
    {
        ResetPresentation();
        foreach (AudioStreamPlayer player in players.Values) { player.Stop(); }
        ramp.Advance(OpeningRegionMaterialAudioProjection.RampSeconds);
        positional.StopPlayback();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (preparation is not null) { preparation.GrindingCompleted -= OnGrinding; }
        if (administration is not null) { administration.TreatmentCompleted -= OnTreatment; }
        if (restoration is not null) { restoration.PresentationReset -= ResetPresentation; }
        if (settings is not null) { settings.Changed -= OnPreferencesChanged; EndTreatmentLease(); }
    }

    private void OnGrinding(IngredientPreparationRecipe recipe, IngredientPreparationStep step, Vector2 station)
    {
        _ = station;
        if (OpeningRegionMaterialAudioProjection.IsGrinding(recipe, step)) { PlayCue("A04"); }
    }

    private void OnTreatment(OpeningRegionFirstPatientSceneBinding binding)
    {
        _ = binding;
        PlayCue("A05");
    }

    private void OnPreferencesChanged()
    {
        if (!OpeningRegionMaterialAudioProjection.EffectsAudible(settings.Preferences)) { EndTreatmentLease(); }
    }

    private void EndTreatmentLease()
    {
        if (treatmentLease != 0) { settings.Ducking.End(treatmentLease); treatmentLease = 0; }
    }

    private void Report(string key, string message)
    {
        if (reported.Add(key)) { GD.PrintErr(message); }
    }
}
