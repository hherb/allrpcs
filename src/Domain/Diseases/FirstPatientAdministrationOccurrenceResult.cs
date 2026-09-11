using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains one atomic administration occurrence and first-patient state result.</summary>
public sealed class FirstPatientAdministrationOccurrenceResult
{
    /// <summary>Creates a result retaining the exact six supplied state snapshots.</summary>
    public FirstPatientAdministrationOccurrenceResult(
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        DiseaseTruthSnapshot diseaseTruth,
        PreparedIngredientBatchSnapshot preparedBatches,
        IngredientExperimentJournalSnapshot administrationExperiments,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(preparedBatches);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(experiments);

        EventLog = eventLog;
        WorldTime = worldTime;
        DiseaseTruth = diseaseTruth;
        PreparedBatches = preparedBatches;
        AdministrationExperiments = administrationExperiments;
        Experiments = experiments;
    }

    /// <summary>Gets the exact resulting authoritative event log.</summary>
    public WorldEventLogSnapshot EventLog { get; }

    /// <summary>Gets the exact resulting qualitative world time.</summary>
    public WorldTimeSnapshot WorldTime { get; }

    /// <summary>Gets the exact resulting authoritative disease truth.</summary>
    public DiseaseTruthSnapshot DiseaseTruth { get; }

    /// <summary>Gets the exact resulting prepared-batch snapshot.</summary>
    public PreparedIngredientBatchSnapshot PreparedBatches { get; }

    /// <summary>Gets the exact resulting administration-specific experiment journal.</summary>
    public IngredientExperimentJournalSnapshot AdministrationExperiments { get; }

    /// <summary>Gets the exact resulting universal experiment journal.</summary>
    public ExperimentJournalSnapshot Experiments { get; }
}
