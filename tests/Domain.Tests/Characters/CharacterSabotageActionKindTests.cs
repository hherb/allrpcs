using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageActionKindTests
{
    private static readonly string[] ExpectedValues =
        ["conceal", "move", "substitute", "contaminate", "misquote", "delay", "influence"];

    private static readonly string[] ExpectedPropertyNames =
    [
        "All",
        "Conceal",
        "Contaminate",
        "Delay",
        "Influence",
        "Misquote",
        "Move",
        "Substitute",
        "Value",
    ];

    [Fact]
    public void DefinesTheClosedSabotageKindsInStableRoadmapOrder()
    {
        Assert.Collection(
            CharacterSabotageActionKind.All,
            kind => Assert.Same(CharacterSabotageActionKind.Conceal, kind),
            kind => Assert.Same(CharacterSabotageActionKind.Move, kind),
            kind => Assert.Same(CharacterSabotageActionKind.Substitute, kind),
            kind => Assert.Same(CharacterSabotageActionKind.Contaminate, kind),
            kind => Assert.Same(CharacterSabotageActionKind.Misquote, kind),
            kind => Assert.Same(CharacterSabotageActionKind.Delay, kind),
            kind => Assert.Same(CharacterSabotageActionKind.Influence, kind));
    }

    [Fact]
    public void CanonicalTextRoundTripsToExactDefinedInstances()
    {
        Assert.Equal(ExpectedValues, CharacterSabotageActionKind.All.Select(kind => kind.Value));

        foreach (CharacterSabotageActionKind kind in CharacterSabotageActionKind.All)
        {
            Assert.Equal(kind.Value, kind.ToString());
            Assert.Same(kind, CharacterSabotageActionKind.Parse(kind.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Conceal")]
    [InlineData("move-object")]
    [InlineData("mis_quote")]
    [InlineData("influence ")]
    [InlineData("examine")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterSabotageActionKind.Parse(value));
    }

    [Fact]
    public void RejectsNullAndExposesOnlyTheClosedKindSurface()
    {
        Assert.Throws<ArgumentNullException>(() => CharacterSabotageActionKind.Parse(null!));

        string[] publicProperties = typeof(CharacterSabotageActionKind)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterSabotageActionKind).GetConstructors());
    }
}
