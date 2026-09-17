using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one influence decision task and exposes only its exact accepted result.</summary>
public partial class CharacterTargetSabotageInfluenceTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterTargetSabotageInfluenceConsiderationResult>? decisionTask;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned influence decision task.</summary>
    public CharacterTargetSabotageInfluenceTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted result installed on the main thread, if successful.</summary>
    public CharacterTargetSabotageInfluenceConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision for the exact retained request.</summary>
    public bool TryStart(
        ICharacterTargetSabotageInfluenceDecisionModel model,
        CharacterTargetSabotageInfluenceConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);

        if (Status != CharacterTargetSabotageInfluenceTaskStatus.Idle
            || decisionTask is not null)
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        Status = CharacterTargetSabotageInfluenceTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterTargetSabotageInfluenceTaskStatus.Running
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
        if (Status is CharacterTargetSabotageInfluenceTaskStatus.Idle
            or CharacterTargetSabotageInfluenceTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterTargetSabotageInfluenceTaskStatus.Idle;
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
        Task<CharacterTargetSabotageInfluenceConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterTargetSabotageInfluenceTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully)
        {
            Result = task.Result;
            Status = CharacterTargetSabotageInfluenceTaskStatus.Completed;
        }
        else
        {
            Status = CharacterTargetSabotageInfluenceTaskStatus.Failed;
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
