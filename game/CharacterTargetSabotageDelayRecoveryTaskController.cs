using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one delay-recovery decision task and exposes only its exact accepted result.</summary>
public partial class CharacterTargetSabotageDelayRecoveryTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterTargetSabotageDelayRecoveryConsiderationResult>? decisionTask;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned delay-recovery decision task.</summary>
    public CharacterTargetSabotageDelayRecoveryTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted result installed on the main thread, if successful.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision for the exact retained delay-recovery request.</summary>
    public bool TryStart(
        ICharacterTargetSabotageDelayRecoveryDecisionModel model,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);

        if (Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            || decisionTask is not null)
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        Status = CharacterTargetSabotageDelayRecoveryTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Running
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
        if (Status is CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            or CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterTargetSabotageDelayRecoveryTaskStatus.Idle;
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
        Task<CharacterTargetSabotageDelayRecoveryConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterTargetSabotageDelayRecoveryTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully)
        {
            Result = task.Result;
            Status = CharacterTargetSabotageDelayRecoveryTaskStatus.Completed;
        }
        else
        {
            Status = CharacterTargetSabotageDelayRecoveryTaskStatus.Failed;
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
