using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

public partial class Main
{
    /// <summary>Registers session-only owners; none adds fields to the persisted save schema.</summary>
    internal void RegisterSessionRestoreParticipants(RestoreParticipantRegistry registry)
    {
        void Register(string owner, Func<PreparedRestore> prepare) =>
            registry.Register(new RestoreParticipant(owner, _ => prepare()));
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
        Register("main-result-tracking", () => new(() =>
        {
            lastProcessedModelResult = null;
            lastProcessedCompletedCharacterTurn = null;
            lastProcessedDialogueDelivery = null;
        }));
        Register("character-response-presentation", GetNode<TemporaryCharacterTurnResponseLabel>(
            "DialogueOverlay/Panel/Response").PrepareSessionReset);
        Register("resolved-perception", () => new(() => { }, RefreshResolvedPerceptionPresentation));
        Register("journal-modal", GetNode<JournalOpenCloseInputController>(
            "JournalOpenCloseInputController").PrepareSessionReset);
        Register("map-modal", GetNode<SeekerMapOpenCloseInputController>(
            "SeekerMapOpenCloseInputController").PrepareSessionReset);
    }
}
