using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact eligible delay outcome through completed recovery routing.</summary>
public partial class CharacterSabotageOutcomeDelayRecoveryRequestMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_OUTCOME_DELAY_RECOVERY_REQUEST_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private CharacterSabotageActionDelayRequestController delayAuthor = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource actionModels = null!;
    private CharacterSabotageActionTaskController actionTask = null!;
    private CharacterSabotageActionRequestConsiderationFlowController actionFlow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator specificRequests = null!;
    private TemporaryCharacterTargetSabotageDelayModelSource delayModels = null!;
    private CharacterTargetSabotageDelayTaskController delayTask = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator specificStart = null!;
    private CharacterSabotageActionSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator specificOutcome = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterSabotageOutcomeDelayRecoveryCoordinator recovery = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateController recoveryCandidates = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController recoveryRequests = null!;
    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource recoveryModels = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController recoveryTask = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterTargetSabotageDelayConsiderationRequest request = null!;
    private CharacterTargetSabotageDelayCandidate candidate = null!;
    private WorldEvent recoveryEvent = null!;
    private RecordingActionModel actionModel = null!;
    private RecordingDelayModel delayModel = null!;
    private CharacterSabotageOutcomeDelayRecoverySelectingModel recoveryModel = null!;
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

        delayAuthor = GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController");
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
        delayModels = GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
            "../TemporaryCharacterTargetSabotageDelayModelSource");
        delayTask = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        specificStart = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        specificResult = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        specificOutcome = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeDelayRecoveryCoordinator>(
            "../CharacterSabotageOutcomeDelayRecoveryCoordinator");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        recoveryCandidates = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
        recoveryRequests =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        recoveryModels = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        recoveryTask = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        request = CreateRequest(out candidate, out recoveryEvent);
        actionModel = new();
        delayModel = new();
        recoveryModel = new();
        if (!delayState.TryInitialize(request.EventLog, request.WorldTime))
        {
            Fail("Main-scene delay recovery request rejected exact authority initialization.");
            return;
        }

        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene delay recovery request exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartActionSelection();
        }
        else if (stage == 1)
        {
            StartDelaySelection();
        }
        else if (stage == 2)
        {
            ComposeRecoveryRequest();
        }
        else if (stage == 3)
        {
            StartRecoveryRequest();
        }
        else if (stage == 4)
        {
            CompleteRecoveryRoute();
        }
        else
        {
            Fail("Main-scene delay recovery request entered an unknown stage.");
        }
    }

    private void StartActionSelection()
    {
        bool inert = authors.All(author => author.Request is null)
            && recovery.AuthorRequest is null
            && recovery.Request is null
            && recoveryCandidates.Candidate is null
            && recoveryRequests.Request is null
            && recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && recoveryState.EventLog is null
            && recoveryState.WorldTime is null
            && recoveryState.LatestAppliedResult is null
            && recovery.CompletedResult is null
            && recovery.RecoveredResult is null
            && !recovery.TryCompose(null, null)
            && !recovery.TryComplete(null, null)
            && StateIsInitial();
        bool authored = delayAuthor.TryCreate(
            request,
            request.Actor,
            request.InteractionAccess,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            candidate);
        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        bool composed = composer.TryCompose(request.Actor, outputs);
        bool started = composer.Request is { } actionRequest
            && actionModels.TrySupplyModel(actionModel)
            && actionFlow.TryStart(actionRequest);

        if (!inert
            || !authored
            || outputs.Count(output => output is not null) != 1
            || !ReferenceEquals(outputs[5], delayAuthor.Request)
            || !composed
            || !started
            || actionTask.Status != CharacterSabotageActionTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene delay author did not start one exact inert generic selection.");
            return;
        }

        stage = 1;
    }

    private void StartDelaySelection()
    {
        if (actionTask.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (actionTask.Status != CharacterSabotageActionTaskStatus.Completed
            || actionTask.Result is not { } actionResult
            || actionModel.CallCount != 1
            || !ReferenceEquals(actionResult.SelectedActionKind, CharacterSabotageActionKind.Delay)
            || !selection.TryObserveCompletedResult()
            || !binding.TryBindObservedSelection()
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
                    selected
            || !ReferenceEquals(selected.Request, delayAuthor.Request)
            || !ReferenceEquals(selected.Candidate, candidate)
            || !binding.TryAcknowledgeBoundResult()
            || !specificRequests.TryCompose(
                binding.SelectedAuthorRequest,
                binding.SelectedCandidateBinding)
            || specificRequests.DelayRequest is not { } delayRequest
            || !ReferenceEquals(delayRequest.WorldEvents, request.WorldEvents)
            || !ReferenceEquals(delayRequest.EventLog, request.EventLog)
            || !ReferenceEquals(delayRequest.WorldTime, request.WorldTime)
            || !ReferenceEquals(delayRequest.Candidates.Single(), candidate)
            || !delayModels.TrySupplyModel(delayModel)
            || !specificStart.TryStart()
            || delayTask.Status != CharacterTargetSabotageDelayTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene generic delay selection did not start its exact typed request.");
            return;
        }

        stage = 2;
    }

    private void ComposeRecoveryRequest()
    {
        if (delayTask.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            request.Actor,
            [CharacterSabotageActionKind.Delay]);
        if (delayTask.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || delayTask.Result is not { SelectedCandidate: { } selected }
            || !ReferenceEquals(selected, candidate)
            || delayModel.CallCount != 1
            || !specificResult.TryObserveCompletedResult()
            || !specificResult.TryAcknowledgeCompletedResult()
            || !specificOutcome.TryBindAcknowledgedOutcome()
            || specificOutcome.Delay is not { } outcome
            || !recoveryCandidates.TryCreate(request.WorldEvents, outcome, recoveryEvent)
            || recoveryCandidates.Candidate is not { } recoveryCandidate
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, null)
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.DelayCandidate, recoveryCandidate)
            || recovery.TryCompose(
                rebuiltAuthor,
                eligibility.DelayCandidate)
            || recovery.TryCompose(
                eligibility.AuthorRequest,
                new CharacterTargetSabotageDelayRecoveryCandidate(outcome, recoveryEvent))
            || !recovery.TryCompose(
                eligibility.AuthorRequest,
                eligibility.DelayCandidate)
            || recovery.Request is not { } recoveryRequest
            || !ReferenceEquals(recoveryRequests.Request, recoveryRequest)
            || !ReferenceEquals(recoveryRequest.Actor, candidate.Occurrence.Target)
            || !ReferenceEquals(recoveryRequest.WorldEvents, request.WorldEvents)
            || !ReferenceEquals(recoveryRequest.EventLog, outcome.EventLog)
            || !ReferenceEquals(recoveryRequest.WorldTime, outcome.WorldTime)
            || !ReferenceEquals(recoveryRequest.Candidates.Single(), recoveryCandidate)
            || !ReferenceEquals(delayState.LatestAppliedResult, outcome)
            || !ReferenceEquals(delayState.EventLog, outcome.EventLog)
            || !ReferenceEquals(delayState.WorldTime, outcome.WorldTime)
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            || recoveryTask.Result is not null
            || recoveryState.EventLog is not null
            || recoveryState.WorldTime is not null
            || recoveryState.LatestAppliedResult is not null
            || actionModel.CallCount != 1
            || delayModel.CallCount != 1
            || recovery.TryCompose(
                eligibility.AuthorRequest,
                eligibility.DelayCandidate))
        {
            Fail("Main-scene exact eligible delay did not compose one inert recovery request.");
            return;
        }

        stage = 3;
    }

    private void StartRecoveryRequest()
    {
        CharacterTargetSabotageDelayRecoveryConsiderationRequest requestToStart =
            recovery.Request!;
        CharacterTargetSabotageDelayRecoveryConsiderationRequest rebuiltRequest = new(
            requestToStart.Actor,
            requestToStart.WorldEvents,
            requestToStart.EventLog,
            requestToStart.WorldTime,
            requestToStart.Candidates);
        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            request.Actor,
            [CharacterSabotageActionKind.Delay]);

        if (!recoveryModels.TrySupplyModel(recoveryModel)
            || recovery.TryComplete(recovery.AuthorRequest, requestToStart)
            || recovery.TryStart(rebuiltAuthor, requestToStart)
            || recovery.TryStart(recovery.AuthorRequest, rebuiltRequest)
            || recoveryModel.CallCount != 0
            || !recovery.TryStart(recovery.AuthorRequest, requestToStart)
            || !ReferenceEquals(recovery.StartedRequest, requestToStart)
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            || recoveryTask.Result is not null
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(recoveryModel.Request, requestToStart)
            || !ReferenceEquals(recoveryState.EventLog, requestToStart.EventLog)
            || !ReferenceEquals(recoveryState.WorldTime, requestToStart.WorldTime)
            || recoveryState.LatestAppliedResult is not null
            || !ReferenceEquals(
                delayState.LatestAppliedResult,
                requestToStart.Candidates[0].DelayResult)
            || !ReferenceEquals(delayState.EventLog, requestToStart.EventLog)
            || !ReferenceEquals(delayState.WorldTime, requestToStart.WorldTime)
            || actionModel.CallCount != 1
            || delayModel.CallCount != 1
            || recovery.TryStart(recovery.AuthorRequest, requestToStart)
            || recoveryModel.CallCount != 1)
        {
            Fail("Main-scene exact composed delay recovery request did not start once.");
            return;
        }

        stage = 4;
    }

    private void CompleteRecoveryRoute()
    {
        if (recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        CharacterTargetSabotageDelayRecoveryConsiderationRequest started = recovery.StartedRequest!;
        CharacterTargetSabotageDelayRecoveryConsiderationRequest rebuilt = new(
            started.Actor,
            started.WorldEvents,
            started.EventLog,
            started.WorldTime,
            started.Candidates);
        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            request.Actor,
            [CharacterSabotageActionKind.Delay]);
        if (recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || recoveryTask.Result is not { } completed
            || recoveryState.LatestAppliedResult is not { } applied)
        {
            Fail("Main-scene exact delay recovery did not complete through its resolution owner.");
            return;
        }

        if (!ReferenceEquals(completed.Request, started)
            || !ReferenceEquals(completed.SelectedCandidate, started.Candidates.Single())
            || !ReferenceEquals(applied.Candidate, completed.SelectedCandidate)
            || recovery.TryComplete(rebuiltAuthor, started)
            || recovery.TryComplete(recovery.AuthorRequest, rebuilt)
            || !recovery.TryComplete(recovery.AuthorRequest, started)
            || !ReferenceEquals(recovery.CompletedResult, completed)
            || !ReferenceEquals(recovery.RecoveredResult, applied)
            || !ReferenceEquals(recovery.RecoveredEventLog, applied.EventLog)
            || !ReferenceEquals(recovery.RecoveredWorldTime, applied.WorldTime)
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            || recoveryTask.Result is not null
            || !ReferenceEquals(recoveryState.EventLog, applied.EventLog)
            || !ReferenceEquals(recoveryState.WorldTime, applied.WorldTime)
            || applied.EventLog.Entries.Count != started.EventLog.Entries.Count + 1
            || !ReferenceEquals(applied.EventLog.Entries[^1], recoveryEvent.EventId)
            || !ReferenceEquals(applied.WorldTime.CurrentEventId, recoveryEvent.EventId)
            || recovery.TryComplete(recovery.AuthorRequest, started)
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(delayState.LatestAppliedResult, started.Candidates[0].DelayResult))
        {
            Fail("Main-scene exact completed delay recovery did not route and acknowledge once.");
            return;
        }

        GD.Print(
            "Main-scene sabotage outcome delay recovery smoke passed: ordinary startup "
            + "stayed inert; one exact live delay selection retained its author, typed candidate, "
            + "acknowledgement, applied outcome, authority, and eligibility tokens; its exact recovery "
            + "request completed through the existing resolution owner; and the generic route retained "
            + "and acknowledged that one exact applied recovery without consuming another model.");
        GetTree().Quit();
    }

    private ICharacterSabotageActionRequestAuthor[] GetAuthors() =>
    [
        GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController"),
        GetNode<CharacterSabotageActionMoveRequestController>(
            "../CharacterSabotageActionMoveRequestController"),
        GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController"),
        GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController"),
        GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController"),
        delayAuthor,
        GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private bool StateIsInitial() =>
        ReferenceEquals(delayState.EventLog, request.EventLog)
        && ReferenceEquals(delayState.WorldTime, request.WorldTime)
        && delayState.LatestAppliedResult is null
        && request.EventLog.Entries.Count == 0
        && request.WorldTime.CurrentEventId is null;

    private static CharacterTargetSabotageDelayConsiderationRequest CreateRequest(
        out CharacterTargetSabotageDelayCandidate candidate,
        out WorldEvent recoveryEvent)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage-generic"),
            WorldEventTimeEffect.Advance);
        recoveryEvent = new WorldEvent(
            new WorldEventId("sage-generic-delay-recovered"),
            WorldEventTimeEffect.Advance);
        candidate = new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent);
        return new CharacterTargetSabotageDelayConsiderationRequest(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([delayEvent, recoveryEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            [candidate]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingActionModel : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    value.Actor.Identity.Id,
                    CharacterSabotageActionKind.Delay));
        }
    }

    private sealed class RecordingDelayModel : ICharacterTargetSabotageDelayDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterTargetSabotageDelayDecision(
                    value.Actor.Identity.Id,
                    value.Candidates.Single()));
        }
    }

}
