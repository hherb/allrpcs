using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnActivityStateTests
{
    private static readonly string[] ExpectedValues =
        ["awaiting-response", "speaking", "completed", "cancelled"];

    private static readonly string[] ExpectedPropertyNames =
        ["All", "AwaitingResponse", "Cancelled", "Completed", "Speaking", "Value"];

    [Fact]
    public void DefinesTheCompleteCanonicalStateSetInStableOrder()
    {
        Assert.Collection(
            CharacterTurnActivityState.All,
            state => Assert.Same(CharacterTurnActivityState.AwaitingResponse, state),
            state => Assert.Same(CharacterTurnActivityState.Speaking, state),
            state => Assert.Same(CharacterTurnActivityState.Completed, state),
            state => Assert.Same(CharacterTurnActivityState.Cancelled, state));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        Assert.Equal(ExpectedValues, CharacterTurnActivityState.All.Select(state => state.Value));

        foreach (CharacterTurnActivityState state in CharacterTurnActivityState.All)
        {
            Assert.Equal(state.Value, state.ToString());
            Assert.Same(state, CharacterTurnActivityState.Parse(state.ToString()));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Awaiting-response")]
    [InlineData("awaiting_response")]
    [InlineData("speaking ")]
    [InlineData("pending")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterTurnActivityState.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => CharacterTurnActivityState.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedStateSurface()
    {
        string[] publicProperties = typeof(CharacterTurnActivityState)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterTurnActivityState).GetConstructors());
    }
}
