using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Forwards each conversation-turn port to its Main-scene owners.</summary>
internal sealed class ConversationTurnNodePorts(
    CharacterModelGenerationFlowController modelFlow,
    TemporaryCharacterModelGenerationInputSource inputSource,
    TemporaryCharacterModelSource modelSource,
    CharacterTurnActivityController turns,
    TemporaryOrdinaryCharacterParticipantSource participants,
    Node2D seeker,
    Healer healer,
    Marker2D sageHearingPosition,
    DialogueInputController dialogueInput,
    DialogueEventController dialogueEvents,
    CharacterTurnTranscriptController transcript,
    OrdinaryPlayerDialogueParticipationCoordinator participation,
    CharacterTurnParticipationRoundTaskController round,
    TemporaryCharacterTurnParticipationModelSource participationModelSource,
    AutonomousConversationBudgetController budget,
    HerbExaminationActionController examination,
    TemporaryCharacterExaminationFlowController examinationScaffold)
    : IConversationTurnPorts<TemporaryCharacterModelGenerationInput>
{
    public bool GenerationHasStarted => modelFlow.HasStarted;

    public bool IsGenerating => modelFlow.IsGenerating;

    public CharacterModelStreamSnapshot? GenerationResult => modelFlow.Result;

    public CharacterModelContextAssemblyStatus? LastGenerationAssemblyStatus =>
        inputSource.LastAssemblyStatus;

    public CharacterTurnActivity? TurnActivity => turns.Activity;

    public IReadOnlyList<Character> OrdinaryParticipants => participants.Participants;

    public bool HealerPresent => healer.Visible;

    public NumericsVector2 SeekerPosition => ToNumerics(seeker.Position);

    public NumericsVector2 HealerPosition => ToNumerics(healer.Position);

    public NumericsVector2 SageHearingPosition => ToNumerics(sageHearingPosition.Position);

    public DialogueDelivery? LastSubmittedDelivery => dialogueInput.LastSubmittedDelivery;

    public string DialogueStatus => dialogueInput.Status;

    public DialogueEvent? LastOccurredDialogueEvent => dialogueEvents.LastOccurredEvent;

    public bool ParticipationBusy => participation.IsBusy;

    public bool ParticipationRoundIdle =>
        round.Status == CharacterTurnParticipationRoundTaskStatus.Idle;

    public bool TryResetGeneration() => modelFlow.TryReset();

    public bool TryCancelGeneration() => modelFlow.TryCancel();

    public TemporaryCharacterModelGenerationInput? CreateGenerationInput(CharacterTurnActivity activity) =>
        inputSource.CreateInput(activity);

    public bool IsGenerationInputCurrent(CharacterTurnRequest request) => inputSource.IsCurrent(request);

    public bool TryStartGeneration(TemporaryCharacterModelGenerationInput input, TimeSpan deadline) =>
        modelFlow.TryStart(
            modelSource.TakeModel(),
            input.GenerationRequest,
            input.WorldObjects,
            input.Affordances,
            deadline);

    public bool TryCancelTurn() => turns.TryCancel();

    public bool TryReceiveTurnResponse(CharacterTurnResponse response) =>
        turns.TryReceiveResponse(response);

    public void SetDialogueStatus(string status) => dialogueInput.SetStatus(status);

    public bool TryOccurDialogue(
        DialogueDelivery delivery,
        NumericsVector2 speakerPosition,
        float ordinaryHearingRangePixels,
        float whisperHearingRangePixels,
        IReadOnlyList<DialogueHearingCandidate> candidates) =>
        dialogueEvents.TryOccur(
            delivery,
            speakerPosition,
            ordinaryHearingRangePixels,
            whisperHearingRangePixels,
            candidates);

    public bool TryAppendSpokenStatement(DialogueDelivery delivery) =>
        transcript.TryAppendSpokenStatement(delivery);

    public bool TryContinueParticipation(
        DialogueEvent heardDialogueEvent,
        IReadOnlyList<Character> scheduledCharacters,
        IReadOnlyList<CharacterId> directlyAddressedCharacterIds,
        TimeSpan deadline) =>
        participation.TryStart(
            heardDialogueEvent,
            scheduledCharacters,
            directlyAddressedCharacterIds,
            participationModelSource,
            round,
            budget,
            turns,
            deadline);

    public bool TryStartParticipationChain(
        DialogueEvent heardDialogueEvent,
        IReadOnlyList<Character> scheduledCharacters,
        IReadOnlyList<CharacterId> directlyAddressedCharacterIds,
        AutonomousConversationBudget initialBudget,
        TimeSpan deadline) =>
        participation.TryStartNewChain(
            heardDialogueEvent,
            scheduledCharacters,
            directlyAddressedCharacterIds,
            participationModelSource,
            round,
            budget,
            turns,
            initialBudget,
            deadline);

    public bool TryCancelParticipationRound() => round.TryCancel();

    public bool TryInterruptConversationBudget() => budget.TryInterrupt();

    public void OfferExamination(CharacterTurnActivity activity)
    {
        // The legacy fixture never runs beside the normal path, even if a check turned it on.
        examinationScaffold.SetProcess(false);
        examination.Offer(activity);
    }

    public void CancelExamination() => examination.Cancel();

    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);
}
