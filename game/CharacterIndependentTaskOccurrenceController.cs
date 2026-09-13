using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Coordinates one atomic independent-task occurrence across scene-owned state.</summary>
public partial class CharacterIndependentTaskOccurrenceController : Node
{
    private CharacterIndependentTaskDepartureController taskController = null!;
    private CharacterIndependentTaskTravelController travelController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private WorldEventController worldController = null!;

    /// <summary>Gets the exact most recently accepted independent-task occurrence.</summary>
    public CharacterIndependentTaskWorldEventOccurrence? LastOccurrence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        taskController = GetNode<CharacterIndependentTaskDepartureController>(
            "../CharacterIndependentTaskDepartureController");
        travelController = GetNode<CharacterIndependentTaskTravelController>(
            "../CharacterIndependentTaskTravelController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        worldController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <summary>Applies one exact candidate only from all exact expected current snapshots.</summary>
    public bool TryOccur(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskTravelPlan expectedTravelPlan,
        CharacterIndependentTaskTravelProgress expectedTravelProgress,
        CharacterIndependentTaskActivitySnapshot expectedActivities,
        WorldEventLogSnapshot expectedEventLog,
        WorldTimeSnapshot expectedWorldTime,
        CharacterTurnTranscriptSnapshot expectedTranscript)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(expectedTravelPlan);
        ArgumentNullException.ThrowIfNull(expectedTravelProgress);
        ArgumentNullException.ThrowIfNull(expectedActivities);
        ArgumentNullException.ThrowIfNull(expectedEventLog);
        ArgumentNullException.ThrowIfNull(expectedWorldTime);
        ArgumentNullException.ThrowIfNull(expectedTranscript);

        CharacterIndependentTaskActivity? activeActivity = taskController.ActiveActivity;
        WorldEventSnapshot? worldEvents = worldController.WorldEvents;
        if (activeActivity is null
            || worldEvents is null
            || !ReferenceEquals(candidate.Activity, activeActivity)
            || !ReferenceEquals(travelController.Plan, expectedTravelPlan)
            || !ReferenceEquals(travelController.Progress, expectedTravelProgress)
            || !ReferenceEquals(taskController.Activities, expectedActivities)
            || !ReferenceEquals(worldController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldController.WorldTime, expectedWorldTime)
            || !ReferenceEquals(transcriptController.Transcript, expectedTranscript))
        {
            return false;
        }

        if (!CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
                expectedTravelPlan,
                expectedTravelProgress,
                activeActivity))
        {
            return false;
        }

        CharacterIndependentTaskWorldEventOccurrenceResult result;
        try
        {
            result = CharacterIndependentTaskWorldEventOccurrenceTransition.Occur(
                candidate,
                expectedActivities,
                worldEvents,
                expectedEventLog,
                expectedWorldTime,
                expectedTranscript);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        if (!taskController.TryInstallCompletion(
                activeActivity,
                expectedActivities,
                result))
        {
            return false;
        }

        worldController.InstallCharacterTurnOccurrence(
            result.WorldEventResult.WorldEventResult);
        transcriptController.InstallCharacterTurnOccurrence(
            result.WorldEventResult.Transcript);
        LastOccurrence = result.Candidate;
        return true;
    }

    /// <summary>Applies one exact final arrival and occurrence as one scene operation.</summary>
    public bool TryArriveAndOccur(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskTravelPlan expectedTravelPlan,
        CharacterIndependentTaskTravelProgress expectedTravelProgress,
        Vector2 reachedWaypoint,
        CharacterIndependentTaskActivitySnapshot expectedActivities,
        WorldEventLogSnapshot expectedEventLog,
        WorldTimeSnapshot expectedWorldTime,
        CharacterTurnTranscriptSnapshot expectedTranscript)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(expectedTravelPlan);
        ArgumentNullException.ThrowIfNull(expectedTravelProgress);
        ArgumentNullException.ThrowIfNull(expectedActivities);
        ArgumentNullException.ThrowIfNull(expectedEventLog);
        ArgumentNullException.ThrowIfNull(expectedWorldTime);
        ArgumentNullException.ThrowIfNull(expectedTranscript);

        CharacterIndependentTaskActivity? activeActivity = taskController.ActiveActivity;
        WorldEventSnapshot? worldEvents = worldController.WorldEvents;
        if (activeActivity is null
            || worldEvents is null
            || !ReferenceEquals(candidate.Activity, activeActivity)
            || !ReferenceEquals(travelController.Plan, expectedTravelPlan)
            || !ReferenceEquals(travelController.Progress, expectedTravelProgress)
            || !ReferenceEquals(taskController.Activities, expectedActivities)
            || !ReferenceEquals(worldController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldController.WorldTime, expectedWorldTime)
            || !ReferenceEquals(transcriptController.Transcript, expectedTranscript))
        {
            return false;
        }

        CharacterIndependentTaskArrivalOccurrenceResult result;
        try
        {
            result = CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                expectedTravelProgress,
                expectedTravelPlan,
                reachedWaypoint,
                candidate,
                expectedActivities,
                worldEvents,
                expectedEventLog,
                expectedWorldTime,
                expectedTranscript);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        if (!travelController.TryInstallArrival(
                expectedTravelPlan,
                expectedTravelProgress,
                result)
            || !taskController.TryInstallCompletion(
                activeActivity,
                expectedActivities,
                result.TaskOccurrenceResult))
        {
            return false;
        }

        worldController.InstallCharacterTurnOccurrence(
            result.TaskOccurrenceResult.WorldEventResult.WorldEventResult);
        transcriptController.InstallCharacterTurnOccurrence(
            result.TaskOccurrenceResult.WorldEventResult.Transcript);
        LastOccurrence = result.TaskOccurrenceResult.Candidate;
        return true;
    }
}
