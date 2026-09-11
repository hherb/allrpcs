using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientQuantityUnitTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalUnitSet()
    {
        Assert.Collection(
            IngredientQuantityUnit.All,
            unit => Assert.Same(IngredientQuantityUnit.Count, unit),
            unit => Assert.Same(IngredientQuantityUnit.Milligram, unit),
            unit => Assert.Same(IngredientQuantityUnit.Millilitre, unit));
        Assert.Equal(["count", "milligram", "millilitre"], IngredientQuantityUnit.All.Select(unit => unit.Value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (IngredientQuantityUnit unit in IngredientQuantityUnit.All)
        {
            Assert.Equal(unit.Value, unit.ToString());
            Assert.Same(unit, IngredientQuantityUnit.Parse(unit.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Count")]
    [InlineData("milligrams")]
    [InlineData("millilitre ")]
    [InlineData("gram")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => IngredientQuantityUnit.Parse(value));
    }

    [Fact]
    public void RejectsNullTextAndExposesOnlyTheClosedSurface()
    {
        Assert.Throws<ArgumentNullException>(() => IngredientQuantityUnit.Parse(null!));

        string[] publicProperties = typeof(IngredientQuantityUnit)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["All", "Count", "Milligram", "Millilitre", "Value"], publicProperties);
        Assert.Empty(typeof(IngredientQuantityUnit).GetConstructors());
    }
}
