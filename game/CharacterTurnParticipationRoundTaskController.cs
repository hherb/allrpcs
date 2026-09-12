using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Owns one bounded participation-decision round task and installs only its complete result.
/// </summary>
public partial class CharacterTurnParticipationRoundTaskController : Node
{
    private CancellationTokenSource? roundCancellation;
    private Task<CharacterTurnParticipationConsiderationRoundResult>? roundTask;
    private bool cancellationRequested;

    /// <summary>Gets the bounded state of the scene-owned participation-round task.</summary>
    public CharacterTurnParticipationRoundTaskStatus Status { get; private set; }

    /// <summary>Gets the exact complete successful round installed on the main thread, if any.</summary>
    public CharacterTurnParticipationConsiderationRoundResult? Result { get; private set; }

    /// <summary>Starts at most one complete participation-decision round with one shared deadline.</summary>
    public bool TryStart(
        ICharacterTurnParticipationModel model,
        IEnumerable<Character> scheduledCharacters,
        DialogueEvent heardDialogueEvent,
        IEnumerable<CharacterId> directlyAddressedCharacterIds,
        TimeSpan deadline)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(scheduledCharacters);
        ArgumentNullException.ThrowIfNull(heardDialogueEvent);
        ArgumentNullException.ThrowIfNull(directlyAddressedCharacterIds);
        if (deadline <= TimeSpan.Zero || deadline == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(deadline));
        }

        if (Status != CharacterTurnParticipationRoundTaskStatus.Idle || roundTask is not null)
        {
            return false;
        }

        cancellationRequested = false;
        roundCancellation = new CancellationTokenSource();
        roundTask = CharacterTurnParticipationConsiderationController.DecideRoundAsync(
            model,
            scheduledCharacters,
            heardDialogueEvent,
            directlyAddressedCharacterIds,
            deadline,
            roundCancellation.Token);
        Status = CharacterTurnParticipationRoundTaskStatus.Running;
        return true;
    }

    /// <summary>Requests caller cancellation once while the participation round remains active.</summary>
    public bool TryCancel()
    {
        if (Status != CharacterTurnParticipationRoundTaskStatus.Running
            || roundTask is null
            || roundTask.IsCompleted
            || cancellationRequested
            || roundCancellation is null)
        {
            return false;
        }

        cancellationRequested = true;
        roundCancellation.Cancel();
        return true;
    }

    /// <summary>Clears one terminal task outcome so another exact round can start.</summary>
    public bool TryReset()
    {
        if (Status is CharacterTurnParticipationRoundTaskStatus.Idle
            or CharacterTurnParticipationRoundTaskStatus.Running
            || roundTask is not null)
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        Status = CharacterTurnParticipationRoundTaskStatus.Idle;
        return true;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CompleteFinishedRound();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        roundCancellation?.Cancel();
        if (roundTask is { IsCompleted: true } completedTask)
        {
            _ = completedTask.Exception;
        }

        DisposeCancellationSource();
    }

    private void CompleteFinishedRound()
    {
        Task<CharacterTurnParticipationConsiderationRoundResult>? task = roundTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        AggregateException? exception = task.Exception;
        if (cancellationRequested)
        {
            Status = CharacterTurnParticipationRoundTaskStatus.Cancelled;
        }
        else if (task.IsCompletedSuccessfully)
        {
            Result = task.Result;
            Status = CharacterTurnParticipationRoundTaskStatus.Completed;
        }
        else if (IsTimeout(exception))
        {
            Status = CharacterTurnParticipationRoundTaskStatus.TimedOut;
        }
        else
        {
            Status = CharacterTurnParticipationRoundTaskStatus.Failed;
        }

        roundTask = null;
        DisposeCancellationSource();
    }

    private static bool IsTimeout(AggregateException? exception)
    {
        if (exception is null)
        {
            return false;
        }

        IReadOnlyCollection<Exception> failures = exception.Flatten().InnerExceptions;
        return failures.Count == 1 && failures.Single() is TimeoutException;
    }

    private void DisposeCancellationSource()
    {
        roundCancellation?.Dispose();
        roundCancellation = null;
    }
}
