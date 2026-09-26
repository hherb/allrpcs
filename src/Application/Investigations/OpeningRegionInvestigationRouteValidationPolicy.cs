using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Validates exact route references without activating or changing either source.</summary>
public static class OpeningRegionInvestigationRouteValidationPolicy
{
    /// <summary>Reports whether every route resolves exactly once and has a distinct ID shape.</summary>
    public static bool IsValid(
        OpeningRegionInvestigationRouteCatalogue catalogue,
        OpeningRegionMapLocationCatalogue locations,
        DiscoveryScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(scenario);

        for (int index = 0; index < catalogue.Routes.Count; index++)
        {
            OpeningRegionInvestigationRoute route = catalogue.Routes[index];
            if (!AllLocationsResolveExactlyOnce(route.LocationIds, locations)
                || !AllEvidenceResolvesExactlyOnce(route.EvidenceIds, scenario)
                || catalogue.Routes.Take(index).Any(previous => SameRoute(previous, route)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AllLocationsResolveExactlyOnce(
        IReadOnlyList<LocationId> identifiers,
        OpeningRegionMapLocationCatalogue locations) => identifiers.All(identifier =>
            locations.Locations.Count(location => ReferenceEquals(location.Id, identifier)) == 1);

    private static bool AllEvidenceResolvesExactlyOnce(
        IReadOnlyList<EvidenceId> identifiers,
        DiscoveryScenario scenario) => identifiers.All(identifier =>
            scenario.Evidence.Entries.Count(evidence =>
                ReferenceEquals(evidence.EvidenceId, identifier)) == 1);

    private static bool SameRoute(
        OpeningRegionInvestigationRoute first,
        OpeningRegionInvestigationRoute second) =>
        first.LocationIds.SequenceEqual(second.LocationIds)
        && first.EvidenceIds.SequenceEqual(second.EvidenceIds);
}
