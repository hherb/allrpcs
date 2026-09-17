using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Applies one eligible target-delay candidate to immutable authoritative world state.</summary>
public static class CharacterTargetSabotageDelayTransition
{
    /// <summary>Returns the exact candidate and its freshly advanced world state.</summary>
    public static CharacterTargetSabotageDelayResult Apply(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTargetSabotageDelayCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterTargetSabotageDelayPolicy.CanApply(
                actor,
                interactionAccess,
                worldEvents,
                eventLog,
                worldTime,
                candidate))
        {
            throw new InvalidOperationException(
                "Only an eligible target-delay candidate may advance world time.");
        }

        WorldEventOccurrenceResult occurrence = WorldEventOccurrenceTransition.Occur(
            candidate.WorldEvent.EventId,
            worldEvents,
            eventLog,
            worldTime);
        return new CharacterTargetSabotageDelayResult(
            candidate,
            occurrence.EventLog,
            occurrence.WorldTime);
    }
}
