using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the exact player speaker and most recently accepted dialogue utterance.</summary>
public partial class DialogueInputController : Node
{
    /// <summary>Gets the exact initialized player character identifier, if any.</summary>
    public CharacterId? SpeakerCharacterId { get; private set; }

    /// <summary>Gets the exact most recently accepted utterance, if any.</summary>
    public DialogueUtterance? LastSubmittedUtterance { get; private set; }

    /// <summary>Initializes this controller once with the exact player character identifier.</summary>
    public bool TryInitialize(CharacterId speakerCharacterId)
    {
        ArgumentNullException.ThrowIfNull(speakerCharacterId);

        if (SpeakerCharacterId is not null)
        {
            return false;
        }

        SpeakerCharacterId = speakerCharacterId;
        return true;
    }

    /// <summary>
    /// Retains an exact utterance when the Domain dialogue-text rules accept the supplied text.
    /// </summary>
    public bool TrySubmit(string text)
    {
        CharacterId? speakerCharacterId = SpeakerCharacterId;
        if (speakerCharacterId is null)
        {
            return false;
        }

        try
        {
            LastSubmittedUtterance = new DialogueUtterance(speakerCharacterId, text);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
