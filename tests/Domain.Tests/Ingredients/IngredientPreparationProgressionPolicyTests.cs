using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationProgressionPolicyTests
{
    [Fact]
    public void AcceptsOnlyTheExactPendingStep()
    {
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationProgress progress = new(plan, 0);

        Assert.True(IngredientPreparationProgressionPolicy.CanAdvance(progress, plan.Steps[0]));
        Assert.False(IngredientPreparationProgressionPolicy.CanAdvance(progress, plan.Steps[1]));
        Assert.Same(plan.Steps[0], progress.NextStep);
        Assert.Equal(0, progress.NextStepIndex);
    }

    [Fact]
    public void RejectsADistinctValueEqualStep()
    {
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationProgress progress = new(plan, 0);
        IngredientPreparationStep replacement = new(IngredientPreparationKind.Grinding);

        Assert.Equal(plan.Steps[0], replacement);
        Assert.NotSame(plan.Steps[0], replacement);
        Assert.False(IngredientPreparationProgressionPolicy.CanAdvance(progress, replacement));
    }

    [Fact]
    public void RejectsAnyStepAfterCompletion()
    {
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationProgress progress = new(plan, plan.Steps.Count);

        Assert.True(progress.IsComplete);
        Assert.False(IngredientPreparationProgressionPolicy.CanAdvance(progress, plan.Steps[1]));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationProgress progress = new(plan, 0);

        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationProgressionPolicy.CanAdvance(null!, plan.Steps[0]));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationProgressionPolicy.CanAdvance(progress, null!));
    }
}
