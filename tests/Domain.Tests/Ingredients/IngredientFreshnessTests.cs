using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientFreshnessTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalFreshnessSet()
    {
        Assert.Collection(
            IngredientFreshness.All,
            value => Assert.Same(IngredientFreshness.Fresh, value),
            value => Assert.Same(IngredientFreshness.Aging, value),
            value => Assert.Same(IngredientFreshness.Spoiled, value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (IngredientFreshness value in IngredientFreshness.All)
        {
            Assert.Equal(value.Value, value.ToString());
            Assert.Same(value, IngredientFreshness.Parse(value.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Fresh")]
    [InlineData("fresh ")]
    [InlineData("stale")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => IngredientFreshness.Parse(value));
    }

    [Fact]
    public void RejectsNullTextAndExposesOnlyTheClosedSurface()
    {
        Assert.Throws<ArgumentNullException>(() => IngredientFreshness.Parse(null!));
        AssertClosedSurface(
            typeof(IngredientFreshness),
            ["Aging", "All", "Fresh", "Spoiled", "Value"]);
    }

    private static void AssertClosedSurface(Type type, string[] expectedProperties)
    {
        Assert.Equal(
            expectedProperties,
            type.GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.Empty(type.GetConstructors());
    }
}
