using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit influence-recovery consideration through one isolated scene flow.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowSmokeCheck
    : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;
    private TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController flow = null!;
    private CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidate candidate = null!;
    private CharacterRelationshipReason influenceReason = null!;
    private CharacterRelationshipReason recoveryReason = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel overlapModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController");
        request = CreateRequest(
            out candidate,
            out influenceReason,
            out recoveryReason);
        selectionModel = new RecordingModel(value => new(value.Actor.Identity.Id, candidate));
        overlapModel = new RecordingModel(value => new(value.Actor.Identity.Id, null));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Influence-recovery consideration flow exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelectionAndStartAbstention();
                break;
            case 2:
                ObserveAbstentionAndStartOfflineFailure();
                break;
            case 3:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Influence-recovery consideration flow entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart(request);

        if (!inertStartup
            || !started
            || !overlapPreserved
            || !ThrowsArgumentNull(() => flow.TryStart(null!))
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Influence-recovery consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, candidate)
            || overlapModel.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Influence-recovery consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || overlapModel.CallCount != 1
            || !ReferenceEquals(overlapModel.Request, request)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Influence-recovery consideration failed its preserved one-shot abstention.");
            return;
        }

        stage = 3;
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
            || overlapModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Influence-recovery consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Influence-recovery consideration flow smoke passed: inert startup, exact request and "
            + "one-shot selection, overlap preservation, explicit reset, stable offline failure, "
            + "closed surface, and unchanged relationship state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(
            TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryStart", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.Relationship.Reasons.Single(), influenceReason)
        && ReferenceEquals(candidate.RecoveryChange.Reason, recoveryReason)
        && request.Relationship.Dimensions.Single().Value == 11;

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

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
        influenceReason = Reason("influence-attempt", "enemy-agent-influence");
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
        recoveryReason = Reason("influence-recovered", "sage-recovery");
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
        public int CallCount { get; private set; }

        public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? Request { get; private set; }

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
