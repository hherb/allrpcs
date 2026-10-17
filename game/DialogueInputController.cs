using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the exact player speaker and most recently accepted dialogue delivery.</summary>
public partial class DialogueInputController : Node
{
    internal void ResetSession() => LastSubmittedDelivery = null;

    /// <summary>Gets the exact initialized player character identifier, if any.</summary>
    public CharacterId? SpeakerCharacterId { get; private set; }

    /// <summary>Gets the exact most recently accepted delivery, if any.</summary>
    public DialogueDelivery? LastSubmittedDelivery { get; private set; }

    /// <summary>Gets the exact utterance retained by the most recent delivery, if any.</summary>
    public DialogueUtterance? LastSubmittedUtterance => LastSubmittedDelivery?.Utterance;

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
    /// Retains an exact delivery when the Domain dialogue-text rules accept the supplied text.
    /// </summary>
    public bool TrySubmit(string text, DialogueDeliveryMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        CharacterId? speakerCharacterId = SpeakerCharacterId;
        if (speakerCharacterId is null)
        {
            return false;
        }

        try
        {
            DialogueUtterance utterance = new(speakerCharacterId, text);
            LastSubmittedDelivery = new DialogueDelivery(utterance, mode);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
