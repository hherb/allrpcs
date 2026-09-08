using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Derives one explicitly typed temporary healer request from occurred dialogue.</summary>
public partial class TemporaryCharacterTurnRequestSource : Node
{
    private readonly IEqualityComparer<CharacterId> characterIdComparer =
        EqualityComparer<CharacterId>.Default;

    /// <summary>
    /// Creates a request for the supplied healer when that healer heard the exact dialogue event.
    /// </summary>
    public CharacterTurnRequest? CreateRequest(
        DialogueEvent dialogueEvent,
        CharacterId healerCharacterId,
        DialogueActType dialogueActType)
    {
        ArgumentNullException.ThrowIfNull(dialogueEvent);
        ArgumentNullException.ThrowIfNull(healerCharacterId);
        ArgumentNullException.ThrowIfNull(dialogueActType);

        if (!dialogueEvent.Audience.ListenerCharacterIds.Contains(
                healerCharacterId,
                characterIdComparer))
        {
            return null;
        }

        DialogueAct dialogueAct = new(dialogueEvent.Utterance, dialogueActType);
        return new CharacterTurnRequest(healerCharacterId, dialogueEvent, dialogueAct);
    }
}
