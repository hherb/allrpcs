using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Tests.Locations;

public sealed class LocationIdentityTests
{
    [Fact]
    public void RetainsStableIdentifierAndAuthoredDisplayNameExactly()
    {
        LocationId id = new("village-square");
        const string DisplayName = "  Village Square — Upper Terrace  ";

        LocationIdentity identity = new(id, DisplayName);

        Assert.Same(id, identity.Id);
        Assert.Equal(DisplayName, identity.DisplayName);
    }

    [Fact]
    public void EqualityUsesIdentifierAndDisplayNameValues()
    {
        LocationIdentity first = new(new LocationId("village-square"), "Village Square");
        LocationIdentity same = new(new LocationId("village-square"), "Village Square");
        LocationIdentity differentId = new(new LocationId("woodland"), "Village Square");
        LocationIdentity differentName = new(new LocationId("village-square"), "The Square");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentId);
        Assert.NotEqual(first, differentName);
    }

    [Fact]
    public void AcceptsTheMaximumDisplayNameLength()
    {
        string displayName = new('n', LocationIdentity.MaximumDisplayNameLength);

        LocationIdentity identity = new(new LocationId("village-square"), displayName);

        Assert.Equal(displayName, identity.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDisplayNames(string displayName)
    {
        Assert.Throws<ArgumentException>(
            () => new LocationIdentity(new LocationId("village-square"), displayName));
    }

    [Fact]
    public void RejectsAnOversizedDisplayName()
    {
        string displayName = new('n', LocationIdentity.MaximumDisplayNameLength + 1);

        Assert.Throws<ArgumentException>(
            () => new LocationIdentity(new LocationId("village-square"), displayName));
    }

    [Fact]
    public void RejectsNullIdentityValues()
    {
        Assert.Throws<ArgumentNullException>(() => new LocationIdentity(null!, "Village Square"));
        Assert.Throws<ArgumentNullException>(
            () => new LocationIdentity(new LocationId("village-square"), null!));
    }

    [Fact]
    public void ExposesOnlyStableLocationIdentityState()
    {
        Type identityType = typeof(LocationIdentity);
        (string Name, Type Type, bool CanWrite)[] properties = identityType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            new[]
            {
                ("DisplayName", typeof(string), false),
                ("Id", typeof(LocationId), false),
            },
            properties);
        Assert.Empty(identityType.GetEvents());
        Assert.True(identityType.IsSealed);
    }

    [Fact]
    public void RemainsStructurallySeparateFromCharacterIdentity()
    {
        object locationIdentity = new LocationIdentity(
            new LocationId("village-square"),
            "Shared authored name");
        object characterIdentity = new CharacterIdentity(
            new CharacterId("village-square"),
            "Shared authored name");

        Assert.NotEqual(locationIdentity, characterIdentity);
        Assert.NotEqual(typeof(LocationIdentity), typeof(CharacterIdentity));
    }
}
