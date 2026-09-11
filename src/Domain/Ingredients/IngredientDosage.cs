namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains one positive bounded dosage from an exact prepared outcome.</summary>
public sealed class IngredientDosage
{
    /// <summary>Creates one immutable dosage without converting its authored unit.</summary>
    public IngredientDosage(
        IngredientPreparationOutcome preparation,
        IngredientQuantity quantity)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(quantity);
        if (!ReferenceEquals(preparation.Quantity.Unit, quantity.Unit))
        {
            throw new ArgumentException(
                "Dosage and prepared outcome must use the same exact unit.",
                nameof(quantity));
        }

        if (quantity.Amount > preparation.Quantity.Amount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Dosage cannot exceed the prepared result amount.");
        }

        Preparation = preparation;
        Quantity = quantity;
    }

    /// <summary>Gets the exact prepared outcome from which this dosage is drawn.</summary>
    public IngredientPreparationOutcome Preparation { get; }

    /// <summary>Gets the exact positive dosage quantity.</summary>
    public IngredientQuantity Quantity { get; }
}
