using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnDispositionTests
{
    private static readonly string[] ExpectedValues =
        ["accept", "refuse", "clarify", "conditionally-accept"];

    private static readonly string[] ExpectedPropertyNames =
        ["Accept", "All", "Clarify", "ConditionallyAccept", "Refuse", "Value"];

    [Fact]
    public void DefinesTheCompleteCanonicalDispositionSetInStableOrder()
    {
        Assert.Collection(
            CharacterTurnDisposition.All,
            disposition => Assert.Same(CharacterTurnDisposition.Accept, disposition),
            disposition => Assert.Same(CharacterTurnDisposition.Refuse, disposition),
            disposition => Assert.Same(CharacterTurnDisposition.Clarify, disposition),
            disposition => Assert.Same(
                CharacterTurnDisposition.ConditionallyAccept,
                disposition));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        Assert.Equal(ExpectedValues, CharacterTurnDisposition.All.Select(type => type.Value));

        foreach (CharacterTurnDisposition disposition in CharacterTurnDisposition.All)
        {
            Assert.Equal(disposition.Value, disposition.ToString());
            Assert.Same(
                disposition,
                CharacterTurnDisposition.Parse(disposition.ToString()));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Accept")]
    [InlineData("conditionally_accept")]
    [InlineData("conditionally--accept")]
    [InlineData("clarify ")]
    [InlineData("reject")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterTurnDisposition.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => CharacterTurnDisposition.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedDispositionSurface()
    {
        string[] publicProperties = typeof(CharacterTurnDisposition)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterTurnDisposition).GetConstructors());
    }
}
