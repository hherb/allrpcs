using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether one active independent task may produce a world occurrence.</summary>
public static class CharacterIndependentTaskWorldEventOccurrencePolicy
{
    /// <summary>
    /// Returns true only for the exact assigned character and intention plus an authoritative event.
    /// </summary>
    public static bool CanOccur(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        CharacterIndependentTaskAssignment assignment = candidate.Activity.Assignment;
        CharacterTurnTranscriptEntry transcriptEntry = candidate.Occurrence.TranscriptEntry;

        return candidate.Activity.State == CharacterIndependentTaskActivityState.Active
            && transcriptEntry.Kind == CharacterTurnTranscriptEntryKind.PhysicalAction
            && ReferenceEquals(transcriptEntry.CharacterId, assignment.CharacterId)
            && ReferenceEquals(transcriptEntry.PhysicalAction, assignment.Intention)
            && CharacterTurnWorldEventOccurrencePolicy.CanOccur(
                candidate.Occurrence,
                worldEvents,
                eventLog,
                worldTime);
    }
}
