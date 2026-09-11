namespace Allrpcs.Domain.Ingredients;

/// <summary>Advances immutable preparation progress by one exact authored step.</summary>
public static class IngredientPreparationProgressionTransition
{
    /// <summary>Returns new progress advanced by exactly one pending step.</summary>
    public static IngredientPreparationProgress Advance(
        IngredientPreparationProgress progress,
        IngredientPreparationStep completedStep)
    {
        if (!IngredientPreparationProgressionPolicy.CanAdvance(progress, completedStep))
        {
            throw new InvalidOperationException(
                "Only the exact pending preparation step may advance progress.");
        }

        return new IngredientPreparationProgress(progress.Plan, progress.NextStepIndex + 1);
    }
}
