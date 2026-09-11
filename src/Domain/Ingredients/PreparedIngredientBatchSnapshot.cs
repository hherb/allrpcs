namespace Allrpcs.Domain.Ingredients;

/// <summary>Contains bounded prepared batches in stable supplied order.</summary>
public sealed class PreparedIngredientBatchSnapshot
{
    /// <summary>Gets the maximum number of prepared batches in one snapshot.</summary>
    public const int MaximumBatchCount = 256;

    /// <summary>Creates an immutable copy of batches with distinct exact preparations.</summary>
    public PreparedIngredientBatchSnapshot(IEnumerable<PreparedIngredientBatch> batches)
    {
        ArgumentNullException.ThrowIfNull(batches);

        List<PreparedIngredientBatch> retainedBatches = [];
        HashSet<IngredientPreparationOutcome> preparations = new(
            ReferenceEqualityComparer.Instance);
        foreach (PreparedIngredientBatch? batch in batches)
        {
            if (batch is null)
            {
                throw new ArgumentException(
                    "Prepared-batch snapshots cannot contain null.",
                    nameof(batches));
            }

            if (!preparations.Add(batch.Preparation))
            {
                throw new ArgumentException(
                    "Prepared-batch snapshots cannot repeat an exact preparation.",
                    nameof(batches));
            }

            retainedBatches.Add(batch);
            if (retainedBatches.Count > MaximumBatchCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(batches),
                    $"A prepared-batch snapshot accepts at most {MaximumBatchCount} batches.");
            }
        }

        Batches = retainedBatches.AsReadOnly();
    }

    /// <summary>Gets exact prepared batches in stable supplied order.</summary>
    public IReadOnlyList<PreparedIngredientBatch> Batches { get; }
}
