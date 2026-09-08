using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterStateProposalTests
{
    [Fact]
    public void PreservesCanonicalKeyAndExactBoundedValue()
    {
        const string Value = "  Moonwort may be safe after inspection.\r\n";

        CharacterStateProposal proposal = new("moonwort-safety-belief", Value);

        Assert.Equal("moonwort-safety-belief", proposal.Key);
        Assert.Equal(Value, proposal.Value);
    }

    [Fact]
    public void EqualityUsesKeyAndExactValue()
    {
        CharacterStateProposal first = new("moonwort-safety-belief", "Possibly safe.");
        CharacterStateProposal same = new("moonwort-safety-belief", "Possibly safe.");
        CharacterStateProposal differentKey = new("moonwort-goal", "Possibly safe.");
        CharacterStateProposal differentValue = new(
            "moonwort-safety-belief",
            "Unknown safety.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentKey);
        Assert.NotEqual(first, differentValue);
    }

    [Fact]
    public void AcceptsCanonicalKeyAndValueAtTheirMaximumLengths()
    {
        string key = new('a', CanonicalIdentifierText.MaximumLength);
        string value = new('v', CharacterStateProposal.MaximumValueLength);

        CharacterStateProposal proposal = new(key, value);

        Assert.Equal(key, proposal.Key);
        Assert.Equal(value, proposal.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Moonwort-safety")]
    [InlineData("moonwort_safety")]
    [InlineData("moonwort--safety")]
    public void RejectsNonCanonicalKeys(string key)
    {
        Assert.Throws<ArgumentException>(() => new CharacterStateProposal(key, "Possibly safe."));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new CharacterStateProposal("belief", value));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedKey = new('a', CanonicalIdentifierText.MaximumLength + 1);
        string oversizedValue = new('v', CharacterStateProposal.MaximumValueLength + 1);

        Assert.Throws<ArgumentException>(() => new CharacterStateProposal(
            oversizedKey,
            "Possibly safe."));
        Assert.Throws<ArgumentException>(() => new CharacterStateProposal(
            "belief",
            oversizedValue));
        Assert.Throws<ArgumentNullException>(() => new CharacterStateProposal(
            null!,
            "Possibly safe."));
        Assert.Throws<ArgumentNullException>(() => new CharacterStateProposal(
            "belief",
            null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProposalSurface()
    {
        string[] publicProperties = typeof(CharacterStateProposal)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["Key", "Value"], publicProperties);
        Assert.Empty(typeof(CharacterStateProposal).GetEvents());
        Assert.True(typeof(CharacterStateProposal).IsSealed);
    }
}
