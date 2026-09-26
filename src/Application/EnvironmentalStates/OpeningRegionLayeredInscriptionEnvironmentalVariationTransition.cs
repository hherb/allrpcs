using Allrpcs.Application.Discovery;
using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.EnvironmentalStates;

/// <summary>Derives one exact woodland environment variation after inscription presentation.</summary>
public static class OpeningRegionLayeredInscriptionEnvironmentalVariationTransition
{
    private const string WoodlandRegionKey = "woodland-edge";

    /// <summary>Creates a varied snapshot without changing discovery or scene state.</summary>
    public static OpeningRegionLayeredInscriptionEnvironmentalVariation Create(
        OpeningRegionEnvironmentalRegionSet baselineRegions,
        OpeningRegionLayeredInscriptionPresentation presentation,
        EnvironmentalState revealedLightingState,
        EnvironmentalState revealedSoundState)
    {
        ArgumentNullException.ThrowIfNull(baselineRegions);
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentNullException.ThrowIfNull(revealedLightingState);
        ArgumentNullException.ThrowIfNull(revealedSoundState);
        OpeningRegionSnapshot presentationTopology = presentation.DiscoveryState
            .InscriptionBinding.ShrineSceneBinding.Projection.Topology;
        if (!ReferenceEquals(baselineRegions.Topology, presentationTopology))
        {
            throw new ArgumentException(
                "The inscription presentation must belong to the baseline's exact topology.",
                nameof(presentation));
        }

        if (revealedLightingState.Key == revealedSoundState.Key
            || baselineRegions.EnvironmentalStates.Entries.Any(state =>
                state.Key == revealedLightingState.Key || state.Key == revealedSoundState.Key))
        {
            throw new ArgumentException(
                "Revealed environmental states require distinct new canonical keys.");
        }

        OpeningRegionEnvironmentalRegion? woodland = baselineRegions.Regions.SingleOrDefault(
            region => region.Key == WoodlandRegionKey);
        Location shrine = presentation.DiscoveryState.InscriptionBinding
            .ShrineSceneBinding.OldShrineAnchor.Location;
        if (woodland is null
            || !woodland.Locations.Any(location => ReferenceEquals(location, shrine)))
        {
            throw new ArgumentException(
                "The exact inscription shrine must remain in the woodland region.",
                nameof(baselineRegions));
        }

        EnvironmentalStateSet variedStates = new(
            baselineRegions.EnvironmentalStates.Entries
                .Append(revealedLightingState)
                .Append(revealedSoundState));
        OpeningRegionEnvironmentalRegion variedWoodland = new(
            woodland.Key,
            woodland.Locations,
            woodland.WeatherState,
            revealedLightingState,
            revealedSoundState);
        OpeningRegionEnvironmentalRegionSet variedRegions = new(
            baselineRegions.Topology,
            variedStates,
            baselineRegions.Regions.Select(region => ReferenceEquals(region, woodland)
                ? variedWoodland
                : region));
        return new OpeningRegionLayeredInscriptionEnvironmentalVariation(
            baselineRegions,
            presentation,
            revealedLightingState,
            revealedSoundState,
            variedRegions);
    }
}
