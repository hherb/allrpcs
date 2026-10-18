using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one sabotage-action decision task and exposes only its exact accepted result.</summary>
public partial class CharacterSabotageActionTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterSabotageActionConsiderationResult>? decisionTask;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned sabotage-action decision task.</summary>
    public CharacterSabotageActionTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted result installed on the main thread, if successful.</summary>
    public CharacterSabotageActionConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision for the exact retained request.</summary>
    public bool TryStart(
        ICharacterSabotageActionDecisionModel model,
        CharacterSabotageActionConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);

        if (Status != CharacterSabotageActionTaskStatus.Idle
            || decisionTask is not null)
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterSabotageActionConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        Status = CharacterSabotageActionTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterSabotageActionTaskStatus.Running
            || decisionTask is null
            || decisionTask.IsCompleted
            || cancellationRequested
            || decisionCancellation is null)
        {
            return false;
        }

        cancellationRequested = true;
        decisionCancellation.Cancel();
        return true;
    }

    /// <summary>Clears one terminal outcome so another exact decision can start.</summary>
    public bool TryReset()
    {
        if (Status is CharacterSabotageActionTaskStatus.Idle
            or CharacterSabotageActionTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterSabotageActionTaskStatus.Idle;
        return true;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CompleteFinishedDecision();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        decisionCancellation?.Cancel();
        if (decisionTask is { IsCompleted: true } completedTask)
        {
            _ = completedTask.Exception;
        }

        ClearActiveTask();
    }

    private void CompleteFinishedDecision()
    {
        Task<CharacterSabotageActionConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterSabotageActionTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully)
        {
            Result = task.Result;
            Status = CharacterSabotageActionTaskStatus.Completed;
        }
        else
        {
            if (task.Exception is { } error)
            {
                GD.PrintErr("Sabotage action decision failed: " + error.GetBaseException().GetType().Name);
            }
            Status = CharacterSabotageActionTaskStatus.Failed;
        }

        ClearActiveTask();
    }

    private void ClearActiveTask()
    {
        decisionTask = null;
        decisionCancellation?.Dispose();
        decisionCancellation = null;
    }
}
