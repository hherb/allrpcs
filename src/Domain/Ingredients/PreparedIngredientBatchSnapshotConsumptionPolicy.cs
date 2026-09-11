namespace Allrpcs.Domain.Ingredients;

/// <summary>Decides whether a dosage can be drawn from one retained prepared batch.</summary>
public static class PreparedIngredientBatchSnapshotConsumptionPolicy
{
    /// <summary>Returns true only when the exact preparation is retained with enough quantity.</summary>
    public static bool CanConsume(
        IngredientDosage dosage,
        PreparedIngredientBatchSnapshot batches)
    {
        ArgumentNullException.ThrowIfNull(dosage);
        ArgumentNullException.ThrowIfNull(batches);

        PreparedIngredientBatch? batch = batches.Batches.FirstOrDefault(
            candidate => ReferenceEquals(candidate.Preparation, dosage.Preparation));
        return batch is not null
            && PreparedIngredientBatchConsumptionPolicy.CanConsume(dosage, batch);
    }
}
