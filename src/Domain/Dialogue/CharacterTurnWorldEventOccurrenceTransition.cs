using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Applies one eligible character-turn occurrence to authoritative event and transcript state.
/// </summary>
public static class CharacterTurnWorldEventOccurrenceTransition
{
    /// <summary>
    /// Records the authoritative event and appends its exact structurally distinct transcript entry.
    /// </summary>
    public static CharacterTurnWorldEventOccurrenceResult Occur(
        CharacterTurnWorldEventOccurrence occurrence,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(transcript);

        if (!CharacterTurnWorldEventOccurrencePolicy.CanOccur(
            occurrence,
            worldEvents,
            eventLog,
            worldTime))
        {
            throw new InvalidOperationException(
                "The supplied character-turn world event cannot occur.");
        }

        WorldEventOccurrenceResult worldEventResult = WorldEventOccurrenceTransition.Occur(
            occurrence.WorldEvent.EventId,
            worldEvents,
            eventLog,
            worldTime);
        CharacterTurnTranscriptSnapshot resultingTranscript =
            CharacterTurnTranscriptAppendTransition.Append(
                occurrence.TranscriptEntry,
                transcript);

        return new CharacterTurnWorldEventOccurrenceResult(
            occurrence,
            worldEventResult,
            resultingTranscript);
    }
}
