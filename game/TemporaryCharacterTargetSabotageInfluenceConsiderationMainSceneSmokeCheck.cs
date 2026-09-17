using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit influence consideration through main-scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceConsiderationMainSceneSmokeCheck
    : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_TARGET_SABOTAGE_INFLUENCE_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageInfluenceModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceTaskController task = null!;
    private TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController flow = null!;
    private CharacterTargetSabotageInfluenceController state = null!;
    private CharacterTargetSabotageInfluenceResolutionCoordinator coordinator = null!;
    private CharacterTargetSabotageInfluenceConsiderationRequest request = null!;
    private CharacterTargetSabotageInfluenceCandidate candidate = null!;
    private RecordingModel selectionModel = null!;
    private CharacterRelationshipMemory? appliedRelationship;
    private CharacterTargetSabotageInfluenceResult? appliedResult;
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

        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController");
        state = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        coordinator = GetNode<CharacterTargetSabotageInfluenceResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceResolutionCoordinator");
        request = CreateRequest(out candidate);
        selectionModel = new(value => new(value.Actor.Identity.Id, candidate));
        if (!state.TryInitialize(request.Relationship))
        {
            Fail("Main-scene influence authority rejected exact smoke initialization.");
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
            Fail("Main-scene influence consideration exceeded its bounded smoke duration.");
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
                Fail("Main-scene influence consideration entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageInfluenceTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && ReferenceEquals(state.Relationship, request.Relationship)
            && state.LatestAppliedResult is null
            && InputsAreUnchanged();
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running;

        if (!inertStartup || !started || !InputsAreUnchanged())
        {
            Fail("Main-scene influence consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        appliedRelationship = state.Relationship;
        appliedResult = state.LatestAppliedResult;
        bool exactSelection = task.Status == CharacterTargetSabotageInfluenceTaskStatus.Completed
            && task.Result is { } result
            && selectionModel.CallCount == 1
            && ReferenceEquals(selectionModel.Request, request)
            && ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.SelectedCandidate, candidate)
            && appliedRelationship is not null
            && appliedResult is not null
            && !ReferenceEquals(appliedRelationship, request.Relationship)
            && ReferenceEquals(appliedResult.Relationship, appliedRelationship)
            && ReferenceEquals(appliedResult.Candidate, candidate)
            && appliedRelationship.Dimensions.Single().Value == 11
            && InputsAreUnchanged();
        if (!exactSelection)
        {
            Fail(
                "Main-scene influence consideration failed its exact selected result.");
            return;
        }

        if (!coordinator.TryAcknowledgeCompletedResult()
            || !flow.TryStart(request)
            || !InputsAreUnchanged())
        {
            Fail("Main-scene influence consideration failed reset or offline restart.");
            return;
        }

        stage = 2;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || !ReferenceEquals(state.Relationship, appliedRelationship)
            || !ReferenceEquals(state.LatestAppliedResult, appliedResult)
            || !InputsAreUnchanged())
        {
            Fail("Main-scene influence offline failure changed source state.");
            return;
        }

        GD.Print(
            "Main-scene influence consideration smoke passed: dependency-ordered inert startup, "
            + "one explicit exact applied selection, guarded acknowledgement, stable offline "
            + "failure, and unchanged source relationship state.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.Actor.Identity.Id, candidate.Occurrence.Proposal.ActorId)
        && request.Relationship.Dimensions.Single().Value == 10
        && request.Relationship.Reasons.Count == 0;

    private static CharacterTargetSabotageInfluenceConsiderationRequest CreateRequest(
        out CharacterTargetSabotageInfluenceCandidate candidate)
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
                CharacterSabotageActionKind.Influence),
            target);
        candidate = new CharacterTargetSabotageInfluenceCandidate(
            occurrence,
            new CharacterRelationshipChange(
                target.Identity.Id,
                actor.Identity.Id,
                1,
                new CharacterRelationshipReason(
                    new RelationshipReasonId("main-scene-influence-attempt"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId("main-scene-enemy-agent-influence"),
                    "The enemy agent attempted to gain influence.")));
        return new CharacterTargetSabotageInfluenceConsiderationRequest(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            [candidate]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterTargetSabotageInfluenceConsiderationRequest,
            CharacterTargetSabotageInfluenceDecision> decide)
        : ICharacterTargetSabotageInfluenceDecisionModel
    {
        internal int CallCount { get; private set; }

        internal CharacterTargetSabotageInfluenceConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }
}
