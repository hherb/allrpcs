namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Associates one exact in-world utterance with the characters who heard it.
/// </summary>
public sealed record DialogueEvent
{
    /// <summary>
    /// Creates an immutable dialogue event when utterance and audience identify the same speaker.
    /// </summary>
    public DialogueEvent(
        DialogueUtterance utterance,
        DialogueAudienceSnapshot audience)
    {
        ArgumentNullException.ThrowIfNull(utterance);
        ArgumentNullException.ThrowIfNull(audience);

        if (utterance.SpeakerCharacterId != audience.SpeakerCharacterId)
        {
            throw new ArgumentException(
                "Dialogue utterance and audience must identify the same speaker.",
                nameof(audience));
        }

        Utterance = utterance;
        Audience = audience;
    }

    /// <summary>
    /// Gets the exact utterance that occurred in the world.
    /// </summary>
    public DialogueUtterance Utterance { get; }

    /// <summary>
    /// Gets the exact audience snapshot captured for the utterance.
    /// </summary>
    public DialogueAudienceSnapshot Audience { get; }
}
