using System.Numerics;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Retains one bounded return route after an exact independent-task arrival and occurrence.</summary>
public sealed class CharacterIndependentTaskReturnTravelPlan
{
    /// <summary>Gets the maximum number of stable waypoints in one return plan.</summary>
    public const int MaximumWaypointCount = CharacterIndependentTaskTravelPlan.MaximumWaypointCount;

    /// <summary>Creates one immutable validated return plan in supplied waypoint order.</summary>
    public CharacterIndependentTaskReturnTravelPlan(
        CharacterIndependentTaskArrivalOccurrenceResult arrivalOccurrenceResult,
        CharacterIndependentTaskActivity completedActivity,
        NavigationWorldBounds worldBounds,
        Vector2 origin,
        IEnumerable<Vector2> waypoints)
    {
        ArgumentNullException.ThrowIfNull(arrivalOccurrenceResult);
        ArgumentNullException.ThrowIfNull(completedActivity);
        ArgumentNullException.ThrowIfNull(waypoints);

        CharacterIndependentTaskTravelProgress outboundProgress =
            arrivalOccurrenceResult.TravelProgress;
        CharacterIndependentTaskTravelPlan outboundPlan = outboundProgress.Plan;
        CharacterIndependentTaskWorldEventOccurrenceResult taskResult =
            arrivalOccurrenceResult.TaskOccurrenceResult;

        if (!outboundProgress.IsComplete
            || !ReferenceEquals(taskResult.Candidate.Activity, outboundPlan.Activity))
        {
            throw new ArgumentException(
                "Return travel requires an exact completed outbound arrival.",
                nameof(arrivalOccurrenceResult));
        }

        if (completedActivity.State != CharacterIndependentTaskActivityState.Completed
            || !taskResult.Activities.Activities.Any(activity =>
                ReferenceEquals(activity, completedActivity))
            || !ReferenceEquals(completedActivity.Assignment, outboundPlan.Activity.Assignment))
        {
            throw new ArgumentException(
                "Return travel requires the exact completed task from the arrival occurrence.",
                nameof(completedActivity));
        }

        if (origin != outboundPlan.Waypoints[^1])
        {
            throw new ArgumentException(
                "Return travel must begin at the outbound plan's final waypoint.",
                nameof(origin));
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
                    $"An independent-task return plan accepts at most {MaximumWaypointCount} waypoints.");
            }
        }

        if (retainedWaypoints.Count == 0)
        {
            throw new ArgumentException(
                "An independent-task return plan requires at least one waypoint.",
                nameof(waypoints));
        }

        _ = NavigationRequestValidator.Create(worldBounds, origin, origin);
        foreach (Vector2 waypoint in retainedWaypoints)
        {
            _ = NavigationRequestValidator.Create(worldBounds, origin, waypoint);
        }

        if (retainedWaypoints[^1] != outboundPlan.Origin)
        {
            throw new ArgumentException(
                "Return travel must end at the outbound plan's origin.",
                nameof(waypoints));
        }

        ArrivalOccurrenceResult = arrivalOccurrenceResult;
        CompletedActivity = completedActivity;
        WorldBounds = worldBounds;
        Origin = origin;
        Waypoints = retainedWaypoints.AsReadOnly();
    }

    /// <summary>Gets the exact successful arrival-occurrence result authorizing the return.</summary>
    public CharacterIndependentTaskArrivalOccurrenceResult ArrivalOccurrenceResult { get; }

    /// <summary>Gets the exact completed independent-task activity returning from its task.</summary>
    public CharacterIndependentTaskActivity CompletedActivity { get; }

    /// <summary>Gets the validated finite world bounds containing the return route.</summary>
    public NavigationWorldBounds WorldBounds { get; }

    /// <summary>Gets the validated finite return-route origin.</summary>
    public Vector2 Origin { get; }

    /// <summary>Gets a stable copied read-only sequence of validated return waypoints.</summary>
    public IReadOnlyList<Vector2> Waypoints { get; }
}
