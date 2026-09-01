using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectIdentityTests
{
    [Fact]
    public void RetainsStableIdentifierAndAuthoredDisplayNameExactly()
    {
        ObjectId id = new("weathered-wagon");
        const string DisplayName = "  Travellers' Wagon — Broken Axle  ";

        WorldObjectIdentity identity = new(id, DisplayName);

        Assert.Same(id, identity.Id);
        Assert.Equal(DisplayName, identity.DisplayName);
    }

    [Fact]
    public void EqualityUsesIdentifierAndDisplayNameValues()
    {
        WorldObjectIdentity first = new(new ObjectId("weathered-wagon"), "Weathered Wagon");
        WorldObjectIdentity same = new(new ObjectId("weathered-wagon"), "Weathered Wagon");
        WorldObjectIdentity differentId = new(new ObjectId("stone-well"), "Weathered Wagon");
        WorldObjectIdentity differentName = new(new ObjectId("weathered-wagon"), "Broken Wagon");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentId);
        Assert.NotEqual(first, differentName);
    }

    [Fact]
    public void AcceptsTheMaximumDisplayNameLength()
    {
        string displayName = new('n', WorldObjectIdentity.MaximumDisplayNameLength);

        WorldObjectIdentity identity = new(new ObjectId("weathered-wagon"), displayName);

        Assert.Equal(displayName, identity.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDisplayNames(string displayName)
    {
        Assert.Throws<ArgumentException>(
            () => new WorldObjectIdentity(new ObjectId("weathered-wagon"), displayName));
    }

    [Fact]
    public void RejectsAnOversizedDisplayName()
    {
        string displayName = new('n', WorldObjectIdentity.MaximumDisplayNameLength + 1);

        Assert.Throws<ArgumentException>(
            () => new WorldObjectIdentity(new ObjectId("weathered-wagon"), displayName));
    }

    [Fact]
    public void RejectsNullIdentityValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new WorldObjectIdentity(null!, "Weathered Wagon"));
        Assert.Throws<ArgumentNullException>(
            () => new WorldObjectIdentity(new ObjectId("weathered-wagon"), null!));
    }

    [Fact]
    public void ExposesOnlyStableWorldObjectIdentityState()
    {
        Type identityType = typeof(WorldObjectIdentity);
        (string Name, Type Type, bool CanWrite)[] properties = identityType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            new[]
            {
                ("DisplayName", typeof(string), false),
                ("Id", typeof(ObjectId), false),
            },
            properties);
        Assert.Empty(identityType.GetEvents());
        Assert.True(identityType.IsSealed);
    }

    [Fact]
    public void RemainsStructurallySeparateFromLocationAndCharacterIdentity()
    {
        object worldObjectIdentity = new WorldObjectIdentity(
            new ObjectId("shared-name"),
            "Shared authored name");
        object locationIdentity = new LocationIdentity(
            new LocationId("shared-name"),
            "Shared authored name");
        object characterIdentity = new CharacterIdentity(
            new CharacterId("shared-name"),
            "Shared authored name");

        Assert.NotEqual(worldObjectIdentity, locationIdentity);
        Assert.NotEqual(worldObjectIdentity, characterIdentity);
        Assert.NotEqual(typeof(WorldObjectIdentity), typeof(LocationIdentity));
        Assert.NotEqual(typeof(WorldObjectIdentity), typeof(CharacterIdentity));
    }
}
