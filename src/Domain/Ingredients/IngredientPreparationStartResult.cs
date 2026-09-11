namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains one started preparation activity and remaining ingredient state.</summary>
public sealed class IngredientPreparationStartResult
{
    /// <summary>Creates an immutable preparation-start result.</summary>
    public IngredientPreparationStartResult(
        IngredientPreparationActivity activity,
        IngredientStateSnapshot ingredients)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(ingredients);

        Activity = activity;
        Ingredients = ingredients;
    }

    /// <summary>Gets the exact started preparation activity.</summary>
    public IngredientPreparationActivity Activity { get; }

    /// <summary>Gets the exact ingredient snapshot after input consumption.</summary>
    public IngredientStateSnapshot Ingredients { get; }
}
