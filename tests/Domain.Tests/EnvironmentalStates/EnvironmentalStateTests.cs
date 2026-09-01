using Allrpcs.Domain.Characters;
using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.EnvironmentalStates;

public sealed class EnvironmentalStateTests
{
    [Fact]
    public void PreservesCanonicalKeyAndAuthoredDescriptionExactly()
    {
        const string Description = "  Rain-softened ground near the stream.\r\n湿った土\n";

        EnvironmentalState state = new("rain-softened-ground", Description);

        Assert.Equal("rain-softened-ground", state.Key);
        Assert.Equal(Description, state.Description);
    }

    [Fact]
    public void EqualityUsesCanonicalKeyAndAuthoredDescription()
    {
        EnvironmentalState first = new("heavy-rain", "Rain falls steadily.");
        EnvironmentalState same = new("heavy-rain", "Rain falls steadily.");
        EnvironmentalState differentKey = new("light-rain", "Rain falls steadily.");
        EnvironmentalState differentDescription = new("heavy-rain", "Rain has stopped.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentKey);
        Assert.NotEqual(first, differentDescription);
    }

    [Fact]
    public void AcceptsCanonicalKeyAtMaximumLength()
    {
        string key = new('a', CanonicalIdentifierText.MaximumLength);

        EnvironmentalState state = new(key, "Authored description");

        Assert.Equal(key, state.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Heavy-rain")]
    [InlineData("heavy_rain")]
    [InlineData("heavy--rain")]
    public void RejectsNonCanonicalKeys(string key)
    {
        Assert.Throws<ArgumentException>(() => new EnvironmentalState(key, "Authored description"));
    }

    [Fact]
    public void PreservesDescriptionAtMaximumLength()
    {
        string description = new('e', EnvironmentalState.MaximumDescriptionLength);

        EnvironmentalState state = new("heavy-rain", description);

        Assert.Equal(description, state.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDescriptions(string description)
    {
        Assert.Throws<ArgumentException>(() => new EnvironmentalState("heavy-rain", description));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);
        string oversizedDescription = new('e', EnvironmentalState.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => new EnvironmentalState(oversizedKey, "Authored description"));
        Assert.Throws<ArgumentException>(
            () => new EnvironmentalState("heavy-rain", oversizedDescription));
        Assert.Throws<ArgumentNullException>(
            () => new EnvironmentalState(null!, "Authored description"));
        Assert.Throws<ArgumentNullException>(() => new EnvironmentalState("heavy-rain", null!));
    }

    [Fact]
    public void RemainsStructurallyDistinctFromCharacterConditions()
    {
        object state = new EnvironmentalState("heavy-rain", "Rain falls steadily.");
        object condition = new CharacterCondition("heavy-rain", "Rain falls steadily.");

        Assert.NotEqual(state, condition);
        Assert.NotEqual(typeof(CharacterCondition), state.GetType());
    }

    [Fact]
    public void ExposesOnlyQualitativeAuthoredState()
    {
        string[] publicProperties = typeof(EnvironmentalState)
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
