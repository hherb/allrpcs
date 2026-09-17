using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains an exact target-delay decision.</summary>
public partial class CharacterTargetSabotageDelayTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterTargetSabotageDelayTaskController task = null!;
    private TaskInput input = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = new CharacterTargetSabotageDelayTaskController();
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
            Fail("Target-delay task did not finish within its bounded smoke duration.");
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
                Fail("Target-delay task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterTargetSabotageDelayDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request)
            && task.Status == CharacterTargetSabotageDelayTaskStatus.Running
            && !task.TryStart(new ImmediateModel(decision), input.Request);
        if (!valid)
        {
            Fail("Target-delay task rejected its exact start or overlap contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartCancellation()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
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
            Fail("Target-delay task failed exact completion, reset, or cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Cancelled
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request))
        {
            Fail("Target-delay task failed safe cancellation or failure start.");
            return;
        }

        stage = 3;
    }

    private void ObserveFailureAndStartInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        CharacterTargetSabotageDelayDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Failed
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new ImmediateModel(invalid), input.Request))
        {
            Fail("Target-delay task exposed a failed result or rejected its reset.");
            return;
        }

        stage = 4;
    }

    private void ObserveInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Failed
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Target-delay task accepted invalid output or changed request state.");
            return;
        }

        GD.Print(
            "Target-delay task controller smoke passed: exact main-thread completion, overlap "
            + "rejection, cancellation, safe model and validation failure, reset, closed surface, "
            + "and unchanged request state.");
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterTargetSabotageDelayDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request))
            && ThrowsArgumentNull(() => task.TryStart(model, null!))
            && InputsAreUnchanged();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayTaskController);
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
            && Enum.GetValues<CharacterTargetSabotageDelayTaskStatus>().SequenceEqual(
                [
                    CharacterTargetSabotageDelayTaskStatus.Idle,
                    CharacterTargetSabotageDelayTaskStatus.Running,
                    CharacterTargetSabotageDelayTaskStatus.Completed,
                    CharacterTargetSabotageDelayTaskStatus.Cancelled,
                    CharacterTargetSabotageDelayTaskStatus.Failed,
                ]);
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Request.Actor, input.Actor)
        && ReferenceEquals(input.Request.InteractionAccess, input.InteractionAccess)
        && ReferenceEquals(input.Request.WorldEvents, input.WorldEvents)
        && ReferenceEquals(input.Request.EventLog, input.EventLog)
        && ReferenceEquals(input.Request.WorldTime, input.WorldTime)
        && ReferenceEquals(input.Request.Candidates.Single(), input.Candidate)
        && input.EventLog.Entries.Count == 0
        && input.WorldTime.CurrentEventId is null;

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
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent worldEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTargetSabotageDelayConsiderationRequest request = new(
            actor,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);
        return new(actor, interactionAccess, worldEvents, eventLog, worldTime, candidate, request);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterTargetSabotageDelayDecision decision)
        : ICharacterTargetSabotageDelayDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed class WaitingModel : ICharacterTargetSabotageDelayDecisionModel
    {
        private readonly TaskCompletionSource<CharacterTargetSabotageDelayDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterTargetSabotageDelayDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageDelayDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }

    private sealed record TaskInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayCandidate Candidate,
        CharacterTargetSabotageDelayConsiderationRequest Request);
}
