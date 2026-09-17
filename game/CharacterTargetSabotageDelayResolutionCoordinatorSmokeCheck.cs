using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact completed target-delay resolution and acknowledgement.</summary>
public partial class CharacterTargetSabotageDelayResolutionCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageDelayTaskController task = null!;
    private CharacterTargetSabotageDelayController state = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator coordinator = null!;
    private DelayInput input = null!;
    private CharacterTargetSabotageDelayResult? firstAppliedResult;
    private WorldEventLogSnapshot? firstChangedEventLog;
    private WorldTimeSnapshot? firstChangedWorldTime;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        state = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        coordinator = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
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
            Fail("Target-delay resolution coordinator exceeded its bounded smoke duration.");
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
                Fail("Target-delay resolution coordinator entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterTargetSabotageDelayConsiderationRequest request = Request(
            input.EventLog,
            input.WorldTime,
            input.FirstCandidate);
        bool valid = state.TryInitialize(input.EventLog, input.WorldTime)
            && !coordinator.TryResolveCompletedResult()
            && !coordinator.TryAcknowledgeCompletedResult()
            && HasClosedSurface()
            && task.TryStart(new ImmediateModel(Decision(input.FirstCandidate)), request);
        if (!valid)
        {
            Fail("Target-delay resolution coordinator failed inert startup or selection start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        firstChangedEventLog = state.EventLog;
        firstChangedWorldTime = state.WorldTime;
        firstAppliedResult = state.LatestAppliedResult;
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || firstChangedEventLog is null
            || firstChangedWorldTime is null
            || firstAppliedResult is null
            || !ReferenceEquals(firstAppliedResult.EventLog, firstChangedEventLog)
            || !ReferenceEquals(firstAppliedResult.WorldTime, firstChangedWorldTime)
            || !ReferenceEquals(firstAppliedResult.Candidate, input.FirstCandidate)
            || !ReferenceEquals(firstChangedEventLog.Entries[^1], input.FirstCandidate.WorldEvent.EventId)
            || !ReferenceEquals(firstChangedWorldTime.CurrentEventId, input.FirstCandidate.WorldEvent.EventId)
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || !task.TryStart(
                new ImmediateModel(Decision(null)),
                Request(firstChangedEventLog, firstChangedWorldTime, input.SecondCandidate)))
        {
            Fail("Target-delay resolution coordinator failed exact selection resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartSecondSelection()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || !ReferenceEquals(state.EventLog, firstChangedEventLog)
            || !ReferenceEquals(state.WorldTime, firstChangedWorldTime)
            || !ReferenceEquals(state.LatestAppliedResult, firstAppliedResult)
            || coordinator.TryResolveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || !task.TryStart(
                new ImmediateModel(Decision(input.SecondCandidate)),
                Request(firstChangedEventLog!, firstChangedWorldTime!, input.SecondCandidate)))
        {
            Fail("Target-delay resolution coordinator changed state for abstention.");
            return;
        }

        stage = 3;
    }

    private void ObserveSecondSelectionAndInvalidateAcknowledgement()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        WorldEventLogSnapshot? resolvedEventLog = state.EventLog;
        WorldTimeSnapshot? resolvedWorldTime = state.WorldTime;
        CharacterTargetSabotageDelayResult? resolvedResult = state.LatestAppliedResult;
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || resolvedEventLog is null
            || resolvedWorldTime is null
            || resolvedResult is null
            || ReferenceEquals(resolvedEventLog, firstChangedEventLog)
            || ReferenceEquals(resolvedWorldTime, firstChangedWorldTime)
            || resolvedEventLog.Entries.Count != 3
            || !ReferenceEquals(resolvedWorldTime.CurrentEventId, input.SecondCandidate.WorldEvent.EventId))
        {
            Fail("Target-delay resolution coordinator failed its second exact selection.");
            return;
        }

        CharacterTargetSabotageDelayConsiderationResult external = new(
            Request(resolvedEventLog, resolvedWorldTime, input.ThirdCandidate),
            Decision(input.ThirdCandidate));
        if (!state.TryResolveConsideration(external)
            || ReferenceEquals(state.EventLog, resolvedEventLog)
            || ReferenceEquals(state.WorldTime, resolvedWorldTime)
            || state.EventLog?.Entries.Count != 4
            || coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryResolveCompletedResult()
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || !InputsAreUnchanged())
        {
            Fail("Target-delay acknowledgement ignored its exact resolved-state tokens.");
            return;
        }

        GD.Print(
            "Target-delay resolution coordinator smoke passed: exact completed selection and "
            + "abstention resolve once, exact world-state authority gates acknowledgement, task "
            + "reset is explicit, stale acknowledgement fails closed, and sources stay unchanged.");
        GetTree().Quit();
    }

    private CharacterTargetSabotageDelayConsiderationRequest Request(
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTargetSabotageDelayCandidate candidate) =>
        new(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            eventLog,
            worldTime,
            [candidate]);

    private CharacterTargetSabotageDelayDecision Decision(
        CharacterTargetSabotageDelayCandidate? candidate) =>
        new(input.Actor.Identity.Id, candidate);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageDelayResolutionCoordinator);
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
        ReferenceEquals(input.EventLog.Entries.Single(), input.PriorEvent.EventId)
        && input.WorldTime.CurrentEventId is null
        && ReferenceEquals(
            input.FirstCandidate.Occurrence.Proposal.ActorId,
            input.Actor.Identity.Id);

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
        WorldEvent priorEvent = Event("sage-began-round", WorldEventTimeEffect.NoAdvance);
        CharacterTargetSabotageDelayCandidate firstCandidate = Candidate(
            occurrence,
            Event("enemy-agent-delayed-sage-once", WorldEventTimeEffect.Advance));
        CharacterTargetSabotageDelayCandidate secondCandidate = Candidate(
            occurrence,
            Event("enemy-agent-delayed-sage-twice", WorldEventTimeEffect.Advance));
        CharacterTargetSabotageDelayCandidate thirdCandidate = Candidate(
            occurrence,
            Event("enemy-agent-delayed-sage-thrice", WorldEventTimeEffect.Advance));

        return new DelayInput(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot(
                [priorEvent, firstCandidate.WorldEvent, secondCandidate.WorldEvent, thirdCandidate.WorldEvent]),
            new WorldEventLogSnapshot([priorEvent.EventId]),
            new WorldTimeSnapshot(null),
            priorEvent,
            firstCandidate,
            secondCandidate,
            thirdCandidate);
    }

    private static WorldEvent Event(string id, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(id), timeEffect);

    private static CharacterTargetSabotageDelayCandidate Candidate(
        CharacterTargetSabotageOccurrence occurrence,
        WorldEvent worldEvent) =>
        new(occurrence, worldEvent);

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

    private sealed record DelayInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        WorldEvent PriorEvent,
        CharacterTargetSabotageDelayCandidate FirstCandidate,
        CharacterTargetSabotageDelayCandidate SecondCandidate,
        CharacterTargetSabotageDelayCandidate ThirdCandidate);
}
