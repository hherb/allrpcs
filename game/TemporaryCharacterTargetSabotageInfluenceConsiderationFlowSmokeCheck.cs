using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit influence consideration through one isolated scene flow.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceConsiderationFlowSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageInfluenceModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceTaskController task = null!;
    private TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController flow = null!;
    private CharacterTargetSabotageInfluenceConsiderationRequest request = null!;
    private CharacterTargetSabotageInfluenceCandidate candidate = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel overlapModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController");
        request = CreateRequest(out candidate);
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
            Fail("Influence consideration flow exceeded its bounded smoke duration.");
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
                Fail("Influence consideration flow entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageInfluenceTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart(request);

        if (!inertStartup
            || !started
            || !overlapPreserved
            || !ThrowsArgumentNull(() => flow.TryStart(null!))
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Influence consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, candidate)
            || overlapModel.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Influence consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || overlapModel.CallCount != 1
            || !ReferenceEquals(overlapModel.Request, request)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Influence consideration failed its preserved one-shot abstention.");
            return;
        }

        stage = 3;
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
            || overlapModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Influence consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Influence consideration flow smoke passed: inert startup, exact request and one-shot "
            + "selection, overlap preservation, explicit reset, stable offline failure, closed "
            + "surface, and unchanged relationship state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController);
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
        && ReferenceEquals(request.Actor.Identity.Id, candidate.Occurrence.Proposal.ActorId)
        && request.Relationship.Dimensions.Single().Value == 10
        && request.Relationship.Reasons.Count == 0;

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
                    new RelationshipReasonId("influence-attempt"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId("enemy-agent-influence"),
                    "The enemy agent attempted to gain influence.")));
        CharacterInteractionAccessSnapshot access = new(actor.Identity.Id, [target.Identity.Id]);
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        return new CharacterTargetSabotageInfluenceConsiderationRequest(
            actor,
            access,
            relationship,
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
        public int CallCount { get; private set; }

        public CharacterTargetSabotageInfluenceConsiderationRequest? Request { get; private set; }

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
