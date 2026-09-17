using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit target-delay consideration through one isolated scene flow.</summary>
public partial class TemporaryCharacterTargetSabotageDelayConsiderationFlowSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageDelayModelSource modelSource = null!;
    private CharacterTargetSabotageDelayTaskController task = null!;
    private TemporaryCharacterTargetSabotageDelayConsiderationFlowController flow = null!;
    private CharacterTargetSabotageDelayConsiderationRequest request = null!;
    private CharacterTargetSabotageDelayCandidate candidate = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel overlapModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
            "../TemporaryCharacterTargetSabotageDelayModelSource");
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageDelayConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageDelayConsiderationFlowController");
        request = CreateRequest(out candidate);
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
            Fail("Target-delay consideration flow exceeded its bounded smoke duration.");
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
                Fail("Target-delay consideration flow entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageDelayTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart(request);

        if (!inertStartup
            || !started
            || !overlapPreserved
            || !ThrowsArgumentNull(() => flow.TryStart(null!))
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Target-delay consideration failed inert startup or explicit start.");
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

        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, candidate)
            || overlapModel.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Target-delay consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || overlapModel.CallCount != 1
            || !ReferenceEquals(overlapModel.Request, request)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Target-delay consideration failed its preserved one-shot abstention.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || overlapModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Target-delay consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Target-delay consideration flow smoke passed: inert startup, exact request and "
            + "one-shot selection, overlap preservation, explicit reset, stable offline failure, "
            + "closed surface, and unchanged authoritative world state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterTargetSabotageDelayConsiderationFlowController);
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
        && ReferenceEquals(request.Actor.Identity.Id, candidate.Occurrence.Proposal.ActorId)
        && request.EventLog.Entries.Count == 0
        && request.WorldTime.CurrentEventId is null;

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

    private static CharacterTargetSabotageDelayConsiderationRequest CreateRequest(
        out CharacterTargetSabotageDelayCandidate candidate)
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
        candidate = new CharacterTargetSabotageDelayCandidate(occurrence, worldEvent);
        return new CharacterTargetSabotageDelayConsiderationRequest(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            [candidate]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterTargetSabotageDelayConsiderationRequest,
            CharacterTargetSabotageDelayDecision> decide)
        : ICharacterTargetSabotageDelayDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }
}
