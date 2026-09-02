using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Tests.Inventory;

public sealed class InventoryOwnershipTests
{
    [Fact]
    public void RetainsObjectAndOwnerCharacterExactly()
    {
        ObjectId objectId = new("stone-mortar");
        CharacterId ownerCharacterId = new("travelling-healer");

        InventoryOwnership ownership = new(objectId, ownerCharacterId);

        Assert.Same(objectId, ownership.ObjectId);
        Assert.Same(ownerCharacterId, ownership.OwnerCharacterId);
    }

    [Fact]
    public void EqualityUsesObjectAndOwnerCharacterValues()
    {
        InventoryOwnership first = new(
            new ObjectId("stone-mortar"),
            new CharacterId("travelling-healer"));
        InventoryOwnership same = new(
            new ObjectId("stone-mortar"),
            new CharacterId("travelling-healer"));
        InventoryOwnership differentObject = new(
            new ObjectId("healing-herb"),
            new CharacterId("travelling-healer"));
        InventoryOwnership differentOwner = new(
            new ObjectId("stone-mortar"),
            new CharacterId("seeker"));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentObject);
        Assert.NotEqual(first, differentOwner);
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new InventoryOwnership(null!, new CharacterId("travelling-healer")));
        Assert.Throws<ArgumentNullException>(
            () => new InventoryOwnership(new ObjectId("stone-mortar"), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableAssociationSurface()
    {
        Type ownershipType = typeof(InventoryOwnership);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ObjectId", typeof(ObjectId)),
            ("OwnerCharacterId", typeof(CharacterId)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = ownershipType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(ownershipType.GetEvents());
        Assert.True(ownershipType.IsSealed);
    }
}
