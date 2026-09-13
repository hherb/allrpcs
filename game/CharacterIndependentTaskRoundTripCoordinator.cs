using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Delegates one explicit outbound or return waypoint through existing scene rules.</summary>
public partial class CharacterIndependentTaskRoundTripCoordinator : Node
{
    private CharacterIndependentTaskOccurrenceController occurrenceController = null!;
    private CharacterIndependentTaskReturnTravelController returnController = null!;
    private CharacterIndependentTaskTravelController outboundController = null!;
    private CharacterIndependentTaskDepartureController taskController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private WorldEventController worldController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        occurrenceController = GetNode<CharacterIndependentTaskOccurrenceController>(
            "../CharacterIndependentTaskOccurrenceController");
        returnController = GetNode<CharacterIndependentTaskReturnTravelController>(
            "../CharacterIndependentTaskReturnTravelController");
        outboundController = GetNode<CharacterIndependentTaskTravelController>(
            "../CharacterIndependentTaskTravelController");
        taskController = GetNode<CharacterIndependentTaskDepartureController>(
            "../CharacterIndependentTaskDepartureController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        worldController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <summary>
    /// Advances one exact outbound waypoint, delegating a final arrival to the atomic occurrence.
    /// </summary>
    public bool TryAdvanceOutbound(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskTravelPlan expectedPlan,
        CharacterIndependentTaskTravelProgress expectedProgress,
        Vector2 reachedWaypoint,
        CharacterIndependentTaskActivitySnapshot expectedActivities,
        WorldEventLogSnapshot expectedEventLog,
        WorldTimeSnapshot expectedWorldTime,
        CharacterTurnTranscriptSnapshot expectedTranscript)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(expectedPlan);
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(expectedActivities);
        ArgumentNullException.ThrowIfNull(expectedEventLog);
        ArgumentNullException.ThrowIfNull(expectedWorldTime);
        ArgumentNullException.ThrowIfNull(expectedTranscript);

        if (!ReferenceEquals(candidate.Activity, taskController.ActiveActivity)
            || !ReferenceEquals(outboundController.Plan, expectedPlan)
            || !ReferenceEquals(outboundController.Progress, expectedProgress)
            || !ReferenceEquals(taskController.Activities, expectedActivities)
            || !ReferenceEquals(worldController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldController.WorldTime, expectedWorldTime)
            || !ReferenceEquals(transcriptController.Transcript, expectedTranscript))
        {
            return false;
        }

        if (IsFinalOutboundStep(expectedPlan, expectedProgress))
        {
            return occurrenceController.TryArriveAndOccur(
                candidate,
                expectedPlan,
                expectedProgress,
                reachedWaypoint,
                expectedActivities,
                expectedEventLog,
                expectedWorldTime,
                expectedTranscript);
        }

        return outboundController.TryAdvance(
            expectedPlan,
            expectedProgress,
            reachedWaypoint);
    }

    /// <summary>
    /// Advances one exact return waypoint, delegating a final arrival to the atomic party rejoin.
    /// </summary>
    public bool TryAdvanceReturn(
        CharacterIndependentTaskReturnTravelPlan expectedPlan,
        CharacterIndependentTaskReturnTravelProgress expectedProgress,
        Vector2 reachedWaypoint,
        CharacterPartySnapshot expectedParty)
    {
        ArgumentNullException.ThrowIfNull(expectedPlan);
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(expectedParty);

        if (!ReferenceEquals(returnController.Plan, expectedPlan)
            || !ReferenceEquals(returnController.Progress, expectedProgress)
            || !ReferenceEquals(taskController.Party, expectedParty))
        {
            return false;
        }

        if (IsFinalReturnStep(expectedPlan, expectedProgress))
        {
            return returnController.TryArriveAndRejoin(
                expectedPlan,
                expectedProgress,
                reachedWaypoint,
                expectedParty);
        }

        return returnController.TryAdvance(
            expectedPlan,
            expectedProgress,
            reachedWaypoint);
    }

    /// <summary>
    /// Selects and advances the only active leg from one exact caller-supplied scene snapshot.
    /// </summary>
    public bool TryAdvanceNext(
        CharacterIndependentTaskTravelPlan expectedOutboundPlan,
        CharacterIndependentTaskTravelProgress expectedOutboundProgress,
        CharacterIndependentTaskReturnTravelPlan? expectedReturnPlan,
        CharacterIndependentTaskReturnTravelProgress? expectedReturnProgress,
        Vector2 reachedWaypoint,
        CharacterIndependentTaskWorldEventOccurrence? candidate,
        CharacterIndependentTaskActivitySnapshot expectedActivities,
        CharacterPartySnapshot expectedParty,
        WorldEventLogSnapshot expectedEventLog,
        WorldTimeSnapshot expectedWorldTime,
        CharacterTurnTranscriptSnapshot expectedTranscript)
    {
        ArgumentNullException.ThrowIfNull(expectedOutboundPlan);
        ArgumentNullException.ThrowIfNull(expectedOutboundProgress);
        ArgumentNullException.ThrowIfNull(expectedActivities);
        ArgumentNullException.ThrowIfNull(expectedParty);
        ArgumentNullException.ThrowIfNull(expectedEventLog);
        ArgumentNullException.ThrowIfNull(expectedWorldTime);
        ArgumentNullException.ThrowIfNull(expectedTranscript);

        if (!ReferenceEquals(outboundController.Plan, expectedOutboundPlan)
            || !ReferenceEquals(outboundController.Progress, expectedOutboundProgress)
            || !ReferenceEquals(taskController.Activities, expectedActivities)
            || !ReferenceEquals(taskController.Party, expectedParty)
            || !ReferenceEquals(worldController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldController.WorldTime, expectedWorldTime)
            || !ReferenceEquals(transcriptController.Transcript, expectedTranscript))
        {
            return false;
        }

        if (returnController.Plan is null && returnController.Progress is null)
        {
            if (expectedReturnPlan is not null
                || expectedReturnProgress is not null
                || candidate is null)
            {
                return false;
            }

            return TryAdvanceOutbound(
                candidate,
                expectedOutboundPlan,
                expectedOutboundProgress,
                reachedWaypoint,
                expectedActivities,
                expectedEventLog,
                expectedWorldTime,
                expectedTranscript);
        }

        if (returnController.Plan is not { } returnPlan
            || returnController.Progress is not { } returnProgress
            || !ReferenceEquals(returnPlan, expectedReturnPlan)
            || !ReferenceEquals(returnProgress, expectedReturnProgress)
            || candidate is not null)
        {
            return false;
        }

        return TryAdvanceReturn(
            returnPlan,
            returnProgress,
            reachedWaypoint,
            expectedParty);
    }

    private static bool IsFinalOutboundStep(
        CharacterIndependentTaskTravelPlan plan,
        CharacterIndependentTaskTravelProgress progress) =>
        progress.CompletedWaypointCount == plan.Waypoints.Count - 1;

    private static bool IsFinalReturnStep(
        CharacterIndependentTaskReturnTravelPlan plan,
        CharacterIndependentTaskReturnTravelProgress progress) =>
        progress.CompletedWaypointCount == plan.Waypoints.Count - 1;
}
