using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains exact disease truth, prepared batches, and journals after stabilization.</summary>
public sealed class FirstPatientStabilizationResult
{
    /// <summary>Creates one immutable atomic stabilization result.</summary>
    public FirstPatientStabilizationResult(
        DiseaseTruthSnapshot diseaseTruth,
        PreparedIngredientBatchSnapshot preparedBatches,
        IngredientExperimentJournalSnapshot administrationExperiments,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(preparedBatches);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(experiments);

        DiseaseTruth = diseaseTruth;
        PreparedBatches = preparedBatches;
        AdministrationExperiments = administrationExperiments;
        Experiments = experiments;
    }

    /// <summary>Gets the exact resulting authoritative disease truth.</summary>
    public DiseaseTruthSnapshot DiseaseTruth { get; }

    /// <summary>Gets the exact resulting prepared-batch snapshot.</summary>
    public PreparedIngredientBatchSnapshot PreparedBatches { get; }

    /// <summary>Gets the exact resulting administration-specific experiment journal.</summary>
    public IngredientExperimentJournalSnapshot AdministrationExperiments { get; }

    /// <summary>Gets the exact resulting universal experiment journal.</summary>
    public ExperimentJournalSnapshot Experiments { get; }
}
