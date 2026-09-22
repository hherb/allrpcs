using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit consideration of the exact live delay-recovery request.</summary>
public partial class CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowSmokeCheck
    : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController flow = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController requests = null!;
    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;
    private CharacterTargetSabotageDelayRecoveryConsiderationRequest request = null!;
    private CharacterTargetSabotageDelayRecoveryCandidate candidate = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel abstentionModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        flow = GetNode<CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController");
        requests = GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        selectionModel = new(input => new(
            input.Actor.Identity.Id,
            input.Candidates.Single()));
        abstentionModel = new(input => new(input.Actor.Identity.Id, null));

        StartExactLiveRequest();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Live delay-recovery request flow exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 1:
                ObserveSelectionAndStartAbstention();
                break;
            case 2:
                ObserveAbstentionAndRepeatedStart();
                break;
            default:
                Fail("Live delay-recovery request flow entered an unknown smoke stage.");
                break;
        }
    }

    private void StartExactLiveRequest()
    {
        CharacterTargetSabotageDelayRecoveryConsiderationRequest staleRequest =
            CreateStandaloneRequest();
        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        bool unavailableAndNullRejected = modelSource.TrySupplyModel(selectionModel)
            && !flow.TryStart(staleRequest)
            && ThrowsArgumentNull(() => flow.TryStart(null!))
            && !modelSource.TrySupplyModel(preservationProbe)
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle;

        bool liveRequestCreated = TryCreateLiveRequest();
        request = requests.Request!;
        candidate = request.Candidates.Single();
        bool started = !flow.TryStart(staleRequest)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            && modelSource.TrySupplyModel(abstentionModel)
            && !flow.TryStart(request)
            && !modelSource.TrySupplyModel(preservationProbe);

        if (!unavailableAndNullRejected
            || !liveRequestCreated
            || !started
            || !HasClosedSurface()
            || !LiveRequestIsUnchanged())
        {
            Fail("Live delay-recovery request did not fail closed before exact start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, candidate)
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || flow.TryStart(request)
            || modelSource.TrySupplyModel(preservationProbe)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Live delay-recovery request lost its queued model after selection.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndRepeatedStart()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || abstentionModel.CallCount != 1
            || !ReferenceEquals(abstentionModel.Request, request)
            || !modelSource.TrySupplyModel(preservationProbe)
            || flow.TryStart(request)
            || modelSource.TrySupplyModel(selectionModel)
            || preservationProbe.CallCount != 0
            || !LiveRequestIsUnchanged())
        {
            Fail("Live delay-recovery request changed state after explicit abstention.");
            return;
        }

        GD.Print(
            "Live delay-recovery candidate consideration flow smoke passed: unavailable, stale, "
            + "null, overlap, and repeated starts preserved the queued model; exact selection and "
            + "abstention completed for the one live request.");
        GetTree().Quit();
    }

    private bool TryCreateLiveRequest()
    {
        CharacterTargetSabotageDelayController delay =
            GetNode<CharacterTargetSabotageDelayController>(
                "../CharacterTargetSabotageDelayController");
        CharacterTargetSabotageDelayRecoveryCandidateController candidates =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateController");
        DelayInput input = CreateInput();
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
        return delay.TryInitialize(input.EventLog, input.WorldTime)
            && delay.TryResolveConsideration(selection)
            && candidates.TryCreate(
                input.WorldEvents,
                delay.LatestAppliedResult!,
                input.RecoveryEvent)
            && requests.TryCreate(input.WorldEvents, candidates.Candidate!);
    }

    private bool LiveRequestIsUnchanged() =>
        ReferenceEquals(requests.Request, request)
        && request.Candidates.Count == 1
        && ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.Actor, candidate.DelayResult.Candidate.Occurrence.Target)
        && ReferenceEquals(request.EventLog, candidate.DelayResult.EventLog)
        && ReferenceEquals(request.WorldTime, candidate.DelayResult.WorldTime)
        && CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            candidate);

    private static bool HasClosedSurface()
    {
        Type type = typeof(
            CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryStart", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
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

    private static CharacterTargetSabotageDelayRecoveryConsiderationRequest
        CreateStandaloneRequest()
    {
        DelayInput input = CreateInput();
        CharacterTargetSabotageDelayResult result = CharacterTargetSabotageDelayTransition.Apply(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.DelayCandidate);
        CharacterTargetSabotageDelayRecoveryCandidate candidate = new(
            result,
            input.RecoveryEvent);
        return new CharacterTargetSabotageDelayRecoveryConsiderationRequest(
            input.Target,
            input.WorldEvents,
            result.EventLog,
            result.WorldTime,
            [candidate]);
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
            new WorldEventId("live-enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("live-sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        return new DelayInput(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([delayEvent, recoveryEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            recoveryEvent,
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterTargetSabotageDelayRecoveryConsiderationRequest,
            CharacterTargetSabotageDelayRecoveryDecision> decide)
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request
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
            return ValueTask.FromResult(decide(value));
        }
    }

    private sealed record DelayInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        WorldEvent RecoveryEvent,
        CharacterTargetSabotageDelayCandidate DelayCandidate);
}
