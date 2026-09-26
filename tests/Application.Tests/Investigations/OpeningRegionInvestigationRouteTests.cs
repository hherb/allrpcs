using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class OpeningRegionInvestigationRouteTests
{
    [Fact]
    public void RetainsExactIdentifiersInStableReadOnlyOrder()
    {
        LocationId stream = new("stream");
        LocationId laundry = new("abandoned-laundry");
        EvidenceId residue = new("residue");
        LocationId[] suppliedLocations = [stream, laundry];
        EvidenceId[] suppliedEvidence = [residue];

        OpeningRegionInvestigationRoute route = new(
            "stream-laundry",
            suppliedLocations,
            suppliedEvidence);
        suppliedLocations[0] = new LocationId("old-shrine");
        suppliedEvidence[0] = new EvidenceId("partial-inscription");

        Assert.Equal("stream-laundry", route.Key);
        Assert.Equal([stream, laundry], route.LocationIds);
        Assert.Equal([residue], route.EvidenceIds);
        Assert.Same(stream, route.LocationIds[0]);
        Assert.Same(laundry, route.LocationIds[1]);
        Assert.Same(residue, route.EvidenceIds[0]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<LocationId>)route.LocationIds).Clear());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<EvidenceId>)route.EvidenceIds).Clear());
    }

    [Fact]
    public void RejectsInvalidKeysEmptyListsNullsAndDuplicateIdentifiers()
    {
        LocationId stream = new("stream");
        EvidenceId residue = new("residue");

        Assert.Throws<ArgumentNullException>(() =>
            new OpeningRegionInvestigationRoute(null!, [stream], [residue]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRoute("Stream Route", [stream], [residue]));
        Assert.Throws<ArgumentNullException>(() =>
            new OpeningRegionInvestigationRoute("route", null!, [residue]));
        Assert.Throws<ArgumentNullException>(() =>
            new OpeningRegionInvestigationRoute("route", [stream], null!));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRoute("route", [], [residue]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRoute("route", [stream], []));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRoute("route", [stream, null!], [residue]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRoute("route", [stream], [residue, null!]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRoute("route", [stream, new("stream")], [residue]));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionInvestigationRoute("route", [stream], [residue, new("residue")]));
    }

    [Fact]
    public void ExposesOnlyTheImmutableIdentifierSurface()
    {
        Type routeType = typeof(OpeningRegionInvestigationRoute);

        Assert.True(routeType.IsSealed);
        Assert.Equal(
            ["EvidenceIds", "Key", "LocationIds"],
            routeType.GetProperties().Select(property => property.Name).Order());
        Assert.All(routeType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(routeType.GetEvents());
        Assert.Empty(routeType.GetMethods().Where(
            method => method.DeclaringType == routeType && !method.IsSpecialName));
    }
}
