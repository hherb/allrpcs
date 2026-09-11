namespace Allrpcs.Domain.Ingredients;

/// <summary>Checks whether exact authored recipe inputs are available without consuming them.</summary>
public static class IngredientPreparationAvailabilityPolicy
{
    /// <summary>Reports whether every recipe input has the same unit and a sufficient amount.</summary>
    public static bool HasRequiredInputs(
        IngredientPreparationRecipe recipe,
        IngredientStateSnapshot ingredients)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(ingredients);

        foreach (IngredientPreparationInputRequirement input in recipe.Inputs)
        {
            IngredientState? state = ingredients.States.FirstOrDefault(
                candidate => ReferenceEquals(candidate.Definition, input.Definition));
            if (state is null ||
                !ReferenceEquals(state.Quantity.Unit, input.Quantity.Unit) ||
                state.Quantity.Amount < input.Quantity.Amount)
            {
                return false;
            }
        }

        return true;
    }
}
