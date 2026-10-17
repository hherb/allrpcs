namespace Allrpcs.Domain.Dialogue;

/// <summary>Appends one observed occurrence to an immutable character-turn transcript.</summary>
public static class CharacterTurnTranscriptAppendTransition
{
    /// <summary>Returns a new transcript with the exact entry appended in stable order.</summary>
    public static CharacterTurnTranscriptSnapshot Append(
        CharacterTurnTranscriptEntry entry,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(transcript);

        return transcript.Append(entry);
    }
}
