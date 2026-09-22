using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit delay-recovery consideration through main-scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageDelayRecoveryConsiderationMainSceneSmokeCheck
    : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_TARGET_SABOTAGE_DELAY_RECOVERY_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;
    private TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController flow = null!;
    private CharacterTargetSabotageDelayRecoveryController state = null!;
    private CharacterTargetSabotageDelayRecoveryResolutionCoordinator coordinator = null!;
    private CharacterTargetSabotageDelayRecoveryConsiderationRequest request = null!;
    private CharacterTargetSabotageDelayRecoveryCandidate candidate = null!;
    private WorldEvent delayEvent = null!;
    private RecordingModel selectionModel = null!;
    private WorldEventLogSnapshot? recoveredEventLog;
    private WorldTimeSnapshot? recoveredWorldTime;
    private CharacterTargetSabotageDelayRecoveryResult? appliedResult;
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

        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController");
        state = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        coordinator = GetNode<CharacterTargetSabotageDelayRecoveryResolutionCoordinator>(
            "../CharacterTargetSabotageDelayRecoveryResolutionCoordinator");
        request = CreateRequest(out candidate, out delayEvent);
        selectionModel = new(value => new(value.Actor.Identity.Id, candidate));
        if (!state.TryInitialize(request.EventLog, request.WorldTime))
        {
            Fail("Main-scene delay-recovery authority rejected exact smoke initialization.");
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
            Fail("Main-scene delay-recovery consideration exceeded its bounded duration.");
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
                Fail("Main-scene delay-recovery consideration entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && ReferenceEquals(state.EventLog, request.EventLog)
            && ReferenceEquals(state.WorldTime, request.WorldTime)
            && state.LatestAppliedResult is null
            && InputsAreUnchanged();
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running;

        if (!inertStartup || !started || !InputsAreUnchanged())
        {
            Fail("Main-scene delay-recovery failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            || state.LatestAppliedResult is null)
        {
            return;
        }

        recoveredEventLog = state.EventLog;
        recoveredWorldTime = state.WorldTime;
        appliedResult = state.LatestAppliedResult;
        bool exactSelection = task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            && task.Result is { } result
            && selectionModel.CallCount == 1
            && ReferenceEquals(selectionModel.Request, request)
            && ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.SelectedCandidate, candidate)
            && recoveredEventLog is not null
            && recoveredWorldTime is not null
            && appliedResult is not null
            && !ReferenceEquals(recoveredEventLog, request.EventLog)
            && !ReferenceEquals(recoveredWorldTime, request.WorldTime)
            && ReferenceEquals(appliedResult.Candidate, candidate)
            && ReferenceEquals(appliedResult.EventLog, recoveredEventLog)
            && ReferenceEquals(appliedResult.WorldTime, recoveredWorldTime)
            && ReferenceEquals(recoveredEventLog.Entries[^1], candidate.RecoveryEvent.EventId)
            && ReferenceEquals(recoveredWorldTime.CurrentEventId, candidate.RecoveryEvent.EventId)
            && InputsAreUnchanged();
        if (!exactSelection
            || !coordinator.TryAcknowledgeCompletedResult()
            || !flow.TryStart(request))
        {
            Fail("Main-scene delay-recovery failed its exact selection or offline restart.");
            return;
        }

        stage = 2;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || !ReferenceEquals(state.EventLog, recoveredEventLog)
            || !ReferenceEquals(state.WorldTime, recoveredWorldTime)
            || !ReferenceEquals(state.LatestAppliedResult, appliedResult)
            || !InputsAreUnchanged())
        {
            Fail("Main-scene delay-recovery offline failure changed source world state.");
            return;
        }

        GD.Print(
            "Main-scene delay-recovery consideration smoke passed: dependency-ordered inert "
            + "startup, one explicit exact applied selection, guarded acknowledgement, stable "
            + "offline failure, and unchanged source world state.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.EventLog, candidate.DelayResult.EventLog)
        && ReferenceEquals(request.WorldTime, candidate.DelayResult.WorldTime)
        && ReferenceEquals(request.EventLog.Entries[^1], delayEvent.EventId)
        && ReferenceEquals(request.WorldTime.CurrentEventId, delayEvent.EventId);

    private static CharacterTargetSabotageDelayRecoveryConsiderationRequest CreateRequest(
        out CharacterTargetSabotageDelayRecoveryCandidate candidate,
        out WorldEvent delayEvent)
    {
        Character enemyAgent = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemyAgent.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemyAgent.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            actor);
        WorldEvent priorEvent = new(
            new WorldEventId("sage-began-round"),
            WorldEventTimeEffect.NoAdvance);
        delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            new WorldEventLogSnapshot([priorEvent.EventId, delayEvent.EventId]),
            new WorldTimeSnapshot(delayEvent.EventId));
        candidate = new CharacterTargetSabotageDelayRecoveryCandidate(
            delayResult,
            recoveryEvent);
        return new CharacterTargetSabotageDelayRecoveryConsiderationRequest(
            actor,
            new WorldEventSnapshot([priorEvent, delayEvent, recoveryEvent]),
            delayResult.EventLog,
            delayResult.WorldTime,
            [candidate]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
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
