namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one bounded provider-neutral character-model failure without provider details.
/// </summary>
public sealed record CharacterModelFailure
{
    /// <summary>
    /// Creates a failure from one defined provider-neutral code.
    /// </summary>
    public CharacterModelFailure(CharacterModelFailureCode code, TimeSpan? retryAfter = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        Code = code;
        RetryAfter = retryAfter is { } delay
            ? delay < TimeSpan.Zero
                ? CharacterModelRetryPolicy.DefaultDelay
                : delay > CharacterModelRetryPolicy.MaximumDelay
                    ? CharacterModelRetryPolicy.MaximumDelay
                    : delay
            : null;
    }

    /// <summary>
    /// Gets the provider-neutral failure code.
    /// </summary>
    public CharacterModelFailureCode Code { get; }

    /// <summary>Gets bounded retry timing without retaining provider header text.</summary>
    public TimeSpan? RetryAfter { get; }
}
