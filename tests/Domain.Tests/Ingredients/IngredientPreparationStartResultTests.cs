using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationStartResultTests
{
    [Fact]
    public void RetainsExactActivityAndRemainingIngredients()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity activity = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, 0));
        IngredientStateSnapshot ingredients = new([]);

        IngredientPreparationStartResult result = new(activity, ingredients);

        Assert.Same(activity, result.Activity);
        Assert.Same(ingredients, result.Ingredients);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableProperties()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity activity = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, 0));
        IngredientStateSnapshot ingredients = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationStartResult(null!, ingredients));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationStartResult(activity, null!));

        Type resultType = typeof(IngredientPreparationStartResult);
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());

        Assert.Equal(["Activity", "Ingredients"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(IngredientPreparationActivity), typeof(IngredientStateSnapshot)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }
}
