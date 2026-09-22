using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit delay-recovery consideration through one isolated scene flow.</summary>
public partial class TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowSmokeCheck
    : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;
    private TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController flow = null!;
    private CharacterTargetSabotageDelayRecoveryConsiderationRequest request = null!;
    private CharacterTargetSabotageDelayRecoveryCandidate candidate = null!;
    private WorldEvent delayEvent = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel overlapModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController");
        request = CreateRequest(out candidate, out delayEvent);
        selectionModel = new RecordingModel(value => new(value.Actor.Identity.Id, candidate));
        overlapModel = new RecordingModel(value => new(value.Actor.Identity.Id, null));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Delay-recovery consideration flow exceeded its bounded smoke duration.");
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
                ObserveAbstentionAndStartOfflineFailure();
                break;
            case 3:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Delay-recovery consideration flow entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart(request);

        if (!inertStartup
            || !started
            || !overlapPreserved
            || !ThrowsArgumentNull(() => flow.TryStart(null!))
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Delay-recovery consideration failed inert startup or explicit start.");
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

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, candidate)
            || overlapModel.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Delay-recovery consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || overlapModel.CallCount != 1
            || !ReferenceEquals(overlapModel.Request, request)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Delay-recovery consideration failed its preserved one-shot abstention.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || overlapModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Delay-recovery consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Delay-recovery consideration flow smoke passed: inert startup, exact request and "
            + "one-shot selection, overlap preservation, explicit reset, stable offline failure, "
            + "closed surface, and unchanged world state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(
            TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController);
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

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.EventLog, candidate.DelayResult.EventLog)
        && ReferenceEquals(request.WorldTime, candidate.DelayResult.WorldTime)
        && ReferenceEquals(request.EventLog.Entries[^1], delayEvent.EventId)
        && ReferenceEquals(request.WorldTime.CurrentEventId, delayEvent.EventId);

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

    private static CharacterTargetSabotageDelayRecoveryConsiderationRequest CreateRequest(
        out CharacterTargetSabotageDelayRecoveryCandidate candidate,
        out WorldEvent delayEvent)
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
        delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            new WorldEventLogSnapshot([priorEvent.EventId, delayEvent.EventId]),
            new WorldTimeSnapshot(delayEvent.EventId));
        candidate = new CharacterTargetSabotageDelayRecoveryCandidate(
            delayResult,
            recoveryEvent);
        return new CharacterTargetSabotageDelayRecoveryConsiderationRequest(
            actor,
            new WorldEventSnapshot([priorEvent, delayEvent, recoveryEvent]),
            delayResult.EventLog,
            delayResult.WorldTime,
            [candidate]);
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

        public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

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
}
