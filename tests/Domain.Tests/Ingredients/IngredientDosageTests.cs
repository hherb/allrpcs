using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientDosageTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void RetainsExactPreparedOutcomeAndBoundedExactUnitQuantity(int amount)
    {
        IngredientPreparationOutcome preparation = Outcome(4, IngredientQuantityUnit.Millilitre);
        IngredientQuantity quantity = new(amount, IngredientQuantityUnit.Millilitre);

        IngredientDosage dosage = new(preparation, quantity);

        Assert.Same(preparation, dosage.Preparation);
        Assert.Same(quantity, dosage.Quantity);
    }

    [Fact]
    public void RejectsExcessiveAmountAndDifferentUnitWithoutConversion()
    {
        IngredientPreparationOutcome preparation = Outcome(4, IngredientQuantityUnit.Millilitre);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IngredientDosage(
                preparation,
                new IngredientQuantity(5, IngredientQuantityUnit.Millilitre)));
        Assert.Throws<ArgumentException>(() =>
            new IngredientDosage(
                preparation,
                new IngredientQuantity(4, IngredientQuantityUnit.Milligram)));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableDosageSurface()
    {
        IngredientPreparationOutcome preparation = Outcome(4, IngredientQuantityUnit.Millilitre);
        IngredientQuantity quantity = new(1, IngredientQuantityUnit.Millilitre);

        Assert.Throws<ArgumentNullException>(() => new IngredientDosage(null!, quantity));
        Assert.Throws<ArgumentNullException>(() => new IngredientDosage(preparation, null!));

        Type dosageType = typeof(IngredientDosage);
        System.Reflection.PropertyInfo[] properties = dosageType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(dosageType.GetConstructors());

        Assert.Equal(["Preparation", "Quantity"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(IngredientPreparationOutcome), typeof(IngredientQuantity)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(dosageType.GetEvents());
        Assert.Empty(dosageType.GetMethods().Where(
            method => method.DeclaringType == dosageType && !method.IsSpecialName));
        Assert.True(dosageType.IsSealed);
    }

    internal static IngredientPreparationOutcome Outcome(
        int resultAmount,
        IngredientQuantityUnit unit)
    {
        IngredientPreparationRecipe source = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationRecipe recipe = new(
            source.RecipeId,
            source.Plan,
            source.Inputs,
            source.ResultDefinition,
            new IngredientQuantity(resultAmount, unit));
        return IngredientPreparationCompletionTransition.Complete(
            IngredientPreparationOutcomeTests.Complete(recipe));
    }
}
