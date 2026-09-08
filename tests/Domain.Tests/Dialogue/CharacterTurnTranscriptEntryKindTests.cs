using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnTranscriptEntryKindTests
{
    private static readonly string[] ExpectedValues = ["spoken-statement", "physical-action"];

    private static readonly string[] ExpectedPropertyNames =
        ["All", "PhysicalAction", "SpokenStatement", "Value"];

    [Fact]
    public void DefinesTheCompleteCanonicalKindSetInStableOrder()
    {
        Assert.Collection(
            CharacterTurnTranscriptEntryKind.All,
            kind => Assert.Same(CharacterTurnTranscriptEntryKind.SpokenStatement, kind),
            kind => Assert.Same(CharacterTurnTranscriptEntryKind.PhysicalAction, kind));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        Assert.Equal(
            ExpectedValues,
            CharacterTurnTranscriptEntryKind.All.Select(kind => kind.Value));

        foreach (CharacterTurnTranscriptEntryKind kind in CharacterTurnTranscriptEntryKind.All)
        {
            Assert.Equal(kind.Value, kind.ToString());
            Assert.Same(kind, CharacterTurnTranscriptEntryKind.Parse(kind.ToString()));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Spoken-statement")]
    [InlineData("spoken_statement")]
    [InlineData("physical-action ")]
    [InlineData("speech")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterTurnTranscriptEntryKind.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnTranscriptEntryKind.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedKindSurface()
    {
        string[] publicProperties = typeof(CharacterTurnTranscriptEntryKind)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterTurnTranscriptEntryKind).GetConstructors());
    }
}
