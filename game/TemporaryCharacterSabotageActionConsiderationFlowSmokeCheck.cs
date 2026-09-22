using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit sabotage-action consideration through one isolated scene flow.</summary>
public partial class TemporaryCharacterSabotageActionConsiderationFlowSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private CharacterSabotageActionTaskController task = null!;
    private TemporaryCharacterSabotageActionConsiderationFlowController flow = null!;
    private CharacterSabotageActionConsiderationRequest request = null!;
    private CharacterSabotageActionKind selectedKind = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel overlapModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        flow = GetNode<TemporaryCharacterSabotageActionConsiderationFlowController>(
            "../TemporaryCharacterSabotageActionConsiderationFlowController");
        request = CreateRequest(out selectedKind);
        selectionModel = new RecordingModel(value => new(value.Actor.Identity.Id, selectedKind));
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
            Fail("Sabotage-action consideration flow exceeded its bounded smoke duration.");
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
                Fail("Sabotage-action consideration flow entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(request)
            && task.Status == CharacterSabotageActionTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart(request);

        if (!inertStartup
            || !started
            || !overlapPreserved
            || !ThrowsArgumentNull(() => flow.TryStart(null!))
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Sabotage-action consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
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
            || overlapModel.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Sabotage-action consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { SelectedActionKind: null }
            || overlapModel.CallCount != 1
            || !ReferenceEquals(overlapModel.Request, request)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Sabotage-action consideration failed its preserved one-shot abstention.");
            return;
        }

        stage = 3;
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
            || overlapModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Sabotage-action consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Sabotage-action consideration flow smoke passed: inert startup, exact request and "
            + "one-shot selection, overlap preservation, explicit reset, stable offline failure, "
            + "closed surface, and unchanged request state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterSabotageActionConsiderationFlowController);
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
        ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Conceal)
        && ReferenceEquals(request.ActionKinds[1], selectedKind)
        && ReferenceEquals(request.Actor.Identity.Id, selectionModel.Request?.Actor.Identity.Id);

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
