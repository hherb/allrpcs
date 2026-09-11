namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains one exact recipe and its immutable preparation progress.</summary>
public sealed class IngredientPreparationActivity
{
    /// <summary>Creates an immutable preparation activity for the recipe's exact plan.</summary>
    public IngredientPreparationActivity(
        IngredientPreparationRecipe recipe,
        IngredientPreparationProgress progress)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(progress);
        if (!ReferenceEquals(recipe.Plan, progress.Plan))
        {
            throw new ArgumentException(
                "Preparation progress must retain the recipe's exact plan.",
                nameof(progress));
        }

        Recipe = recipe;
        Progress = progress;
    }

    /// <summary>Gets the exact authored recipe.</summary>
    public IngredientPreparationRecipe Recipe { get; }

    /// <summary>Gets the exact immutable progress for that recipe's plan.</summary>
    public IngredientPreparationProgress Progress { get; }
}
