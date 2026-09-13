using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one concealment-decision task and resolves only its exact accepted result.</summary>
public partial class CharacterObjectSabotageConcealmentTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterObjectSabotageConcealmentConsiderationResult>? decisionTask;
    private CharacterObjectSabotageConcealmentController? state;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned concealment-decision task.</summary>
    public CharacterObjectSabotageConcealmentTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted and resolved result, if successful.</summary>
    public CharacterObjectSabotageConcealmentConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision only for exact current scene-owned request snapshots.</summary>
    public bool TryStart(
        ICharacterObjectSabotageConcealmentDecisionModel model,
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        CharacterObjectSabotageConcealmentController state)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(state);

        if (Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || decisionTask is not null
            || !ReferenceEquals(state.Concealment, request.Concealment)
            || !ReferenceEquals(state.Evidence, request.Evidence))
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterObjectSabotageConcealmentConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        this.state = state;
        Status = CharacterObjectSabotageConcealmentTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterObjectSabotageConcealmentTaskStatus.Running
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
        if (Status is CharacterObjectSabotageConcealmentTaskStatus.Idle
            or CharacterObjectSabotageConcealmentTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterObjectSabotageConcealmentTaskStatus.Idle;
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
        Task<CharacterObjectSabotageConcealmentConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterObjectSabotageConcealmentTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully
            && state is not null
            && state.TryResolveConsideration(task.Result))
        {
            Result = task.Result;
            Status = CharacterObjectSabotageConcealmentTaskStatus.Completed;
        }
        else
        {
            Status = CharacterObjectSabotageConcealmentTaskStatus.Failed;
        }

        ClearActiveTask();
    }

    private void ClearActiveTask()
    {
        decisionTask = null;
        state = null;
        decisionCancellation?.Dispose();
        decisionCancellation = null;
    }
}
