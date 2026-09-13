using Allrpcs.Application.Navigation;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns one exact independent-task return plan and its immutable scene progress.</summary>
public partial class CharacterIndependentTaskReturnTravelController : Node
{
    /// <summary>Gets the exact initialized return plan, if any.</summary>
    public CharacterIndependentTaskReturnTravelPlan? Plan { get; private set; }

    /// <summary>Gets the exact current immutable return progress, if any.</summary>
    public CharacterIndependentTaskReturnTravelProgress? Progress { get; private set; }

    /// <summary>Initializes this controller once from one exact validated return plan.</summary>
    public bool TryInitialize(CharacterIndependentTaskReturnTravelPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (Plan is not null || Progress is not null)
        {
            return false;
        }

        Plan = plan;
        Progress = new CharacterIndependentTaskReturnTravelProgress(plan);
        return true;
    }

    /// <summary>Advances only the exact expected current return plan and progress by one waypoint.</summary>
    public bool TryAdvance(
        CharacterIndependentTaskReturnTravelPlan expectedPlan,
        CharacterIndependentTaskReturnTravelProgress expectedProgress,
        Vector2 reachedWaypoint)
    {
        ArgumentNullException.ThrowIfNull(expectedPlan);
        ArgumentNullException.ThrowIfNull(expectedProgress);

        if (!ReferenceEquals(Plan, expectedPlan)
            || !ReferenceEquals(Progress, expectedProgress))
        {
            return false;
        }

        CharacterIndependentTaskReturnTravelProgress nextProgress;
        try
        {
            nextProgress = CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                expectedProgress,
                expectedPlan,
                reachedWaypoint);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Progress = nextProgress;
        return true;
    }
}
