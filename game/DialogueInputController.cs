using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the exact player speaker and most recently accepted dialogue delivery.</summary>
public partial class DialogueInputController : Node
{
    internal void ResetSession() { LastSubmittedDelivery = null; Status = "Dialogue submission: READY"; }
    internal void PublishStatus() => StatusChanged?.Invoke(Status);
    internal Func<bool>? CanAccept { get; set; }
    internal string Status { get; private set; } = "Dialogue submission: READY";
    internal event Action<string>? StatusChanged;
    internal void SetStatus(string value)
    {
        if (Status == value) { return; }
        Status = value;
        StatusChanged?.Invoke(value);
    }

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

        DialogueUtterance utterance;
        try
        {
            utterance = new DialogueUtterance(speakerCharacterId, text);
        }
        catch (ArgumentException)
        {
            SetStatus("Dialogue submission: REJECTED");
            return false;
        }

        if (CanAccept is not null && !CanAccept())
        {
            SetStatus("Dialogue submission: BUSY — wait for the current reply, then retry.");
            return false;
        }
        LastSubmittedDelivery = new DialogueDelivery(utterance, mode);
        SetStatus(TemporaryDialogueInputPanel.AcceptedStatusText);
        return true;
    }
}
