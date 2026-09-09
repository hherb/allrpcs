using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Executes one provider-neutral character-model generation within a caller-visible deadline and
/// applies the bounded retry policy without executing any generated intention.
/// </summary>
public static class CharacterModelGenerationController
{
    /// <summary>
    /// Returns one complete bounded stream snapshot, translating caller cancellation, deadline
    /// expiry, and missing provider termination into provider-neutral terminal outcomes.
    /// </summary>
    public static async Task<CharacterModelStreamSnapshot> GenerateAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances,
        TimeSpan deadline,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(affordances);

        if (deadline <= TimeSpan.Zero || deadline == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(deadline));
        }

        using CancellationTokenSource deadlineCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadlineCancellation.CancelAfter(deadline);

        for (int attemptCount = 1;
             attemptCount <= CharacterModelRetryPolicy.MaximumAttemptCount;
             attemptCount++)
        {
            CharacterModelStreamSnapshot attempt = await RunAttemptAsync(
                model,
                request,
                deadlineCancellation.Token,
                cancellationToken).ConfigureAwait(false);
            attempt = ValidateCompletedResponse(request, worldObjects, affordances, attempt);

            if (!CharacterModelRetryPolicy.CanRetry(attemptCount, attempt))
            {
                return attempt;
            }
        }

        throw new InvalidOperationException("The bounded retry loop did not return a result.");
    }

    private static async Task<CharacterModelStreamSnapshot> RunAttemptAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        CancellationToken deadlineToken,
        CancellationToken callerToken)
    {
        CharacterModelStreamSnapshot snapshot = new([]);
        try
        {
            await foreach (CharacterModelStreamUpdate update in model
                .StreamResponseAsync(request, deadlineToken)
                .WithCancellation(deadlineToken)
                .ConfigureAwait(false))
            {
                snapshot = CharacterModelStreamAppendTransition.Append(update, snapshot);
            }
        }
        catch (OperationCanceledException) when (deadlineToken.IsCancellationRequested)
        {
            CharacterModelStreamTerminationStatus status = callerToken.IsCancellationRequested
                ? CharacterModelStreamTerminationStatus.Cancelled
                : CharacterModelStreamTerminationStatus.TimedOut;
            return CharacterModelStreamAppendTransition.Append(
                new CharacterModelStreamTermination(status, null, null),
                snapshot);
        }

        if (snapshot.Updates.Count > 0
            && snapshot.Updates[^1] is CharacterModelStreamTermination termination)
        {
            if (termination.Status == CharacterModelStreamTerminationStatus.Completed
                && !ReferenceEquals(termination.Response?.Request, request.TurnRequest))
            {
                return ReplaceTerminationWithInvalidResponse(snapshot);
            }

            return snapshot;
        }

        return CharacterModelStreamAppendTransition.Append(
            new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Failed,
                null,
                new CharacterModelFailure(CharacterModelFailureCode.InvalidResponse)),
            snapshot);
    }

    private static CharacterModelStreamSnapshot ReplaceTerminationWithInvalidResponse(
        CharacterModelStreamSnapshot snapshot)
    {
        List<CharacterModelStreamUpdate> updates = snapshot.Updates
            .Take(snapshot.Updates.Count - 1)
            .ToList();
        updates.Add(new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(CharacterModelFailureCode.InvalidResponse)));
        return new CharacterModelStreamSnapshot(updates);
    }

    private static CharacterModelStreamSnapshot ValidateCompletedResponse(
        CharacterModelGenerationRequest request,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances,
        CharacterModelStreamSnapshot snapshot)
    {
        if (snapshot.Updates.Count == 0
            || snapshot.Updates[^1] is not CharacterModelStreamTermination
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Response: not null,
            } termination)
        {
            return snapshot;
        }

        CharacterTurnResponseValidationResult validation =
            CharacterTurnResponseValidationPolicy.Validate(
                request.TurnRequest,
                termination.Response,
                worldObjects,
                affordances);
        return validation.IsValid
            ? snapshot
            : ReplaceTerminationWithInvalidResponse(snapshot);
    }
}
