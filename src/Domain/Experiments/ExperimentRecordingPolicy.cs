using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Experiments;

/// <summary>Decides whether one universal experiment may enter the journal.</summary>
public static class ExperimentRecordingPolicy
{
    /// <summary>Returns true only for a new experiment tied to the exact latest authored event.</summary>
    public static bool CanRecord(
        ExperimentId experimentId,
        WorldEvent worldEvent,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        ExperimentJournalSnapshot journal)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(journal);

        return journal.Records.Count < ExperimentJournalSnapshot.MaximumRecordCount
            && !journal.Records.Any(record => record.ExperimentId == experimentId)
            && !journal.Records.Any(record => record.WorldEvent.EventId == worldEvent.EventId)
            && worldEvents.Entries.Any(candidate => ReferenceEquals(candidate, worldEvent))
            && eventLog.Entries.Count > 0
            && ReferenceEquals(eventLog.Entries[^1], worldEvent.EventId);
    }
}
