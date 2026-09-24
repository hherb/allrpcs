using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact eligible move outcome through main-scene recovery dependencies.</summary>
public partial class CharacterSabotageOutcomeMoveRecoveryMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_OUTCOME_MOVE_RECOVERY_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private CharacterSabotageActionMoveRequestController moveAuthor = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource actionModels = null!;
    private CharacterSabotageActionTaskController actionTask = null!;
    private CharacterSabotageActionRequestConsiderationFlowController actionFlow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator specificRequests = null!;
    private TemporaryCharacterObjectSabotageMoveModelSource moveModels = null!;
    private CharacterObjectSabotageMoveTaskController moveTask = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator specificStart = null!;
    private CharacterSabotageActionSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator specificOutcome = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterSabotageOutcomeRecoveryCoordinator recovery = null!;
    private CharacterSabotageTerminalRouteCoordinator terminalRoute = null!;
    private CharacterSabotageTurnCompletionCoordinator turnCompletion = null!;
    private CharacterSabotageTurnAdmissionCoordinator turnAdmission = null!;
    private CharacterSabotageOrdinaryTurnBoundaryCoordinator turnBoundary = null!;
    private CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator
        schedulingAuthorization = null!;
    private CharacterSabotageCycleBoundaryCoordinator sabotageCycleBoundary = null!;
    private CharacterTurnActivityController ordinaryTurns = null!;
    private TemporaryCharacterTurnSource ordinaryTurnSource = null!;
    private RecordingActionModel actionModel = null!;
    private RecordingMoveModel moveModel = null!;
    private int processFrames;
    private int stage;
    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            SetProcess(false);
            return;
        }

        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        moveAuthor = GetNode<CharacterSabotageActionMoveRequestController>(
            "../CharacterSabotageActionMoveRequestController");
        authors = GetAuthors();
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        actionModels = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        actionTask = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        actionFlow = GetNode<CharacterSabotageActionRequestConsiderationFlowController>(
            "../CharacterSabotageActionRequestConsiderationFlowController");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        specificRequests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        moveModels = GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
            "../TemporaryCharacterObjectSabotageMoveModelSource");
        moveTask = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../CharacterObjectSabotageMoveTaskController");
        specificStart = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        specificResult = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        specificOutcome = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeRecoveryCoordinator>(
            "../CharacterSabotageOutcomeRecoveryCoordinator");
        terminalRoute = GetNode<CharacterSabotageTerminalRouteCoordinator>(
            "../CharacterSabotageTerminalRouteCoordinator");
        turnCompletion = GetNode<CharacterSabotageTurnCompletionCoordinator>(
            "../CharacterSabotageTurnCompletionCoordinator");
        turnAdmission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        turnBoundary = GetNode<CharacterSabotageOrdinaryTurnBoundaryCoordinator>(
            "../CharacterSabotageOrdinaryTurnBoundaryCoordinator");
        schedulingAuthorization =
            GetNode<CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator");
        sabotageCycleBoundary = GetNode<CharacterSabotageCycleBoundaryCoordinator>(
            "../CharacterSabotageCycleBoundaryCoordinator");
        ordinaryTurns = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        ordinaryTurnSource = GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource");
        actionModel = new();
        moveModel = new();
        SetProcess(true);
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene move recovery exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartActionSelection();
        }
        else if (stage == 1)
        {
            StartMoveSelection();
        }
        else if (stage == 2)
        {
            RecoverMove();
        }
        else
        {
            Fail("Main-scene move recovery entered an unknown stage.");
        }
    }
    private void StartActionSelection()
    {
        bool inert = authors.All(author => author.Request is null)
            && recovery.AuthorRequest is null
            && recovery.EvidenceView is null
            && recovery.RecoveredConcealment is null
            && recovery.RecoveredPlacements is null
            && !recovery.TryRecoverMove(null, null, null)
            && StateIsInitial();
        bool authored = moveAuthor.TryCreate(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            source.Placements,
            source.Evidence,
            source.Destination,
            source.Candidate);
        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        bool composed = composer.TryCompose(source.Character, outputs);
        bool started = composer.Request is { } request
            && actionModels.TrySupplyModel(actionModel)
            && actionFlow.TryStart(request);

        if (!inert
            || !authored
            || outputs.Count(output => output is not null) != 1
            || !ReferenceEquals(outputs[1], moveAuthor.Request)
            || !composed
            || !started
            || actionTask.Status != CharacterSabotageActionTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene move author did not start one exact inert generic selection.");
            return;
        }

        stage = 1;
    }
    private void StartMoveSelection()
    {
        if (actionTask.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (actionTask.Status != CharacterSabotageActionTaskStatus.Completed
            || actionTask.Result is not { } result
            || actionModel.CallCount != 1
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Move)
            || !selection.TryObserveCompletedResult()
            || !binding.TryBindObservedSelection()
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageMoveCandidate> selected
            || !ReferenceEquals(selected.Request, moveAuthor.Request)
            || !ReferenceEquals(selected.Candidate, source.Candidate)
            || !binding.TryAcknowledgeBoundResult()
            || !specificRequests.TryCompose(
                binding.SelectedAuthorRequest,
                binding.SelectedCandidateBinding)
            || specificRequests.MoveRequest is not { } moveRequest
            || !ReferenceEquals(moveRequest.Placements, source.Placements)
            || !ReferenceEquals(moveRequest.Evidence, source.Evidence)
            || !ReferenceEquals(moveRequest.Candidates.Single(), source.Candidate)
            || !moveModels.TrySupplyModel(moveModel)
            || !specificStart.TryStart()
            || moveTask.Status != CharacterObjectSabotageMoveTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene generic move selection did not start its exact typed request.");
            return;
        }
        stage = 2;
    }

    private void RecoverMove()
    {
        if (moveTask.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }
        CharacterEvidenceViewSnapshot view = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.Evidence.EvidenceId]);
        CharacterEvidenceViewSnapshot rebuiltView = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.Evidence.EvidenceId]);
        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            source.Character,
            [CharacterSabotageActionKind.Move]);
        Character foreignActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            source.Candidate,
            source.RecoveryCandidate.SourcePlacement);
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            source.Character,
            binding.SelectedAuthorRequest!,
            CharacterSabotageActionKind.Move);
        CharacterSabotageTurnCompletion foreignActorCompletion = new(
            foreignActor,
            binding.SelectedAuthorRequest!,
            CharacterSabotageActionKind.Move);
        CharacterSabotageTurnCompletion rebuiltAuthorCompletion = new(
            source.Character,
            rebuiltAuthor,
            CharacterSabotageActionKind.Move);
        CharacterSabotageTurnCompletion wrongKindCompletion = new(
            source.Character,
            binding.SelectedAuthorRequest!,
            CharacterSabotageActionKind.Conceal);

        if (moveTask.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || moveTask.Result is not { SelectedCandidate: { } selected }
            || !ReferenceEquals(selected, source.Candidate)
            || moveModel.CallCount != 1
            || !specificResult.TryObserveCompletedResult()
            || !specificResult.TryAcknowledgeCompletedResult()
            || !specificOutcome.TryBindAcknowledgedOutcome()
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, view)
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.MoveCandidate, source.RecoveryCandidate)
            || recovery.TryRecoverMove(
                rebuiltAuthor,
                eligibility.MoveCandidate,
                eligibility.EvidenceView)
            || recovery.TryRecoverMove(
                eligibility.AuthorRequest,
                rebuiltCandidate,
                eligibility.EvidenceView)
            || recovery.TryRecoverMove(
                eligibility.AuthorRequest,
                eligibility.MoveCandidate,
                rebuiltView)
            || !recovery.TryRecoverMove(
                eligibility.AuthorRequest,
                eligibility.MoveCandidate,
                eligibility.EvidenceView)
            || recovery.RecoveredPlacements is not { } recovered
            || !ReferenceEquals(moveState.Placements, recovered)
            || recovered.Entries.Count != 1
            || !ReferenceEquals(recovered.Entries.Single(), source.RecoveryCandidate.SourcePlacement)
            || !ReferenceEquals(moveState.Evidence, specificOutcome.MoveEvidence)
            || actionModel.CallCount != 1
            || moveModel.CallCount != 1
            || !terminalRoute.TryRetain(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || !ReferenceEquals(terminalRoute.RecoveredPlacements, recovered)
            || terminalRoute.TerminalOutputCount != 1
            || terminalRoute.TryRetain(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || turnCompletion.TryComplete(
                source.Character,
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || terminalRoute.TryReset(rebuiltAuthor, CharacterSabotageActionKind.Move)
            || terminalRoute.TryReset(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Conceal)
            || !terminalRoute.TryReset(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || terminalRoute.AuthorRequest is not null
            || terminalRoute.ActionKind is not null
            || terminalRoute.RecoveredPlacements is not null
            || terminalRoute.TerminalOutputCount != 0
            || !ReferenceEquals(
                terminalRoute.ResetAuthorRequest,
                binding.SelectedAuthorRequest)
            || !ReferenceEquals(
                terminalRoute.ResetActionKind,
                CharacterSabotageActionKind.Move)
            || !ReferenceEquals(recovery.RecoveredPlacements, recovered)
            || !ReferenceEquals(moveState.Placements, recovered)
            || turnCompletion.TryComplete(
                null,
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || turnCompletion.TryComplete(
                source.Character,
                null,
                CharacterSabotageActionKind.Move)
            || turnCompletion.TryComplete(
                source.Character,
                rebuiltAuthor,
                CharacterSabotageActionKind.Move)
            || turnCompletion.TryComplete(
                foreignActor,
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || turnCompletion.TryComplete(
                source.Character,
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Conceal)
            || !turnCompletion.TryComplete(
                source.Character,
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || turnCompletion.Completion is not { } completion
            || !ReferenceEquals(completion.Actor, source.Character)
            || !ReferenceEquals(completion.AuthorRequest, binding.SelectedAuthorRequest)
            || !ReferenceEquals(completion.ActionKind, CharacterSabotageActionKind.Move)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(null, completion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(completion, null)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                rebuiltCompletion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                foreignActorCompletion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                rebuiltAuthorCompletion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                wrongKindCompletion)
            || !CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                completion)
            || turnAdmission.TryAdmit(null)
            || turnAdmission.TryAdmit(rebuiltCompletion)
            || turnAdmission.TryAdmit(foreignActorCompletion)
            || turnAdmission.TryAdmit(rebuiltAuthorCompletion)
            || turnAdmission.TryAdmit(wrongKindCompletion)
            || turnAdmission.Admission is not null
            || !ReferenceEquals(turnCompletion.Completion, completion)
            || !ReferenceEquals(recovery.RecoveredPlacements, recovered)
            || !ReferenceEquals(moveState.Placements, recovered)
            || actionModel.CallCount != 1
            || moveModel.CallCount != 1
            || !turnAdmission.TryAdmit(completion)
            || !ReferenceEquals(turnAdmission.Admission, completion)
            || turnAdmission.TryAdmit(completion)
            || !ReferenceEquals(turnAdmission.Admission, completion)
            || !ReferenceEquals(turnCompletion.Completion, completion)
            || !ReferenceEquals(recovery.RecoveredPlacements, recovered)
            || !ReferenceEquals(moveState.Placements, recovered)
            || actionModel.CallCount != 1
            || moveModel.CallCount != 1
            || turnBoundary.TryBind(null, ordinaryTurns.Activity)
            || turnBoundary.TryBind(rebuiltCompletion, ordinaryTurns.Activity)
            || turnBoundary.TryBind(completion, ordinaryTurnSource.Activity)
            || turnBoundary.Admission is not null
            || turnBoundary.TurnActivity is not null
            || ordinaryTurns.Activity is not null
            || !turnBoundary.TryBind(completion, ordinaryTurns.Activity)
            || !ReferenceEquals(turnBoundary.Admission, completion)
            || turnBoundary.TurnActivity is not null
            || turnBoundary.TryBind(completion, ordinaryTurns.Activity)
            || !ReferenceEquals(turnBoundary.Admission, completion)
            || turnBoundary.TurnActivity is not null
            || ordinaryTurns.Activity is not null
            || CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                null, completion, null, null)
            || CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                turnBoundary.Admission, rebuiltCompletion, null, null)
            || CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                turnBoundary.Admission, completion, null, ordinaryTurnSource.Activity)
            || CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                turnBoundary.Admission, completion,
                ordinaryTurnSource.Activity, ordinaryTurnSource.Activity)
            || !CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                turnBoundary.Admission, completion, null, null)
            || schedulingAuthorization.TryAuthorize(null, ordinaryTurns.Activity)
            || schedulingAuthorization.TryAuthorize(rebuiltCompletion, ordinaryTurns.Activity)
            || schedulingAuthorization.TryAuthorize(completion, ordinaryTurnSource.Activity)
            || schedulingAuthorization.Authorization is not null
            || !schedulingAuthorization.TryAuthorize(completion, ordinaryTurns.Activity)
            || schedulingAuthorization.Authorization is not { } authorization
            || !ReferenceEquals(authorization.Admission, completion)
            || authorization.TurnActivity is not null
            || schedulingAuthorization.TryAuthorize(completion, ordinaryTurns.Activity)
            || !ReferenceEquals(schedulingAuthorization.Authorization, authorization)
            || sabotageCycleBoundary.TryEnter(null)
            || sabotageCycleBoundary.TryEnter(
                new CharacterSabotageOrdinaryTurnSchedulingAuthorization(completion, null))
            || sabotageCycleBoundary.Boundary is not null
            || !sabotageCycleBoundary.TryEnter(authorization)
            || sabotageCycleBoundary.Boundary is not { } cycleBoundary
            || !ReferenceEquals(cycleBoundary.Authorization, authorization)
            || sabotageCycleBoundary.TryEnter(authorization)
            || !ReferenceEquals(sabotageCycleBoundary.Boundary, cycleBoundary)
            || !CharacterSabotageActionMoveAuthoringSmokeVerifier.PassesMainScene(
                this,
                cycleBoundary,
                source,
                moveState,
                ordinaryTurnSource.Activity)
            || !ReferenceEquals(moveAuthor.Request, binding.SelectedAuthorRequest)
            || actionModel.CallCount != 1 || moveModel.CallCount != 1
            || !ReferenceEquals(turnBoundary.Admission, completion)
            || turnBoundary.TurnActivity is not null
            || ordinaryTurns.Activity is not null
            || turnCompletion.TryComplete(
                source.Character,
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || terminalRoute.TryReset(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || terminalRoute.TryRetain(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Move)
            || recovery.TryRecoverMove(
                eligibility.AuthorRequest,
                eligibility.MoveCandidate,
                eligibility.EvidenceView))
        {
            Fail("Main-scene exact eligible move did not recover once without another model call.");
            return;
        }

        GD.Print(
            "Main-scene sabotage outcome move recovery smoke passed: ordinary startup stayed inert; "
            + "the first Move completed its exact recovery and terminal route; its completion passed "
            + "the pure scheduling gate and explicit cycle boundary; the admitted follow-up Move was "
            + "authored, selected, applied, recovered, routed, and completed through exact retained "
            + "tokens; its empty-turn authorization entered a second explicit cycle boundary; that "
            + "admission guarded a third eligible Move author and one fresh generic decision; only "
            + "that exact result was observed, bound to the third author's candidate, and acknowledged "
            + "while resetting only its task and leaving authoritative state unchanged, the ordinary "
            + "slot empty, and automatic sabotage disabled.");
        GetTree().Quit();
    }
    private ICharacterSabotageActionRequestAuthor[] GetAuthors() =>
    [
        GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController"),
        moveAuthor,
        GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController"),
        GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController"),
        GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController"),
        GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController"),
        GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private bool StateIsInitial() =>
        ReferenceEquals(moveState.Placements, source.Placements)
        && ReferenceEquals(moveState.Evidence, source.Evidence)
        && source.Placements.Entries.Count == 1
        && source.Evidence.Entries.Count == 0;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
