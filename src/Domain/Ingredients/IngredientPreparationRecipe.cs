using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains one bounded authored preparation recipe without executing it.</summary>
public sealed class IngredientPreparationRecipe
{
    /// <summary>Gets the maximum number of distinct ingredient inputs in one recipe.</summary>
    public const int MaximumInputCount = 16;

    /// <summary>Creates an immutable authored preparation recipe.</summary>
    public IngredientPreparationRecipe(
        PreparationRecipeId recipeId,
        IngredientPreparationPlan plan,
        IEnumerable<IngredientPreparationInputRequirement> inputs,
        IngredientDefinition resultDefinition,
        IngredientQuantity resultQuantity)
    {
        ArgumentNullException.ThrowIfNull(recipeId);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(resultDefinition);
        ArgumentNullException.ThrowIfNull(resultQuantity);

        List<IngredientPreparationInputRequirement> retainedInputs = [];
        HashSet<IngredientId> ingredientIds = [];
        foreach (IngredientPreparationInputRequirement? input in inputs)
        {
            if (input is null)
            {
                throw new ArgumentException("Preparation inputs cannot contain null.", nameof(inputs));
            }

            if (!ingredientIds.Add(input.Definition.IngredientId))
            {
                throw new ArgumentException(
                    "Preparation inputs cannot contain duplicate ingredient identities.",
                    nameof(inputs));
            }

            retainedInputs.Add(input);
            if (retainedInputs.Count > MaximumInputCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(inputs),
                    $"A preparation recipe accepts at most {MaximumInputCount} inputs.");
            }
        }

        if (retainedInputs.Count == 0)
        {
            throw new ArgumentException("A preparation recipe requires at least one input.", nameof(inputs));
        }

        RecipeId = recipeId;
        Plan = plan;
        Inputs = retainedInputs.AsReadOnly();
        ResultDefinition = resultDefinition;
        ResultQuantity = resultQuantity;
    }

    /// <summary>Gets the exact canonical recipe identity.</summary>
    public PreparationRecipeId RecipeId { get; }

    /// <summary>Gets the exact authored preparation plan.</summary>
    public IngredientPreparationPlan Plan { get; }

    /// <summary>Gets exact input requirements in stable supplied order.</summary>
    public IReadOnlyList<IngredientPreparationInputRequirement> Inputs { get; }

    /// <summary>Gets the exact authored result ingredient definition.</summary>
    public IngredientDefinition ResultDefinition { get; }

    /// <summary>Gets the exact authored result quantity without unit conversion.</summary>
    public IngredientQuantity ResultQuantity { get; }
}
