using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Describes one provider-neutral request for a character to consider heard in-world dialogue.
/// </summary>
public sealed record CharacterTurnRequest
{
    /// <summary>
    /// Creates an immutable request for a responder who heard the matching utterance.
    /// </summary>
    public CharacterTurnRequest(
        CharacterId respondingCharacterId,
        DialogueEvent dialogueEvent,
        DialogueAct dialogueAct)
    {
        ArgumentNullException.ThrowIfNull(respondingCharacterId);
        ArgumentNullException.ThrowIfNull(dialogueEvent);
        ArgumentNullException.ThrowIfNull(dialogueAct);

        if (dialogueEvent.Utterance != dialogueAct.Utterance)
        {
            throw new ArgumentException(
                "Dialogue event and act must represent the same utterance.",
                nameof(dialogueAct));
        }

        if (!dialogueEvent.Audience.ListenerCharacterIds.Contains(respondingCharacterId))
        {
            throw new ArgumentException(
                "The responding character must be a listener in the dialogue audience.",
                nameof(respondingCharacterId));
        }

        RespondingCharacterId = respondingCharacterId;
        DialogueEvent = dialogueEvent;
        DialogueAct = dialogueAct;
    }

    /// <summary>
    /// Gets the exact identifier of the character asked to consider the turn.
    /// </summary>
    public CharacterId RespondingCharacterId { get; }

    /// <summary>
    /// Gets the exact in-world dialogue event presented to the character.
    /// </summary>
    public DialogueEvent DialogueEvent { get; }

    /// <summary>
    /// Gets the exact provider-neutral dialogue-act representation.
    /// </summary>
    public DialogueAct DialogueAct { get; }
}
