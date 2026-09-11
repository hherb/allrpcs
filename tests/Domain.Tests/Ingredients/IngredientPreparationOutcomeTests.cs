using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationOutcomeTests
{
    [Fact]
    public void RetainsCompletedActivityAndItsExactAuthoredResult()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity activity = Complete(recipe);

        IngredientPreparationOutcome outcome = new(
            activity,
            recipe.ResultDefinition,
            recipe.ResultQuantity);

        Assert.Same(activity, outcome.Activity);
        Assert.Same(recipe.ResultDefinition, outcome.Definition);
        Assert.Same(recipe.ResultQuantity, outcome.Quantity);
    }

    [Fact]
    public void RejectsIncompleteMismatchedAndNullValues()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity complete = Complete(recipe);
        IngredientPreparationActivity incomplete = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, 0));
        IngredientDefinition replacementDefinition = new(
            new IngredientId(recipe.ResultDefinition.IngredientId.Value));
        IngredientQuantity replacementQuantity = new(
            recipe.ResultQuantity.Amount,
            recipe.ResultQuantity.Unit);

        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationOutcome(
                null!,
                recipe.ResultDefinition,
                recipe.ResultQuantity));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationOutcome(complete, null!, recipe.ResultQuantity));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationOutcome(complete, recipe.ResultDefinition, null!));
        Assert.Throws<ArgumentException>(() =>
            new IngredientPreparationOutcome(
                incomplete,
                recipe.ResultDefinition,
                recipe.ResultQuantity));
        Assert.Throws<ArgumentException>(() =>
            new IngredientPreparationOutcome(complete, replacementDefinition, recipe.ResultQuantity));
        Assert.Throws<ArgumentException>(() =>
            new IngredientPreparationOutcome(complete, recipe.ResultDefinition, replacementQuantity));
    }

    [Fact]
    public void ExposesOnlyTheImmutableOutcomeSurface()
    {
        Type outcomeType = typeof(IngredientPreparationOutcome);
        System.Reflection.PropertyInfo[] properties = outcomeType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(outcomeType.GetConstructors());

        Assert.Equal(
            ["Activity", "Definition", "Quantity"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(IngredientPreparationActivity),
                typeof(IngredientDefinition),
                typeof(IngredientQuantity),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(outcomeType.GetEvents());
        Assert.Empty(outcomeType.GetMethods().Where(
            method => method.DeclaringType == outcomeType && !method.IsSpecialName));
        Assert.True(outcomeType.IsSealed);
    }

    internal static IngredientPreparationActivity Complete(IngredientPreparationRecipe recipe) =>
        new(recipe, new IngredientPreparationProgress(recipe.Plan, recipe.Plan.Steps.Count));
}
