using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Tests.Locations;

public sealed class LocationTests
{
    [Fact]
    public void RetainsBothSuppliedComponentsExactly()
    {
        LocationIdentity identity = new(new LocationId("village-square"), "Village Square");
        LocationDescription description = new("Weathered stones surround the well.");

        Location location = new(identity, description);

        Assert.Same(identity, location.Identity);
        Assert.Same(description, location.Description);
    }

    [Fact]
    public void ComposesAnEmptyDescription()
    {
        Location location = new(
            new LocationIdentity(new LocationId("woodland"), "Woodland"),
            new LocationDescription(string.Empty));

        Assert.Empty(location.Description.Text);
    }

    [Fact]
    public void RejectsEitherNullComponent()
    {
        LocationIdentity identity = new(new LocationId("stream"), "Stream");
        LocationDescription description = new("A shallow stream crosses the path.");

        Assert.Throws<ArgumentNullException>(() => new Location(null!, description));
        Assert.Throws<ArgumentNullException>(() => new Location(identity, null!));
    }

    [Fact]
    public void ExposesOnlyTheCompleteReadOnlyCompositionSurface()
    {
        Type locationType = typeof(Location);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Description", typeof(LocationDescription)),
            ("Identity", typeof(LocationIdentity)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = locationType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = locationType
            .GetMethods()
            .Where(method => method.DeclaringType == locationType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(locationType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(locationType.IsSealed);
    }
}
