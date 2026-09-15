using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectContaminationSnapshotTests
{
    [Fact]
    public void ConstructorRetainsStableExactIdentifiersAndCopiesInput()
    {
        ObjectId first = new("village-water-cask");
        ObjectId second = new("healers-mortar");
        List<ObjectId> source = [first, second];

        WorldObjectContaminationSnapshot snapshot = new(source);
        source.Clear();

        Assert.Collection(
            snapshot.ContaminatedObjectIds,
            objectId => Assert.Same(first, objectId),
            objectId => Assert.Same(second, objectId));
        Assert.Empty(source);
    }

    [Fact]
    public void IsContaminatedUsesIdentifierValueAndLeavesCallerInputUnchanged()
    {
        ObjectId contaminated = new("village-water-cask");
        List<ObjectId> source = [contaminated];
        WorldObjectContaminationSnapshot snapshot = new(source);

        Assert.True(snapshot.IsContaminated(contaminated));
        Assert.True(snapshot.IsContaminated(new ObjectId(contaminated.Value)));
        Assert.False(snapshot.IsContaminated(new ObjectId("healers-mortar")));
        Assert.Throws<ArgumentNullException>(() => snapshot.IsContaminated(null!));
        Assert.Collection(source, objectId => Assert.Same(contaminated, objectId));
    }

    [Fact]
    public void ConstructorAcceptsEmptyAndMaximumCount()
    {
        WorldObjectContaminationSnapshot empty = new([]);
        ObjectId[] maximum = Enumerable.Range(
                0,
                WorldObjectContaminationSnapshot.MaximumContaminatedObjectCount)
            .Select(index => new ObjectId($"object-{index}"))
            .ToArray();

        WorldObjectContaminationSnapshot full = new(maximum);

        Assert.Empty(empty.ContaminatedObjectIds);
        Assert.Equal(WorldObjectContaminationSnapshot.MaximumContaminatedObjectCount, full.ContaminatedObjectIds.Count);
        Assert.All(maximum, objectId => Assert.True(full.IsContaminated(objectId)));
    }

    [Fact]
    public void ConstructorRejectsNullDuplicateAndOverBoundInputsBeforeRetainingState()
    {
        ObjectId first = new("village-water-cask");
        ObjectId duplicate = new(first.Value);
        IEnumerable<ObjectId> overBound = Enumerable.Range(
                0,
                WorldObjectContaminationSnapshot.MaximumContaminatedObjectCount + 1)
            .Select(index => new ObjectId($"object-{index}"));

        Assert.NotSame(first, duplicate);
        Assert.Equal(first, duplicate);
        Assert.Throws<ArgumentNullException>(() => new WorldObjectContaminationSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new WorldObjectContaminationSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new WorldObjectContaminationSnapshot([first, duplicate]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorldObjectContaminationSnapshot(overBound));
    }

    [Fact]
    public void SurfaceIsClosedAndGetOnly()
    {
        Assert.All(
            typeof(WorldObjectContaminationSnapshot).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.DoesNotContain(
            typeof(WorldObjectContaminationSnapshot).GetMethods(),
            method => method.DeclaringType == typeof(WorldObjectContaminationSnapshot)
                && method.IsPublic
                && method.Name.StartsWith("set_", StringComparison.Ordinal));
    }
}
