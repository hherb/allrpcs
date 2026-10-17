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
        CancellationToken cancellationToken,
        TimeProvider? timeProvider = null)
    {
        return await GenerateCoreAsync(
            model,
            request,
            worldObjects,
            affordances,
            deadline,
            null,
            null,
            cancellationToken, timeProvider).ConfigureAwait(false);
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
        CancellationToken cancellationToken,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        return await GenerateCoreAsync(
            model,
            request,
            worldObjects,
            affordances,
            deadline,
            diagnostics,
            null,
            cancellationToken, timeProvider).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns one complete bounded stream snapshot while best-effort observing accepted text
    /// updates and only the final validated terminal update.
    /// </summary>
    public static async Task<CharacterModelStreamSnapshot> GenerateAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances,
        TimeSpan deadline,
        CharacterModelStreamObservation observation,
        CancellationToken cancellationToken,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(observation);
        return await GenerateCoreAsync(
            model,
            request,
            worldObjects,
            affordances,
            deadline,
            null,
            observation,
            cancellationToken, timeProvider).ConfigureAwait(false);
    }

    private static async Task<CharacterModelStreamSnapshot> GenerateCoreAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances,
        TimeSpan deadline,
        CharacterModelGenerationControllerDiagnostics? diagnostics,
        CharacterModelStreamObservation? observation,
        CancellationToken cancellationToken,
        TimeProvider? timeProvider = null)
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

        timeProvider ??= TimeProvider.System;
        using CancellationTokenSource deadlineTimer = new(deadline, timeProvider);
        using CancellationTokenSource deadlineCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, deadlineTimer.Token);

        long started = timeProvider.GetTimestamp();
        TimeSpan retryDelay = TimeSpan.Zero;
        for (int attemptCount = 1;
             attemptCount <= CharacterModelRetryPolicy.MaximumAttemptCount;
             attemptCount++)
        {
            (CharacterModelStreamSnapshot attempt, bool synthesizedTermination) =
                await RunAttemptAsync(
                model,
                request,
                observation,
                retryDelay,
                timeProvider,
                deadlineCancellation.Token,
                cancellationToken).ConfigureAwait(false);
            attempt = ValidateCompletedResponse(request, worldObjects, affordances, attempt);

            bool retry = CharacterModelRetryPolicy.CanRetry(attemptCount, attempt);
            if (retry)
            {
                CharacterModelStreamTermination failed = (CharacterModelStreamTermination)attempt.Updates[^1];
                retryDelay = CharacterModelRetryPolicy.GetDelay(failed.Failure!);
                // A wait that cannot finish before the deadline would report TimedOut instead of the provider's failure.
                retry = retryDelay < deadline - timeProvider.GetElapsedTime(started);
            }

            if (!retry)
            {
                await TryObserveTerminalAsync(attempt, observation).ConfigureAwait(false);
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
        CharacterModelStreamObservation? observation,
        TimeSpan retryDelay,
        TimeProvider timeProvider,
        CancellationToken deadlineToken,
        CancellationToken callerToken)
    {
        CharacterModelStreamSnapshot snapshot = new([]);
        try
        {
            if (retryDelay > TimeSpan.Zero)
            {
                await Task.Delay(retryDelay, timeProvider, deadlineToken).ConfigureAwait(false);
            }

            deadlineToken.ThrowIfCancellationRequested();
            await foreach (CharacterModelStreamUpdate update in model
                .StreamResponseAsync(request, deadlineToken)
                .WithCancellation(deadlineToken)
                .ConfigureAwait(false))
            {
                snapshot = CharacterModelStreamAppendTransition.Append(update, snapshot);
                if (update is CharacterModelTextDelta)
                {
                    await TryObserveUpdateAsync(update, observation).ConfigureAwait(false);
                }
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

    private static async ValueTask TryObserveTerminalAsync(
        CharacterModelStreamSnapshot snapshot,
        CharacterModelStreamObservation? observation)
    {
        if (snapshot.Updates.Count == 0
            || snapshot.Updates[^1] is not CharacterModelStreamTermination termination)
        {
            return;
        }

        await TryObserveUpdateAsync(termination, observation).ConfigureAwait(false);
    }

    private static async ValueTask TryObserveUpdateAsync(
        CharacterModelStreamUpdate update,
        CharacterModelStreamObservation? observation)
    {
        if (observation is null)
        {
            return;
        }

        try
        {
            await observation.Sink.RecordAsync(update, observation.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (IsObservationalFailure(exception))
        {
            return;
        }
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
