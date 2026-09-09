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
        return await GenerateCoreAsync(
            model,
            request,
            worldObjects,
            affordances,
            deadline,
            null,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns one complete bounded stream snapshot and observes only controller-synthesized
    /// cancellation or timeout through the supplied diagnostic boundary.
    /// </summary>
    public static async Task<CharacterModelStreamSnapshot> GenerateAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances,
        TimeSpan deadline,
        CharacterModelGenerationControllerDiagnostics diagnostics,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        return await GenerateCoreAsync(
            model,
            request,
            worldObjects,
            affordances,
            deadline,
            diagnostics,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<CharacterModelStreamSnapshot> GenerateCoreAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances,
        TimeSpan deadline,
        CharacterModelGenerationControllerDiagnostics? diagnostics,
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

        long? diagnosticStartTimestamp = TryGetDiagnosticTimestamp(diagnostics);

        using CancellationTokenSource deadlineCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadlineCancellation.CancelAfter(deadline);

        for (int attemptCount = 1;
             attemptCount <= CharacterModelRetryPolicy.MaximumAttemptCount;
             attemptCount++)
        {
            (CharacterModelStreamSnapshot attempt, bool synthesizedTermination) =
                await RunAttemptAsync(
                model,
                request,
                deadlineCancellation.Token,
                cancellationToken).ConfigureAwait(false);
            attempt = ValidateCompletedResponse(request, worldObjects, affordances, attempt);

            if (!CharacterModelRetryPolicy.CanRetry(attemptCount, attempt))
            {
                if (synthesizedTermination)
                {
                    await TryRecordSynthesizedDiagnosticAsync(
                        attempt,
                        diagnostics,
                        diagnosticStartTimestamp).ConfigureAwait(false);
                }

                return attempt;
            }
        }

        throw new InvalidOperationException("The bounded retry loop did not return a result.");
    }

    private static async Task<(
        CharacterModelStreamSnapshot Snapshot,
        bool SynthesizedTermination)> RunAttemptAsync(
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
            return (
                CharacterModelStreamAppendTransition.Append(
                    new CharacterModelStreamTermination(status, null, null),
                    snapshot),
                true);
        }

        if (snapshot.Updates.Count > 0
            && snapshot.Updates[^1] is CharacterModelStreamTermination termination)
        {
            if (termination.Status == CharacterModelStreamTerminationStatus.Completed
                && !ReferenceEquals(termination.Response?.Request, request.TurnRequest))
            {
                return (ReplaceTerminationWithInvalidResponse(snapshot), false);
            }

            return (snapshot, false);
        }

        return (
            CharacterModelStreamAppendTransition.Append(
                new CharacterModelStreamTermination(
                    CharacterModelStreamTerminationStatus.Failed,
                    null,
                    new CharacterModelFailure(CharacterModelFailureCode.InvalidResponse)),
                snapshot),
            false);
    }

    private static long? TryGetDiagnosticTimestamp(
        CharacterModelGenerationControllerDiagnostics? diagnostics)
    {
        if (diagnostics is null)
        {
            return null;
        }

        try
        {
            return diagnostics.TimeProvider.GetTimestamp();
        }
        catch (Exception exception) when (IsObservationalFailure(exception))
        {
            return null;
        }
    }

    private static async ValueTask TryRecordSynthesizedDiagnosticAsync(
        CharacterModelStreamSnapshot snapshot,
        CharacterModelGenerationControllerDiagnostics? diagnostics,
        long? startTimestamp)
    {
        if (diagnostics is null
            || startTimestamp is null
            || snapshot.Updates[^1] is not CharacterModelStreamTermination termination)
        {
            return;
        }

        try
        {
            TimeSpan elapsed = diagnostics.TimeProvider.GetElapsedTime(startTimestamp.Value);
            elapsed = elapsed < TimeSpan.Zero
                ? TimeSpan.Zero
                : elapsed > CharacterModelGenerationDiagnostic.MaximumElapsed
                    ? CharacterModelGenerationDiagnostic.MaximumElapsed
                    : elapsed;
            CharacterModelGenerationDiagnostic diagnostic = new(
                diagnostics.Provider,
                termination.Status,
                null,
                elapsed,
                null,
                null,
                null);
            await CharacterModelGenerationDiagnosticRecorder.TryRecordAsync(
                    diagnostics.Sink,
                    diagnostic,
                    diagnostics.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (IsObservationalFailure(exception))
        {
            return;
        }
    }

    private static bool IsObservationalFailure(Exception exception) =>
        exception is not OutOfMemoryException
        and not StackOverflowException
        and not AccessViolationException;

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
