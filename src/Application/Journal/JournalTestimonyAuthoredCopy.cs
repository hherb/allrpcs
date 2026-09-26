using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Copies audience-free authored testimony from one safely heard dialogue event.</summary>
public sealed class JournalTestimonyAuthoredCopy
{
    internal JournalTestimonyAuthoredCopy(
        CharacterId speakerCharacterId,
        string text,
        DialogueDeliveryMode deliveryMode)
    {
        SpeakerCharacterId = speakerCharacterId;
        Text = text;
        DeliveryMode = deliveryMode;
    }

    /// <summary>Gets the exact identifier of the character who authored the heard statement.</summary>
    public CharacterId SpeakerCharacterId { get; }

    /// <summary>Gets the exact heard dialogue text.</summary>
    public string Text { get; }

    /// <summary>Gets the exact provider-neutral delivery mode of the heard statement.</summary>
    public DialogueDeliveryMode DeliveryMode { get; }
}
