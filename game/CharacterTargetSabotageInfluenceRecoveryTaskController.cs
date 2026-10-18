using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one recovery decision task and exposes only its exact accepted result.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterTargetSabotageInfluenceRecoveryConsiderationResult>? decisionTask;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned influence-recovery decision task.</summary>
    public CharacterTargetSabotageInfluenceRecoveryTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted result installed on the main thread, if successful.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision for the exact retained recovery request.</summary>
    public bool TryStart(
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);

        if (Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            || decisionTask is not null)
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        Status = CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running
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
        if (Status is CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            or CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle;
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
        Task<CharacterTargetSabotageInfluenceRecoveryConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterTargetSabotageInfluenceRecoveryTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully)
        {
            Result = task.Result;
            Status = CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed;
        }
        else
        {
            if (task.Exception is { } error)
            {
                GD.PrintErr("Target sabotage influence recovery decision failed: " + error.GetBaseException().GetType().Name);
            }
            Status = CharacterTargetSabotageInfluenceRecoveryTaskStatus.Failed;
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
