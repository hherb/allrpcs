using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Contains one completed travel advance and its exact authoritative task occurrence.</summary>
public sealed record CharacterIndependentTaskArrivalOccurrenceResult
{
    /// <summary>Creates one immutable combined arrival and occurrence result.</summary>
    public CharacterIndependentTaskArrivalOccurrenceResult(
        CharacterIndependentTaskTravelProgress travelProgress,
        CharacterIndependentTaskWorldEventOccurrenceResult taskOccurrenceResult)
    {
        ArgumentNullException.ThrowIfNull(travelProgress);
        ArgumentNullException.ThrowIfNull(taskOccurrenceResult);

        TravelProgress = travelProgress;
        TaskOccurrenceResult = taskOccurrenceResult;
    }

    /// <summary>Gets the fresh progress completed by the exact final waypoint.</summary>
    public CharacterIndependentTaskTravelProgress TravelProgress { get; }

    /// <summary>Gets the fresh task, event-log, world-time, and transcript result.</summary>
    public CharacterIndependentTaskWorldEventOccurrenceResult TaskOccurrenceResult { get; }
}
