namespace Allrpcs.Application.Navigation;

/// <summary>Retains immutable one-waypoint-at-a-time progress over one exact travel plan.</summary>
public sealed class CharacterIndependentTaskTravelProgress
{
    /// <summary>Creates progress before any waypoint in the exact plan has been completed.</summary>
    public CharacterIndependentTaskTravelProgress(CharacterIndependentTaskTravelPlan plan)
        : this(plan, 0)
    {
    }

    internal CharacterIndependentTaskTravelProgress(
        CharacterIndependentTaskTravelPlan plan,
        int completedWaypointCount)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (completedWaypointCount < 0 || completedWaypointCount > plan.Waypoints.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedWaypointCount),
                completedWaypointCount,
                "Completed waypoint count must remain inside the exact plan.");
        }

        Plan = plan;
        CompletedWaypointCount = completedWaypointCount;
    }

    /// <summary>Gets the exact travel plan represented by this progress.</summary>
    public CharacterIndependentTaskTravelPlan Plan { get; }

    /// <summary>Gets the number of waypoints completed in stable route order.</summary>
    public int CompletedWaypointCount { get; }

    /// <summary>Gets whether every waypoint in the exact plan has been completed.</summary>
    public bool IsComplete => CompletedWaypointCount == Plan.Waypoints.Count;
}
