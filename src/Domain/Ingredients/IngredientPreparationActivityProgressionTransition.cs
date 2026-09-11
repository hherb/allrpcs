namespace Allrpcs.Domain.Ingredients;

/// <summary>Advances immutable preparation activity through its exact authored order.</summary>
public static class IngredientPreparationActivityProgressionTransition
{
    /// <summary>Returns a new activity advanced once through the exact next step.</summary>
    public static IngredientPreparationActivity Advance(
        IngredientPreparationActivity activity,
        IngredientPreparationStep completedStep)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(completedStep);

        IngredientPreparationProgress progress = IngredientPreparationProgressionTransition.Advance(
            activity.Progress,
            completedStep);
        return new IngredientPreparationActivity(activity.Recipe, progress);
    }
}
