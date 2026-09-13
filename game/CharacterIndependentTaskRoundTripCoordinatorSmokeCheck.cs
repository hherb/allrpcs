using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit authored independent-task round trip through existing scene rules.</summary>
public partial class CharacterIndependentTaskRoundTripCoordinatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_ROUND_TRIP_COORDINATOR_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        CharacterIndependentTaskRoundTripCoordinator coordinator =
            GetNode<CharacterIndependentTaskRoundTripCoordinator>(
                "../CharacterIndependentTaskRoundTripCoordinator");
        CharacterIndependentTaskDepartureController taskController =
            GetNode<CharacterIndependentTaskDepartureController>(
                "../CharacterIndependentTaskDepartureController");
        CharacterIndependentTaskOccurrenceController occurrenceController =
            GetNode<CharacterIndependentTaskOccurrenceController>(
                "../CharacterIndependentTaskOccurrenceController");
        CharacterIndependentTaskTravelController outboundController =
            GetNode<CharacterIndependentTaskTravelController>(
                "../CharacterIndependentTaskTravelController");
        CharacterIndependentTaskReturnTravelController returnController =
            GetNode<CharacterIndependentTaskReturnTravelController>(
                "../CharacterIndependentTaskReturnTravelController");
        TemporaryCharacterIndependentTaskOccurrenceSource occurrenceSource =
            GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>(
                "../TemporaryCharacterIndependentTaskOccurrenceSource");
        TemporaryCharacterIndependentTaskTravelSource outboundSource =
            GetNode<TemporaryCharacterIndependentTaskTravelSource>(
                "../TemporaryCharacterIndependentTaskTravelSource");
        TemporaryCharacterIndependentTaskReturnTravelSource returnSource =
            GetNode<TemporaryCharacterIndependentTaskReturnTravelSource>(
                "../TemporaryCharacterIndependentTaskReturnTravelSource");
        WorldEventController worldController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");

        CharacterIndependentTaskActivity active = taskController.ActiveActivity!;
        CharacterIndependentTaskWorldEventOccurrence candidate =
            occurrenceSource.CreateOccurrence(active);
        CharacterIndependentTaskTravelPlan outboundPlan = outboundController.Plan!;
        CharacterIndependentTaskTravelProgress outboundProgress = outboundController.Progress!;
        CharacterIndependentTaskActivitySnapshot activeActivities = taskController.Activities!;
        CharacterPartySnapshot separatedParty = taskController.Party!;
        WorldEventLogSnapshot eventLog = worldController.EventLog!;
        WorldTimeSnapshot worldTime = worldController.WorldTime!;
        CharacterTurnTranscriptSnapshot transcript = transcriptController.Transcript!;

        CharacterIndependentTaskTravelPlan distinctOutboundPlan =
            outboundSource.CreatePlan(active, outboundPlan.Origin);
        CharacterIndependentTaskTravelProgress distinctOutboundProgress = new(outboundPlan);
        CharacterIndependentTaskActivitySnapshot staleActivities =
            new(activeActivities.Activities);
        if (coordinator.TryAdvanceOutbound(
                candidate,
                distinctOutboundPlan,
                outboundProgress,
                distinctOutboundPlan.Waypoints.Single(),
                activeActivities,
                eventLog,
                worldTime,
                transcript)
            || coordinator.TryAdvanceOutbound(
                candidate,
                outboundPlan,
                distinctOutboundProgress,
                outboundPlan.Waypoints.Single(),
                activeActivities,
                eventLog,
                worldTime,
                transcript)
            || coordinator.TryAdvanceOutbound(
                candidate,
                outboundPlan,
                outboundProgress,
                outboundPlan.Waypoints.Single(),
                staleActivities,
                eventLog,
                worldTime,
                transcript)
            || !RejectsOutboundNulls(
                coordinator,
                candidate,
                outboundPlan,
                outboundProgress,
                activeActivities,
                eventLog,
                worldTime,
                transcript)
            || !HasInitialState(
                taskController,
                occurrenceController,
                outboundController,
                returnController,
                worldController,
                transcriptController,
                active,
                outboundPlan,
                outboundProgress,
                activeActivities,
                separatedParty,
                eventLog,
                worldTime,
                transcript))
        {
            Fail("Stale, mismatched, or null outbound input changed round-trip scene state.");
            return;
        }

        if (!coordinator.TryAdvanceOutbound(
                candidate,
                outboundPlan,
                outboundProgress,
                outboundPlan.Waypoints.Single(),
                activeActivities,
                eventLog,
                worldTime,
                transcript)
            || outboundController.Progress is not { IsComplete: true } completedOutboundProgress
            || occurrenceController.LastArrivalOccurrenceResult is not { } arrival
            || taskController.Activities is not { } completedActivities
            || completedActivities.Activities.Single() is not { } completedActivity
            || taskController.ActiveActivity is not null
            || !ReferenceEquals(taskController.Party, separatedParty)
            || worldController.EventLog is not { } completedEventLog
            || worldController.WorldTime is not { } completedWorldTime
            || transcriptController.Transcript is not { } completedTranscript
            || returnController.Plan is not { } returnPlan
            || returnController.Progress is not { } returnProgress
            || !ReferenceEquals(arrival.TravelProgress, completedOutboundProgress)
            || !ReferenceEquals(returnPlan.ArrivalOccurrenceResult, arrival)
            || !ReferenceEquals(returnPlan.CompletedActivity, completedActivity))
        {
            Fail("The coordinator did not complete the outbound arrival and task occurrence in order.");
            return;
        }

        CharacterIndependentTaskReturnTravelPlan distinctReturnPlan =
            returnSource.CreatePlan(arrival, completedActivity);
        CharacterIndependentTaskReturnTravelProgress distinctReturnProgress = new(returnPlan);
        CharacterPartySnapshot staleParty = new(separatedParty.MemberCharacterIds);
        if (coordinator.TryAdvanceReturn(
                distinctReturnPlan,
                returnProgress,
                distinctReturnPlan.Waypoints.Single(),
                separatedParty)
            || coordinator.TryAdvanceReturn(
                returnPlan,
                distinctReturnProgress,
                returnPlan.Waypoints.Single(),
                separatedParty)
            || coordinator.TryAdvanceReturn(
                returnPlan,
                returnProgress,
                returnPlan.Waypoints.Single(),
                staleParty)
            || !RejectsReturnNulls(
                coordinator,
                returnPlan,
                returnProgress,
                separatedParty)
            || !HasCompletedTaskState(
                taskController,
                outboundController,
                returnController,
                worldController,
                transcriptController,
                completedOutboundProgress,
                completedActivities,
                separatedParty,
                completedEventLog,
                completedWorldTime,
                completedTranscript,
                returnPlan,
                returnProgress))
        {
            Fail("Stale, mismatched, or null return input changed round-trip scene state.");
            return;
        }

        if (!coordinator.TryAdvanceReturn(
                returnPlan,
                returnProgress,
                returnPlan.Waypoints.Single(),
                separatedParty)
            || returnController.Progress is not { IsComplete: true } completedReturnProgress
            || taskController.Party is not { } rejoinedParty
            || !ReferenceEquals(completedReturnProgress.Plan, returnPlan)
            || !ReferenceEquals(
                rejoinedParty.MemberCharacterIds[^1],
                completedActivity.Assignment.CharacterId)
            || coordinator.TryAdvanceReturn(
                returnPlan,
                completedReturnProgress,
                returnPlan.Waypoints.Single(),
                rejoinedParty)
            || !ReferenceEquals(returnController.Progress, completedReturnProgress)
            || !ReferenceEquals(taskController.Party, rejoinedParty)
            || !ReferenceEquals(taskController.Activities, completedActivities)
            || !ReferenceEquals(worldController.EventLog, completedEventLog)
            || !ReferenceEquals(worldController.WorldTime, completedWorldTime)
            || !ReferenceEquals(transcriptController.Transcript, completedTranscript))
        {
            Fail("The coordinator did not complete the exact return and party rejoin once.");
            return;
        }

        GD.Print(
            "Independent-task round-trip coordinator smoke passed: the exact authored outbound "
            + "waypoint, task occurrence, return waypoint, and party rejoin progressed in order, "
            + "while stale, mismatched, repeated, and null input preserved all scene snapshots.");
        GetTree().Quit();
    }

    private static bool HasInitialState(
        CharacterIndependentTaskDepartureController taskController,
        CharacterIndependentTaskOccurrenceController occurrenceController,
        CharacterIndependentTaskTravelController outboundController,
        CharacterIndependentTaskReturnTravelController returnController,
        WorldEventController worldController,
        CharacterTurnTranscriptController transcriptController,
        CharacterIndependentTaskActivity active,
        CharacterIndependentTaskTravelPlan outboundPlan,
        CharacterIndependentTaskTravelProgress outboundProgress,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        ReferenceEquals(taskController.ActiveActivity, active)
        && ReferenceEquals(taskController.Activities, activities)
        && ReferenceEquals(taskController.Party, party)
        && ReferenceEquals(outboundController.Plan, outboundPlan)
        && ReferenceEquals(outboundController.Progress, outboundProgress)
        && returnController.Plan is null
        && returnController.Progress is null
        && occurrenceController.LastOccurrence is null
        && occurrenceController.LastArrivalOccurrenceResult is null
        && ReferenceEquals(worldController.EventLog, eventLog)
        && ReferenceEquals(worldController.WorldTime, worldTime)
        && ReferenceEquals(transcriptController.Transcript, transcript);

    private static bool HasCompletedTaskState(
        CharacterIndependentTaskDepartureController taskController,
        CharacterIndependentTaskTravelController outboundController,
        CharacterIndependentTaskReturnTravelController returnController,
        WorldEventController worldController,
        CharacterTurnTranscriptController transcriptController,
        CharacterIndependentTaskTravelProgress outboundProgress,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript,
        CharacterIndependentTaskReturnTravelPlan returnPlan,
        CharacterIndependentTaskReturnTravelProgress returnProgress) =>
        taskController.ActiveActivity is null
        && ReferenceEquals(taskController.Activities, activities)
        && ReferenceEquals(taskController.Party, party)
        && ReferenceEquals(outboundController.Progress, outboundProgress)
        && ReferenceEquals(returnController.Plan, returnPlan)
        && ReferenceEquals(returnController.Progress, returnProgress)
        && ReferenceEquals(worldController.EventLog, eventLog)
        && ReferenceEquals(worldController.WorldTime, worldTime)
        && ReferenceEquals(transcriptController.Transcript, transcript);

    private static bool RejectsOutboundNulls(
        CharacterIndependentTaskRoundTripCoordinator coordinator,
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskTravelPlan plan,
        CharacterIndependentTaskTravelProgress progress,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        ThrowsArgumentNull(() => coordinator.TryAdvanceOutbound(
            null!, plan, progress, plan.Waypoints.Single(), activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceOutbound(
            candidate, null!, progress, plan.Waypoints.Single(), activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceOutbound(
            candidate, plan, null!, plan.Waypoints.Single(), activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceOutbound(
            candidate, plan, progress, plan.Waypoints.Single(), null!, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceOutbound(
            candidate, plan, progress, plan.Waypoints.Single(), activities, null!, worldTime, transcript))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceOutbound(
            candidate, plan, progress, plan.Waypoints.Single(), activities, eventLog, null!, transcript))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceOutbound(
            candidate, plan, progress, plan.Waypoints.Single(), activities, eventLog, worldTime, null!));

    private static bool RejectsReturnNulls(
        CharacterIndependentTaskRoundTripCoordinator coordinator,
        CharacterIndependentTaskReturnTravelPlan plan,
        CharacterIndependentTaskReturnTravelProgress progress,
        CharacterPartySnapshot party) =>
        ThrowsArgumentNull(() => coordinator.TryAdvanceReturn(
            null!, progress, plan.Waypoints.Single(), party))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceReturn(
            plan, null!, plan.Waypoints.Single(), party))
        && ThrowsArgumentNull(() => coordinator.TryAdvanceReturn(
            plan, progress, plan.Waypoints.Single(), null!));

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
