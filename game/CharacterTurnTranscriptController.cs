using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one current immutable character-turn transcript snapshot.</summary>
public partial class CharacterTurnTranscriptController : Node
{
    /// <summary>Gets the exact initialized or transitioned transcript snapshot, if any.</summary>
    public CharacterTurnTranscriptSnapshot? Transcript { get; private set; }

    /// <summary>Initializes this controller once from one exact transcript snapshot.</summary>
    public bool TryInitialize(CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(transcript);

        if (Transcript is not null)
        {
            return false;
        }

        Transcript = transcript;
        return true;
    }

    /// <summary>Appends one exact spoken occurrence through the Domain transcript transition.</summary>
    public bool TryAppendSpokenStatement(DialogueUtterance spokenStatement)
    {
        ArgumentNullException.ThrowIfNull(spokenStatement);

        CharacterTurnTranscriptSnapshot? transcript = Transcript;
        if (transcript is null)
        {
            return false;
        }

        CharacterTurnTranscriptEntry entry = new(spokenStatement);
        Transcript = CharacterTurnTranscriptAppendTransition.Append(entry, transcript);
        return true;
    }
}
