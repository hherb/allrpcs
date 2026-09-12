using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationKindTests
{
    private static readonly string[] ExpectedValues =
        ["respond", "interject", "remain-silent"];

    private static readonly string[] ExpectedPropertyNames =
        ["All", "Interject", "RemainSilent", "Respond", "Value"];

    [Fact]
    public void DefinesTheCompleteCanonicalParticipationSetInStableOrder()
    {
        Assert.Collection(
            CharacterTurnParticipationKind.All,
            kind => Assert.Same(CharacterTurnParticipationKind.Respond, kind),
            kind => Assert.Same(CharacterTurnParticipationKind.Interject, kind),
            kind => Assert.Same(CharacterTurnParticipationKind.RemainSilent, kind));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        Assert.Equal(
            ExpectedValues,
            CharacterTurnParticipationKind.All.Select(kind => kind.Value));

        foreach (CharacterTurnParticipationKind kind in CharacterTurnParticipationKind.All)
        {
            Assert.Equal(kind.Value, kind.ToString());
            Assert.Same(kind, CharacterTurnParticipationKind.Parse(kind.ToString()));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Respond")]
    [InlineData("remain_silent")]
    [InlineData("remain--silent")]
    [InlineData("interject ")]
    [InlineData("ignore")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterTurnParticipationKind.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationKind.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedParticipationSurface()
    {
        string[] publicProperties = typeof(CharacterTurnParticipationKind)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterTurnParticipationKind).GetConstructors());
    }
}
