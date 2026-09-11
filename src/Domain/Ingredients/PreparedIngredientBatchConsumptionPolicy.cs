namespace Allrpcs.Domain.Ingredients;

/// <summary>Decides whether one dosage can be drawn from a prepared batch.</summary>
public static class PreparedIngredientBatchConsumptionPolicy
{
    /// <summary>Returns true only for an exact preparation with enough remaining quantity.</summary>
    public static bool CanConsume(IngredientDosage dosage, PreparedIngredientBatch batch)
    {
        ArgumentNullException.ThrowIfNull(dosage);
        ArgumentNullException.ThrowIfNull(batch);

        return ReferenceEquals(dosage.Preparation, batch.Preparation)
            && ReferenceEquals(dosage.Quantity.Unit, batch.RemainingQuantity.Unit)
            && dosage.Quantity.Amount <= batch.RemainingQuantity.Amount;
    }
}
