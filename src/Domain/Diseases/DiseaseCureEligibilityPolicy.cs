using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Decides whether an authoritative successful-cure outcome may change disease truth.</summary>
public static class DiseaseCureEligibilityPolicy
{
    /// <summary>Returns true only for an exact infection and an exact occurred authored event.</summary>
    public static bool CanApply(
        SuccessfulDiseaseCureOutcome outcome,
        DiseaseTruthSnapshot truth,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        return truth.Infections.Any(infection => ReferenceEquals(infection, outcome.Infection))
            && worldEvents.Entries.Any(worldEvent =>
                ReferenceEquals(worldEvent, outcome.WorldEvent))
            && eventLog.Entries.Count > 0
            && ReferenceEquals(eventLog.Entries[^1], outcome.WorldEvent.EventId);
    }
}
