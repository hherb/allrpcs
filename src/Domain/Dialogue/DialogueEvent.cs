namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Associates one exact in-world dialogue delivery with the characters who heard it.
/// </summary>
public sealed record DialogueEvent
{
    /// <summary>
    /// Creates an immutable dialogue event when delivery and audience identify the same speaker.
    /// </summary>
    public DialogueEvent(
        DialogueDelivery delivery,
        DialogueAudienceSnapshot audience)
    {
        ArgumentNullException.ThrowIfNull(delivery);
        ArgumentNullException.ThrowIfNull(audience);

        if (delivery.Utterance.SpeakerCharacterId != audience.SpeakerCharacterId)
        {
            throw new ArgumentException(
                "Dialogue delivery and audience must identify the same speaker.",
                nameof(audience));
        }

        Delivery = delivery;
        Audience = audience;
    }

    /// <summary>
    /// Gets the exact dialogue delivery that occurred in the world.
    /// </summary>
    public DialogueDelivery Delivery { get; }

    /// <summary>
    /// Gets the exact utterance retained by the delivery.
    /// </summary>
    public DialogueUtterance Utterance => Delivery.Utterance;

    /// <summary>
    /// Gets the exact audience snapshot captured for the utterance.
    /// </summary>
    public DialogueAudienceSnapshot Audience { get; }
}
