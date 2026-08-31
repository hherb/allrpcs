using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterLimitationTests
{
    [Fact]
    public void PreservesCanonicalKeyAndAuthoredDescriptionExactly()
    {
        const string Description = "  Cannot cross deep water unaided.\n";

        CharacterLimitation limitation = new("deep-water", Description);

        Assert.Equal("deep-water", limitation.Key);
        Assert.Equal(Description, limitation.Description);
    }

    [Fact]
    public void EqualityUsesCanonicalKeyAndAuthoredDescription()
    {
        CharacterLimitation first = new("poor-vision", "Cannot identify distant details.");
        CharacterLimitation same = new("poor-vision", "Cannot identify distant details.");
        CharacterLimitation differentKey = new("poor-hearing", "Cannot identify distant details.");
        CharacterLimitation differentDescription = new("poor-vision", "Needs spectacles.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentKey);
        Assert.NotEqual(first, differentDescription);
    }

    [Fact]
    public void AcceptsCanonicalKeyAtMaximumLength()
    {
        string key = new('a', CanonicalIdentifierText.MaximumLength);

        CharacterLimitation limitation = new(key, "Authored description");

        Assert.Equal(key, limitation.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Poor-vision")]
    [InlineData("poor_vision")]
    [InlineData("poor--vision")]
    public void RejectsNonCanonicalKeys(string key)
    {
        Assert.Throws<ArgumentException>(
            () => new CharacterLimitation(key, "Authored description"));
    }

    [Fact]
    public void PreservesDescriptionAtMaximumLength()
    {
        string description = new('l', CharacterLimitation.MaximumDescriptionLength);

        CharacterLimitation limitation = new("poor-vision", description);

        Assert.Equal(description, limitation.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDescriptions(string description)
    {
        Assert.Throws<ArgumentException>(
            () => new CharacterLimitation("poor-vision", description));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);
        string oversizedDescription = new('l', CharacterLimitation.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => new CharacterLimitation(oversizedKey, "Authored description"));
        Assert.Throws<ArgumentException>(
            () => new CharacterLimitation("poor-vision", oversizedDescription));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterLimitation(null!, "Authored description"));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterLimitation("poor-vision", null!));
    }

    [Fact]
    public void CapabilityAndLimitationRemainDistinctValueTypes()
    {
        object capability = new CharacterCapability("field-medicine", "Authored description");
        object limitation = new CharacterLimitation("field-medicine", "Authored description");

        Assert.NotEqual(capability, limitation);
    }

    [Fact]
    public void ExposesNoVisibilityOrExecutablePolicySurface()
    {
        string[] publicProperties = typeof(CharacterLimitation)
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
