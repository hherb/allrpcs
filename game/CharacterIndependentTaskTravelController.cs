using Allrpcs.Application.Navigation;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns one exact independent-task travel plan and its immutable scene progress.</summary>
public partial class CharacterIndependentTaskTravelController : Node
{
    /// <summary>Gets the exact initialized travel plan, if any.</summary>
    public CharacterIndependentTaskTravelPlan? Plan { get; private set; }

    /// <summary>Gets the exact current immutable progress, if any.</summary>
    public CharacterIndependentTaskTravelProgress? Progress { get; private set; }

    /// <summary>Initializes this controller once from one exact validated travel plan.</summary>
    public bool TryInitialize(CharacterIndependentTaskTravelPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (Plan is not null || Progress is not null)
        {
            return false;
        }

        Plan = plan;
        Progress = new CharacterIndependentTaskTravelProgress(plan);
        return true;
    }

    /// <summary>Advances only the exact expected current plan and progress by one waypoint.</summary>
    public bool TryAdvance(
        CharacterIndependentTaskTravelPlan expectedPlan,
        CharacterIndependentTaskTravelProgress expectedProgress,
        Vector2 reachedWaypoint)
    {
        ArgumentNullException.ThrowIfNull(expectedPlan);
        ArgumentNullException.ThrowIfNull(expectedProgress);

        if (!ReferenceEquals(Plan, expectedPlan)
            || !ReferenceEquals(Progress, expectedProgress))
        {
            return false;
        }

        CharacterIndependentTaskTravelProgress nextProgress;
        try
        {
            nextProgress = CharacterIndependentTaskTravelProgressTransition.Advance(
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

    /// <summary>Installs only an exact prevalidated final-arrival result from current state.</summary>
    internal bool TryInstallArrival(
        CharacterIndependentTaskTravelPlan expectedPlan,
        CharacterIndependentTaskTravelProgress expectedProgress,
        CharacterIndependentTaskArrivalOccurrenceResult result)
    {
        ArgumentNullException.ThrowIfNull(expectedPlan);
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(result);

        if (!ReferenceEquals(Plan, expectedPlan)
            || !ReferenceEquals(Progress, expectedProgress)
            || !ReferenceEquals(result.TravelProgress.Plan, expectedPlan)
            || !result.TravelProgress.IsComplete
            || result.TravelProgress.CompletedWaypointCount
                != expectedProgress.CompletedWaypointCount + 1)
        {
            return false;
        }

        Progress = result.TravelProgress;
        return true;
    }
}
