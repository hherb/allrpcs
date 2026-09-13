using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks fail-closed composition of elapsed timing with one task round-trip step.</summary>
public partial class CharacterIndependentTaskTimedRoundTripCoordinatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_TIMED_ROUND_TRIP_COORDINATOR_CHECK";
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
        CharacterIndependentTaskStepClockController clockController =
            GetNode<CharacterIndependentTaskStepClockController>(
                "../CharacterIndependentTaskStepClockController");
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
        WorldEventController worldController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");

        CharacterIndependentTaskStepCadence cadence = new(TimeSpan.FromSeconds(3));
        if (!clockController.TryInitialize(cadence)
            || clockController.Clock is not { } initialClock)
        {
            Fail("The timed round-trip check could not initialize its exact cadence.");
            return;
        }

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

        if (!coordinator.TryAdvanceElapsed(
                cadence,
                initialClock,
                TimeSpan.FromSeconds(2),
                outboundPlan,
                outboundProgress,
                null,
                null,
                outboundPlan.Waypoints.Single(),
                candidate,
                activeActivities,
                separatedParty,
                eventLog,
                worldTime,
                transcript)
            || clockController.Clock is not { } accumulatedClock
            || clockController.LastAdvanceResult is not { IsStepDue: false }
            || accumulatedClock.ElapsedTowardNextStep != TimeSpan.FromSeconds(2)
            || !HasOutboundState(
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
            Fail("Sub-interval timing changed independent-task route or occurrence state.");
            return;
        }

        CharacterIndependentTaskActivitySnapshot staleActivities =
            new(activeActivities.Activities);
        CharacterIndependentTaskStepClockAdvanceResult accumulatedResult =
            clockController.LastAdvanceResult!;
        if (coordinator.TryAdvanceElapsed(
                cadence,
                accumulatedClock,
                TimeSpan.FromSeconds(2),
                outboundPlan,
                outboundProgress,
                null,
                null,
                outboundPlan.Waypoints.Single(),
                candidate,
                staleActivities,
                separatedParty,
                eventLog,
                worldTime,
                transcript)
            || coordinator.TryAdvanceElapsed(
                cadence,
                accumulatedClock,
                cadence.Interval + TimeSpan.FromTicks(1),
                outboundPlan,
                outboundProgress,
                null,
                null,
                outboundPlan.Waypoints.Single(),
                candidate,
                activeActivities,
                separatedParty,
                eventLog,
                worldTime,
                transcript)
            || !ReferenceEquals(clockController.Clock, accumulatedClock)
            || !ReferenceEquals(clockController.LastAdvanceResult, accumulatedResult)
            || !HasOutboundState(
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
            Fail("Rejected due-step input consumed timing or changed outbound scene state.");
            return;
        }

        if (!coordinator.TryAdvanceElapsed(
                cadence,
                accumulatedClock,
                TimeSpan.FromSeconds(2),
                outboundPlan,
                outboundProgress,
                null,
                null,
                outboundPlan.Waypoints.Single(),
                candidate,
                activeActivities,
                separatedParty,
                eventLog,
                worldTime,
                transcript)
            || clockController.Clock is not { } outboundDueClock
            || clockController.LastAdvanceResult is not { IsStepDue: true } outboundDueResult
            || !ReferenceEquals(outboundDueResult.PreviousClock, accumulatedClock)
            || outboundDueClock.ElapsedTowardNextStep != TimeSpan.FromSeconds(1)
            || occurrenceController.LastArrivalOccurrenceResult is not { }
            || outboundController.Progress is not { IsComplete: true } completedOutboundProgress
            || taskController.Activities is not { } completedActivities
            || taskController.Party is not { } completedParty
            || worldController.EventLog is not { } completedEventLog
            || worldController.WorldTime is not { } completedWorldTime
            || transcriptController.Transcript is not { } completedTranscript
            || returnController.Plan is not { } returnPlan
            || returnController.Progress is not { } returnProgress)
        {
            Fail("One due interval did not advance the exact outbound occurrence once.");
            return;
        }

        if (!coordinator.TryAdvanceElapsed(
                cadence,
                outboundDueClock,
                TimeSpan.FromSeconds(2),
                outboundPlan,
                completedOutboundProgress,
                returnPlan,
                returnProgress,
                returnPlan.Waypoints.Single(),
                null,
                completedActivities,
                completedParty,
                completedEventLog,
                completedWorldTime,
                completedTranscript)
            || clockController.Clock is not { } returnDueClock
            || clockController.LastAdvanceResult is not { IsStepDue: true } returnDueResult
            || !ReferenceEquals(returnDueResult.PreviousClock, outboundDueClock)
            || returnDueClock.ElapsedTowardNextStep != TimeSpan.Zero
            || returnController.Progress is not { IsComplete: true } completedReturnProgress
            || taskController.Party is not { } rejoinedParty)
        {
            Fail("The next due interval did not advance the exact return rejoin once.");
            return;
        }

        CharacterIndependentTaskStepClockAdvanceResult finalClockResult =
            clockController.LastAdvanceResult!;
        if (coordinator.TryAdvanceElapsed(
                cadence,
                returnDueClock,
                cadence.Interval,
                outboundPlan,
                completedOutboundProgress,
                returnPlan,
                completedReturnProgress,
                returnPlan.Waypoints.Single(),
                null,
                completedActivities,
                rejoinedParty,
                completedEventLog,
                completedWorldTime,
                completedTranscript)
            || !ReferenceEquals(clockController.Clock, returnDueClock)
            || !ReferenceEquals(clockController.LastAdvanceResult, finalClockResult)
            || !ReferenceEquals(returnController.Progress, completedReturnProgress)
            || !ReferenceEquals(taskController.Party, rejoinedParty))
        {
            Fail("A repeated completed round trip consumed another due interval.");
            return;
        }

        GD.Print(
            "Timed independent-task round-trip coordinator smoke passed: sub-interval time "
            + "changed only the clock, each due interval advanced one exact active leg, and "
            + "rejected or repeated route input left the due clock unconsumed.");
        GetTree().Quit();
    }

    private static bool HasOutboundState(
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
        && occurrenceController.LastArrivalOccurrenceResult is null
        && ReferenceEquals(worldController.EventLog, eventLog)
        && ReferenceEquals(worldController.WorldTime, worldTime)
        && ReferenceEquals(transcriptController.Transcript, transcript);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
