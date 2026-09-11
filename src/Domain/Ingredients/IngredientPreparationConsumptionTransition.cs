namespace Allrpcs.Domain.Ingredients;

/// <summary>Consumes exact authored recipe inputs from immutable ingredient state.</summary>
public static class IngredientPreparationConsumptionTransition
{
    /// <summary>Returns a new snapshot with each exact required amount consumed once.</summary>
    public static IngredientStateSnapshot Consume(
        IngredientPreparationRecipe recipe,
        IngredientStateSnapshot ingredients)
    {
        if (!IngredientPreparationAvailabilityPolicy.HasRequiredInputs(recipe, ingredients))
        {
            throw new InvalidOperationException("The required preparation inputs are unavailable.");
        }

        List<IngredientState> retainedStates = [];
        foreach (IngredientState state in ingredients.States)
        {
            IngredientPreparationInputRequirement? input = recipe.Inputs.FirstOrDefault(
                candidate => ReferenceEquals(candidate.Definition, state.Definition));
            if (input is null)
            {
                retainedStates.Add(state);
                continue;
            }

            int remainingAmount = state.Quantity.Amount - input.Quantity.Amount;
            if (remainingAmount == 0)
            {
                continue;
            }

            retainedStates.Add(new IngredientState(
                state.Definition,
                new IngredientQuantity(remainingAmount, state.Quantity.Unit),
                state.Freshness,
                state.PhysicalCondition,
                state.Contamination));
        }

        return new IngredientStateSnapshot(retainedStates);
    }
}
