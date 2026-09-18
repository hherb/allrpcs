using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Applies one exact-current target-delay recovery to authoritative world state.</summary>
public static class CharacterTargetSabotageDelayRecoveryTransition
{
    /// <summary>Returns the exact recovery candidate and freshly advanced world state.</summary>
    public static CharacterTargetSabotageDelayRecoveryResult Recover(
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot currentEventLog,
        WorldTimeSnapshot currentWorldTime,
        CharacterTargetSabotageDelayRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(currentEventLog);
        ArgumentNullException.ThrowIfNull(currentWorldTime);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                worldEvents,
                currentEventLog,
                currentWorldTime,
                candidate))
        {
            throw new InvalidOperationException(
                "Only an exact-current eligible target delay may be recovered.");
        }

        WorldEventOccurrenceResult occurrence = WorldEventOccurrenceTransition.Occur(
            candidate.RecoveryEvent.EventId,
            worldEvents,
            currentEventLog,
            currentWorldTime);
        return new CharacterTargetSabotageDelayRecoveryResult(
            candidate,
            occurrence.EventLog,
            occurrence.WorldTime);
    }
}
