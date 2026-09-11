namespace Allrpcs.Domain.Ingredients;

/// <summary>Atomically consumes available recipe inputs and starts immutable progress.</summary>
public static class IngredientPreparationStartTransition
{
    /// <summary>Starts the exact recipe at its first step and returns remaining ingredients.</summary>
    public static IngredientPreparationStartResult Start(
        IngredientPreparationRecipe recipe,
        IngredientStateSnapshot ingredients)
    {
        IngredientStateSnapshot remaining = IngredientPreparationConsumptionTransition.Consume(
            recipe,
            ingredients);
        IngredientPreparationProgress progress = new(recipe.Plan, 0);
        IngredientPreparationActivity activity = new(recipe, progress);

        return new IngredientPreparationStartResult(activity, remaining);
    }
}
