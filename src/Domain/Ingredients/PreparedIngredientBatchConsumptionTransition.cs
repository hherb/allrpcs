namespace Allrpcs.Domain.Ingredients;

/// <summary>Consumes one exact dosage without mutating the prepared batch.</summary>
public static class PreparedIngredientBatchConsumptionTransition
{
    /// <summary>Returns the remaining batch, or null when the exact dosage consumes it fully.</summary>
    public static PreparedIngredientBatch? Consume(
        IngredientDosage dosage,
        PreparedIngredientBatch batch)
    {
        ArgumentNullException.ThrowIfNull(dosage);
        ArgumentNullException.ThrowIfNull(batch);
        if (!PreparedIngredientBatchConsumptionPolicy.CanConsume(dosage, batch))
        {
            throw new InvalidOperationException(
                "The dosage cannot be consumed from the supplied prepared batch.");
        }

        int remainingAmount = batch.RemainingQuantity.Amount - dosage.Quantity.Amount;
        if (remainingAmount == 0)
        {
            return null;
        }

        IngredientQuantity remainingQuantity = new(
            remainingAmount,
            batch.RemainingQuantity.Unit);
        return new PreparedIngredientBatch(batch.Preparation, remainingQuantity);
    }
}
