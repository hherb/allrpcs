using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains exact disease truth and journal state after first-patient stabilization.</summary>
public sealed class FirstPatientStabilizationResult
{
    /// <summary>Creates one immutable atomic stabilization result.</summary>
    public FirstPatientStabilizationResult(
        DiseaseTruthSnapshot diseaseTruth,
        IngredientExperimentJournalSnapshot experimentJournal)
    {
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(experimentJournal);

        DiseaseTruth = diseaseTruth;
        ExperimentJournal = experimentJournal;
    }

    /// <summary>Gets the exact resulting authoritative disease truth.</summary>
    public DiseaseTruthSnapshot DiseaseTruth { get; }

    /// <summary>Gets the exact resulting experiment journal.</summary>
    public IngredientExperimentJournalSnapshot ExperimentJournal { get; }
}
