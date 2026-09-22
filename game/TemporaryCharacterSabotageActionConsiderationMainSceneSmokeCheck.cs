using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit sabotage-action consideration through main-scene dependencies.</summary>
public partial class TemporaryCharacterSabotageActionConsiderationMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_ACTION_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private CharacterSabotageActionTaskController task = null!;
    private TemporaryCharacterSabotageActionConsiderationFlowController flow = null!;
    private CharacterSabotageActionConsiderationRequest request = null!;
    private CharacterSabotageActionKind selectedKind = null!;
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

        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        flow = GetNode<TemporaryCharacterSabotageActionConsiderationFlowController>(
            "../TemporaryCharacterSabotageActionConsiderationFlowController");
        request = CreateRequest(out selectedKind);
        selectionModel = new(value => new(value.Actor.Identity.Id, selectedKind));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene sabotage-action consideration exceeded its bounded duration.");
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
                Fail("Main-scene sabotage-action consideration entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && InputsAreUnchanged();
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterSabotageActionTaskStatus.Running;

        if (!inertStartup || !started || !InputsAreUnchanged())
        {
            Fail("Main-scene sabotage-action consideration failed inert startup or selection.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartOfflineFailure()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedActionKind, selectedKind)
            || !task.TryReset()
            || !flow.TryStart(request)
            || !InputsAreUnchanged())
        {
            Fail("Main-scene sabotage-action consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Main-scene sabotage-action consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Main-scene sabotage-action consideration smoke passed: inert startup, one exact "
            + "explicit selection, explicit reset, stable offline failure, and unchanged request "
            + "state.");
        GetTree().Quit();
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Conceal)
        && ReferenceEquals(request.ActionKinds[1], selectedKind)
        && (selectionModel.Request is null
            || ReferenceEquals(selectionModel.Request.Actor, request.Actor));

    private static CharacterSabotageActionConsiderationRequest CreateRequest(
        out CharacterSabotageActionKind selectedActionKind)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        selectedActionKind = CharacterSabotageActionKind.Misquote;
        return new CharacterSabotageActionConsiderationRequest(
            actor,
            [CharacterSabotageActionKind.Conceal, selectedActionKind]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> decide)
        : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }
}
