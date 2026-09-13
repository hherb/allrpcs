using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterPhysicalAccessTests
{
    [Fact]
    public void SnapshotRetainsExactCharacterAndStableCopiedAccessibleObjects()
    {
        CharacterId characterId = new("enemy-agent");
        ObjectId first = new("remedy-cabinet");
        ObjectId second = new("water-cask");
        ObjectId[] source = [first, second];

        CharacterPhysicalAccessSnapshot snapshot = new(characterId, source);
        source[0] = new ObjectId("replacement");

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Equal(2, snapshot.AccessibleObjectIds.Count);
        Assert.Same(first, snapshot.AccessibleObjectIds[0]);
        Assert.Same(second, snapshot.AccessibleObjectIds[1]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<ObjectId>)snapshot.AccessibleObjectIds).Add(new ObjectId("extra")));
        Assert.All(
            typeof(CharacterPhysicalAccessSnapshot).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void SnapshotAcceptsEmptyAndMaximumBoundaries()
    {
        CharacterPhysicalAccessSnapshot empty = new(new CharacterId("observer"), []);
        ObjectId[] maximum = Enumerable
            .Range(0, CharacterPhysicalAccessSnapshot.MaximumAccessibleObjectCount)
            .Select(index => new ObjectId($"object-{index}"))
            .ToArray();
        CharacterPhysicalAccessSnapshot full = new(new CharacterId("agent"), maximum);

        Assert.Empty(empty.AccessibleObjectIds);
        Assert.Equal(
            CharacterPhysicalAccessSnapshot.MaximumAccessibleObjectCount,
            full.AccessibleObjectIds.Count);
    }

    [Fact]
    public void SnapshotRejectsNullDuplicateAndOversizedInput()
    {
        CharacterId characterId = new("enemy-agent");
        ObjectId objectId = new("remedy-cabinet");
        ObjectId[] oversized = Enumerable
            .Range(0, CharacterPhysicalAccessSnapshot.MaximumAccessibleObjectCount + 1)
            .Select(index => new ObjectId($"object-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterPhysicalAccessSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterPhysicalAccessSnapshot(characterId, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterPhysicalAccessSnapshot(characterId, [objectId, null!]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterPhysicalAccessSnapshot(
                characterId,
                [objectId, new ObjectId(objectId.Value)]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterPhysicalAccessSnapshot(characterId, oversized));
    }

    [Fact]
    public void PolicyRequiresValueMatchingCharacterAndAccessibleObject()
    {
        CharacterPhysicalAccessSnapshot snapshot = new(
            new CharacterId("enemy-agent"),
            [new ObjectId("remedy-cabinet"), new ObjectId("water-cask")]);

        Assert.True(CharacterPhysicalAccessPolicy.CanAccess(
            snapshot,
            new CharacterId("enemy-agent"),
            new ObjectId("water-cask")));
        Assert.False(CharacterPhysicalAccessPolicy.CanAccess(
            snapshot,
            new CharacterId("village-sage"),
            new ObjectId("water-cask")));
        Assert.False(CharacterPhysicalAccessPolicy.CanAccess(
            snapshot,
            new CharacterId("enemy-agent"),
            new ObjectId("herb-patch")));
    }

    [Fact]
    public void PolicyRejectsNullsAndExposesOnlyOnePureOperation()
    {
        CharacterPhysicalAccessSnapshot snapshot = new(new CharacterId("enemy-agent"), []);
        CharacterId characterId = new("enemy-agent");
        ObjectId objectId = new("remedy-cabinet");

        Assert.Throws<ArgumentNullException>(() =>
            CharacterPhysicalAccessPolicy.CanAccess(null!, characterId, objectId));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPhysicalAccessPolicy.CanAccess(snapshot, null!, objectId));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPhysicalAccessPolicy.CanAccess(snapshot, characterId, null!));

        Type policyType = typeof(CharacterPhysicalAccessPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccess", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}
