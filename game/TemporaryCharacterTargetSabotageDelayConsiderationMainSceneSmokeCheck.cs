using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit target-delay consideration through main-scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageDelayConsiderationMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_TARGET_SABOTAGE_DELAY_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterTargetSabotageDelayModelSource modelSource = null!;
    private CharacterTargetSabotageDelayTaskController task = null!;
    private TemporaryCharacterTargetSabotageDelayConsiderationFlowController flow = null!;
    private CharacterTargetSabotageDelayConsiderationRequest request = null!;
    private CharacterTargetSabotageDelayCandidate candidate = null!;
    private RecordingModel selectionModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        if (!isEnabled)
        {
            SetProcess(false);
            return;
        }

        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
            "../TemporaryCharacterTargetSabotageDelayModelSource");
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        flow = GetNode<TemporaryCharacterTargetSabotageDelayConsiderationFlowController>(
            "../TemporaryCharacterTargetSabotageDelayConsiderationFlowController");
        request = CreateRequest(out candidate);
        selectionModel = new(value => new(value.Actor.Identity.Id, candidate));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene target-delay consideration exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelectionAndStartOfflineFailure();
                break;
            case 2:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Main-scene target-delay consideration entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageDelayTaskStatus.Running;

        if (!inertStartup || !started || !InputsAreUnchanged())
        {
            Fail("Main-scene target-delay consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartOfflineFailure()
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
            || !task.TryReset()
            || !flow.TryStart(request)
            || !InputsAreUnchanged())
        {
            Fail("Main-scene target-delay consideration failed its exact selected result.");
            return;
        }

        stage = 2;
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
            || !InputsAreUnchanged())
        {
            Fail("Main-scene target-delay consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Main-scene target-delay consideration smoke passed: inert startup, exact explicit "
            + "selection, reset, stable offline failure, and unchanged authoritative world state.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.Actor.Identity.Id, candidate.Occurrence.Proposal.ActorId)
        && request.EventLog.Entries.Count == 0
        && request.WorldTime.CurrentEventId is null;

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
        internal int CallCount { get; private set; }

        internal CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

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
