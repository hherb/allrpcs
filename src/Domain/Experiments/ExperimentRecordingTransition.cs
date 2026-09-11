using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Experiments;

/// <summary>Atomically records one authored experiment and its observed outcome.</summary>
public static class ExperimentRecordingTransition
{
    /// <summary>Constructs and appends one eligible universal experiment record.</summary>
    public static ExperimentJournalSnapshot Record(
        ExperimentId experimentId,
        WorldEvent worldEvent,
        string procedure,
        string observedOutcome,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        ExperimentJournalSnapshot journal)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(procedure);
        ArgumentNullException.ThrowIfNull(observedOutcome);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(journal);

        if (!ExperimentRecordingPolicy.CanRecord(
            experimentId,
            worldEvent,
            worldEvents,
            eventLog,
            journal))
        {
            throw new InvalidOperationException(
                "The experiment is not a new occurrence of the latest exact authored event.");
        }

        ExperimentJournalRecord record = new(
            experimentId,
            worldEvent,
            procedure,
            observedOutcome);
        return new ExperimentJournalSnapshot(journal.Records.Append(record));
    }
}
