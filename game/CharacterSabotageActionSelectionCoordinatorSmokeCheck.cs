using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit observation and acknowledgement of sabotage-action results.</summary>
public partial class CharacterSabotageActionSelectionCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 240;

    private CharacterSabotageActionTaskController task = null!;
    private CharacterSabotageActionSelectionCoordinator coordinator = null!;
    private Character actor = null!;
    private CharacterSabotageActionConsiderationRequest request = null!;
    private CharacterSabotageActionKind selectedKind = null!;
    private CharacterSabotageActionConsiderationResult? abstention;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = GetNode<CharacterSabotageActionTaskController>(
            "CharacterSabotageActionTaskController");
        coordinator = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "CharacterSabotageActionSelectionCoordinator");
        actor = RepresentativeEnemyAgentCharacterSource.Create();
        request = CreateRequest(actor, out selectedKind);
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Sabotage-action selection coordination exceeded its bounded duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveAndAcknowledgeSelection();
                break;
            case 2:
                ObserveAbstentionAndStartReplacement();
                break;
            case 3:
                RejectStaleAcknowledgementAndStartCancellation();
                break;
            case 4:
                ObserveCancellationAndStartFailure();
                break;
            case 5:
                ObserveFailure();
                break;
            default:
                Fail("Sabotage-action selection coordination entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterSabotageActionDecision decision = new(
            request.Actor.Identity.Id,
            selectedKind);
        bool inert = coordinator.ObservedResult is null
            && !coordinator.TryObserveCompletedResult()
            && !coordinator.TryAcknowledgeCompletedResult()
            && HasClosedSurface()
            && InputsAreUnchanged();
        if (!inert
            || !task.TryStart(new ImmediateModel(decision), request)
            || coordinator.TryObserveCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult())
        {
            Fail("Selection did not start from explicit inert coordination state.");
            return;
        }

        stage = 1;
    }

    private void ObserveAndAcknowledgeSelection()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageActionConsiderationResult? result = task.Result;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || result is null
            || coordinator.TryAcknowledgeCompletedResult()
            || !coordinator.TryObserveCompletedResult()
            || !ReferenceEquals(coordinator.ObservedResult, result)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.Decision.ActorId, request.Actor.Identity.Id)
            || !ReferenceEquals(result.SelectedActionKind, selectedKind)
            || coordinator.TryObserveCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.ObservedResult is not null
            || task.Status != CharacterSabotageActionTaskStatus.Idle)
        {
            Fail("Fresh exact selection was not observed once and acknowledged explicitly.");
            return;
        }

        CharacterSabotageActionDecision decision = new(request.Actor.Identity.Id, null);
        if (!task.TryStart(new ImmediateModel(decision), request))
        {
            Fail("Abstention could not start after exact acknowledgement.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartReplacement()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        abstention = task.Result;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || abstention is null
            || abstention.SelectedActionKind is not null
            || !coordinator.TryObserveCompletedResult()
            || !ReferenceEquals(coordinator.ObservedResult, abstention)
            || !ReferenceEquals(abstention.Request, request)
            || !ReferenceEquals(abstention.Decision.ActorId, request.Actor.Identity.Id)
            || coordinator.TryObserveCompletedResult()
            || !task.TryReset())
        {
            Fail("Exact abstention was not retained as one observed result.");
            return;
        }

        CharacterSabotageActionDecision decision = new(
            request.Actor.Identity.Id,
            selectedKind);
        if (!task.TryStart(new ImmediateModel(decision), request))
        {
            Fail("Replacement result could not start for stale acknowledgement coverage.");
            return;
        }

        stage = 3;
    }

    private void RejectStaleAcknowledgementAndStartCancellation()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageActionConsiderationResult? replacement = task.Result;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || replacement is null
            || ReferenceEquals(replacement, abstention)
            || coordinator.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(coordinator.ObservedResult, abstention)
            || !coordinator.TryObserveCompletedResult()
            || !ReferenceEquals(coordinator.ObservedResult, replacement)
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.ObservedResult is not null
            || !task.TryStart(new WaitingModel(), request)
            || coordinator.TryObserveCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || !task.TryCancel())
        {
            Fail("Stale observation was not rejected before exact replacement observation.");
            return;
        }

        stage = 4;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Cancelled
            || task.Result is not null
            || coordinator.ObservedResult is not null
            || coordinator.TryObserveCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), request))
        {
            Fail("Cancelled task became observable or blocked the failure scenario.");
            return;
        }

        stage = 5;
    }

    private void ObserveFailure()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Failed
            || task.Result is not null
            || coordinator.ObservedResult is not null
            || coordinator.TryObserveCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || !InputsAreUnchanged())
        {
            Fail("Failed task changed or exposed sabotage-action selection state.");
            return;
        }

        GD.Print(
            "Sabotage-action selection coordinator smoke passed: explicit exact observation, "
            + "selection and abstention retention, duplicate and stale rejection, explicit "
            + "acknowledgement, terminal failure isolation, closed surface, and unchanged input.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionSelectionCoordinator);
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

        return properties.SequenceEqual(["ObservedResult"])
            && methods.SequenceEqual(
                ["TryAcknowledgeCompletedResult", "TryObserveCompletedResult", "_Ready"]);
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(request.Actor, actor)
        && ReferenceEquals(request.Actor.Identity.Id, actor.Identity.Id)
        && ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Conceal)
        && ReferenceEquals(request.ActionKinds[1], selectedKind);

    private static CharacterSabotageActionConsiderationRequest CreateRequest(
        Character actor,
        out CharacterSabotageActionKind selection)
    {
        selection = CharacterSabotageActionKind.Misquote;
        return new CharacterSabotageActionConsiderationRequest(
            actor,
            [CharacterSabotageActionKind.Conceal, selection]);
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
}
