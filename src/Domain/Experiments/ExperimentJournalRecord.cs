using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Experiments;

/// <summary>Records one authored experiment procedure and its observed qualitative outcome.</summary>
public sealed class ExperimentJournalRecord
{
    /// <summary>Gets the maximum procedure length in UTF-16 code units.</summary>
    public const int MaximumProcedureLength = 500;

    /// <summary>Gets the maximum observed-outcome length in UTF-16 code units.</summary>
    public const int MaximumObservedOutcomeLength = 500;

    /// <summary>Creates one immutable journal record without interpreting its outcome.</summary>
    public ExperimentJournalRecord(
        ExperimentId experimentId,
        WorldEvent worldEvent,
        string procedure,
        string observedOutcome)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(procedure);
        ArgumentNullException.ThrowIfNull(observedOutcome);

        if (string.IsNullOrWhiteSpace(procedure) || procedure.Length > MaximumProcedureLength)
        {
            throw new ArgumentException(
                $"An experiment procedure must be nonblank and at most {MaximumProcedureLength} "
                    + "UTF-16 code units.",
                nameof(procedure));
        }

        if (string.IsNullOrWhiteSpace(observedOutcome)
            || observedOutcome.Length > MaximumObservedOutcomeLength)
        {
            throw new ArgumentException(
                "An experiment outcome must be nonblank and at most "
                    + $"{MaximumObservedOutcomeLength} UTF-16 code units.",
                nameof(observedOutcome));
        }

        ExperimentId = experimentId;
        WorldEvent = worldEvent;
        Procedure = procedure;
        ObservedOutcome = observedOutcome;
    }

    /// <summary>Gets the exact canonical experiment identity.</summary>
    public ExperimentId ExperimentId { get; }

    /// <summary>Gets the exact authoritative event that occurred for this experiment.</summary>
    public WorldEvent WorldEvent { get; }

    /// <summary>Gets the authored procedure exactly as supplied.</summary>
    public string Procedure { get; }

    /// <summary>Gets the observed qualitative outcome exactly as supplied.</summary>
    public string ObservedOutcome { get; }
}
