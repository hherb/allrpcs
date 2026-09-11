namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains the exact authored and qualitative state of one ingredient quantity.</summary>
public sealed record IngredientState
{
    /// <summary>Creates one immutable ingredient state.</summary>
    public IngredientState(
        IngredientDefinition definition,
        IngredientQuantity quantity,
        IngredientFreshness freshness,
        IngredientPhysicalCondition physicalCondition,
        IngredientContamination contamination)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(quantity);
        ArgumentNullException.ThrowIfNull(freshness);
        ArgumentNullException.ThrowIfNull(physicalCondition);
        ArgumentNullException.ThrowIfNull(contamination);

        Definition = definition;
        Quantity = quantity;
        Freshness = freshness;
        PhysicalCondition = physicalCondition;
        Contamination = contamination;
    }

    /// <summary>Gets the exact authored definition.</summary>
    public IngredientDefinition Definition { get; }

    /// <summary>Gets the exact positive bounded quantity.</summary>
    public IngredientQuantity Quantity { get; }

    /// <summary>Gets the exact defined freshness.</summary>
    public IngredientFreshness Freshness { get; }

    /// <summary>Gets the exact defined physical condition.</summary>
    public IngredientPhysicalCondition PhysicalCondition { get; }

    /// <summary>Gets the exact defined contamination state.</summary>
    public IngredientContamination Contamination { get; }
}
