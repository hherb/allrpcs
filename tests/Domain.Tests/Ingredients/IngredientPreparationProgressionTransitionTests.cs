using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationProgressionTransitionTests
{
    [Fact]
    public void AdvancesOneExactStepAndRetainsTheExactPlan()
    {
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationProgress progress = new(plan, 0);

        IngredientPreparationProgress result =
            IngredientPreparationProgressionTransition.Advance(progress, plan.Steps[0]);

        Assert.NotSame(progress, result);
        Assert.Same(plan, result.Plan);
        Assert.Equal(1, result.NextStepIndex);
        Assert.Same(plan.Steps[1], result.NextStep);
        Assert.False(result.IsComplete);
        Assert.Equal(0, progress.NextStepIndex);
    }

    [Fact]
    public void CompletesAfterTheLastExactStep()
    {
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationProgress progress = new(plan, 1);

        IngredientPreparationProgress result =
            IngredientPreparationProgressionTransition.Advance(progress, plan.Steps[1]);

        Assert.Equal(2, result.NextStepIndex);
        Assert.True(result.IsComplete);
        Assert.Null(result.NextStep);
    }

    [Fact]
    public void RejectsOutOfOrderReplacementCompletedAndNullInputs()
    {
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationProgress initial = new(plan, 0);
        IngredientPreparationProgress complete = new(plan, plan.Steps.Count);
        IngredientPreparationStep replacement = new(IngredientPreparationKind.Grinding);

        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationProgressionTransition.Advance(initial, plan.Steps[1]));
        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationProgressionTransition.Advance(initial, replacement));
        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationProgressionTransition.Advance(complete, plan.Steps[1]));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationProgressionTransition.Advance(null!, plan.Steps[0]));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationProgressionTransition.Advance(initial, null!));
        Assert.Equal(0, initial.NextStepIndex);
        Assert.Equal(plan.Steps.Count, complete.NextStepIndex);
    }
}
