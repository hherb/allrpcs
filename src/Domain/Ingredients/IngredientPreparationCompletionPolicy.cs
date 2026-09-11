namespace Allrpcs.Domain.Ingredients;

/// <summary>Checks whether one preparation activity has completed every authored step.</summary>
public static class IngredientPreparationCompletionPolicy
{
    /// <summary>Reports whether the activity is exactly at the end of its recipe plan.</summary>
    public static bool CanComplete(IngredientPreparationActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        return activity.Progress.NextStepIndex == activity.Recipe.Plan.Steps.Count;
    }
}
