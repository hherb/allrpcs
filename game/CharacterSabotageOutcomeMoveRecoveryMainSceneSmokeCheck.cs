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
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            source.Candidate,
            source.RecoveryCandidate.SourcePlacement);

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
            "Main-scene sabotage outcome move recovery smoke passed: ordinary startup stayed "
            + "inert; one exact live move selection retained its author, typed candidate, "
            + "acknowledgement, outcome, evidence-view, and source-placement tokens; and the "
            + "existing recovery transition restored that placement once while preserving "
            + "authoritative evidence and both model call counts, then the exact terminal route "
            + "was retained and explicitly reset once without clearing its route owner or "
            + "scheduling another action.");
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

    private sealed class RecordingActionModel : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    request.Actor.Identity.Id,
                    CharacterSabotageActionKind.Move));
        }
    }

    private sealed class RecordingMoveModel : ICharacterObjectSabotageMoveDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterObjectSabotageMoveDecision(
                    request.Character.Identity.Id,
                    request.Candidates.Single().Evidence.Evidence.EvidenceId));
        }
    }
}
