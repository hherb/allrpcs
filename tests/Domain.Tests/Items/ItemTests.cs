using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Items;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Items;

public sealed class ItemTests
{
    [Fact]
    public void RetainsTheSuppliedWorldObjectExactly()
    {
        WorldObject worldObject = new(
            new WorldObjectIdentity(new ObjectId("stone-mortar"), "Stone Mortar"),
            new WorldObjectDescription("A heavy mortar worn smooth by use."));

        Item item = new(worldObject);

        Assert.Same(worldObject, item.WorldObject);
    }

    [Fact]
    public void RejectsANullWorldObject()
    {
        Assert.Throws<ArgumentNullException>(() => new Item(null!));
    }

    [Fact]
    public void ExposesOnlyTheCompleteReadOnlyClassificationSurface()
    {
        Type itemType = typeof(Item);
        (string Name, Type Type)[] expectedProperties =
        [
            ("WorldObject", typeof(WorldObject)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = itemType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = itemType
            .GetMethods()
            .Where(method => method.DeclaringType == itemType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(itemType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(itemType.IsSealed);
    }
}
