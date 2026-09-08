using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains the exact authoritative world-event result and resulting character-turn transcript.
/// </summary>
public sealed class CharacterTurnWorldEventOccurrenceResult
{
    /// <summary>Creates a result from exact immutable nested values.</summary>
    public CharacterTurnWorldEventOccurrenceResult(
        CharacterTurnWorldEventOccurrence occurrence,
        WorldEventOccurrenceResult worldEventResult,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(worldEventResult);
        ArgumentNullException.ThrowIfNull(transcript);

        Occurrence = occurrence;
        WorldEventResult = worldEventResult;
        Transcript = transcript;
    }

    /// <summary>Gets the exact world-event and structurally distinct transcript occurrence.</summary>
    public CharacterTurnWorldEventOccurrence Occurrence { get; }

    /// <summary>Gets the exact authoritative event-log and world-time result.</summary>
    public WorldEventOccurrenceResult WorldEventResult { get; }

    /// <summary>Gets the exact resulting structurally distinct transcript.</summary>
    public CharacterTurnTranscriptSnapshot Transcript { get; }
}
