namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Creates one provider-neutral follow-up request for an already-selected speaking listener.
/// </summary>
public static class AutonomousConversationFollowUpRequestTransition
{
    /// <summary>Creates a request retaining the selected responder and exact heard event.</summary>
    public static CharacterTurnRequest Create(
        CharacterTurnParticipationDecision decision,
        DialogueEvent dialogueEvent,
        DialogueActType dialogueActType)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(dialogueEvent);
        ArgumentNullException.ThrowIfNull(dialogueActType);

        if (decision.Kind != CharacterTurnParticipationKind.Respond
            && decision.Kind != CharacterTurnParticipationKind.Interject)
        {
            throw new InvalidOperationException(
                "Meaningful silence cannot produce an autonomous follow-up request.");
        }

        DialogueAct dialogueAct = new(dialogueEvent.Utterance, dialogueActType);
        return new CharacterTurnRequest(
            decision.ScheduledCharacterId,
            dialogueEvent,
            dialogueAct);
    }
}
