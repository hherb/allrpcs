using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Application.Audio;

/// <summary>One presentation-only regional bed, without adding location or hearing facts.</summary>
public sealed record MaterialAudioPresentation(int BedIndex, string ClipId, string Caption, double Gain);

/// <summary>Maps exact authored sources into the six authorized material sounds.</summary>
public static class OpeningRegionMaterialAudioProjection
{
    /// <summary>Seconds used by every regional retarget, including revealed-state attenuation.</summary>
    public const double RampSeconds = 0.75;
    /// <summary>Amplitude of the existing authored quieter woodland variation.</summary>
    public static readonly double QuieterWoodlandGain = Math.Pow(10, -3.0 / 20);

    /// <summary>Selects the closest exact anchor; unknown or foreign inputs produce no bed.</summary>
    public static MaterialAudioPresentation? Select(OpeningRegionProjection? projection,
        OpeningRegionEnvironmentalRegionSet? regions, Vector2 position)
    {
        if (projection is null || regions is null || !ReferenceEquals(projection.Topology, regions.Topology)
            || !float.IsFinite(position.X) || !float.IsFinite(position.Y))
        {
            return null;
        }
        OpeningRegionAnchor anchor = projection.Anchors
            .OrderBy(candidate => Vector2.DistanceSquared(candidate.Position, position))
            .ThenBy(candidate => candidate.Location.Identity.Id.Value, StringComparer.Ordinal).First();
        OpeningRegionEnvironmentalRegion? region =
            OpeningRegionEnvironmentalRegionLookup.FindByLocation(regions, anchor.Location);
        return region?.SoundState.Key switch
        {
            "muted-village-rain" => new(0, "B01", "Sheltered rain", 1),
            "running-water-and-rain" => new(1, "B02", "Running water and rain", 1),
            "rain-under-trees" => new(2, "B03", "Rain in leaves", 1),
            "road-wind-and-wagon" => new(3, "B04", "Wind, rain and timber creak", 1),
            "inscription-revealed-sound" when region.Key == "woodland-edge" =>
                new(2, "B03", "Rain in leaves, quieter", QuieterWoodlandGain),
            _ => null,
        };
    }

    /// <summary>Recognizes only an exact retained recipe step owning grinding feedback.</summary>
    public static bool IsGrinding(IngredientPreparationRecipe recipe, IngredientPreparationStep step) =>
        recipe.Plan.Steps.Any(candidate => ReferenceEquals(candidate, step))
        && ReferenceEquals(step.Kind, IngredientPreparationKind.Grinding);

    /// <summary>Tests the existing user mute/gain contract for consequential-cue leases.</summary>
    public static bool EffectsAudible(AudioPreferences preferences) =>
        preferences.Channels.Where(channel => channel.Category is AudioCategory.Master or AudioCategory.Effects)
            .All(channel => channel.Gain > 0 && !channel.Muted);
}

/// <summary>Four bounded linear weights; retargeting always begins at their current values.</summary>
public sealed class MaterialAudioRamp
{
    private readonly double[] current = new double[4];
    private readonly double[] start = new double[4];
    private readonly double[] target = new double[4];
    private double elapsed;

    /// <summary>Returns one current amplitude without exposing mutable storage.</summary>
    public double Gain(int index) => current[index];

    /// <summary>Retargets once per changed presentation; null ramps to silence.</summary>
    public void Retarget(MaterialAudioPresentation? presentation)
    {
        if (presentation is not null && (presentation.BedIndex is < 0 or > 3
            || !double.IsFinite(presentation.Gain) || presentation.Gain is < 0 or > 1))
        {
            throw new ArgumentOutOfRangeException(nameof(presentation));
        }
        double[] next = new double[4];
        if (presentation is not null) { next[presentation.BedIndex] = presentation.Gain; }
        if (next.SequenceEqual(target)) { return; }
        Array.Copy(current, start, 4);
        Array.Copy(next, target, 4);
        elapsed = 0;
    }

    /// <summary>Advances presentation time only; values are convex combinations, never amplified.</summary>
    public void Advance(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0) { throw new ArgumentOutOfRangeException(nameof(seconds)); }
        elapsed = Math.Min(OpeningRegionMaterialAudioProjection.RampSeconds, elapsed + seconds);
        double ratio = elapsed / OpeningRegionMaterialAudioProjection.RampSeconds;
        for (int index = 0; index < 4; index++)
        {
            current[index] = start[index] + (target[index] - start[index]) * ratio;
        }
    }
}
