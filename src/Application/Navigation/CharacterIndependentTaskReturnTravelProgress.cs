namespace Allrpcs.Application.Navigation;

/// <summary>Retains immutable one-waypoint-at-a-time progress over one exact return plan.</summary>
public sealed class CharacterIndependentTaskReturnTravelProgress
{
    /// <summary>Creates progress before any waypoint in the exact return plan has been completed.</summary>
    public CharacterIndependentTaskReturnTravelProgress(CharacterIndependentTaskReturnTravelPlan plan)
        : this(plan, 0)
    {
    }

    internal CharacterIndependentTaskReturnTravelProgress(
        CharacterIndependentTaskReturnTravelPlan plan,
        int completedWaypointCount)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (completedWaypointCount < 0 || completedWaypointCount > plan.Waypoints.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedWaypointCount),
                completedWaypointCount,
                "Completed waypoint count must remain inside the exact return plan.");
        }

        Plan = plan;
        CompletedWaypointCount = completedWaypointCount;
    }

    /// <summary>Gets the exact return-travel plan represented by this progress.</summary>
    public CharacterIndependentTaskReturnTravelPlan Plan { get; }

    /// <summary>Gets the number of return waypoints completed in stable route order.</summary>
    public int CompletedWaypointCount { get; }

    /// <summary>Gets whether every waypoint in the exact return plan has been completed.</summary>
    public bool IsComplete => CompletedWaypointCount == Plan.Waypoints.Count;
}
