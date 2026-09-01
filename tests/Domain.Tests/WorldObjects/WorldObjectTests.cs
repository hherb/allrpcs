using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectTests
{
    [Fact]
    public void RetainsBothSuppliedComponentsExactly()
    {
        WorldObjectIdentity identity = new(new ObjectId("healing-herb"), "Healing Herb");
        WorldObjectDescription description = new("A narrow-leafed plant with a sharp scent.");

        WorldObject worldObject = new(identity, description);

        Assert.Same(identity, worldObject.Identity);
        Assert.Same(description, worldObject.Description);
    }

    [Fact]
    public void ComposesAnEmptyDescription()
    {
        WorldObject worldObject = new(
            new WorldObjectIdentity(new ObjectId("wagon"), "Abandoned Wagon"),
            new WorldObjectDescription(string.Empty));

        Assert.Empty(worldObject.Description.Text);
    }

    [Fact]
    public void RejectsEitherNullComponent()
    {
        WorldObjectIdentity identity = new(new ObjectId("mortar"), "Stone Mortar");
        WorldObjectDescription description = new("A heavy mortar worn smooth by use.");

        Assert.Throws<ArgumentNullException>(() => new WorldObject(null!, description));
        Assert.Throws<ArgumentNullException>(() => new WorldObject(identity, null!));
    }

    [Fact]
    public void ExposesOnlyTheCompleteReadOnlyCompositionSurface()
    {
        Type worldObjectType = typeof(WorldObject);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Description", typeof(WorldObjectDescription)),
            ("Identity", typeof(WorldObjectIdentity)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = worldObjectType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = worldObjectType
            .GetMethods()
            .Where(method => method.DeclaringType == worldObjectType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(worldObjectType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(worldObjectType.IsSealed);
    }
}
