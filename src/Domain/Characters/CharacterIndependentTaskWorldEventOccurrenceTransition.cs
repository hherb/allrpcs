using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Atomically records one eligible task occurrence and completes its exact activity.</summary>
public static class CharacterIndependentTaskWorldEventOccurrenceTransition
{
    /// <summary>Returns fresh task, event-log, time, and transcript snapshots.</summary>
    public static CharacterIndependentTaskWorldEventOccurrenceResult Occur(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(transcript);

        if (!activities.Activities.Any(activity =>
                ReferenceEquals(activity, candidate.Activity))
            || !CharacterIndependentTaskWorldEventOccurrencePolicy.CanOccur(
                candidate,
                worldEvents,
                eventLog,
                worldTime))
        {
            throw new InvalidOperationException(
                "Only an eligible occurrence from the exact active task can be applied.");
        }

        CharacterIndependentTaskActivitySnapshot resultingActivities =
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                activities,
                candidate.Activity,
                CharacterIndependentTaskActivityState.Completed);
        CharacterTurnWorldEventOccurrenceResult worldEventResult =
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                candidate.Occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript);

        return new CharacterIndependentTaskWorldEventOccurrenceResult(
            candidate,
            resultingActivities,
            worldEventResult);
    }
}
