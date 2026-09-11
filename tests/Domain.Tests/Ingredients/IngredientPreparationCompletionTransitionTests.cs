using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationCompletionTransitionTests
{
    [Fact]
    public void ProducesTheExactAuthoredResultForACompletedActivity()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity activity = IngredientPreparationOutcomeTests.Complete(recipe);

        IngredientPreparationOutcome outcome = IngredientPreparationCompletionTransition.Complete(
            activity);

        Assert.Same(activity, outcome.Activity);
        Assert.Same(recipe.ResultDefinition, outcome.Definition);
        Assert.Same(recipe.ResultQuantity, outcome.Quantity);
    }

    [Fact]
    public void RejectsIncompleteAndNullActivity()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity incomplete = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, 0));

        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationCompletionTransition.Complete(incomplete));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationCompletionTransition.Complete(null!));
        Assert.Equal(0, incomplete.Progress.NextStepIndex);
    }

    [Fact]
    public void ExposesOnePureStaticOperation()
    {
        Type transitionType = typeof(IngredientPreparationCompletionTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Complete", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IngredientPreparationOutcome), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(IngredientPreparationActivity), parameter.ParameterType));
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
