namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Finite transport budgets and clock, independently of a caller's generation deadline.</summary>
public sealed class ProviderHttpTimeoutOptions
{
    /// <summary>Creates bounded budgets; shorter values and a clock may be supplied by tests.</summary>
    public ProviderHttpTimeoutOptions(
        TimeSpan? requestTimeout = null,
        TimeSpan? idleTimeout = null,
        TimeProvider? timeProvider = null)
    {
        RequestTimeout = Validate(requestTimeout ?? TimeSpan.FromSeconds(120), 120, nameof(requestTimeout));
        IdleTimeout = Validate(idleTimeout ?? TimeSpan.FromSeconds(30), 30, nameof(idleTimeout));
        TimeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>Gets the maximum lifetime from sending headers until response disposal.</summary>
    public TimeSpan RequestTimeout { get; }

    /// <summary>Gets the maximum wait for each transport read.</summary>
    public TimeSpan IdleTimeout { get; }

    /// <summary>Gets the clock used by both cancellation timers.</summary>
    public TimeProvider TimeProvider { get; }

    private static TimeSpan Validate(TimeSpan value, int maximumSeconds, string name) =>
        value > TimeSpan.Zero && value <= TimeSpan.FromSeconds(maximumSeconds)
            ? value
            : throw new ArgumentOutOfRangeException(name);
}
