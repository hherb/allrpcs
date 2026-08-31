using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterCapabilityTests
{
    [Fact]
    public void PreservesCanonicalKeyAndAuthoredDescriptionExactly()
    {
        const string Description = "  Can identify common healing plants.\r\n";

        CharacterCapability capability = new("identify-healing-plants", Description);

        Assert.Equal("identify-healing-plants", capability.Key);
        Assert.Equal(Description, capability.Description);
    }

    [Fact]
    public void EqualityUsesCanonicalKeyAndAuthoredDescription()
    {
        CharacterCapability first = new("field-medicine", "Can stabilize minor wounds.");
        CharacterCapability same = new("field-medicine", "Can stabilize minor wounds.");
        CharacterCapability differentKey = new("herbalism", "Can stabilize minor wounds.");
        CharacterCapability differentDescription = new("field-medicine", "Can assess wounds.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentKey);
        Assert.NotEqual(first, differentDescription);
    }

    [Fact]
    public void AcceptsCanonicalKeyAtMaximumLength()
    {
        string key = new('a', CanonicalIdentifierText.MaximumLength);

        CharacterCapability capability = new(key, "Authored description");

        Assert.Equal(key, capability.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Field-medicine")]
    [InlineData("field_medicine")]
    [InlineData("field--medicine")]
    public void RejectsNonCanonicalKeys(string key)
    {
        Assert.Throws<ArgumentException>(
            () => new CharacterCapability(key, "Authored description"));
    }

    [Fact]
    public void PreservesDescriptionAtMaximumLength()
    {
        string description = new('c', CharacterCapability.MaximumDescriptionLength);

        CharacterCapability capability = new("herbalism", description);

        Assert.Equal(description, capability.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDescriptions(string description)
    {
        Assert.Throws<ArgumentException>(() => new CharacterCapability("herbalism", description));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);
        string oversizedDescription = new('c', CharacterCapability.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => new CharacterCapability(oversizedKey, "Authored description"));
        Assert.Throws<ArgumentException>(
            () => new CharacterCapability("herbalism", oversizedDescription));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterCapability(null!, "Authored description"));
        Assert.Throws<ArgumentNullException>(() => new CharacterCapability("herbalism", null!));
    }

    [Fact]
    public void ExposesNoVisibilityOrExecutablePolicySurface()
    {
        string[] publicProperties = typeof(CharacterCapability)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            publicProperties,
            property => Assert.Equal("Description", property),
            property => Assert.Equal("Key", property));
    }
}
