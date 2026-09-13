namespace Allrpcs.Application.Navigation;

/// <summary>Retains one exact immutable clock advance and whether one travel step became due.</summary>
public sealed class CharacterIndependentTaskStepClockAdvanceResult
{
    internal CharacterIndependentTaskStepClockAdvanceResult(
        CharacterIndependentTaskStepClock previousClock,
        CharacterIndependentTaskStepClock clock,
        bool isStepDue)
    {
        PreviousClock = previousClock;
        Clock = clock;
        IsStepDue = isStepDue;
    }

    /// <summary>Gets the exact clock supplied to the transition.</summary>
    public CharacterIndependentTaskStepClock PreviousClock { get; }

    /// <summary>Gets the fresh clock retaining unconsumed elapsed time.</summary>
    public CharacterIndependentTaskStepClock Clock { get; }

    /// <summary>Gets whether exactly one travel step became due.</summary>
    public bool IsStepDue { get; }
}
