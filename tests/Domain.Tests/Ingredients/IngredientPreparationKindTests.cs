using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationKindTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalPreparationSet()
    {
        Assert.Collection(
            IngredientPreparationKind.All,
            value => Assert.Same(IngredientPreparationKind.Grinding, value),
            value => Assert.Same(IngredientPreparationKind.Heating, value),
            value => Assert.Same(IngredientPreparationKind.Steeping, value),
            value => Assert.Same(IngredientPreparationKind.Cooling, value),
            value => Assert.Same(IngredientPreparationKind.Fermentation, value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (IngredientPreparationKind value in IngredientPreparationKind.All)
        {
            Assert.Equal(value.Value, value.ToString());
            Assert.Same(value, IngredientPreparationKind.Parse(value.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Grinding")]
    [InlineData("steeping ")]
    [InlineData("mixing")]
    [InlineData("administering")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => IngredientPreparationKind.Parse(value));
    }

    [Fact]
    public void RejectsNullTextAndExposesOnlyTheClosedSurface()
    {
        Assert.Throws<ArgumentNullException>(() => IngredientPreparationKind.Parse(null!));
        Assert.Equal(
            ["All", "Cooling", "Fermentation", "Grinding", "Heating", "Steeping", "Value"],
            typeof(IngredientPreparationKind)
                .GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.Empty(typeof(IngredientPreparationKind).GetConstructors());
    }
}
