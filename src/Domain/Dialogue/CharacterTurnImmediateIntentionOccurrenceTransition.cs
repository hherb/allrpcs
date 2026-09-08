using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Applies one eligible speaking response's immediate physical intention occurrence.
/// </summary>
public static class CharacterTurnImmediateIntentionOccurrenceTransition
{
    /// <summary>
    /// Records the exact authoritative event and action after validating their response origin.
    /// </summary>
    public static CharacterTurnWorldEventOccurrenceResult Occur(
        CharacterTurnActivity activity,
        CharacterTurnWorldEventOccurrence occurrence,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript)
    {
        if (!CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity,
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript))
        {
            throw new InvalidOperationException(
                "Only an eligible exact immediate character-turn intention can occur.");
        }

        return CharacterTurnWorldEventOccurrenceTransition.Occur(
            occurrence,
            worldEvents,
            eventLog,
            worldTime,
            transcript);
    }
}
