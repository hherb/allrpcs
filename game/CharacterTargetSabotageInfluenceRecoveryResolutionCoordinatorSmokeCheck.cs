using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact completed influence-recovery resolution and acknowledgement.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryResolutionCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;
    private CharacterTargetSabotageInfluenceRecoveryController state = null!;
    private CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator coordinator = null!;
    private RecoveryInput input = null!;
    private CharacterTargetSabotageInfluenceRecoveryResult? firstAppliedResult;
    private CharacterRelationshipMemory? firstRestoredRelationship;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        state = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
        coordinator = GetNode<CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator");
        input = CreateInput();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Influence-recovery resolution exceeded its bounded smoke duration.");
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
                ObserveAbstentionAndStartStaleSelection();
                break;
            case 3:
                ObserveStaleSelection();
                break;
            default:
                Fail("Influence-recovery resolution entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool valid = state.TryInitialize(input.InfluencedRelationship)
            && !coordinator.TryResolveCompletedResult()
            && !coordinator.TryAcknowledgeCompletedResult()
            && HasClosedSurface()
            && task.TryStart(
                new ImmediateModel(Decision(input.FirstCandidate)),
                Request(input.InfluencedRelationship, input.FirstCandidate));
        if (!valid)
        {
            Fail("Influence-recovery resolution failed inert startup or selection start.");
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

        firstRestoredRelationship = state.Relationship;
        firstAppliedResult = state.LatestAppliedResult;
        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || firstRestoredRelationship is null
            || firstAppliedResult is null
            || !ReferenceEquals(firstAppliedResult.Relationship, firstRestoredRelationship)
            || !ReferenceEquals(firstAppliedResult.Candidate, input.FirstCandidate)
            || firstRestoredRelationship.Dimensions.Single().Value != 10
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            || !task.TryStart(
                new ImmediateModel(Decision(null)),
                Request(firstRestoredRelationship, input.IneligibleSecondCandidate)))
        {
            Fail("Influence-recovery resolution failed exact selection resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartStaleSelection()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || !ReferenceEquals(state.Relationship, firstRestoredRelationship)
            || !ReferenceEquals(state.LatestAppliedResult, firstAppliedResult)
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || !task.TryStart(
                new ImmediateModel(Decision(input.IneligibleSecondCandidate)),
                Request(input.InfluencedRelationship, input.IneligibleSecondCandidate)))
        {
            Fail("Influence-recovery resolution changed state for abstention.");
            return;
        }

        stage = 3;
    }

    private void ObserveStaleSelection()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || coordinator.TryResolveCompletedResult()
            || coordinator.TryResolveCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(state.Relationship, firstRestoredRelationship)
            || !ReferenceEquals(state.LatestAppliedResult, firstAppliedResult)
            || task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || !InputsAreUnchanged())
        {
            Fail("Influence-recovery resolution accepted stale relationship authority.");
            return;
        }

        GD.Print(
            "Influence-recovery resolution smoke passed: exact completed selection and abstention "
            + "resolve once, acknowledgement is explicit, stale authority fails closed, and "
            + "source graphs stay unchanged.");
        GetTree().Quit();
    }

    private CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request(
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate) =>
        new(input.Actor, relationship, [candidate]);

    private CharacterTargetSabotageInfluenceRecoveryDecision Decision(
        CharacterTargetSabotageInfluenceRecoveryCandidate? candidate) =>
        new(input.Actor.Identity.Id, candidate);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(
            ["TryAcknowledgeCompletedResult", "TryResolveCompletedResult", "_Process", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private bool InputsAreUnchanged() =>
        input.InfluencedRelationship.Dimensions.Single().Value == 11
        && input.InfluencedRelationship.Reasons.Count == 1
        && ReferenceEquals(
            input.FirstCandidate.InfluenceResult.Candidate.Occurrence.Proposal.ActorId,
            input.Enemy.Identity.Id);

    private static RecoveryInput CreateInput()
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
        CharacterRelationshipChange influenceChange = Change(
            actor.Identity.Id,
            enemy.Identity.Id,
            1,
            "influence-attempt",
            "enemy-agent-influence");
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory influencedRelationship =
            CharacterRelationshipChangeTransition.Apply(
                new CharacterRelationshipMemory(
                    actor.Identity.Id,
                    enemy.Identity.Id,
                    [new CharacterRelationshipDimensionValue(
                        CharacterRelationshipDimension.Trust,
                        10)],
                    []),
                influenceChange);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            influencedRelationship);
        CharacterTargetSabotageInfluenceRecoveryCandidate firstCandidate = new(
            influenceResult,
            Change(
                actor.Identity.Id,
                enemy.Identity.Id,
                -1,
                "influence-recovered",
                "sage-recovery"));
        CharacterTargetSabotageInfluenceRecoveryCandidate ineligibleSecondCandidate = new(
            influenceResult,
            Change(
                actor.Identity.Id,
                enemy.Identity.Id,
                -1,
                "later-recovery",
                "later-sage-recovery"));

        return new RecoveryInput(
            enemy,
            actor,
            influencedRelationship,
            firstCandidate,
            ineligibleSecondCandidate);
    }

    private static CharacterRelationshipChange Change(
        CharacterId characterId,
        CharacterId relatedCharacterId,
        int delta,
        string reasonId,
        string turnId) =>
        new(
            characterId,
            relatedCharacterId,
            delta,
            new CharacterRelationshipReason(
                new RelationshipReasonId(reasonId),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId(turnId),
                reasonId));

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(
        CharacterTargetSabotageInfluenceRecoveryDecision decision)
        : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed record RecoveryInput(
        Character Enemy,
        Character Actor,
        CharacterRelationshipMemory InfluencedRelationship,
        CharacterTargetSabotageInfluenceRecoveryCandidate FirstCandidate,
        CharacterTargetSabotageInfluenceRecoveryCandidate IneligibleSecondCandidate);
}
