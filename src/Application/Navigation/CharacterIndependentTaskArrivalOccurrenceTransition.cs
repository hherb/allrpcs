using System.Numerics;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Navigation;

/// <summary>Combines an exact final travel advance with one authoritative task occurrence.</summary>
public static class CharacterIndependentTaskArrivalOccurrenceTransition
{
    /// <summary>Returns fresh completed travel and authoritative occurrence state atomically.</summary>
    public static CharacterIndependentTaskArrivalOccurrenceResult ArriveAndOccur(
        CharacterIndependentTaskTravelProgress progress,
        CharacterIndependentTaskTravelPlan plan,
        Vector2 reachedWaypoint,
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(transcript);

        CharacterIndependentTaskTravelProgress completedProgress =
            CharacterIndependentTaskTravelProgressTransition.Advance(
                progress,
                plan,
                reachedWaypoint);
        if (!completedProgress.IsComplete
            || !CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
                plan,
                completedProgress,
                candidate.Activity))
        {
            throw new InvalidOperationException(
                "A task occurrence requires the exact final waypoint of its exact active task.");
        }

        CharacterIndependentTaskWorldEventOccurrenceResult occurrenceResult =
            CharacterIndependentTaskWorldEventOccurrenceTransition.Occur(
                candidate,
                activities,
                worldEvents,
                eventLog,
                worldTime,
                transcript);
        return new CharacterIndependentTaskArrivalOccurrenceResult(
            completedProgress,
            occurrenceResult);
    }
}
