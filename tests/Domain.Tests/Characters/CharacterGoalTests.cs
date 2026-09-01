using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterGoalTests
{
    [Fact]
    public void PreservesCanonicalKeyAndAuthoredDescriptionExactly()
    {
        const string Description = "  Find safe passage through the woodland.\r\n";

        CharacterGoal goal = new("find-safe-passage", Description);

        Assert.Equal("find-safe-passage", goal.Key);
        Assert.Equal(Description, goal.Description);
    }

    [Fact]
    public void EqualityUsesCanonicalKeyAndAuthoredDescription()
    {
        CharacterGoal first = new("find-safe-passage", "Find safe passage through the woodland.");
        CharacterGoal same = new("find-safe-passage", "Find safe passage through the woodland.");
        CharacterGoal differentKey = new("reach-the-village", "Find safe passage through the woodland.");
        CharacterGoal differentDescription = new("find-safe-passage", "Reach the village safely.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentKey);
        Assert.NotEqual(first, differentDescription);
    }

    [Fact]
    public void AcceptsCanonicalKeyAtMaximumLength()
    {
        string key = new('a', CanonicalIdentifierText.MaximumLength);

        CharacterGoal goal = new(key, "Find safe passage.");

        Assert.Equal(key, goal.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Find-safe-passage")]
    [InlineData("find_safe_passage")]
    [InlineData("find--safe-passage")]
    public void RejectsNonCanonicalKeys(string key)
    {
        Assert.Throws<ArgumentException>(() => new CharacterGoal(key, "Find safe passage."));
    }

    [Fact]
    public void PreservesDescriptionAtMaximumLength()
    {
        string description = new('g', CharacterGoal.MaximumDescriptionLength);

        CharacterGoal goal = new("find-safe-passage", description);

        Assert.Equal(description, goal.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDescriptions(string description)
    {
        Assert.Throws<ArgumentException>(() => new CharacterGoal("find-safe-passage", description));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);
        string oversizedDescription = new('g', CharacterGoal.MaximumDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => new CharacterGoal(oversizedKey, "Find safe passage."));
        Assert.Throws<ArgumentException>(
            () => new CharacterGoal("find-safe-passage", oversizedDescription));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterGoal(null!, "Find safe passage."));
        Assert.Throws<ArgumentNullException>(() => new CharacterGoal("find-safe-passage", null!));
    }

    [Fact]
    public void ExposesOnlyAuthoredGoalState()
    {
        string[] publicProperties = typeof(CharacterGoal)
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
