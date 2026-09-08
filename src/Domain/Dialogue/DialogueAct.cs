namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Associates one exact in-world utterance with one explicitly declared dialogue-act type.
/// </summary>
public sealed record DialogueAct
{
    /// <summary>
    /// Creates an immutable dialogue act without classifying or interpreting its text.
    /// </summary>
    public DialogueAct(DialogueUtterance utterance, DialogueActType type)
    {
        ArgumentNullException.ThrowIfNull(utterance);
        ArgumentNullException.ThrowIfNull(type);

        Utterance = utterance;
        Type = type;
    }

    /// <summary>
    /// Gets the exact in-world utterance being represented.
    /// </summary>
    public DialogueUtterance Utterance { get; }

    /// <summary>
    /// Gets the explicitly declared dialogue-act type.
    /// </summary>
    public DialogueActType Type { get; }
}
