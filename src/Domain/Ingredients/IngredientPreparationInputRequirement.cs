namespace Allrpcs.Domain.Ingredients;

/// <summary>Requires one exact authored ingredient definition and positive quantity as recipe input.</summary>
public sealed record IngredientPreparationInputRequirement
{
    /// <summary>Creates one immutable preparation input requirement.</summary>
    public IngredientPreparationInputRequirement(
        IngredientDefinition definition,
        IngredientQuantity quantity)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(quantity);

        Definition = definition;
        Quantity = quantity;
    }

    /// <summary>Gets the exact required ingredient definition.</summary>
    public IngredientDefinition Definition { get; }

    /// <summary>Gets the exact required positive quantity without unit conversion.</summary>
    public IngredientQuantity Quantity { get; }
}
