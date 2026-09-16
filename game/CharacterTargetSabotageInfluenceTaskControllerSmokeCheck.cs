using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains an exact influence decision.</summary>
public partial class CharacterTargetSabotageInfluenceTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterTargetSabotageInfluenceTaskController task = null!;
    private TaskInput input = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = new CharacterTargetSabotageInfluenceTaskController();
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
            Fail("Influence task did not finish within its bounded smoke duration.");
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
                Fail("Influence task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterTargetSabotageInfluenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterTargetSabotageInfluenceTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request)
            && task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running
            && !task.TryStart(new ImmediateModel(decision), input.Request);
        if (!valid)
        {
            Fail("Influence task rejected its exact start or overlap contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartCancellation()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.Decision.ActorId, input.Actor.Identity.Id)
            || !ReferenceEquals(result.SelectedCandidate, input.Candidate)
            || !task.TryReset()
            || task.Result is not null)
        {
            Fail("Influence task failed exact completion and reset.");
            return;
        }

        if (!task.TryStart(new WaitingModel(), input.Request)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Influence task failed bounded caller cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Cancelled
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request))
        {
            Fail("Influence task failed safe cancellation or failure start.");
            return;
        }

        stage = 3;
    }

    private void ObserveFailureAndStartInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        CharacterTargetSabotageInfluenceDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);
        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Failed
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new ImmediateModel(invalid), input.Request))
        {
            Fail("Influence task exposed a failed result or rejected its reset.");
            return;
        }

        stage = 4;
    }

    private void ObserveInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Failed
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Influence task accepted invalid output or changed request state.");
            return;
        }

        GD.Print(
            "Influence task controller smoke passed: exact main-thread completion, overlap "
            + "rejection, cancellation, safe model and validation failure, reset, closed surface, "
            + "and unchanged request state.");
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterTargetSabotageInfluenceDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request))
            && ThrowsArgumentNull(() => task.TryStart(model, null!))
            && InputsAreUnchanged();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceTaskController);
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
            && Enum.GetValues<CharacterTargetSabotageInfluenceTaskStatus>().SequenceEqual(
                [
                    CharacterTargetSabotageInfluenceTaskStatus.Idle,
                    CharacterTargetSabotageInfluenceTaskStatus.Running,
                    CharacterTargetSabotageInfluenceTaskStatus.Completed,
                    CharacterTargetSabotageInfluenceTaskStatus.Cancelled,
                    CharacterTargetSabotageInfluenceTaskStatus.Failed,
                ]);
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Request.Actor, input.Actor)
        && ReferenceEquals(input.Request.InteractionAccess, input.InteractionAccess)
        && ReferenceEquals(input.Request.Relationship, input.Relationship)
        && ReferenceEquals(input.Request.Candidates.Single(), input.Candidate)
        && input.Relationship.Reasons.Count == 0;

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
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterTargetSabotageInfluenceConsiderationRequest request = new(
            actor,
            interactionAccess,
            relationship,
            [candidate]);
        return new(actor, interactionAccess, relationship, candidate, request);
    }

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

    private sealed class WaitingModel : ICharacterTargetSabotageInfluenceDecisionModel
    {
        private readonly TaskCompletionSource<CharacterTargetSabotageInfluenceDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageInfluenceDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }

    private sealed record TaskInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate,
        CharacterTargetSabotageInfluenceConsiderationRequest Request);
}
