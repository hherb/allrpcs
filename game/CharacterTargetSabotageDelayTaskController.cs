using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one target-delay decision task and exposes only its exact accepted result.</summary>
public partial class CharacterTargetSabotageDelayTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterTargetSabotageDelayConsiderationResult>? decisionTask;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned target-delay decision task.</summary>
    public CharacterTargetSabotageDelayTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted result installed on the main thread, if successful.</summary>
    public CharacterTargetSabotageDelayConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision for the exact retained request.</summary>
    public bool TryStart(
        ICharacterTargetSabotageDelayDecisionModel model,
        CharacterTargetSabotageDelayConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);

        if (Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || decisionTask is not null)
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterTargetSabotageDelayConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        Status = CharacterTargetSabotageDelayTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterTargetSabotageDelayTaskStatus.Running
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
        if (Status is CharacterTargetSabotageDelayTaskStatus.Idle
            or CharacterTargetSabotageDelayTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterTargetSabotageDelayTaskStatus.Idle;
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
        Task<CharacterTargetSabotageDelayConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterTargetSabotageDelayTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully)
        {
            Result = task.Result;
            Status = CharacterTargetSabotageDelayTaskStatus.Completed;
        }
        else
        {
            if (task.Exception is { } error)
            {
                GD.PrintErr("Target sabotage delay decision failed: " + error.GetBaseException().GetType().Name);
            }
            Status = CharacterTargetSabotageDelayTaskStatus.Failed;
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
