using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Establishes one authored ingredient independently of physical world objects.</summary>
public sealed record IngredientDefinition
{
    /// <summary>Creates an immutable authored ingredient definition.</summary>
    public IngredientDefinition(IngredientId ingredientId)
    {
        ArgumentNullException.ThrowIfNull(ingredientId);
        IngredientId = ingredientId;
    }

    /// <summary>Gets the exact canonical ingredient identity.</summary>
    public IngredientId IngredientId { get; }
}
