using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains an exact delay-recovery decision.</summary>
public partial class CharacterTargetSabotageDelayRecoveryTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;
    private TaskInput input = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = new CharacterTargetSabotageDelayRecoveryTaskController();
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
            Fail("Delay-recovery task exceeded its bounded smoke duration.");
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
                Fail("Delay-recovery task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterTargetSabotageDelayRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request)
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            && !task.TryStart(new ImmediateModel(decision), input.Request);
        if (!valid)
        {
            Fail("Delay-recovery task rejected its exact start or overlap contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartCancellation()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
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
            Fail("Delay-recovery task failed exact completion, reset, or cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Cancelled
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request))
        {
            Fail("Delay-recovery task failed safe cancellation or failure start.");
            return;
        }

        stage = 3;
    }

    private void ObserveFailureAndStartInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        CharacterTargetSabotageDelayRecoveryDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);
        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Failed
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new ImmediateModel(invalid), input.Request))
        {
            Fail("Delay-recovery task exposed a failed result or rejected its reset.");
            return;
        }

        stage = 4;
    }

    private void ObserveInvalidDecision()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Failed
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Delay-recovery task accepted invalid output or changed request state.");
            return;
        }

        GD.Print(
            "Delay-recovery task smoke passed: exact main-thread completion, overlap rejection, "
            + "cancellation, safe model and validation failure, reset, closed surface, and "
            + "unchanged request state.");
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterTargetSabotageDelayRecoveryDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request))
            && ThrowsArgumentNull(() => task.TryStart(model, null!))
            && InputsAreUnchanged();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayRecoveryTaskController);
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
            && Enum.GetValues<CharacterTargetSabotageDelayRecoveryTaskStatus>().SequenceEqual(
                [
                    CharacterTargetSabotageDelayRecoveryTaskStatus.Idle,
                    CharacterTargetSabotageDelayRecoveryTaskStatus.Running,
                    CharacterTargetSabotageDelayRecoveryTaskStatus.Completed,
                    CharacterTargetSabotageDelayRecoveryTaskStatus.Cancelled,
                    CharacterTargetSabotageDelayRecoveryTaskStatus.Failed,
                ]);
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Request.Actor, input.Actor)
        && ReferenceEquals(input.Request.WorldEvents, input.WorldEvents)
        && ReferenceEquals(input.Request.EventLog, input.EventLog)
        && ReferenceEquals(input.Request.WorldTime, input.WorldTime)
        && ReferenceEquals(input.Request.Candidates.Single(), input.Candidate)
        && ReferenceEquals(input.EventLog.Entries[^1], input.DelayEvent.EventId)
        && ReferenceEquals(input.WorldTime.CurrentEventId, input.DelayEvent.EventId);

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
        Character enemyAgent = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemyAgent.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemyAgent.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            actor);
        WorldEvent priorEvent = new(
            new WorldEventId("sage-began-round"),
            WorldEventTimeEffect.NoAdvance);
        WorldEvent delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        WorldEventLogSnapshot eventLog = new([priorEvent.EventId, delayEvent.EventId]);
        WorldTimeSnapshot worldTime = new(delayEvent.EventId);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            eventLog,
            worldTime);
        CharacterTargetSabotageDelayRecoveryCandidate candidate = new(
            delayResult,
            recoveryEvent);
        WorldEventSnapshot worldEvents = new([priorEvent, delayEvent, recoveryEvent]);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = new(
            actor,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);

        return new(actor, delayEvent, worldEvents, eventLog, worldTime, candidate, request);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterTargetSabotageDelayRecoveryDecision decision)
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed class WaitingModel : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        private readonly TaskCompletionSource<CharacterTargetSabotageDelayRecoveryDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageDelayRecoveryDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }

    private sealed record TaskInput(
        Character Actor,
        WorldEvent DelayEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayRecoveryCandidate Candidate,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest Request);
}
