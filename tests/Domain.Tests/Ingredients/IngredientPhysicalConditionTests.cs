using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPhysicalConditionTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalConditionSet()
    {
        Assert.Collection(
            IngredientPhysicalCondition.All,
            value => Assert.Same(IngredientPhysicalCondition.Intact, value),
            value => Assert.Same(IngredientPhysicalCondition.Damaged, value),
            value => Assert.Same(IngredientPhysicalCondition.Ruined, value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (IngredientPhysicalCondition value in IngredientPhysicalCondition.All)
        {
            Assert.Equal(value.Value, value.ToString());
            Assert.Same(value, IngredientPhysicalCondition.Parse(value.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Intact")]
    [InlineData("damaged ")]
    [InlineData("ground")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => IngredientPhysicalCondition.Parse(value));
    }

    [Fact]
    public void RejectsNullTextAndExposesOnlyTheClosedSurface()
    {
        Assert.Throws<ArgumentNullException>(() => IngredientPhysicalCondition.Parse(null!));
        Assert.Equal(
            ["All", "Damaged", "Intact", "Ruined", "Value"],
            typeof(IngredientPhysicalCondition)
                .GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.Empty(typeof(IngredientPhysicalCondition).GetConstructors());
    }
}
