namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one exact in-world utterance and its explicit provider-neutral delivery mode.
/// </summary>
public sealed record DialogueDelivery
{
    /// <summary>
    /// Creates one immutable delivery from an existing utterance and exact defined mode.
    /// </summary>
    public DialogueDelivery(DialogueUtterance utterance, DialogueDeliveryMode mode)
    {
        ArgumentNullException.ThrowIfNull(utterance);
        ArgumentNullException.ThrowIfNull(mode);

        Utterance = utterance;
        Mode = mode;
    }

    /// <summary>Gets the exact utterance to deliver.</summary>
    public DialogueUtterance Utterance { get; }

    /// <summary>Gets the exact explicitly authored delivery mode.</summary>
    public DialogueDeliveryMode Mode { get; }
}
