using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact independent-task cadence and its immutable elapsed scene clock.</summary>
public partial class CharacterIndependentTaskStepClockController : Node
{
    /// <summary>Gets the exact initialized task-step cadence, if any.</summary>
    public CharacterIndependentTaskStepCadence? Cadence { get; private set; }

    /// <summary>Gets the exact current immutable task-step clock, if any.</summary>
    public CharacterIndependentTaskStepClock? Clock { get; private set; }

    /// <summary>Gets the exact most recently accepted clock-advance result, if any.</summary>
    public CharacterIndependentTaskStepClockAdvanceResult? LastAdvanceResult { get; private set; }

    /// <summary>Initializes one exact cadence and its zero-elapsed clock once.</summary>
    public bool TryInitialize(CharacterIndependentTaskStepCadence cadence)
    {
        ArgumentNullException.ThrowIfNull(cadence);

        if (Cadence is not null || Clock is not null || LastAdvanceResult is not null)
        {
            return false;
        }

        Cadence = cadence;
        Clock = new CharacterIndependentTaskStepClock(cadence);
        return true;
    }

    /// <summary>Advances only the exact expected cadence and clock with bounded elapsed time.</summary>
    public bool TryAdvance(
        CharacterIndependentTaskStepCadence expectedCadence,
        CharacterIndependentTaskStepClock expectedClock,
        TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(expectedCadence);
        ArgumentNullException.ThrowIfNull(expectedClock);

        if (!ReferenceEquals(Cadence, expectedCadence)
            || !ReferenceEquals(Clock, expectedClock))
        {
            return false;
        }

        CharacterIndependentTaskStepClockAdvanceResult result;
        try
        {
            result = CharacterIndependentTaskStepClockTransition.Advance(
                expectedClock,
                expectedCadence,
                elapsed);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Clock = result.Clock;
        LastAdvanceResult = result;
        return true;
    }
}
