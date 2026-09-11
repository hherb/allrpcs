using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Decides whether an authored comfort or stabilization outcome may change disease truth.</summary>
public static class DiseaseCareEligibilityPolicy
{
    /// <summary>Returns true only for exact current state, a stronger effect, and a current event.</summary>
    public static bool CanApply(
        DiseaseCareOutcome outcome,
        DiseaseTruthSnapshot truth,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        return truth.Infections.Any(infection => ReferenceEquals(infection, outcome.Infection))
            && IsStronger(outcome.Infection.CareState, outcome.ResultingCareState)
            && worldEvents.Entries.Any(worldEvent =>
                ReferenceEquals(worldEvent, outcome.WorldEvent))
            && eventLog.Entries.Count > 0
            && ReferenceEquals(eventLog.Entries[^1], outcome.WorldEvent.EventId);
    }

    private static bool IsStronger(DiseaseCareState current, DiseaseCareState resulting) =>
        ReferenceEquals(current, DiseaseCareState.Unassisted)
        || (ReferenceEquals(current, DiseaseCareState.Comforted)
            && ReferenceEquals(resulting, DiseaseCareState.Stabilized));
}
