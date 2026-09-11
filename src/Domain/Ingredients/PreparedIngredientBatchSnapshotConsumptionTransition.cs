namespace Allrpcs.Domain.Ingredients;

/// <summary>Consumes a dosage from one exact batch while preserving stable batch order.</summary>
public static class PreparedIngredientBatchSnapshotConsumptionTransition
{
    /// <summary>Returns a new snapshot with the exact batch reduced or removed.</summary>
    public static PreparedIngredientBatchSnapshot Consume(
        IngredientDosage dosage,
        PreparedIngredientBatchSnapshot batches)
    {
        ArgumentNullException.ThrowIfNull(dosage);
        ArgumentNullException.ThrowIfNull(batches);
        if (!PreparedIngredientBatchSnapshotConsumptionPolicy.CanConsume(dosage, batches))
        {
            throw new InvalidOperationException(
                "No retained prepared batch can supply the exact dosage.");
        }

        PreparedIngredientBatch retainedBatch = batches.Batches.First(
            candidate => ReferenceEquals(candidate.Preparation, dosage.Preparation));
        PreparedIngredientBatch? remaining =
            PreparedIngredientBatchConsumptionTransition.Consume(dosage, retainedBatch);
        IEnumerable<PreparedIngredientBatch> resultingBatches = batches.Batches.SelectMany(
            batch => ReferenceEquals(batch, retainedBatch)
                ? remaining is null ? [] : new[] { remaining }
                : new[] { batch });
        return new PreparedIngredientBatchSnapshot(resultingBatches);
    }
}
