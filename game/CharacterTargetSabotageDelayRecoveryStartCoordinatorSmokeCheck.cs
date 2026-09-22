using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit exact-state delay-recovery consideration start.</summary>
public partial class CharacterTargetSabotageDelayRecoveryStartCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageDelayController delay = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateController candidates = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController requests = null!;
    private CharacterTargetSabotageDelayRecoveryController recovery = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;
    private CharacterTargetSabotageDelayRecoveryStartCoordinator coordinator = null!;
    private RecordingModel model = null!;
    private CharacterTargetSabotageDelayResult currentResult = null!;
    private CharacterTargetSabotageDelayRecoveryCandidate retainedCandidate = null!;
    private CharacterTargetSabotageDelayRecoveryConsiderationRequest retainedRequest = null!;
    private WorldEvent retainedRecoveryEvent = null!;
    private int processFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        delay = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        candidates = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
        requests = GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        recovery = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        coordinator = GetNode<CharacterTargetSabotageDelayRecoveryStartCoordinator>(
            "../CharacterTargetSabotageDelayRecoveryStartCoordinator");
        TemporaryCharacterTargetSabotageDelayRecoveryModelSource modelSource =
            GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
                "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");

        DelayInput input = CreateInput();
        CharacterTargetSabotageDelayResult unavailableResult =
            CharacterTargetSabotageDelayTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                input.DelayCandidate);
        model = new();
        if (!modelSource.TrySupplyModel(model))
        {
            Fail("Delay-recovery start smoke could not queue its one-shot model.");
            return;
        }

        bool unavailableRejected = !coordinator.TryStart(
            input.WorldEvents,
            unavailableResult,
            input.RecoveryEvent);
        CharacterTargetSabotageDelayConsiderationResult selection = new(
            new CharacterTargetSabotageDelayConsiderationRequest(
                input.Actor,
                input.InteractionAccess,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                [input.DelayCandidate]),
            new CharacterTargetSabotageDelayDecision(
                input.Actor.Identity.Id,
                input.DelayCandidate));
        if (!delay.TryInitialize(input.EventLog, input.WorldTime)
            || !delay.TryResolveConsideration(selection)
            || delay.LatestAppliedResult is not { } appliedResult)
        {
            Fail("Delay-recovery start smoke could not create exact current delay state.");
            return;
        }

        CharacterTargetSabotageDelayResult staleResult = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        CharacterTargetSabotageDelayResult rebuiltResult = new(
            appliedResult.Candidate,
            new WorldEventLogSnapshot(appliedResult.EventLog.Entries),
            new WorldTimeSnapshot(appliedResult.WorldTime.CurrentEventId));
        CharacterTargetSabotageDelayResult foreignResult = CreateForeignResult();
        WorldEventSnapshot missingRecovery = new([input.DelayEvent]);
        WorldEvent inapplicableRecovery = new(
            input.RecoveryEvent.EventId,
            WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot inapplicableWorldEvents = new(
            [input.DelayEvent, inapplicableRecovery]);

        bool invalidInputsRejected = unavailableRejected
            && !coordinator.TryStart(input.WorldEvents, staleResult, input.RecoveryEvent)
            && !coordinator.TryStart(input.WorldEvents, rebuiltResult, input.RecoveryEvent)
            && !coordinator.TryStart(input.WorldEvents, foreignResult, input.RecoveryEvent)
            && !coordinator.TryStart(missingRecovery, appliedResult, input.RecoveryEvent)
            && !coordinator.TryStart(
                inapplicableWorldEvents,
                appliedResult,
                inapplicableRecovery)
            && ThrowsArgumentNull(
                () => coordinator.TryStart(null!, appliedResult, input.RecoveryEvent))
            && ThrowsArgumentNull(
                () => coordinator.TryStart(input.WorldEvents, null!, input.RecoveryEvent))
            && ThrowsArgumentNull(
                () => coordinator.TryStart(input.WorldEvents, appliedResult, null!))
            && candidates.Candidate is null
            && requests.Request is null
            && recovery.EventLog is null
            && recovery.WorldTime is null
            && recovery.LatestAppliedResult is null
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && task.Result is null
            && model.CallCount == 0;

        bool started = coordinator.TryStart(
                input.WorldEvents,
                appliedResult,
                input.RecoveryEvent)
            && candidates.Candidate is { } createdCandidate
            && requests.Request is { } createdRequest
            && ReferenceEquals(createdCandidate.DelayResult, appliedResult)
            && ReferenceEquals(createdCandidate.RecoveryEvent, input.RecoveryEvent)
            && ReferenceEquals(createdRequest.Actor, input.Target)
            && ReferenceEquals(createdRequest.WorldEvents, input.WorldEvents)
            && ReferenceEquals(createdRequest.EventLog, appliedResult.EventLog)
            && ReferenceEquals(createdRequest.WorldTime, appliedResult.WorldTime)
            && createdRequest.Candidates.Count == 1
            && ReferenceEquals(createdRequest.Candidates.Single(), createdCandidate)
            && ReferenceEquals(recovery.EventLog, appliedResult.EventLog)
            && ReferenceEquals(recovery.WorldTime, appliedResult.WorldTime)
            && recovery.LatestAppliedResult is null
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            && task.Result is null
            && model.CallCount == 1
            && ReferenceEquals(model.Request, createdRequest);

        if (!invalidInputsRejected || !started)
        {
            Fail("Delay-recovery start did not preserve exact preflight and ownership gates.");
            return;
        }

        currentResult = appliedResult;
        retainedCandidate = candidates.Candidate!;
        retainedRequest = requests.Request!;
        retainedRecoveryEvent = input.RecoveryEvent;
        bool repeatedIsInert = !coordinator.TryStart(
                input.WorldEvents,
                appliedResult,
                input.RecoveryEvent)
            && ReferenceEquals(candidates.Candidate, retainedCandidate)
            && ReferenceEquals(requests.Request, retainedRequest)
            && ReferenceEquals(recovery.EventLog, appliedResult.EventLog)
            && ReferenceEquals(recovery.WorldTime, appliedResult.WorldTime)
            && recovery.LatestAppliedResult is null
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            && task.Result is null
            && model.CallCount == 1
            && InputsAreUnchanged(input, rebuiltResult);
        if (!repeatedIsInert || !HasClosedSurface())
        {
            Fail("Repeated delay-recovery start changed retained scene or source state.");
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (++processFrames > MaximumProcessFrames)
        {
            Fail("Delay-recovery start decision did not complete in time.");
            return;
        }

        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        bool completed = task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            && task.Result is { } result
            && ReferenceEquals(result.Request, retainedRequest)
            && ReferenceEquals(result.SelectedCandidate, retainedCandidate)
            && model.CallCount == 1
            && ReferenceEquals(model.Request, retainedRequest)
            && ReferenceEquals(candidates.Candidate, retainedCandidate)
            && ReferenceEquals(retainedCandidate.DelayResult, currentResult)
            && ReferenceEquals(retainedCandidate.RecoveryEvent, retainedRecoveryEvent)
            && ReferenceEquals(requests.Request, retainedRequest)
            && ReferenceEquals(recovery.EventLog, currentResult.EventLog)
            && ReferenceEquals(recovery.WorldTime, currentResult.WorldTime)
            && recovery.LatestAppliedResult is null
            && ReferenceEquals(delay.LatestAppliedResult, currentResult)
            && ReferenceEquals(delay.EventLog, currentResult.EventLog)
            && ReferenceEquals(delay.WorldTime, currentResult.WorldTime);
        if (!completed)
        {
            Fail("Delay-recovery start changed exact state before resolution.");
            return;
        }

        GD.Print(
            "Delay recovery start smoke passed: unavailable, stale, rebuilt, foreign, missing, "
            + "inapplicable, null, repeated, and occupied input stayed inert; one exact graph "
            + "initialized authority and consumed one model.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged(
        DelayInput input,
        CharacterTargetSabotageDelayResult rebuiltResult) =>
        ReferenceEquals(delay.LatestAppliedResult, currentResult)
        && ReferenceEquals(delay.EventLog, currentResult.EventLog)
        && ReferenceEquals(delay.WorldTime, currentResult.WorldTime)
        && ReferenceEquals(currentResult.Candidate, input.DelayCandidate)
        && ReferenceEquals(currentResult.EventLog.Entries.Single(), input.DelayEvent.EventId)
        && ReferenceEquals(currentResult.WorldTime.CurrentEventId, input.DelayEvent.EventId)
        && ReferenceEquals(rebuiltResult.EventLog.Entries.Single(), input.DelayEvent.EventId)
        && ReferenceEquals(rebuiltResult.WorldTime.CurrentEventId, input.DelayEvent.EventId);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayRecoveryStartCoordinator);
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

    private static CharacterTargetSabotageDelayResult CreateForeignResult()
    {
        DelayInput input = CreateInput();
        return CharacterTargetSabotageDelayTransition.Apply(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.DelayCandidate);
    }

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

    private static DelayInput CreateInput()
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
        WorldEvent delayEvent = new(
            new WorldEventId("coordinated-enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("coordinated-sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        return new DelayInput(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([delayEvent, recoveryEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            delayEvent,
            recoveryEvent,
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent));
    }

    private sealed class RecordingModel
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        internal int CallCount { get; private set; }

        internal CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request
        {
            get;
            private set;
        }

        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(new CharacterTargetSabotageDelayRecoveryDecision(
                value.Actor.Identity.Id,
                value.Candidates.Single()));
        }
    }

    private sealed record DelayInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        WorldEvent DelayEvent,
        WorldEvent RecoveryEvent,
        CharacterTargetSabotageDelayCandidate DelayCandidate);
}
