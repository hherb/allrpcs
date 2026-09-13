namespace Allrpcs.Application.Navigation;

/// <summary>Retains immutable elapsed progress toward one independent-task travel step.</summary>
public sealed class CharacterIndependentTaskStepClock
{
    /// <summary>Creates a clock at the start of one exact cadence interval.</summary>
    public CharacterIndependentTaskStepClock(CharacterIndependentTaskStepCadence cadence)
        : this(cadence, TimeSpan.Zero)
    {
    }

    internal CharacterIndependentTaskStepClock(
        CharacterIndependentTaskStepCadence cadence,
        TimeSpan elapsedTowardNextStep)
    {
        ArgumentNullException.ThrowIfNull(cadence);

        if (elapsedTowardNextStep < TimeSpan.Zero
            || elapsedTowardNextStep >= cadence.Interval)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsedTowardNextStep),
                elapsedTowardNextStep,
                "Elapsed task-step time must remain inside the exact cadence interval.");
        }

        Cadence = cadence;
        ElapsedTowardNextStep = elapsedTowardNextStep;
    }

    /// <summary>Gets the exact cadence represented by this clock.</summary>
    public CharacterIndependentTaskStepCadence Cadence { get; }

    /// <summary>Gets accumulated elapsed time not yet consumed by a due step.</summary>
    public TimeSpan ElapsedTowardNextStep { get; }
}
