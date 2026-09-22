using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact completed target-delay recovery resolution and acknowledgement.</summary>
public partial class CharacterTargetSabotageDelayRecoveryResolutionCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;
    private CharacterTargetSabotageDelayRecoveryController state = null!;
    private CharacterTargetSabotageDelayRecoveryResolutionCoordinator coordinator = null!;
    private RecoveryInput input = null!;
    private CharacterTargetSabotageDelayRecoveryResult? firstAppliedResult;
    private WorldEventLogSnapshot? firstChangedEventLog;
    private WorldTimeSnapshot? firstChangedWorldTime;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        state = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        coordinator = GetNode<CharacterTargetSabotageDelayRecoveryResolutionCoordinator>(
            "../CharacterTargetSabotageDelayRecoveryResolutionCoordinator");
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
            Fail("Target-delay recovery resolution exceeded its bounded smoke duration.");
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
                Fail("Target-delay recovery resolution entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = Request(
            input.EventLog,
            input.WorldTime,
            [input.FirstCandidate]);
        bool valid = state.TryInitialize(input.EventLog, input.WorldTime)
            && !coordinator.TryResolveCompletedResult()
            && !coordinator.TryAcknowledgeCompletedResult()
            && HasClosedSurface()
            && task.TryStart(
                new ImmediateModel(Decision(input.FirstCandidate)),
                request);
        if (!valid)
        {
            Fail("Target-delay recovery resolution failed inert startup or selection start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            || state.LatestAppliedResult is null)
        {
            return;
        }

        firstChangedEventLog = state.EventLog;
        firstChangedWorldTime = state.WorldTime;
        firstAppliedResult = state.LatestAppliedResult;
        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || firstChangedEventLog is null
            || firstChangedWorldTime is null
            || !ReferenceEquals(firstAppliedResult.EventLog, firstChangedEventLog)
            || !ReferenceEquals(firstAppliedResult.WorldTime, firstChangedWorldTime)
            || !ReferenceEquals(firstAppliedResult.Candidate, input.FirstCandidate)
            || !ReferenceEquals(
                firstChangedEventLog.Entries[^1],
                input.FirstCandidate.RecoveryEvent.EventId)
            || !ReferenceEquals(
                firstChangedWorldTime.CurrentEventId,
                input.FirstCandidate.RecoveryEvent.EventId)
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            || !task.TryStart(
                new ImmediateModel(Decision(null)),
                Request(
                    firstChangedEventLog,
                    firstChangedWorldTime,
                    [input.FirstCandidate])))
        {
            Fail("Target-delay recovery resolution failed exact selection resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartStaleSelection()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || !ReferenceEquals(state.EventLog, firstChangedEventLog)
            || !ReferenceEquals(state.WorldTime, firstChangedWorldTime)
            || !ReferenceEquals(state.LatestAppliedResult, firstAppliedResult)
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult())
        {
            Fail("Target-delay recovery resolution changed authority for abstention.");
            return;
        }

        CharacterTargetSabotageDelayResult laterDelayResult = new(
            new CharacterTargetSabotageDelayCandidate(
                input.Occurrence,
                input.FirstRecoveryEvent),
            firstChangedEventLog!,
            firstChangedWorldTime!);
        input.PendingCandidate = new(
            laterDelayResult,
            input.SecondRecoveryEvent);
        input.ExternalCandidate = new(
            laterDelayResult,
            input.ThirdRecoveryEvent);
        coordinator.SetProcess(false);
        if (!task.TryStart(
                new ImmediateModel(Decision(input.PendingCandidate)),
                Request(
                    firstChangedEventLog!,
                    firstChangedWorldTime!,
                    [input.PendingCandidate, input.ExternalCandidate])))
        {
            Fail("Target-delay recovery resolution rejected its stale-selection setup.");
            return;
        }

        stage = 3;
    }

    private void ObserveStaleSelection()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        CharacterTargetSabotageDelayRecoveryConsiderationResult external = new(
            Request(
                firstChangedEventLog!,
                firstChangedWorldTime!,
                [input.ExternalCandidate!]),
            Decision(input.ExternalCandidate));
        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || !state.TryResolveConsideration(external)
            || ReferenceEquals(state.EventLog, firstChangedEventLog)
            || ReferenceEquals(state.WorldTime, firstChangedWorldTime)
            || state.LatestAppliedResult is not { } externalResult
            || !ReferenceEquals(externalResult.Candidate, input.ExternalCandidate)
            || coordinator.TryResolveCompletedResult()
            || coordinator.TryResolveCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || !InputsAreUnchanged())
        {
            Fail("Target-delay recovery acknowledgement ignored exact authority tokens.");
            return;
        }

        GD.Print(
            "Target-delay recovery resolution smoke passed: exact completed selection and "
            + "abstention resolve once, acknowledgement is explicit, stale authority fails "
            + "closed, and source graphs stay unchanged.");
        GetTree().Quit();
    }

    private CharacterTargetSabotageDelayRecoveryConsiderationRequest Request(
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        IEnumerable<CharacterTargetSabotageDelayRecoveryCandidate> candidates) =>
        new(input.Actor, input.WorldEvents, eventLog, worldTime, candidates);

    private CharacterTargetSabotageDelayRecoveryDecision Decision(
        CharacterTargetSabotageDelayRecoveryCandidate? candidate) =>
        new(input.Actor.Identity.Id, candidate);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayRecoveryResolutionCoordinator);
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
        input.EventLog.Entries.Count == 2
        && ReferenceEquals(input.EventLog.Entries[0], input.PriorEvent.EventId)
        && ReferenceEquals(input.EventLog.Entries[1], input.DelayEvent.EventId)
        && ReferenceEquals(input.WorldTime.CurrentEventId, input.DelayEvent.EventId)
        && ReferenceEquals(input.FirstCandidate.DelayResult.EventLog, input.EventLog)
        && ReferenceEquals(input.FirstCandidate.DelayResult.WorldTime, input.WorldTime);

    private static RecoveryInput CreateInput()
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
        WorldEvent priorEvent = Event("sage-began-round");
        WorldEvent delayEvent = Event("enemy-agent-delayed-sage");
        WorldEvent firstRecoveryEvent = Event("sage-delay-recovered-once");
        WorldEvent secondRecoveryEvent = Event("sage-delay-recovered-twice");
        WorldEvent thirdRecoveryEvent = Event("sage-delay-recovered-thrice");
        WorldEventLogSnapshot eventLog = new([priorEvent.EventId, delayEvent.EventId]);
        WorldTimeSnapshot worldTime = new(delayEvent.EventId);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            eventLog,
            worldTime);

        return new RecoveryInput(
            actor,
            occurrence,
            new WorldEventSnapshot(
                [
                    priorEvent,
                    delayEvent,
                    firstRecoveryEvent,
                    secondRecoveryEvent,
                    thirdRecoveryEvent,
                ]),
            eventLog,
            worldTime,
            priorEvent,
            delayEvent,
            firstRecoveryEvent,
            secondRecoveryEvent,
            thirdRecoveryEvent,
            new CharacterTargetSabotageDelayRecoveryCandidate(
                delayResult,
                firstRecoveryEvent));
    }

    private static WorldEvent Event(string id) =>
        new(new WorldEventId(id), WorldEventTimeEffect.Advance);

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

    private sealed class RecoveryInput(
        Character actor,
        CharacterTargetSabotageOccurrence occurrence,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        WorldEvent priorEvent,
        WorldEvent delayEvent,
        WorldEvent firstRecoveryEvent,
        WorldEvent secondRecoveryEvent,
        WorldEvent thirdRecoveryEvent,
        CharacterTargetSabotageDelayRecoveryCandidate firstCandidate)
    {
        internal Character Actor { get; } = actor;
        internal CharacterTargetSabotageOccurrence Occurrence { get; } = occurrence;
        internal WorldEventSnapshot WorldEvents { get; } = worldEvents;
        internal WorldEventLogSnapshot EventLog { get; } = eventLog;
        internal WorldTimeSnapshot WorldTime { get; } = worldTime;
        internal WorldEvent PriorEvent { get; } = priorEvent;
        internal WorldEvent DelayEvent { get; } = delayEvent;
        internal WorldEvent FirstRecoveryEvent { get; } = firstRecoveryEvent;
        internal WorldEvent SecondRecoveryEvent { get; } = secondRecoveryEvent;
        internal WorldEvent ThirdRecoveryEvent { get; } = thirdRecoveryEvent;
        internal CharacterTargetSabotageDelayRecoveryCandidate FirstCandidate { get; } =
            firstCandidate;
        internal CharacterTargetSabotageDelayRecoveryCandidate? PendingCandidate { get; set; }
        internal CharacterTargetSabotageDelayRecoveryCandidate? ExternalCandidate { get; set; }
    }
}
