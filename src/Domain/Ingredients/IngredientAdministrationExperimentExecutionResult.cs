using Allrpcs.Domain.Experiments;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains prepared batches and both journals after one atomic administration.</summary>
public sealed class IngredientAdministrationExperimentExecutionResult
{
    /// <summary>Creates one immutable result from exact resulting snapshots.</summary>
    public IngredientAdministrationExperimentExecutionResult(
        PreparedIngredientBatchSnapshot preparedBatches,
        IngredientExperimentJournalSnapshot administrationExperiments,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(preparedBatches);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(experiments);

        PreparedBatches = preparedBatches;
        AdministrationExperiments = administrationExperiments;
        Experiments = experiments;
    }

    /// <summary>Gets the exact resulting prepared-batch snapshot.</summary>
    public PreparedIngredientBatchSnapshot PreparedBatches { get; }

    /// <summary>Gets the exact resulting administration-specific experiment journal.</summary>
    public IngredientExperimentJournalSnapshot AdministrationExperiments { get; }

    /// <summary>Gets the exact resulting universal experiment journal.</summary>
    public ExperimentJournalSnapshot Experiments { get; }
}
