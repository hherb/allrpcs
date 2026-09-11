namespace Allrpcs.Domain.Ingredients;

/// <summary>Appends one exact completed preparation as a new full batch.</summary>
public static class PreparedIngredientBatchSnapshotAppendTransition
{
    /// <summary>Returns a new snapshot with one exact full batch appended in stable order.</summary>
    public static PreparedIngredientBatchSnapshot Append(
        IngredientPreparationOutcome preparation,
        PreparedIngredientBatchSnapshot batches)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(batches);
        if (batches.Batches.Count >= PreparedIngredientBatchSnapshot.MaximumBatchCount
            || batches.Batches.Any(
                batch => ReferenceEquals(batch.Preparation, preparation)))
        {
            throw new InvalidOperationException(
                "The completed preparation cannot be appended to this batch snapshot.");
        }

        return new PreparedIngredientBatchSnapshot(
            batches.Batches.Append(new PreparedIngredientBatch(preparation)));
    }
}
