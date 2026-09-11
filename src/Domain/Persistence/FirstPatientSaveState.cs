using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Persistence;

/// <summary>Composes the immutable snapshots that describe current first-patient state.</summary>
public sealed record FirstPatientSaveState
{
    /// <summary>Creates first-patient save state from exact existing snapshots.</summary>
    public FirstPatientSaveState(
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

    /// <summary>Gets the exact retained authoritative disease truth.</summary>
    public DiseaseTruthSnapshot DiseaseTruth { get; }

    /// <summary>Gets the exact retained prepared-batch snapshot.</summary>
    public PreparedIngredientBatchSnapshot PreparedBatches { get; }

    /// <summary>Gets the exact retained administration-specific experiment journal.</summary>
    public IngredientExperimentJournalSnapshot AdministrationExperiments { get; }

    /// <summary>Gets the exact retained universal experiment journal.</summary>
    public ExperimentJournalSnapshot Experiments { get; }
}
