using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one move-decision task and resolves only its exact accepted result.</summary>
public partial class CharacterObjectSabotageMoveTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterObjectSabotageMoveConsiderationResult>? decisionTask;
    private CharacterObjectSabotageMoveController? state;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned move-decision task.</summary>
    public CharacterObjectSabotageMoveTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted and resolved result, if successful.</summary>
    public CharacterObjectSabotageMoveConsiderationResult? Result { get; private set; }

    /// <summary>Gets the exact authoritative placements installed for the completed result.</summary>
    internal WorldObjectPlacementSnapshot? ResolvedPlacements { get; private set; }

    /// <summary>Gets the exact authoritative evidence installed for the completed result.</summary>
    internal DiscoveryEvidenceSnapshot? ResolvedEvidence { get; private set; }

    /// <summary>Starts one decision only for exact current scene-owned request snapshots.</summary>
    public bool TryStart(
        ICharacterObjectSabotageMoveDecisionModel model,
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveController state)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(state);

        if (Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || decisionTask is not null
            || !ReferenceEquals(state.Placements, request.Placements)
            || !ReferenceEquals(state.Evidence, request.Evidence))
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterObjectSabotageMoveConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        this.state = state;
        Status = CharacterObjectSabotageMoveTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterObjectSabotageMoveTaskStatus.Running
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
        if (Status is CharacterObjectSabotageMoveTaskStatus.Idle
            or CharacterObjectSabotageMoveTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        ResolvedPlacements = null;
        ResolvedEvidence = null;
        cancellationRequested = false;
        Status = CharacterObjectSabotageMoveTaskStatus.Idle;
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
        Task<CharacterObjectSabotageMoveConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterObjectSabotageMoveTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully
            && state is not null
            && state.TryResolveConsideration(task.Result))
        {
            Result = task.Result;
            ResolvedPlacements = state.Placements;
            ResolvedEvidence = state.Evidence;
            Status = CharacterObjectSabotageMoveTaskStatus.Completed;
        }
        else
        {
            Status = CharacterObjectSabotageMoveTaskStatus.Failed;
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
