namespace Allrpcs.Domain.Ingredients;

/// <summary>Tracks the exact remaining quantity of one completed preparation.</summary>
public sealed class PreparedIngredientBatch
{
    /// <summary>Creates one reusable batch containing the complete prepared result.</summary>
    public PreparedIngredientBatch(IngredientPreparationOutcome preparation)
        : this(preparation, preparation?.Quantity!)
    {
    }

    internal PreparedIngredientBatch(
        IngredientPreparationOutcome preparation,
        IngredientQuantity remainingQuantity)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(remainingQuantity);
        if (!ReferenceEquals(preparation.Quantity.Unit, remainingQuantity.Unit)
            || remainingQuantity.Amount > preparation.Quantity.Amount)
        {
            throw new ArgumentException(
                "Remaining prepared quantity must fit the exact preparation unit and amount.",
                nameof(remainingQuantity));
        }

        Preparation = preparation;
        RemainingQuantity = remainingQuantity;
    }

    /// <summary>Gets the exact completed preparation represented by this batch.</summary>
    public IngredientPreparationOutcome Preparation { get; }

    /// <summary>Gets the exact positive quantity remaining in this batch.</summary>
    public IngredientQuantity RemainingQuantity { get; }
}
