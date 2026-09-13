using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectConcealmentSnapshotTests
{
    [Fact]
    public void ConstructorRetainsStableExactIdentifiersAndCopiesInput()
    {
        ObjectId first = new("village-water-cask");
        ObjectId second = new("storehouse-ledger");
        List<ObjectId> source = [first, second];

        WorldObjectConcealmentSnapshot snapshot = new(source);
        source.Clear();

        Assert.Collection(
            snapshot.ConcealedObjectIds,
            objectId => Assert.Same(first, objectId),
            objectId => Assert.Same(second, objectId));
        Assert.DoesNotContain(
            snapshot.ConcealedObjectIds,
            objectId => objectId == new ObjectId("unconcealed-object"));
        Assert.All(
            typeof(WorldObjectConcealmentSnapshot).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void ConstructorRejectsNullDuplicateAndOverBoundInputs()
    {
        ObjectId first = new("village-water-cask");
        ObjectId duplicate = new(first.Value);
        IEnumerable<ObjectId> overBound = Enumerable.Range(
                0,
                WorldObjectConcealmentSnapshot.MaximumConcealedObjectCount + 1)
            .Select(index => new ObjectId($"object-{index}"));

        Assert.Throws<ArgumentNullException>(() => new WorldObjectConcealmentSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new WorldObjectConcealmentSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new WorldObjectConcealmentSnapshot([first, duplicate]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorldObjectConcealmentSnapshot(overBound));
    }
}
