using System.Numerics;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Retains one bounded provider-neutral route for an exact active independent task.</summary>
public sealed class CharacterIndependentTaskTravelPlan
{
    /// <summary>Gets the maximum number of stable waypoints in one travel plan.</summary>
    public const int MaximumWaypointCount = 32;

    /// <summary>Creates one immutable validated travel plan in supplied waypoint order.</summary>
    public CharacterIndependentTaskTravelPlan(
        CharacterIndependentTaskActivity activity,
        NavigationWorldBounds worldBounds,
        Vector2 origin,
        IEnumerable<Vector2> waypoints)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(waypoints);

        if (activity.State != CharacterIndependentTaskActivityState.Active)
        {
            throw new ArgumentException(
                "Independent-task travel requires an active task.",
                nameof(activity));
        }

        List<Vector2> retainedWaypoints = new();
        foreach (Vector2 waypoint in waypoints)
        {
            retainedWaypoints.Add(waypoint);
            if (retainedWaypoints.Count > MaximumWaypointCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(waypoints),
                    retainedWaypoints.Count,
                    $"An independent-task travel plan accepts at most {MaximumWaypointCount} waypoints.");
            }
        }

        if (retainedWaypoints.Count == 0)
        {
            throw new ArgumentException(
                "An independent-task travel plan requires at least one waypoint.",
                nameof(waypoints));
        }

        _ = NavigationRequestValidator.Create(worldBounds, origin, origin);
        foreach (Vector2 waypoint in retainedWaypoints)
        {
            _ = NavigationRequestValidator.Create(worldBounds, origin, waypoint);
        }

        Activity = activity;
        WorldBounds = worldBounds;
        Origin = origin;
        Waypoints = retainedWaypoints.AsReadOnly();
    }

    /// <summary>Gets the exact active independent-task activity represented by this plan.</summary>
    public CharacterIndependentTaskActivity Activity { get; }

    /// <summary>Gets the validated finite world bounds containing the route.</summary>
    public NavigationWorldBounds WorldBounds { get; }

    /// <summary>Gets the validated finite route origin.</summary>
    public Vector2 Origin { get; }

    /// <summary>Gets a stable copied read-only sequence of validated waypoints.</summary>
    public IReadOnlyList<Vector2> Waypoints { get; }
}
