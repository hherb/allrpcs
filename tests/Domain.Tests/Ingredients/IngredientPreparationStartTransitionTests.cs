using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationStartTransitionTests
{
    [Fact]
    public void AtomicallyConsumesInputsAndStartsTheExactRecipeAtStepZero()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationInputRequirement input = Assert.Single(recipe.Inputs);
        IngredientState consumed = new(
            input.Definition,
            input.Quantity,
            IngredientFreshness.Fresh,
            IngredientPhysicalCondition.Intact,
            IngredientContamination.Uncontaminated);
        IngredientStateSnapshot ingredients = new([consumed]);

        IngredientPreparationStartResult result = IngredientPreparationStartTransition.Start(
            recipe,
            ingredients);

        Assert.Same(recipe, result.Activity.Recipe);
        Assert.Same(recipe.Plan, result.Activity.Progress.Plan);
        Assert.Equal(0, result.Activity.Progress.NextStepIndex);
        Assert.Empty(result.Ingredients.States);
        Assert.Same(consumed, Assert.Single(ingredients.States));
    }

    [Fact]
    public void RejectsUnavailableAndNullInputsWithoutChangingState()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientStateSnapshot ingredients = new([]);

        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationStartTransition.Start(recipe, ingredients));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationStartTransition.Start(null!, ingredients));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationStartTransition.Start(recipe, null!));
        Assert.Empty(ingredients.States);
    }

    [Fact]
    public void ExposesOnePureStaticOperation()
    {
        Type transitionType = typeof(IngredientPreparationStartTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Start", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IngredientPreparationStartResult), method.ReturnType);
        Assert.Equal(
            [typeof(IngredientPreparationRecipe), typeof(IngredientStateSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
