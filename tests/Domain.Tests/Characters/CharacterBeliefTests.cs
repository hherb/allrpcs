using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefTests
{
    [Fact]
    public void PreservesCanonicalKeyAndAuthoredStatementExactly()
    {
        const string Statement = "  The moonwort blooms after rainfall.\r\n";

        CharacterBelief belief = new("moonwort-blooms-after-rain", Statement);

        Assert.Equal("moonwort-blooms-after-rain", belief.Key);
        Assert.Equal(Statement, belief.Statement);
    }

    [Fact]
    public void EqualityUsesCanonicalKeyAndAuthoredStatement()
    {
        CharacterBelief first = new("moonwort-is-cooling", "Moonwort cools a fever.");
        CharacterBelief same = new("moonwort-is-cooling", "Moonwort cools a fever.");
        CharacterBelief differentKey = new("moonwort-is-warming", "Moonwort cools a fever.");
        CharacterBelief differentStatement = new("moonwort-is-cooling", "Moonwort warms a fever.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentKey);
        Assert.NotEqual(first, differentStatement);
    }

    [Fact]
    public void AcceptsCanonicalKeyAtMaximumLength()
    {
        string key = new('a', CanonicalIdentifierText.MaximumLength);

        CharacterBelief belief = new(key, "Authored statement");

        Assert.Equal(key, belief.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Moonwort-is-cooling")]
    [InlineData("moonwort_is_cooling")]
    [InlineData("moonwort--is-cooling")]
    public void RejectsNonCanonicalKeys(string key)
    {
        Assert.Throws<ArgumentException>(() => new CharacterBelief(key, "Authored statement"));
    }

    [Fact]
    public void PreservesStatementAtMaximumLength()
    {
        string statement = new('b', CharacterBelief.MaximumStatementLength);

        CharacterBelief belief = new("moonwort-is-cooling", statement);

        Assert.Equal(statement, belief.Statement);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyStatements(string statement)
    {
        Assert.Throws<ArgumentException>(() => new CharacterBelief("moonwort-is-cooling", statement));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);
        string oversizedStatement = new('b', CharacterBelief.MaximumStatementLength + 1);

        Assert.Throws<ArgumentException>(
            () => new CharacterBelief(oversizedKey, "Authored statement"));
        Assert.Throws<ArgumentException>(
            () => new CharacterBelief("moonwort-is-cooling", oversizedStatement));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterBelief(null!, "Authored statement"));
        Assert.Throws<ArgumentNullException>(() => new CharacterBelief("moonwort-is-cooling", null!));
    }

    [Fact]
    public void ExposesOnlyAuthoredBeliefState()
    {
        string[] publicProperties = typeof(CharacterBelief)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            publicProperties,
            property => Assert.Equal("Key", property),
            property => Assert.Equal("Statement", property));
    }
}
