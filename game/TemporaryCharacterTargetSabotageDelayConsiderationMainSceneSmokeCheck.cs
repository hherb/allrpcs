using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit target-delay consideration through main-scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageDelayConsiderationMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_TARGET_SABOTAGE_DELAY_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageDelayModelSource modelSource = null!;
    private CharacterTargetSabotageDelayTaskController task = null!;
    private TemporaryCharacterTargetSabotageDelayConsiderationFlowController flow = null!;
    private CharacterTargetSabotageDelayController state = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateController recoveryCandidates = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController recoveryRequests =
        null!;
    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource recoveryModelSource = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController recoveryFlow =
        null!;
    private CharacterTargetSabotageDelayRecoveryTaskController recoveryTask = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterTargetSabotageDelayRecoveryResolutionCoordinator recoveryCoordinator = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator coordinator = null!;
    private CharacterTargetSabotageDelayConsiderationRequest request = null!;
    private CharacterTargetSabotageDelayCandidate candidate = null!;
    private WorldEvent recoveryEvent = null!;
    private RecordingModel selectionModel = null!;
    private RecordingRecoveryModel recoverySelectionModel = null!;
    private WorldEventLogSnapshot? appliedEventLog;
    private WorldTimeSnapshot? appliedWorldTime;
    private CharacterTargetSabotageDelayResult? appliedResult;
    private CharacterTargetSabotageDelayRecoveryCandidate? recoveryCandidate;
    private CharacterTargetSabotageDelayRecoveryConsiderationRequest? recoveryRequest;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        if (!isEnabled)
        {
            SetProcess(false);
            return;
        }

        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
            "../TemporaryCharacterTargetSabotageDelayModelSource");
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageDelayConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageDelayConsiderationFlowController");
        state = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        recoveryCandidates = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
        recoveryRequests =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        recoveryModelSource = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        recoveryFlow =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController");
        recoveryTask = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        recoveryCoordinator =
            GetNode<CharacterTargetSabotageDelayRecoveryResolutionCoordinator>(
                "../CharacterTargetSabotageDelayRecoveryResolutionCoordinator");
        coordinator = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        request = CreateRequest(out candidate, out recoveryEvent);
        selectionModel = new(value => new(value.Actor.Identity.Id, candidate));
        recoverySelectionModel = new(value => new(
            value.Actor.Identity.Id,
            value.Candidates.Single()));
        if (!state.TryInitialize(request.EventLog, request.WorldTime))
        {
            Fail("Main-scene target-delay authority rejected exact smoke initialization.");
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
            Fail("Main-scene target-delay consideration exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelectionAndStartOfflineFailure();
                break;
            case 2:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Main-scene target-delay consideration entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && ReferenceEquals(state.EventLog, request.EventLog)
            && ReferenceEquals(state.WorldTime, request.WorldTime)
            && state.LatestAppliedResult is null
            && recoveryCandidates.Candidate is null
            && recoveryRequests.Request is null
            && recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && recoveryTask.Result is null
            && recoveryState.EventLog is null
            && recoveryState.WorldTime is null
            && recoveryState.LatestAppliedResult is null
            && recoverySelectionModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageDelayTaskStatus.Running;

        if (!inertStartup || !started || !InputsAreUnchanged())
        {
            Fail("Main-scene target-delay consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        appliedEventLog = state.EventLog;
        appliedWorldTime = state.WorldTime;
        appliedResult = state.LatestAppliedResult;
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, candidate)
            || appliedEventLog is null
            || appliedWorldTime is null
            || appliedResult is null
            || !ReferenceEquals(appliedResult.EventLog, appliedEventLog)
            || !ReferenceEquals(appliedResult.WorldTime, appliedWorldTime)
            || !ReferenceEquals(appliedResult.Candidate, candidate)
            || !ReferenceEquals(appliedEventLog.Entries.Single(), candidate.WorldEvent.EventId)
            || !ReferenceEquals(appliedWorldTime.CurrentEventId, candidate.WorldEvent.EventId)
            || !recoveryCandidates.TryCreate(request.WorldEvents, appliedResult, recoveryEvent)
            || recoveryCandidates.Candidate is not { } recoveryCandidate
            || !ReferenceEquals(recoveryCandidate.DelayResult, appliedResult)
            || !ReferenceEquals(recoveryCandidate.RecoveryEvent, recoveryEvent)
            || !recoveryRequests.TryCreate(request.WorldEvents, recoveryCandidate)
            || recoveryRequests.Request is not { } recoveryRequest
            || !ReferenceEquals(recoveryRequest.Actor, candidate.Occurrence.Target)
            || !ReferenceEquals(recoveryRequest.WorldEvents, request.WorldEvents)
            || !ReferenceEquals(recoveryRequest.EventLog, appliedEventLog)
            || !ReferenceEquals(recoveryRequest.WorldTime, appliedWorldTime)
            || recoveryRequest.Candidates.Count != 1
            || !ReferenceEquals(recoveryRequest.Candidates.Single(), recoveryCandidate)
            || !recoveryState.TryInitialize(recoveryRequest.EventLog, recoveryRequest.WorldTime)
            || !recoveryModelSource.TrySupplyModel(recoverySelectionModel)
            || !recoveryFlow.TryStart(recoveryRequest)
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            || !coordinator.TryAcknowledgeCompletedResult()
            || !flow.TryStart(request)
            || !InputsAreUnchanged())
        {
            Fail("Main-scene target-delay consideration failed its exact selected result.");
            return;
        }

        this.recoveryCandidate = recoveryCandidate;
        this.recoveryRequest = recoveryRequest;
        stage = 2;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        if (recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        WorldEventLogSnapshot? recoveredEventLog = recoveryState.EventLog;
        WorldTimeSnapshot? recoveredWorldTime = recoveryState.WorldTime;
        CharacterTargetSabotageDelayRecoveryResult? recoveryResult =
            recoveryState.LatestAppliedResult;
        bool exactRecovery = recoveryTask.Status
                == CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            && recoveryTask.Result is { } recoveryConsideration
            && ReferenceEquals(recoveryConsideration.Request, recoveryRequest)
            && ReferenceEquals(recoveryConsideration.SelectedCandidate, recoveryCandidate)
            && recoverySelectionModel.CallCount == 1
            && ReferenceEquals(recoverySelectionModel.Request, recoveryRequest)
            && recoveredEventLog is not null
            && recoveredWorldTime is not null
            && recoveryResult is not null
            && !ReferenceEquals(recoveredEventLog, appliedEventLog)
            && !ReferenceEquals(recoveredWorldTime, appliedWorldTime)
            && ReferenceEquals(recoveryResult.EventLog, recoveredEventLog)
            && ReferenceEquals(recoveryResult.WorldTime, recoveredWorldTime)
            && ReferenceEquals(recoveryResult.Candidate, recoveryCandidate)
            && recoveredEventLog.Entries.Count == 2
            && ReferenceEquals(recoveredEventLog.Entries[^1], recoveryEvent.EventId)
            && ReferenceEquals(recoveredWorldTime.CurrentEventId, recoveryEvent.EventId);
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || !exactRecovery
            || !ReferenceEquals(state.EventLog, appliedEventLog)
            || !ReferenceEquals(state.WorldTime, appliedWorldTime)
            || !ReferenceEquals(state.LatestAppliedResult, appliedResult)
            || !ReferenceEquals(recoveryCandidates.Candidate, recoveryCandidate)
            || !ReferenceEquals(recoveryRequests.Request, recoveryRequest)
            || !recoveryCoordinator.TryAcknowledgeCompletedResult()
            || !InputsAreUnchanged())
        {
            Fail("Main-scene target-delay consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Main-scene target-delay consideration smoke passed: dependency-ordered inert startup, "
            + "one explicit exact delay and recovery selection, candidate and request, guarded "
            + "acknowledgement, stable offline failure, and unchanged source world state.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.Actor.Identity.Id, candidate.Occurrence.Proposal.ActorId)
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
        WorldEvent worldEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        recoveryEvent = new WorldEvent(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        candidate = new CharacterTargetSabotageDelayCandidate(occurrence, worldEvent);
        return new CharacterTargetSabotageDelayConsiderationRequest(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([worldEvent, recoveryEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            [candidate]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterTargetSabotageDelayConsiderationRequest,
            CharacterTargetSabotageDelayDecision> decide)
        : ICharacterTargetSabotageDelayDecisionModel
    {
        internal int CallCount { get; private set; }

        internal CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }

    private sealed class RecordingRecoveryModel(
        Func<CharacterTargetSabotageDelayRecoveryConsiderationRequest,
            CharacterTargetSabotageDelayRecoveryDecision> decide)
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        internal int CallCount { get; private set; }

        internal CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request
        {
            get;
            private set;
        }

        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }
}
