using Allrpcs.Application.Investigations;

namespace Allrpcs.Game;

/// <summary>Arranges exact empty route lifecycle state for persistence smoke checks.</summary>
internal static class SaveRouteLifecycleSmokeArrangement
{
    /// <summary>Initializes canonical empty progress from the exact live map and discovery sources.</summary>
    internal static bool TryInitializeEmpty(
        OpeningRegionInvestigationRouteProgressController progress,
        SeekerMapController map,
        OpeningRegionOldShrineSceneSource shrine)
    {
        if (map.Catalogue is not { } locations)
        {
            return false;
        }

        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(
                locations,
                shrine.DiscoveryScenario);
        return progress.TryInitialize(routes, locations, shrine.DiscoveryScenario);
    }
}
