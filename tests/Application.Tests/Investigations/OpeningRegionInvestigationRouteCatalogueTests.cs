using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class OpeningRegionInvestigationRouteCatalogueTests
{
    [Fact]
    public void AuthorsThreeExactIdOnlyRoutesInStableOrder()
    {
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        OpeningRegionInvestigationRouteCatalogue catalogue =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);

        Assert.Collection(
            catalogue.Routes,
            route => AssertRoute(
                route,
                "stream-laundry",
                ["stream", "abandoned-laundry"],
                ["residue"]),
            route => AssertRoute(route, "shrine", ["old-shrine"], ["partial-inscription"]),
            route => AssertRoute(
                route,
                "road-wagon",
                ["southern-road", "abandoned-wagon"],
                ["tool-gated-clue"]));
        Assert.All(catalogue.Routes, route =>
        {
            Assert.All(route.LocationIds, locationId => Assert.Contains(
                locations.Locations,
                location => ReferenceEquals(location.Id, locationId)));
            Assert.All(route.EvidenceIds, evidenceId => Assert.Contains(
                scenario.Evidence.Entries,
                evidence => ReferenceEquals(evidence.EvidenceId, evidenceId)));
        });
        Assert.True(OpeningRegionInvestigationRouteValidationPolicy.IsValid(
            catalogue,
            locations,
            scenario));
    }

    [Fact]
    public void CatalogueCopiesRoutesAndRequiresCompleteCanonicalOrder()
    {
        OpeningRegionInvestigationRoute first = Route("stream-laundry", "stream", "residue");
        OpeningRegionInvestigationRoute second = Route("shrine", "old-shrine", "partial-inscription");
        OpeningRegionInvestigationRoute third = Route("road-wagon", "southern-road", "tool-gated-clue");
        OpeningRegionInvestigationRoute[] supplied = [first, second, third];

        OpeningRegionInvestigationRouteCatalogue catalogue = new(supplied);
        supplied[0] = second;

        Assert.Equal([first, second, third], catalogue.Routes);
        Assert.Same(first, catalogue.Routes[0]);
        Assert.Same(second, catalogue.Routes[1]);
        Assert.Same(third, catalogue.Routes[2]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<OpeningRegionInvestigationRoute>)catalogue.Routes).Clear());
        Assert.Throws<ArgumentNullException>(() =>
            new OpeningRegionInvestigationRouteCatalogue(null!));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRouteCatalogue([]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRouteCatalogue([first, null!, third]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRouteCatalogue([second, first, third]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRouteCatalogue([first, first, third]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRouteCatalogue(
                [first, second, Route("unknown", "southern-road", "tool-gated-clue")]));
    }

    [Fact]
    public void ValidationRejectsReplacementUnknownAndRepeatedRouteReferences()
    {
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue source =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);

        Assert.False(OpeningRegionInvestigationRouteValidationPolicy.IsValid(
            Replace(
                source,
                0,
                new(
                    "stream-laundry",
                    [new LocationId(source.Routes[0].LocationIds[0].Value)],
                    [source.Routes[0].EvidenceIds[0]])),
            locations,
            scenario));
        Assert.False(OpeningRegionInvestigationRouteValidationPolicy.IsValid(
            Replace(
                source,
                0,
                new(
                    "stream-laundry",
                    [source.Routes[0].LocationIds[0]],
                    [new EvidenceId(source.Routes[0].EvidenceIds[0].Value)])),
            locations,
            scenario));
        Assert.False(OpeningRegionInvestigationRouteValidationPolicy.IsValid(
            Replace(
                source,
                0,
                Route("stream-laundry", "unknown-location", "unknown-evidence")),
            locations,
            scenario));
        Assert.False(OpeningRegionInvestigationRouteValidationPolicy.IsValid(
            new OpeningRegionInvestigationRouteCatalogue(
            [
                source.Routes[0],
                new(
                    "shrine",
                    source.Routes[0].LocationIds,
                    source.Routes[0].EvidenceIds),
                source.Routes[2],
            ]),
            locations,
            scenario));
    }

    [Fact]
    public void ValidationRejectsNullsWithoutChangingAnyInput()
    {
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue catalogue =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        OpeningRegionInvestigationRoute[] routes = catalogue.Routes.ToArray();
        LocationId[] locationIds = catalogue.Routes.SelectMany(route => route.LocationIds).ToArray();
        EvidenceId[] evidenceIds = catalogue.Routes.SelectMany(route => route.EvidenceIds).ToArray();

        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteValidationPolicy.IsValid(null!, locations, scenario));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteValidationPolicy.IsValid(catalogue, null!, scenario));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteValidationPolicy.IsValid(catalogue, locations, null!));

        Assert.Equal(routes, catalogue.Routes);
        Assert.Equal(locationIds, catalogue.Routes.SelectMany(route => route.LocationIds));
        Assert.Equal(evidenceIds, catalogue.Routes.SelectMany(route => route.EvidenceIds));
    }

    private static OpeningRegionInvestigationRoute Route(
        string key,
        string locationId,
        string evidenceId) => new(
            key,
            [new LocationId(locationId)],
            [new EvidenceId(evidenceId)]);

    private static OpeningRegionInvestigationRouteCatalogue Replace(
        OpeningRegionInvestigationRouteCatalogue source,
        int index,
        OpeningRegionInvestigationRoute replacement)
    {
        OpeningRegionInvestigationRoute[] routes = source.Routes.ToArray();
        routes[index] = replacement;
        return new OpeningRegionInvestigationRouteCatalogue(routes);
    }

    private static void AssertRoute(
        OpeningRegionInvestigationRoute route,
        string key,
        string[] locationIds,
        string[] evidenceIds)
    {
        Assert.Equal(key, route.Key);
        Assert.Equal(locationIds, route.LocationIds.Select(id => id.Value));
        Assert.Equal(evidenceIds, route.EvidenceIds.Select(id => id.Value));
    }
}
