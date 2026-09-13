using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Advances immutable independent-task travel through the exact next waypoint only.</summary>
public static class CharacterIndependentTaskTravelProgressTransition
{
    /// <summary>Returns fresh progress advanced by exactly one expected waypoint.</summary>
    public static CharacterIndependentTaskTravelProgress Advance(
        CharacterIndependentTaskTravelProgress progress,
        CharacterIndependentTaskTravelPlan plan,
        Vector2 reachedWaypoint)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(plan);

        if (!ReferenceEquals(progress.Plan, plan))
        {
            throw new InvalidOperationException(
                "Independent-task travel can advance only against its exact plan.");
        }

        if (progress.IsComplete
            || reachedWaypoint != plan.Waypoints[progress.CompletedWaypointCount])
        {
            throw new InvalidOperationException(
                "Independent-task travel can advance only through its exact next waypoint.");
        }

        return new CharacterIndependentTaskTravelProgress(
            plan,
            progress.CompletedWaypointCount + 1);
    }
}
