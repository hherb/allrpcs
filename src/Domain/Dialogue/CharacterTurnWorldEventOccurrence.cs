using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one exact authoritative world event and its structurally distinct transcript occurrence.
/// </summary>
public sealed record CharacterTurnWorldEventOccurrence
{
    /// <summary>Creates one occurrence without recording or applying either retained value.</summary>
    public CharacterTurnWorldEventOccurrence(
        WorldEvent worldEvent,
        CharacterTurnTranscriptEntry transcriptEntry)
    {
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(transcriptEntry);

        WorldEvent = worldEvent;
        TranscriptEntry = transcriptEntry;
    }

    /// <summary>Gets the exact authoritative world-event definition.</summary>
    public WorldEvent WorldEvent { get; }

    /// <summary>Gets the exact structurally distinct transcript occurrence.</summary>
    public CharacterTurnTranscriptEntry TranscriptEntry { get; }
}
