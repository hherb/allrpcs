namespace Allrpcs.Application.Navigation;

/// <summary>Accumulates bounded elapsed time and exposes at most one due task travel step.</summary>
public static class CharacterIndependentTaskStepClockTransition
{
    /// <summary>Returns one fresh clock and consumes one complete interval when due.</summary>
    public static CharacterIndependentTaskStepClockAdvanceResult Advance(
        CharacterIndependentTaskStepClock clock,
        CharacterIndependentTaskStepCadence cadence,
        TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(cadence);

        if (!ReferenceEquals(clock.Cadence, cadence))
        {
            throw new InvalidOperationException(
                "An independent-task step clock can advance only with its exact cadence.");
        }

        if (elapsed < TimeSpan.Zero || elapsed > cadence.Interval)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsed),
                elapsed,
                "One clock advance must remain between zero and one exact cadence interval.");
        }

        TimeSpan accumulated = clock.ElapsedTowardNextStep + elapsed;
        bool isStepDue = accumulated >= cadence.Interval;
        TimeSpan retainedElapsed = isStepDue
            ? accumulated - cadence.Interval
            : accumulated;
        CharacterIndependentTaskStepClock nextClock = new(cadence, retainedElapsed);
        return new CharacterIndependentTaskStepClockAdvanceResult(
            clock,
            nextClock,
            isStepDue);
    }
}
