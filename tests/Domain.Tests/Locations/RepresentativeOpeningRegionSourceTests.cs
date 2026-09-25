using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Tests.Locations;

public sealed class RepresentativeOpeningRegionSourceTests
{
    [Fact]
    public void AuthorsEveryOpeningLandmarkAsOneContinuousMultiRouteRegion()
    {
        OpeningRegionSnapshot region = RepresentativeOpeningRegionSource.Create();

        Assert.True(OpeningRegionContinuityPolicy.IsContinuous(region));
        Assert.Same(region.Locations[0], region.EntryLocation);
        Assert.Equal(
            [
                "sickhouse",
                "healer-garden",
                "village-square",
                "village-edge",
                "stream",
                "abandoned-laundry",
                "woodland-clearing",
                "old-shrine",
                "southern-road",
                "abandoned-wagon",
                "closed-deeper-forest-path",
            ],
            region.Locations.Select(location => location.Identity.Id.Value));
        Assert.True(HasConnection(region, "village-square", "sickhouse"));
        Assert.True(HasConnection(region, "village-square", "healer-garden"));
        Assert.True(HasConnection(region, "village-edge", "stream"));
        Assert.True(HasConnection(region, "stream", "abandoned-laundry"));
        Assert.True(HasConnection(region, "village-edge", "woodland-clearing"));
        Assert.True(HasConnection(region, "woodland-clearing", "old-shrine"));
        Assert.True(HasConnection(region, "southern-road", "abandoned-wagon"));
        Assert.True(HasConnection(region, "woodland-clearing", "closed-deeper-forest-path"));
        Assert.True(HasConnection(region, "abandoned-laundry", "woodland-clearing"));
        Assert.True(HasConnection(region, "old-shrine", "southern-road"));
    }

    [Fact]
    public void ContinuityRejectsDisconnectedAndForeignEndpointGraphs()
    {
        OpeningRegionSnapshot region = RepresentativeOpeningRegionSource.Create();
        OpeningRegionConnection first = region.Connections[0];
        Location foreign = Location("foreign", "Foreign");
        OpeningRegionSnapshot disconnected = new(
            region.EntryLocation,
            region.Locations,
            region.Connections.Where(connection =>
                !ReferenceEquals(connection.First, region.Locations[^1])
                && !ReferenceEquals(connection.Second, region.Locations[^1])));

        Assert.False(OpeningRegionContinuityPolicy.IsContinuous(disconnected));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSnapshot(
            region.EntryLocation,
            region.Locations,
            region.Connections.Append(new OpeningRegionConnection(first.First, foreign))));
    }

    [Fact]
    public void ContractsRejectDuplicateLocationsConnectionsAndSelfLoops()
    {
        Location first = Location("first", "First");
        Location second = Location("second", "Second");
        OpeningRegionConnection connection = new(first, second);

        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionConnection(first, first));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSnapshot(
            first,
            [first, first],
            [connection]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSnapshot(
            first,
            [first, second],
            [connection, new OpeningRegionConnection(second, first)]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSnapshot(
            Location("rebuilt", "First"),
            [first, second],
            [connection]));
    }

    private static bool HasConnection(
        OpeningRegionSnapshot region,
        string firstId,
        string secondId) => region.Connections.Any(connection =>
            HasId(connection.First, firstId) && HasId(connection.Second, secondId)
            || HasId(connection.First, secondId) && HasId(connection.Second, firstId));

    private static bool HasId(Location location, string id) => location.Identity.Id.Value == id;

    private static Location Location(string id, string displayName) => new(
        new LocationIdentity(new LocationId(id), displayName),
        new LocationDescription($"The authored {displayName.ToLowerInvariant()} landmark."));
}
