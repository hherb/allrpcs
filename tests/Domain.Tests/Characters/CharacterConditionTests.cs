using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterConditionTests
{
    [Fact]
    public void PreservesCanonicalKeyAndAuthoredDescriptionExactly()
    {
        const string Description = "  Appears tired after the journey.\r\n";

        CharacterCondition condition = new("travel-weary", Description);

        Assert.Equal("travel-weary", condition.Key);
        Assert.Equal(Description, condition.Description);
    }

    [Fact]
    public void EqualityUsesCanonicalKeyAndAuthoredDescription()
    {
        CharacterCondition first = new("travel-weary", "Appears tired after the journey.");
        CharacterCondition same = new("travel-weary", "Appears tired after the journey.");
        CharacterCondition differentKey = new("well-rested", "Appears tired after the journey.");
        CharacterCondition differentDescription = new("travel-weary", "Moves cautiously.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentKey);
        Assert.NotEqual(first, differentDescription);
    }

    [Fact]
    public void AcceptsCanonicalKeyAtMaximumLength()
    {
        string key = new('a', CanonicalIdentifierText.MaximumLength);

        CharacterCondition condition = new(key, "Authored description");

        Assert.Equal(key, condition.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Travel-weary")]
    [InlineData("travel_weary")]
    [InlineData("travel--weary")]
    public void RejectsNonCanonicalKeys(string key)
    {
        Assert.Throws<ArgumentException>(() => new CharacterCondition(key, "Authored description"));
    }

    [Fact]
    public void PreservesDescriptionAtMaximumLength()
    {
        string description = new('c', CharacterCondition.MaximumDescriptionLength);

        CharacterCondition condition = new("travel-weary", description);

        Assert.Equal(description, condition.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDescriptions(string description)
    {
        Assert.Throws<ArgumentException>(() => new CharacterCondition("travel-weary", description));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);
        string oversizedDescription = new('c', CharacterCondition.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => new CharacterCondition(oversizedKey, "Authored description"));
        Assert.Throws<ArgumentException>(
            () => new CharacterCondition("travel-weary", oversizedDescription));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterCondition(null!, "Authored description"));
        Assert.Throws<ArgumentNullException>(() => new CharacterCondition("travel-weary", null!));
    }

    [Fact]
    public void RemainsDistinctFromCapabilitiesAndLimitations()
    {
        object condition = new CharacterCondition("travel-weary", "Authored description");
        object capability = new CharacterCapability("travel-weary", "Authored description");
        object limitation = new CharacterLimitation("travel-weary", "Authored description");

        Assert.NotEqual(condition, capability);
        Assert.NotEqual(condition, limitation);
    }

    [Fact]
    public void ExposesOnlyQualitativeAuthoredState()
    {
        string[] publicProperties = typeof(CharacterCondition)
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
