using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryCapabilityRequirementTests
{
    [Fact]
    public void RetainsObjectAndRequiredCapabilityKeyExactly()
    {
        ObjectId objectId = new("concealed-healing-herb");

        DiscoveryCapabilityRequirement requirement = new(objectId, "identify-healing-plants");

        Assert.Same(objectId, requirement.ObjectId);
        Assert.Equal("identify-healing-plants", requirement.RequiredCapabilityKey);
    }

    [Fact]
    public void EqualityUsesObjectAndRequiredCapabilityKeyValues()
    {
        DiscoveryCapabilityRequirement first = new(
            new ObjectId("concealed-healing-herb"),
            "identify-healing-plants");
        DiscoveryCapabilityRequirement same = new(
            new ObjectId("concealed-healing-herb"),
            "identify-healing-plants");
        DiscoveryCapabilityRequirement differentObject = new(
            new ObjectId("partial-inscription"),
            "identify-healing-plants");
        DiscoveryCapabilityRequirement differentCapability = new(
            new ObjectId("concealed-healing-herb"),
            "read-ancient-script");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentObject);
        Assert.NotEqual(first, differentCapability);
    }

    [Fact]
    public void AcceptsCanonicalCapabilityKeyBoundaries()
    {
        string maximumLengthKey = new('a', CanonicalIdentifierText.MaximumLength);

        DiscoveryCapabilityRequirement minimum = new(new ObjectId("hidden-residue"), "a");
        DiscoveryCapabilityRequirement maximum = new(
            new ObjectId("concealed-healing-herb"),
            maximumLengthKey);

        Assert.Equal("a", minimum.RequiredCapabilityKey);
        Assert.Equal(maximumLengthKey, maximum.RequiredCapabilityKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Identify-healing-plants")]
    [InlineData("identify_healing_plants")]
    [InlineData("identify--healing-plants")]
    [InlineData("identify-healing-plants-")]
    public void RejectsNonCanonicalCapabilityKeys(string requiredCapabilityKey)
    {
        Assert.Throws<ArgumentException>(
            () => new DiscoveryCapabilityRequirement(
                new ObjectId("concealed-healing-herb"),
                requiredCapabilityKey));
    }

    [Fact]
    public void RejectsOversizedCapabilityKey()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);

        Assert.Throws<ArgumentException>(
            () => new DiscoveryCapabilityRequirement(
                new ObjectId("concealed-healing-herb"),
                oversizedKey));
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryCapabilityRequirement(null!, "identify-healing-plants"));
        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryCapabilityRequirement(
                new ObjectId("concealed-healing-herb"),
                null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableRequirementSurface()
    {
        Type requirementType = typeof(DiscoveryCapabilityRequirement);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ObjectId", typeof(ObjectId)),
            ("RequiredCapabilityKey", typeof(string)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = requirementType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(requirementType.GetEvents());
        Assert.True(requirementType.IsSealed);
    }
}
