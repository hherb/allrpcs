using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires exact current delay state and an applicable recovery world event.</summary>
public static class CharacterTargetSabotageDelayRecoveryPolicy
{
    /// <summary>Returns whether the exact current delay may be recovered once.</summary>
    public static bool CanRecover(
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot currentEventLog,
        WorldTimeSnapshot currentWorldTime,
        CharacterTargetSabotageDelayRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(currentEventLog);
        ArgumentNullException.ThrowIfNull(currentWorldTime);
        ArgumentNullException.ThrowIfNull(candidate);

        CharacterTargetSabotageDelayResult delayResult = candidate.DelayResult;
        WorldEvent delayEvent = delayResult.Candidate.WorldEvent;
        WorldEvent recoveryEvent = candidate.RecoveryEvent;
        WorldEvent? authoritativeDelayEvent = WorldEventLookup.FindById(
            delayEvent.EventId,
            worldEvents);
        WorldEvent? authoritativeRecoveryEvent = WorldEventLookup.FindById(
            recoveryEvent.EventId,
            worldEvents);
        IReadOnlyList<Allrpcs.Domain.Identifiers.WorldEventId> currentEntries =
            currentEventLog.Entries;

        return ReferenceEquals(currentEventLog, delayResult.EventLog)
            && ReferenceEquals(currentWorldTime, delayResult.WorldTime)
            && delayResult.Candidate.Occurrence.Proposal.ActionKind
                == CharacterSabotageActionKind.Delay
            && ReferenceEquals(authoritativeDelayEvent, delayEvent)
            && ReferenceEquals(delayEvent.TimeEffect, WorldEventTimeEffect.Advance)
            && currentEntries.Count > 0
            && ReferenceEquals(currentEntries[^1], delayEvent.EventId)
            && ReferenceEquals(currentWorldTime.CurrentEventId, delayEvent.EventId)
            && ReferenceEquals(authoritativeRecoveryEvent, recoveryEvent)
            && ReferenceEquals(recoveryEvent.TimeEffect, WorldEventTimeEffect.Advance)
            && WorldEventOccurrencePolicy.CanOccur(
                recoveryEvent.EventId,
                worldEvents,
                currentEventLog,
                currentWorldTime);
    }
}
