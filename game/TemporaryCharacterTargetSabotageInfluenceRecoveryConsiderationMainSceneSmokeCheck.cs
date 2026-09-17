using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit influence-recovery consideration through main-scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationMainSceneSmokeCheck
    : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_TARGET_SABOTAGE_INFLUENCE_RECOVERY_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;
    private TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController flow = null!;
    private CharacterTargetSabotageInfluenceRecoveryController state = null!;
    private CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator coordinator = null!;
    private CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidate candidate = null!;
    private CharacterRelationshipReason influenceReason = null!;
    private CharacterRelationshipReason recoveryReason = null!;
    private RecordingModel selectionModel = null!;
    private CharacterRelationshipMemory? restoredRelationship;
    private CharacterTargetSabotageInfluenceRecoveryResult? appliedResult;
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

        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController");
        state = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
        coordinator = GetNode<CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator");
        request = CreateRequest(out candidate, out influenceReason, out recoveryReason);
        selectionModel = new(value => new(value.Actor.Identity.Id, candidate));
        if (!state.TryInitialize(request.Relationship))
        {
            Fail("Main-scene influence-recovery authority rejected exact smoke initialization.");
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
            Fail("Main-scene influence-recovery consideration exceeded its bounded duration.");
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
                Fail("Main-scene influence-recovery consideration entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup =
            task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && ReferenceEquals(state.Relationship, request.Relationship)
            && state.LatestAppliedResult is null
            && InputsAreUnchanged();
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running;

        if (!inertStartup || !started || !InputsAreUnchanged())
        {
            Fail("Main-scene influence-recovery failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        restoredRelationship = state.Relationship;
        appliedResult = state.LatestAppliedResult;
        bool exactSelection = task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            && task.Result is { } result
            && selectionModel.CallCount == 1
            && ReferenceEquals(selectionModel.Request, request)
            && ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.SelectedCandidate, candidate)
            && restoredRelationship is not null
            && appliedResult is not null
            && !ReferenceEquals(restoredRelationship, request.Relationship)
            && ReferenceEquals(appliedResult.Relationship, restoredRelationship)
            && ReferenceEquals(appliedResult.Candidate, candidate)
            && restoredRelationship.Dimensions.Single().Value == 10
            && InputsAreUnchanged();
        if (!exactSelection
            || !coordinator.TryAcknowledgeCompletedResult()
            || !flow.TryStart(request))
        {
            Fail("Main-scene influence-recovery failed its exact selection or offline restart.");
            return;
        }

        stage = 2;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || !ReferenceEquals(state.Relationship, restoredRelationship)
            || !ReferenceEquals(state.LatestAppliedResult, appliedResult)
            || !InputsAreUnchanged())
        {
            Fail("Main-scene influence-recovery offline failure changed source state.");
            return;
        }

        GD.Print(
            "Main-scene influence-recovery consideration smoke passed: dependency-ordered inert "
            + "startup, one explicit exact applied selection, guarded acknowledgement, stable "
            + "offline failure, and unchanged source relationship state.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.Relationship.Reasons.Single(), influenceReason)
        && ReferenceEquals(candidate.RecoveryChange.Reason, recoveryReason)
        && request.Relationship.Dimensions.Single().Value == 11;

    private static CharacterTargetSabotageInfluenceRecoveryConsiderationRequest CreateRequest(
        out CharacterTargetSabotageInfluenceRecoveryCandidate candidate,
        out CharacterRelationshipReason influenceReason,
        out CharacterRelationshipReason recoveryReason)
    {
        Character enemy = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemy.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemy.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            actor);
        influenceReason = Reason("main-scene-influence-attempt", "main-scene-enemy-influence");
        CharacterRelationshipChange influenceChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            1,
            influenceReason);
        CharacterRelationshipMemory relationship = CharacterRelationshipChangeTransition.Apply(
            new CharacterRelationshipMemory(
                actor.Identity.Id,
                enemy.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            influenceChange);
        recoveryReason = Reason("main-scene-influence-recovered", "main-scene-sage-recovery");
        candidate = new CharacterTargetSabotageInfluenceRecoveryCandidate(
            new CharacterTargetSabotageInfluenceResult(
                new CharacterTargetSabotageInfluenceCandidate(occurrence, influenceChange),
                relationship),
            new CharacterRelationshipChange(
                actor.Identity.Id,
                enemy.Identity.Id,
                -1,
                recoveryReason));
        return new CharacterTargetSabotageInfluenceRecoveryConsiderationRequest(
            actor,
            relationship,
            [candidate]);
    }

    private static CharacterRelationshipReason Reason(string id, string turnId) =>
        new(
            new RelationshipReasonId(id),
            CharacterRelationshipDimension.Trust,
            new CharacterTurnId(turnId),
            id);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest,
            CharacterTargetSabotageInfluenceRecoveryDecision> decide)
        : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        internal int CallCount { get; private set; }

        internal CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? Request
        {
            get;
            private set;
        }

        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }
}
