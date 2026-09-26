using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Authors the representative opening-region investigation routes from exact sources.</summary>
public static class RepresentativeOpeningRegionInvestigationRouteSource
{
    /// <summary>Creates three fresh ID-only routes bound to exact source identifiers.</summary>
    public static OpeningRegionInvestigationRouteCatalogue Create(
        OpeningRegionMapLocationCatalogue locations,
        DiscoveryScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(scenario);

        return new OpeningRegionInvestigationRouteCatalogue(
        [
            new(
                "stream-laundry",
                [Location(locations, "stream"), Location(locations, "abandoned-laundry")],
                [Evidence(scenario, "residue")]),
            new(
                "shrine",
                [Location(locations, "old-shrine")],
                [Evidence(scenario, "partial-inscription")]),
            new(
                "road-wagon",
                [Location(locations, "southern-road"), Location(locations, "abandoned-wagon")],
                [Evidence(scenario, "tool-gated-clue")]),
        ]);
    }

    private static LocationId Location(
        OpeningRegionMapLocationCatalogue locations,
        string value) => locations.Locations
            .Single(location => string.Equals(location.Id.Value, value, StringComparison.Ordinal))
            .Id;

    private static EvidenceId Evidence(DiscoveryScenario scenario, string value) =>
        scenario.Evidence.Entries
            .Single(evidence => string.Equals(
                evidence.EvidenceId.Value,
                value,
                StringComparison.Ordinal))
            .EvidenceId;
}
