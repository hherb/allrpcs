using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientContaminationTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalContaminationSet()
    {
        Assert.Collection(
            IngredientContamination.All,
            value => Assert.Same(IngredientContamination.Uncontaminated, value),
            value => Assert.Same(IngredientContamination.Suspected, value),
            value => Assert.Same(IngredientContamination.Contaminated, value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (IngredientContamination value in IngredientContamination.All)
        {
            Assert.Equal(value.Value, value.ToString());
            Assert.Same(value, IngredientContamination.Parse(value.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Uncontaminated")]
    [InlineData("suspected ")]
    [InlineData("clean")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => IngredientContamination.Parse(value));
    }

    [Fact]
    public void RejectsNullTextAndExposesOnlyTheClosedSurface()
    {
        Assert.Throws<ArgumentNullException>(() => IngredientContamination.Parse(null!));
        Assert.Equal(
            ["All", "Contaminated", "Suspected", "Uncontaminated", "Value"],
            typeof(IngredientContamination)
                .GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.Empty(typeof(IngredientContamination).GetConstructors());
    }
}
