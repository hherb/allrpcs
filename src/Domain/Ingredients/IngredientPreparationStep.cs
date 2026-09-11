namespace Allrpcs.Domain.Ingredients;

/// <summary>Defines one exact process in an authored ingredient-preparation order.</summary>
public sealed record IngredientPreparationStep
{
    /// <summary>Creates one immutable preparation step.</summary>
    public IngredientPreparationStep(IngredientPreparationKind kind)
    {
        ArgumentNullException.ThrowIfNull(kind);
        Kind = kind;
    }

    /// <summary>Gets the exact defined preparation process.</summary>
    public IngredientPreparationKind Kind { get; }
}
