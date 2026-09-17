using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires an eligible delay occurrence and an applicable advancing world event.</summary>
public static class CharacterTargetSabotageDelayPolicy
{
    /// <summary>Returns whether the candidate may advance world time for the exact target delay.</summary>
    public static bool CanApply(
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

        WorldEvent? authoritativeEvent = WorldEventLookup.FindById(
            candidate.WorldEvent.EventId,
            worldEvents);
        return candidate.Occurrence.Proposal.ActionKind == CharacterSabotageActionKind.Delay
            && ReferenceEquals(authoritativeEvent, candidate.WorldEvent)
            && CharacterTargetSabotageOccurrencePolicy.CanOccur(
                actor,
                interactionAccess,
                candidate.Occurrence)
            && ReferenceEquals(candidate.WorldEvent.TimeEffect, WorldEventTimeEffect.Advance)
            && WorldEventOccurrencePolicy.CanOccur(
                candidate.WorldEvent.EventId,
                worldEvents,
                eventLog,
                worldTime);
    }
}
