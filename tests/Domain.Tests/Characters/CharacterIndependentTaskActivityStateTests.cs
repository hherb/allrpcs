using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskActivityStateTests
{
    private static readonly string[] ExpectedValues =
        ["assigned", "active", "completed", "cancelled"];

    private static readonly string[] ExpectedPropertyNames =
        ["Active", "All", "Assigned", "Cancelled", "Completed", "Value"];

    [Fact]
    public void DefinesTheCompleteCanonicalStateSetInLifecycleOrder()
    {
        Assert.Collection(
            CharacterIndependentTaskActivityState.All,
            state => Assert.Same(CharacterIndependentTaskActivityState.Assigned, state),
            state => Assert.Same(CharacterIndependentTaskActivityState.Active, state),
            state => Assert.Same(CharacterIndependentTaskActivityState.Completed, state),
            state => Assert.Same(CharacterIndependentTaskActivityState.Cancelled, state));
    }

    [Fact]
    public void CanonicalTextRoundTripsToDefinedInstances()
    {
        Assert.Equal(
            ExpectedValues,
            CharacterIndependentTaskActivityState.All.Select(state => state.Value));

        foreach (CharacterIndependentTaskActivityState state in
            CharacterIndependentTaskActivityState.All)
        {
            Assert.Equal(state.Value, state.ToString());
            Assert.Same(state, CharacterIndependentTaskActivityState.Parse(state.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Assigned")]
    [InlineData("in-progress")]
    [InlineData("active ")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() =>
            CharacterIndependentTaskActivityState.Parse(value));
    }

    [Fact]
    public void RejectsNullAndExposesOnlyTheClosedStateSurface()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskActivityState.Parse(null!));
        Assert.Equal(
            ExpectedPropertyNames,
            typeof(CharacterIndependentTaskActivityState)
                .GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.Empty(typeof(CharacterIndependentTaskActivityState).GetConstructors());
    }
}
