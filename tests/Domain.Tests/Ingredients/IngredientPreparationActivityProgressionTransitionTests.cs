using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationActivityProgressionTransitionTests
{
    [Fact]
    public void AdvancesOnlyTheExactNextStepAndRetainsTheExactRecipe()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity activity = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, 0));

        IngredientPreparationActivity result =
            IngredientPreparationActivityProgressionTransition.Advance(
                activity,
                recipe.Plan.Steps[0]);

        Assert.NotSame(activity, result);
        Assert.Same(recipe, result.Recipe);
        Assert.NotSame(activity.Progress, result.Progress);
        Assert.Same(recipe.Plan, result.Progress.Plan);
        Assert.Equal(1, result.Progress.NextStepIndex);
        Assert.Equal(0, activity.Progress.NextStepIndex);
    }

    [Fact]
    public void RejectsOutOfOrderValueEqualCompleteAndNullInputs()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity activity = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, 0));
        IngredientPreparationActivity complete = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, recipe.Plan.Steps.Count));
        IngredientPreparationStep replacement = new(recipe.Plan.Steps[0].Kind);

        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationActivityProgressionTransition.Advance(
                activity,
                recipe.Plan.Steps[1]));
        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationActivityProgressionTransition.Advance(activity, replacement));
        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationActivityProgressionTransition.Advance(
                complete,
                recipe.Plan.Steps[0]));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationActivityProgressionTransition.Advance(
                null!,
                recipe.Plan.Steps[0]));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationActivityProgressionTransition.Advance(activity, null!));
        Assert.Equal(0, activity.Progress.NextStepIndex);
    }

    [Fact]
    public void ExposesOnePureStaticOperation()
    {
        Type transitionType = typeof(IngredientPreparationActivityProgressionTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Advance", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IngredientPreparationActivity), method.ReturnType);
        Assert.Equal(
            [typeof(IngredientPreparationActivity), typeof(IngredientPreparationStep)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
