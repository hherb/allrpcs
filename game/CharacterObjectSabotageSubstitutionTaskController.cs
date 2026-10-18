using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one substitution-decision task and resolves only its exact accepted result.</summary>
public partial class CharacterObjectSabotageSubstitutionTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterObjectSabotageSubstitutionConsiderationResult>? decisionTask;
    private CharacterObjectSabotageSubstitutionController? state;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned substitution-decision task.</summary>
    public CharacterObjectSabotageSubstitutionTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted and resolved result, if successful.</summary>
    public CharacterObjectSabotageSubstitutionConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision only for exact current scene-owned request snapshots.</summary>
    public bool TryStart(
        ICharacterObjectSabotageSubstitutionDecisionModel model,
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        CharacterObjectSabotageSubstitutionController state)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(state);

        if (Status != CharacterObjectSabotageSubstitutionTaskStatus.Idle
            || decisionTask is not null
            || !ReferenceEquals(state.Substitutions, request.Substitutions)
            || !ReferenceEquals(state.Evidence, request.Evidence))
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterObjectSabotageSubstitutionConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        this.state = state;
        Status = CharacterObjectSabotageSubstitutionTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterObjectSabotageSubstitutionTaskStatus.Running
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
        if (Status is CharacterObjectSabotageSubstitutionTaskStatus.Idle
            or CharacterObjectSabotageSubstitutionTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterObjectSabotageSubstitutionTaskStatus.Idle;
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
        Task<CharacterObjectSabotageSubstitutionConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterObjectSabotageSubstitutionTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully
            && state is not null
            && state.TryResolveConsideration(task.Result))
        {
            Result = task.Result;
            Status = CharacterObjectSabotageSubstitutionTaskStatus.Completed;
        }
        else
        {
            if (task.Exception is { } error)
            {
                GD.PrintErr("Object sabotage substitution decision failed: " + error.GetBaseException().GetType().Name);
            }
            Status = CharacterObjectSabotageSubstitutionTaskStatus.Failed;
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
