using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Diseases;

/// <summary>Links one exact disease definition to one exact stabilizing preparation recipe.</summary>
public sealed class StabilizingRemedyDefinition
{
    /// <summary>Creates an immutable authored remedy definition without applying treatment.</summary>
    public StabilizingRemedyDefinition(
        DiseaseDefinition disease,
        IngredientPreparationRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(disease);
        ArgumentNullException.ThrowIfNull(recipe);

        Disease = disease;
        Recipe = recipe;
    }

    /// <summary>Gets the exact authoritative disease definition.</summary>
    public DiseaseDefinition Disease { get; }

    /// <summary>Gets the exact authored preparation recipe.</summary>
    public IngredientPreparationRecipe Recipe { get; }
}
