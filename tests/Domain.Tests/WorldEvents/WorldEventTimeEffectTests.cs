using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventTimeEffectTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalTimeEffectSetInStableOrder()
    {
        Assert.Collection(
            WorldEventTimeEffect.All,
            effect => Assert.Equal("no-advance", effect.Value),
            effect => Assert.Equal("advance", effect.Value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (WorldEventTimeEffect effect in WorldEventTimeEffect.All)
        {
            Assert.Equal(effect.Value, effect.ToString());
            Assert.Same(effect, WorldEventTimeEffect.Parse(effect.ToString()));
        }
    }

    [Fact]
    public void EqualityDistinguishesTimeEffectsByExactDefinedInstance()
    {
        Assert.Equal(
            WorldEventTimeEffect.NoAdvance,
            WorldEventTimeEffect.Parse("no-advance"));
        Assert.True(
            WorldEventTimeEffect.NoAdvance == WorldEventTimeEffect.Parse("no-advance"));
        Assert.NotEqual(WorldEventTimeEffect.NoAdvance, WorldEventTimeEffect.Advance);
        Assert.True(WorldEventTimeEffect.NoAdvance != WorldEventTimeEffect.Advance);
    }

    [Theory]
    [InlineData("")]
    [InlineData("NoAdvance")]
    [InlineData("no_advance")]
    [InlineData("advance ")]
    [InlineData("pause")]
    [InlineData("duration")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => WorldEventTimeEffect.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => WorldEventTimeEffect.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedTimeEffectSurface()
    {
        string[] publicProperties = typeof(WorldEventTimeEffect)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            publicProperties,
            property => Assert.Equal("Advance", property),
            property => Assert.Equal("All", property),
            property => Assert.Equal("NoAdvance", property),
            property => Assert.Equal("Value", property));
        Assert.Empty(typeof(WorldEventTimeEffect).GetConstructors());
    }
}
