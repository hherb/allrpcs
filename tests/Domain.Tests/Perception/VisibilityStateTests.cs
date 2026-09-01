using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Perception;

public sealed class VisibilityStateTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalVisibilityStateSetInRoadmapOrder()
    {
        Assert.Collection(
            VisibilityState.All,
            state => Assert.Equal("unnoticed", state.Value),
            state => Assert.Equal("noticed", state.Value),
            state => Assert.Equal("identified", state.Value),
            state => Assert.Equal("understood", state.Value),
            state => Assert.Equal("altered", state.Value),
            state => Assert.Equal("collected", state.Value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (VisibilityState state in VisibilityState.All)
        {
            Assert.Equal(state.Value, state.ToString());
            Assert.Same(state, VisibilityState.Parse(state.ToString()));
        }
    }

    [Fact]
    public void EqualityDistinguishesVisibilityStates()
    {
        Assert.Equal(VisibilityState.Unnoticed, VisibilityState.Parse("unnoticed"));
        Assert.True(VisibilityState.Unnoticed == VisibilityState.Parse("unnoticed"));
        Assert.NotEqual(VisibilityState.Unnoticed, VisibilityState.Noticed);
        Assert.True(VisibilityState.Unnoticed != VisibilityState.Noticed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Unnoticed")]
    [InlineData("noticed ")]
    [InlineData("partially-identified")]
    [InlineData("owned")]
    [InlineData("altered-or-collected")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => VisibilityState.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => VisibilityState.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedVisibilityValueSurface()
    {
        string[] publicProperties = typeof(VisibilityState)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            publicProperties,
            property => Assert.Equal("All", property),
            property => Assert.Equal("Altered", property),
            property => Assert.Equal("Collected", property),
            property => Assert.Equal("Identified", property),
            property => Assert.Equal("Noticed", property),
            property => Assert.Equal("Understood", property),
            property => Assert.Equal("Unnoticed", property),
            property => Assert.Equal("Value", property));
        Assert.Empty(typeof(VisibilityState).GetConstructors());
    }
}
