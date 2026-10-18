using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one misquotation-evidence decision task and exposes only its exact accepted result.</summary>
public partial class CharacterDialogueMisquotationEvidenceTaskController : Node
{
    private CancellationTokenSource? decisionCancellation;
    private Task<CharacterDialogueMisquotationEvidenceConsiderationResult>? decisionTask;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned misquotation-evidence decision task.</summary>
    public CharacterDialogueMisquotationEvidenceTaskStatus Status { get; private set; }

    /// <summary>Gets the exact accepted result installed on the main thread, if successful.</summary>
    public CharacterDialogueMisquotationEvidenceConsiderationResult? Result { get; private set; }

    /// <summary>Starts one decision for the exact retained request.</summary>
    public bool TryStart(
        ICharacterDialogueMisquotationEvidenceDecisionModel model,
        CharacterDialogueMisquotationEvidenceConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);

        if (Status != CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            || decisionTask is not null)
        {
            return false;
        }

        cancellationRequested = false;
        decisionCancellation = new CancellationTokenSource();
        decisionTask = CharacterDialogueMisquotationEvidenceConsiderationController.DecideAsync(
            model,
            request,
            decisionCancellation.Token);
        Status = CharacterDialogueMisquotationEvidenceTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the exact decision remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterDialogueMisquotationEvidenceTaskStatus.Running
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
        if (Status is CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            or CharacterDialogueMisquotationEvidenceTaskStatus.Running
            || decisionTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterDialogueMisquotationEvidenceTaskStatus.Idle;
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
        Task<CharacterDialogueMisquotationEvidenceConsiderationResult>? task = decisionTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        _ = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterDialogueMisquotationEvidenceTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully)
        {
            Result = task.Result;
            Status = CharacterDialogueMisquotationEvidenceTaskStatus.Completed;
        }
        else
        {
            if (task.Exception is { } error)
            {
                GD.PrintErr("Dialogue misquotation evidence decision failed: " + error.GetBaseException().GetType().Name);
            }
            Status = CharacterDialogueMisquotationEvidenceTaskStatus.Failed;
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
