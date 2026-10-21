using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

public partial class Main
{
    /// <summary>Registers session-only owners; none adds fields to the persisted save schema.</summary>
    internal void RegisterSessionRestoreParticipants(RestoreParticipantRegistry registry)
    {
        void Register(string owner, Func<PreparedRestore> prepare) =>
            registry.Register(new RestoreParticipant(owner, _ => prepare()));
        OrdinaryPlayerDialogueParticipationCoordinator participationCoordinator =
            GetNode<OrdinaryPlayerDialogueParticipationCoordinator>("OrdinaryPlayerDialogueParticipationCoordinator");
        CharacterTurnParticipationRoundTaskController participationRoundController =
            GetNode<CharacterTurnParticipationRoundTaskController>("CharacterTurnParticipationRoundTaskController");
        AutonomousConversationBudgetController autonomousConversationBudgetController =
            GetNode<AutonomousConversationBudgetController>("AutonomousConversationBudgetController");
        TemporaryCharacterModelGenerationInputSource characterModelInputSource =
            GetNode<TemporaryCharacterModelGenerationInputSource>("TemporaryCharacterModelGenerationInputSource");
        CharacterModelGenerationFlowController modelFlowController =
            GetNode<CharacterModelGenerationFlowController>("CharacterModelGenerationFlowController");
        CharacterTurnActivityController characterTurnController =
            GetNode<CharacterTurnActivityController>("CharacterTurnActivityController");
        CharacterTurnTranscriptController characterTurnTranscriptController =
            GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController");
        DialogueInputController dialogueInputController = GetNode<DialogueInputController>("DialogueInputController");
        Register("participation-coordinator", () => new(participationCoordinator.ResetSession));
        Register("participation-round", participationRoundController.PrepareSessionReset);
        Register("conversation-budget", () => new(autonomousConversationBudgetController.ResetSession));
        Register("generation-input-fence", characterModelInputSource.PrepareSessionReset);
        Register("generation-flow-and-stream", modelFlowController.PrepareSessionReset);
        Register("character-turn", characterTurnController.PrepareSessionReset);
        Register("transcript", () =>
        {
            CharacterTurnTranscriptSnapshot empty = new([]);
            return new(() => characterTurnTranscriptController.InstallCharacterTurnOccurrence(empty));
        });
        Register("dialogue-input", () => new(dialogueInputController.ResetSession, dialogueInputController.PublishStatus));
        Register("main-result-tracking", () => new(() => ConversationTurns.ResetSession()));
        Register("character-response-presentation", GetNode<TemporaryCharacterTurnResponseLabel>(
            "DialogueOverlay/Panel/Response").PrepareSessionReset);
        Register("resolved-perception", () => new(() => { }, RefreshResolvedPerceptionPresentation));
        Register("journal-modal", GetNode<JournalOpenCloseInputController>(
            "JournalOpenCloseInputController").PrepareSessionReset);
        Register("journal-hypothesis-proposal", () => new(GetNode<JournalHypothesisProposalController>(
            "JournalHypothesisProposalController").ResetSession));
        Register("map-modal", GetNode<SeekerMapOpenCloseInputController>(
            "SeekerMapOpenCloseInputController").PrepareSessionReset);
    }
}
