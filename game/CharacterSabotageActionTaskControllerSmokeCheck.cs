using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains an exact sabotage-action decision.</summary>
public partial class CharacterSabotageActionTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterSabotageActionTaskController task = null!;
    private TaskInput input = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = new CharacterSabotageActionTaskController();
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
            Fail("Sabotage-action task did not finish within its bounded smoke duration.");
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
                Fail("Sabotage-action task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterSabotageActionDecision decision = new(
            input.Actor.Identity.Id,
            input.SelectedKind);
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request)
            && task.Status == CharacterSabotageActionTaskStatus.Running
            && !task.TryStart(new ImmediateModel(decision), input.Request);
        if (!valid)
        {
            Fail("Sabotage-action task rejected its exact start or overlap contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartCancellation()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.Decision.ActorId, input.Actor.Identity.Id)
            || !ReferenceEquals(result.SelectedActionKind, input.SelectedKind)
            || !task.TryReset()
            || task.Result is not null)
        {
            Fail("Sabotage-action task failed exact completion and reset.");
            return;
        }

        if (!task.TryStart(new WaitingModel(), input.Request)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Sabotage-action task failed bounded caller cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Cancelled
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request))
        {
            Fail("Sabotage-action task failed safe cancellation or failure start.");
            return;
        }

        stage = 3;
    }

    private void ObserveFailureAndStartInvalidDecision()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageActionDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.SelectedKind);
        if (task.Status != CharacterSabotageActionTaskStatus.Failed
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new ImmediateModel(invalid), input.Request))
        {
            Fail("Sabotage-action task exposed a failed result or rejected its reset.");
            return;
        }

        stage = 4;
    }

    private void ObserveInvalidDecision()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Failed
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Sabotage-action task accepted invalid output or changed request state.");
            return;
        }

        GD.Print(
            "Sabotage-action task controller smoke passed: exact main-thread completion, overlap "
            + "rejection, cancellation, safe model and validation failure, reset, closed surface, "
            + "and unchanged request state.");
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterSabotageActionDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request))
            && ThrowsArgumentNull(() => task.TryStart(model, null!))
            && InputsAreUnchanged();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionTaskController);
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
            && Enum.GetValues<CharacterSabotageActionTaskStatus>().SequenceEqual(
                [
                    CharacterSabotageActionTaskStatus.Idle,
                    CharacterSabotageActionTaskStatus.Running,
                    CharacterSabotageActionTaskStatus.Completed,
                    CharacterSabotageActionTaskStatus.Cancelled,
                    CharacterSabotageActionTaskStatus.Failed,
                ]);
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Request.Actor, input.Actor)
        && ReferenceEquals(input.Request.ActionKinds[0], input.FirstKind)
        && ReferenceEquals(input.Request.ActionKinds[1], input.SelectedKind);

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
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionKind firstKind = CharacterSabotageActionKind.Conceal;
        CharacterSabotageActionKind selectedKind = CharacterSabotageActionKind.Misquote;
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [firstKind, selectedKind]);
        return new(actor, firstKind, selectedKind, request);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterSabotageActionDecision decision)
        : ICharacterSabotageActionDecisionModel
    {
        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed class WaitingModel : ICharacterSabotageActionDecisionModel
    {
        private readonly TaskCompletionSource<CharacterSabotageActionDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterSabotageActionDecisionModel
    {
        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterSabotageActionDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }

    private sealed record TaskInput(
        Character Actor,
        CharacterSabotageActionKind FirstKind,
        CharacterSabotageActionKind SelectedKind,
        CharacterSabotageActionConsiderationRequest Request);
}
