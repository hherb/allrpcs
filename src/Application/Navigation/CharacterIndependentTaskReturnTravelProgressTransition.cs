using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Advances immutable independent-task return travel through the exact next waypoint only.</summary>
public static class CharacterIndependentTaskReturnTravelProgressTransition
{
    /// <summary>Returns fresh return progress advanced by exactly one expected waypoint.</summary>
    public static CharacterIndependentTaskReturnTravelProgress Advance(
        CharacterIndependentTaskReturnTravelProgress progress,
        CharacterIndependentTaskReturnTravelPlan plan,
        Vector2 reachedWaypoint)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(plan);

        if (!ReferenceEquals(progress.Plan, plan))
        {
            throw new InvalidOperationException(
                "Independent-task return travel can advance only against its exact plan.");
        }

        if (progress.IsComplete
            || reachedWaypoint != plan.Waypoints[progress.CompletedWaypointCount])
        {
            throw new InvalidOperationException(
                "Independent-task return travel can advance only through its exact next waypoint.");
        }

        return new CharacterIndependentTaskReturnTravelProgress(
            plan,
            progress.CompletedWaypointCount + 1);
    }
}
