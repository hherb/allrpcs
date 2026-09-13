namespace Allrpcs.Application.Navigation;

/// <summary>Defines one bounded provider-neutral cadence for independent-task travel steps.</summary>
public sealed class CharacterIndependentTaskStepCadence
{
    /// <summary>Gets the maximum accepted interval between independent-task travel steps.</summary>
    public static TimeSpan MaximumInterval { get; } = TimeSpan.FromHours(1);

    /// <summary>Creates one cadence with a finite positive bounded interval.</summary>
    public CharacterIndependentTaskStepCadence(TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero
            || interval == Timeout.InfiniteTimeSpan
            || interval > MaximumInterval)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                interval,
                $"A task-step interval must be positive and no greater than {MaximumInterval}.");
        }

        Interval = interval;
    }

    /// <summary>Gets the exact validated interval between task travel steps.</summary>
    public TimeSpan Interval { get; }
}
