namespace Allrpcs.Domain.Ingredients;

/// <summary>Produces the exact recipe-authored result from completed preparation activity.</summary>
public static class IngredientPreparationCompletionTransition
{
    /// <summary>Returns an immutable outcome only after every exact step is complete.</summary>
    public static IngredientPreparationOutcome Complete(IngredientPreparationActivity activity)
    {
        if (!IngredientPreparationCompletionPolicy.CanComplete(activity))
        {
            throw new InvalidOperationException("Preparation activity is not complete.");
        }

        return new IngredientPreparationOutcome(
            activity,
            activity.Recipe.ResultDefinition,
            activity.Recipe.ResultQuantity);
    }
}
