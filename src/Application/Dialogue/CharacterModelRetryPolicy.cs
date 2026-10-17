namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Allows one bounded retry only for a transient text-free character-model generation failure.
/// </summary>
public static class CharacterModelRetryPolicy
{
    /// <summary>Gets the fallback delay when a retryable failure has no valid timing hint.</summary>
    public static TimeSpan DefaultDelay => TimeSpan.FromSeconds(1);

    /// <summary>Gets the longest permitted server-directed retry delay.</summary>
    public static TimeSpan MaximumDelay => TimeSpan.FromSeconds(30);

    /// <summary>Gets the bounded delay for a retryable failure.</summary>
    public static TimeSpan GetDelay(CharacterModelFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return failure.RetryAfter ?? DefaultDelay;
    }

    /// <summary>Gets the initial attempt plus the single permitted retry.</summary>
    public const int MaximumAttemptCount = 2;

    /// <summary>
    /// Returns whether a completed attempt is safe to repeat without replacing exposed text.
    /// </summary>
    public static bool CanRetry(
        int completedAttemptCount,
        CharacterModelStreamSnapshot attempt)
    {
        if (completedAttemptCount < 1 || completedAttemptCount > MaximumAttemptCount)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAttemptCount));
        }

        ArgumentNullException.ThrowIfNull(attempt);

        if (completedAttemptCount == MaximumAttemptCount
            || attempt.Updates.Any(update => update is CharacterModelTextDelta)
            || attempt.Updates.Count == 0
            || attempt.Updates[^1] is not CharacterModelStreamTermination
            {
                Status: CharacterModelStreamTerminationStatus.Failed,
                Failure: not null,
            } termination)
        {
            return false;
        }

        return termination.Failure.Code is CharacterModelFailureCode.Unavailable
            or CharacterModelFailureCode.RateLimited
            or CharacterModelFailureCode.Transport;
    }
}
