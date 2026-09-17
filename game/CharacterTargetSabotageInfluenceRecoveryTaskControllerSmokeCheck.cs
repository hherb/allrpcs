using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains an exact influence-recovery decision.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;
    private TaskInput input = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = new CharacterTargetSabotageInfluenceRecoveryTaskController();
        AddChild(task);
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
            Fail("Influence-recovery task exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelectionAndStartCancellation();
                break;
            case 2:
                ObserveCancellationAndStartFailure();
                break;
            case 3:
                ObserveFailureAndStartInvalidDecision();
                break;
            case 4:
                ObserveInvalidDecision();
                break;
            default:
                Fail("Influence-recovery task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterTargetSabotageInfluenceRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request)
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running
            && !task.TryStart(new ImmediateModel(decision), input.Request);
        if (!valid)
        {
            Fail("Influence-recovery task rejected its exact start or overlap contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartCancellation()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.Decision.ActorId, input.Actor.Identity.Id)
            || !ReferenceEquals(result.SelectedCandidate, input.Candidate)
            || !task.TryReset()
            || task.Result is not null
            || !task.TryStart(new WaitingModel(), input.Request)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Influence-recovery task failed exact completion, reset, or cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Cancelled
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request))
        {
            Fail("Influence-recovery task failed safe cancellation or failure start.");
            return;
        }

        stage = 3;
    }

    private void ObserveFailureAndStartInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        CharacterTargetSabotageInfluenceRecoveryDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);
        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Failed
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new ImmediateModel(invalid), input.Request))
        {
            Fail("Influence-recovery task exposed a failed result or rejected its reset.");
            return;
        }

        stage = 4;
    }

    private void ObserveInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Failed
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Influence-recovery task accepted invalid output or changed request state.");
            return;
        }

        GD.Print(
            "Influence-recovery task smoke passed: exact main-thread completion, overlap "
            + "rejection, cancellation, safe model and validation failure, reset, closed surface, "
            + "and unchanged request state.");
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request))
            && ThrowsArgumentNull(() => task.TryStart(model, null!))
            && InputsAreUnchanged();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceRecoveryTaskController);
        string[] properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return properties.SequenceEqual(["Result", "Status"])
            && methods.SequenceEqual(["TryCancel", "TryReset", "TryStart", "_ExitTree", "_Process"])
            && Enum.GetValues<CharacterTargetSabotageInfluenceRecoveryTaskStatus>().SequenceEqual(
                [
                    CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle,
                    CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running,
                    CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed,
                    CharacterTargetSabotageInfluenceRecoveryTaskStatus.Cancelled,
                    CharacterTargetSabotageInfluenceRecoveryTaskStatus.Failed,
                ]);
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Request.Actor, input.Actor)
        && ReferenceEquals(input.Request.Relationship, input.Relationship)
        && ReferenceEquals(input.Request.Candidates.Single(), input.Candidate)
        && ReferenceEquals(input.Relationship.Reasons.Single(), input.InfluenceReason)
        && ReferenceEquals(input.Candidate.RecoveryChange.Reason, input.RecoveryReason);

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

    private static TaskInput CreateInput()
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
        CharacterRelationshipReason influenceReason = Reason(
            "influence-attempt",
            "enemy-agent-influence",
            "The enemy agent attempted to gain influence.");
        CharacterRelationshipChange influenceChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            1,
            influenceReason);
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory relationship = CharacterRelationshipChangeTransition.Apply(
            new CharacterRelationshipMemory(
                actor.Identity.Id,
                enemy.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            influenceChange);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            relationship);
        CharacterRelationshipReason recoveryReason = Reason(
            "influence-recovered",
            "sage-recovery",
            "The sage recovered from the influence attempt.");
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            influenceResult,
            new CharacterRelationshipChange(
                actor.Identity.Id,
                enemy.Identity.Id,
                -1,
                recoveryReason));
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = new(
            actor,
            relationship,
            [candidate]);
        return new(actor, relationship, influenceReason, recoveryReason, candidate, request);
    }

    private static CharacterRelationshipReason Reason(
        string id,
        string turnId,
        string summary) =>
        new(
            new RelationshipReasonId(id),
            CharacterRelationshipDimension.Trust,
            new CharacterTurnId(turnId),
            summary);

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

    private sealed class WaitingModel : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        private readonly TaskCompletionSource<CharacterTargetSabotageInfluenceRecoveryDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageInfluenceRecoveryDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }

    private sealed record TaskInput(
        Character Actor,
        CharacterRelationshipMemory Relationship,
        CharacterRelationshipReason InfluenceReason,
        CharacterRelationshipReason RecoveryReason,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request);
}
