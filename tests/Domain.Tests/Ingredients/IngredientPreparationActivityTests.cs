using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationActivityTests
{
    [Fact]
    public void RetainsExactRecipeAndProgressForItsExactPlan()
    {
        IngredientPreparationRecipe recipe = Recipe();
        IngredientPreparationProgress progress = new(recipe.Plan, 1);

        IngredientPreparationActivity activity = new(recipe, progress);

        Assert.Same(recipe, activity.Recipe);
        Assert.Same(progress, activity.Progress);
    }

    [Fact]
    public void RejectsNullsAndProgressForAnotherPlan()
    {
        IngredientPreparationRecipe recipe = Recipe();
        IngredientPreparationProgress progress = new(recipe.Plan, 0);
        IngredientPreparationProgress otherProgress = new(
            new IngredientPreparationPlan(
                [new IngredientPreparationStep(IngredientPreparationKind.Grinding)]),
            0);

        Assert.Throws<ArgumentNullException>(() => new IngredientPreparationActivity(null!, progress));
        Assert.Throws<ArgumentNullException>(() => new IngredientPreparationActivity(recipe, null!));
        Assert.Throws<ArgumentException>(() => new IngredientPreparationActivity(recipe, otherProgress));
    }

    [Fact]
    public void ExposesOnlyTheImmutableActivitySurface()
    {
        Type activityType = typeof(IngredientPreparationActivity);
        System.Reflection.PropertyInfo[] properties = activityType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(activityType.GetConstructors());

        Assert.Equal(["Progress", "Recipe"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(IngredientPreparationRecipe), typeof(IngredientPreparationProgress)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(activityType.GetEvents());
        Assert.Empty(activityType.GetMethods().Where(
            method => method.DeclaringType == activityType && !method.IsSpecialName));
        Assert.True(activityType.IsSealed);
    }

    internal static IngredientPreparationRecipe Recipe() =>
        new(
            new PreparationRecipeId("cooling-tonic"),
            IngredientPreparationProgressTests.Plan(),
            [
                new IngredientPreparationInputRequirement(
                    new IngredientDefinition(new IngredientId("moonwort-leaf")),
                    new IngredientQuantity(1, IngredientQuantityUnit.Count)),
            ],
            new IngredientDefinition(new IngredientId("cooling-tonic")),
            new IngredientQuantity(1, IngredientQuantityUnit.Count));
}
