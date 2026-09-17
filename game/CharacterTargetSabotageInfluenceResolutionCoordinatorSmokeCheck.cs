using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact completed influence resolution and acknowledgement.</summary>
public partial class CharacterTargetSabotageInfluenceResolutionCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageInfluenceTaskController task = null!;
    private CharacterTargetSabotageInfluenceController state = null!;
    private CharacterTargetSabotageInfluenceResolutionCoordinator coordinator = null!;
    private InfluenceInput input = null!;
    private CharacterTargetSabotageInfluenceResult? firstAppliedResult;
    private CharacterRelationshipMemory? firstChangedRelationship;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
        state = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        coordinator = GetNode<CharacterTargetSabotageInfluenceResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceResolutionCoordinator");
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
            Fail("Influence resolution coordinator exceeded its bounded smoke duration.");
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
                ObserveAbstentionAndStartSecondSelection();
                break;
            case 3:
                ObserveSecondSelectionAndInvalidateAcknowledgement();
                break;
            default:
                Fail("Influence resolution coordinator entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterTargetSabotageInfluenceConsiderationRequest request = Request(
            input.Relationship,
            input.FirstCandidate);
        bool valid = state.TryInitialize(input.Relationship)
            && !coordinator.TryResolveCompletedResult()
            && !coordinator.TryAcknowledgeCompletedResult()
            && HasClosedSurface()
            && task.TryStart(new ImmediateModel(Decision(input.FirstCandidate)), request);
        if (!valid)
        {
            Fail("Influence resolution coordinator failed inert startup or selection start.");
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

        firstChangedRelationship = state.Relationship;
        firstAppliedResult = state.LatestAppliedResult;
        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || firstChangedRelationship is null
            || firstAppliedResult is null
            || !ReferenceEquals(firstAppliedResult.Relationship, firstChangedRelationship)
            || !ReferenceEquals(firstAppliedResult.Candidate, input.FirstCandidate)
            || firstChangedRelationship.Dimensions.Single().Value != 11
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterTargetSabotageInfluenceTaskStatus.Idle
            || !task.TryStart(
                new ImmediateModel(Decision(null)),
                Request(firstChangedRelationship, input.SecondCandidate)))
        {
            Fail("Influence resolution coordinator failed exact selection resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartSecondSelection()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || !ReferenceEquals(state.Relationship, firstChangedRelationship)
            || !ReferenceEquals(state.LatestAppliedResult, firstAppliedResult)
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || !task.TryStart(
                new ImmediateModel(Decision(input.SecondCandidate)),
                Request(firstChangedRelationship!, input.SecondCandidate)))
        {
            Fail("Influence resolution coordinator changed state for abstention.");
            return;
        }

        stage = 3;
    }

    private void ObserveSecondSelectionAndInvalidateAcknowledgement()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        CharacterRelationshipMemory? resolvedRelationship = state.Relationship;
        CharacterTargetSabotageInfluenceResult? resolvedResult = state.LatestAppliedResult;
        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || resolvedRelationship is null
            || resolvedResult is null
            || ReferenceEquals(resolvedRelationship, firstChangedRelationship)
            || resolvedRelationship.Dimensions.Single().Value != 12)
        {
            Fail("Influence resolution coordinator failed its second exact selection.");
            return;
        }

        CharacterTargetSabotageInfluenceConsiderationResult external = new(
            Request(resolvedRelationship, input.ThirdCandidate),
            Decision(input.ThirdCandidate));
        if (!state.TryResolveConsideration(external)
            || ReferenceEquals(state.Relationship, resolvedRelationship)
            || state.Relationship?.Dimensions.Single().Value != 13
            || coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryResolveCompletedResult()
            || task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || !InputsAreUnchanged())
        {
            Fail("Influence acknowledgement ignored its exact resolved-state token.");
            return;
        }

        GD.Print(
            "Influence resolution coordinator smoke passed: exact completed selection and "
            + "abstention resolve once, exact resolved authority gates acknowledgement, task reset "
            + "is explicit, stale acknowledgement fails closed, and sources stay unchanged.");
        GetTree().Quit();
    }

    private CharacterTargetSabotageInfluenceConsiderationRequest Request(
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceCandidate candidate) =>
        new(input.Actor, input.InteractionAccess, relationship, [candidate]);

    private CharacterTargetSabotageInfluenceDecision Decision(
        CharacterTargetSabotageInfluenceCandidate? candidate) =>
        new(input.Actor.Identity.Id, candidate);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceResolutionCoordinator);
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
        input.Relationship.Dimensions.Single().Value == 10
        && input.Relationship.Reasons.Count == 0
        && ReferenceEquals(
            input.FirstCandidate.Occurrence.Proposal.ActorId,
            input.Actor.Identity.Id);

    private static InfluenceInput CreateInput()
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
        CharacterTargetSabotageInfluenceCandidate firstCandidate = Candidate(
            occurrence,
            "influence-attempt-1");
        CharacterTargetSabotageInfluenceCandidate secondCandidate = Candidate(
            occurrence,
            "influence-attempt-2");
        CharacterTargetSabotageInfluenceCandidate thirdCandidate = Candidate(
            occurrence,
            "influence-attempt-3");
        CharacterInteractionAccessSnapshot access = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);

        return new InfluenceInput(
            actor,
            access,
            relationship,
            firstCandidate,
            secondCandidate,
            thirdCandidate);
    }

    private static CharacterTargetSabotageInfluenceCandidate Candidate(
        CharacterTargetSabotageOccurrence occurrence,
        string id) =>
        new(
            occurrence,
            new CharacterRelationshipChange(
                occurrence.Target.Identity.Id,
                occurrence.Proposal.ActorId,
                1,
                new CharacterRelationshipReason(
                    new RelationshipReasonId(id),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId(id),
                    "The enemy agent attempted to gain influence.")));

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterTargetSabotageInfluenceDecision decision)
        : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed record InfluenceInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate FirstCandidate,
        CharacterTargetSabotageInfluenceCandidate SecondCandidate,
        CharacterTargetSabotageInfluenceCandidate ThirdCandidate);
}
