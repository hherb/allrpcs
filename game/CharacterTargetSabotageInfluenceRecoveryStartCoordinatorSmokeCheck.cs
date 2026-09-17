using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit exact-state influence-recovery consideration start.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryStartCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageInfluenceController influence = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateController candidates = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateRequestController requests = null!;
    private CharacterTargetSabotageInfluenceRecoveryController recovery = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;
    private CharacterTargetSabotageInfluenceRecoveryStartCoordinator coordinator = null!;
    private RecordingModel model = null!;
    private CharacterTargetSabotageInfluenceResult currentResult = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidate retainedCandidate = null!;
    private CharacterTargetSabotageInfluenceRecoveryConsiderationRequest retainedRequest = null!;
    private CharacterRelationshipChange retainedRecoveryChange = null!;
    private int processFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        influence = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        candidates = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
        requests = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateRequestController");
        recovery = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        coordinator = GetNode<CharacterTargetSabotageInfluenceRecoveryStartCoordinator>(
            "../CharacterTargetSabotageInfluenceRecoveryStartCoordinator");
        TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource modelSource =
            GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource>(
                "../TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource");

        InfluenceInput input = CreateInput();
        CharacterTargetSabotageInfluenceResult unavailableResult =
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate);
        CharacterRelationshipChange recoveryChange = RecoveryChange(input, -1);
        model = new();
        if (!modelSource.TrySupplyModel(model))
        {
            Fail("Influence-recovery start smoke could not queue its one-shot model.");
            return;
        }

        bool unavailableRejected = !coordinator.TryStart(unavailableResult, recoveryChange);
        CharacterTargetSabotageInfluenceConsiderationResult selection = new(
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                [input.Candidate]),
            new CharacterTargetSabotageInfluenceDecision(
                input.Actor.Identity.Id,
                input.Candidate));
        if (!influence.TryInitialize(input.Relationship)
            || !influence.TryResolveConsideration(selection)
            || influence.LatestAppliedResult is not { } appliedResult)
        {
            Fail("Influence-recovery start smoke could not create exact current influence state.");
            return;
        }

        CharacterTargetSabotageInfluenceResult staleResult = new(
            appliedResult.Candidate,
            appliedResult.Relationship);
        CharacterRelationshipMemory rebuiltRelationship = new(
            appliedResult.Relationship.CharacterId,
            appliedResult.Relationship.RelatedCharacterId,
            appliedResult.Relationship.Dimensions,
            appliedResult.Relationship.Reasons);
        CharacterTargetSabotageInfluenceResult rebuiltResult = new(
            appliedResult.Candidate,
            rebuiltRelationship);
        CharacterTargetSabotageInfluenceResult foreignResult = CreateForeignResult();

        bool invalidInputsRejected = unavailableRejected
            && !coordinator.TryStart(staleResult, recoveryChange)
            && !coordinator.TryStart(rebuiltResult, recoveryChange)
            && !coordinator.TryStart(foreignResult, recoveryChange)
            && !coordinator.TryStart(appliedResult, MisdirectedChange(input))
            && !coordinator.TryStart(appliedResult, WrongDimensionChange(input))
            && !coordinator.TryStart(appliedResult, RecoveryChange(input, -2))
            && ThrowsArgumentNull(() => coordinator.TryStart(null!, recoveryChange))
            && ThrowsArgumentNull(() => coordinator.TryStart(appliedResult, null!))
            && candidates.Candidate is null
            && requests.Request is null
            && recovery.Relationship is null
            && recovery.LatestAppliedResult is null
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            && task.Result is null
            && model.CallCount == 0;

        bool started = coordinator.TryStart(appliedResult, recoveryChange)
            && candidates.Candidate is { } createdCandidate
            && requests.Request is { } createdRequest
            && ReferenceEquals(createdCandidate.InfluenceResult, appliedResult)
            && ReferenceEquals(createdCandidate.RecoveryChange, recoveryChange)
            && ReferenceEquals(createdRequest.Actor, input.Target)
            && ReferenceEquals(createdRequest.Relationship, appliedResult.Relationship)
            && createdRequest.Candidates.Count == 1
            && ReferenceEquals(createdRequest.Candidates.Single(), createdCandidate)
            && ReferenceEquals(recovery.Relationship, appliedResult.Relationship)
            && recovery.LatestAppliedResult is null
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running
            && task.Result is null
            && model.CallCount == 1
            && ReferenceEquals(model.Request, createdRequest);

        if (!invalidInputsRejected || !started)
        {
            Fail("Influence-recovery start did not preserve exact preflight and ownership gates.");
            return;
        }

        currentResult = appliedResult;
        retainedCandidate = candidates.Candidate!;
        retainedRequest = requests.Request!;
        retainedRecoveryChange = recoveryChange;
        bool repeatedIsInert = !coordinator.TryStart(appliedResult, recoveryChange)
            && ReferenceEquals(candidates.Candidate, retainedCandidate)
            && ReferenceEquals(requests.Request, retainedRequest)
            && ReferenceEquals(recovery.Relationship, appliedResult.Relationship)
            && recovery.LatestAppliedResult is null
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running
            && task.Result is null
            && model.CallCount == 1
            && InputsAreUnchanged(input, rebuiltRelationship);
        if (!repeatedIsInert || !HasClosedSurface())
        {
            Fail("Repeated influence-recovery start changed retained scene or source state.");
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (++processFrames > MaximumProcessFrames)
        {
            Fail("Influence-recovery start decision did not complete in time.");
            return;
        }

        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        bool completed = task.Status
                == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            && task.Result is { } result
            && ReferenceEquals(result.Request, retainedRequest)
            && ReferenceEquals(result.SelectedCandidate, retainedCandidate)
            && model.CallCount == 1
            && ReferenceEquals(model.Request, retainedRequest)
            && ReferenceEquals(candidates.Candidate, retainedCandidate)
            && ReferenceEquals(retainedCandidate.InfluenceResult, currentResult)
            && ReferenceEquals(retainedCandidate.RecoveryChange, retainedRecoveryChange)
            && ReferenceEquals(requests.Request, retainedRequest)
            && ReferenceEquals(recovery.Relationship, currentResult.Relationship)
            && recovery.LatestAppliedResult is null
            && ReferenceEquals(influence.LatestAppliedResult, currentResult)
            && ReferenceEquals(influence.Relationship, currentResult.Relationship);
        if (!completed)
        {
            Fail("Influence-recovery start changed exact state before resolution.");
            return;
        }

        GD.Print(
            "Influence recovery start smoke passed: unavailable, stale, rebuilt, foreign, "
            + "misdirected, wrong-dimension, wrong-delta, null, repeated, and occupied input "
            + "stayed inert; one exact graph initialized authority and consumed one model.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged(
        InfluenceInput input,
        CharacterRelationshipMemory rebuiltRelationship) =>
        ReferenceEquals(influence.LatestAppliedResult, currentResult)
        && ReferenceEquals(influence.Relationship, currentResult.Relationship)
        && ReferenceEquals(currentResult.Candidate, input.Candidate)
        && ReferenceEquals(currentResult.Relationship.Reasons.Single(),
            input.Candidate.RelationshipChange.Reason)
        && currentResult.Relationship.Dimensions.Single().Value == 11
        && rebuiltRelationship.Dimensions.Single().Value == 11
        && rebuiltRelationship.Reasons.Count == 1;

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceRecoveryStartCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methods.SequenceEqual(["TryStart", "_Ready"])
            && properties.Length == 0;
    }

    private static CharacterTargetSabotageInfluenceResult CreateForeignResult()
    {
        InfluenceInput input = CreateInput();
        return CharacterTargetSabotageInfluenceTransition.Apply(
            input.Actor,
            input.InteractionAccess,
            input.Relationship,
            input.Candidate);
    }

    private static CharacterRelationshipChange RecoveryChange(InfluenceInput input, int delta) =>
        new(
            input.Target.Identity.Id,
            input.Actor.Identity.Id,
            delta,
            new CharacterRelationshipReason(
                new RelationshipReasonId($"influence-recovery-{Math.Abs(delta)}"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId($"influence-recovery-{Math.Abs(delta)}"),
                "The target reconsidered the enemy agent's influence."));

    private static CharacterRelationshipChange MisdirectedChange(InfluenceInput input) =>
        new(
            input.Actor.Identity.Id,
            input.Target.Identity.Id,
            -1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("misdirected-influence-recovery"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId("misdirected-influence-recovery"),
                "The change points in the wrong direction."));

    private static CharacterRelationshipChange WrongDimensionChange(InfluenceInput input) =>
        new(
            input.Target.Identity.Id,
            input.Actor.Identity.Id,
            -1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("wrong-dimension-influence-recovery"),
                CharacterRelationshipDimension.Fear,
                new CharacterTurnId("wrong-dimension-influence-recovery"),
                "The change targets the wrong dimension."));

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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

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
        CharacterTargetSabotageInfluenceCandidate candidate = new(
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
        return new InfluenceInput(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(
                    CharacterRelationshipDimension.Trust,
                    10)],
                []),
            candidate);
    }

    private sealed class RecordingModel
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
            return ValueTask.FromResult(new CharacterTargetSabotageInfluenceRecoveryDecision(
                value.Actor.Identity.Id,
                value.Candidates.Single()));
        }
    }

    private sealed record InfluenceInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate);
}
