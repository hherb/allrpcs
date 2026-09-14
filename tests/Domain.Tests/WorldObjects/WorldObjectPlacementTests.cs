using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectPlacementTests
{
    [Fact]
    public void PlacementRetainsExactWorldObjectAndLocation()
    {
        WorldObject worldObject = WorldObject("village-water-cask");
        Location location = Location("village-square");

        WorldObjectPlacement placement = new(worldObject, location);

        Assert.Same(worldObject, placement.WorldObject);
        Assert.Same(location, placement.Location);
    }

    [Fact]
    public void SnapshotRetainsStableExactPlacementsAndCopiesCallerCollection()
    {
        WorldObjectPlacement first = Placement("village-water-cask", "village-square");
        WorldObjectPlacement second = Placement("storehouse-ledger", "storehouse");
        WorldObjectPlacement replacement = Placement("wagon-crate", "southern-road");
        WorldObjectPlacement[] source = [first, second];

        WorldObjectPlacementSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<WorldObjectPlacement>)snapshot.Entries).Add(replacement));
    }

    [Fact]
    public void SnapshotAllowsDistinctObjectsAtTheSameExactLocation()
    {
        Location location = Location("village-square");
        WorldObjectPlacement first = new(WorldObject("village-water-cask"), location);
        WorldObjectPlacement second = new(WorldObject("well-bucket"), location);

        WorldObjectPlacementSnapshot snapshot = new([first, second]);

        Assert.All(snapshot.Entries, entry => Assert.Same(location, entry.Location));
    }

    [Fact]
    public void SnapshotRejectsDuplicateObjectIdentifiersIncludingValueEqualObjects()
    {
        WorldObjectPlacement first = Placement("village-water-cask", "village-square");
        WorldObjectPlacement duplicate = Placement("village-water-cask", "abandoned-wagon");

        Assert.NotSame(first.WorldObject, duplicate.WorldObject);
        Assert.Equal(first.WorldObject.Identity.Id, duplicate.WorldObject.Identity.Id);
        Assert.Throws<ArgumentException>(() =>
            new WorldObjectPlacementSnapshot([first, duplicate]));
    }

    [Fact]
    public void SnapshotAcceptsTheBoundAndRejectsOverflow()
    {
        WorldObjectPlacement[] maximum = Enumerable.Range(
                0,
                WorldObjectPlacementSnapshot.MaximumPlacementCount)
            .Select(index => Placement($"object-{index}", $"location-{index}"))
            .ToArray();

        WorldObjectPlacementSnapshot snapshot = new(maximum);

        Assert.Equal(WorldObjectPlacementSnapshot.MaximumPlacementCount, snapshot.Entries.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WorldObjectPlacementSnapshot(
                maximum.Append(Placement("overflow", "overflow-location"))));
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyClosedReadOnlyState()
    {
        WorldObject worldObject = WorldObject("village-water-cask");
        Location location = Location("village-square");

        Assert.Throws<ArgumentNullException>(() => new WorldObjectPlacement(null!, location));
        Assert.Throws<ArgumentNullException>(() => new WorldObjectPlacement(worldObject, null!));
        Assert.Throws<ArgumentNullException>(() => new WorldObjectPlacementSnapshot(null!));
        Assert.Throws<ArgumentException>(() =>
            new WorldObjectPlacementSnapshot(new WorldObjectPlacement[] { null! }));

        AssertClosedState(
            typeof(WorldObjectPlacement),
            [
                ("Location", typeof(Location)),
                ("WorldObject", typeof(WorldObject)),
            ]);
        AssertClosedState(
            typeof(WorldObjectPlacementSnapshot),
            [("Entries", typeof(IReadOnlyList<WorldObjectPlacement>))]);
    }

    private static void AssertClosedState(
        Type type,
        (string Name, Type Type)[] expectedProperties)
    {
        (string Name, Type Type, bool CanWrite)[] properties = type
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = type
            .GetMethods()
            .Where(method => method.DeclaringType == type && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(type.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(type.IsSealed);
    }

    private static WorldObjectPlacement Placement(string objectId, string locationId) =>
        new(WorldObject(objectId), Location(locationId));

    private static WorldObject WorldObject(string objectId) =>
        new(
            new WorldObjectIdentity(new ObjectId(objectId), $"Object {objectId}"),
            new WorldObjectDescription($"Description for {objectId}."));

    private static Location Location(string locationId) =>
        new(
            new LocationIdentity(new LocationId(locationId), $"Location {locationId}"),
            new LocationDescription($"Description for {locationId}."));
}
