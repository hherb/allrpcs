using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the bounded live-scene cadence through one authored task round trip.</summary>
public partial class IndependentTaskLiveRoundTripFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_LIVE_ROUND_TRIP_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private static readonly TimeSpan ExpectedCadence = TimeSpan.FromSeconds(3);

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

        Main main = GetNode<Main>("..");
        Healer healer = GetNode<Healer>("../Healer");
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
        WorldEventController worldController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");

        CharacterIndependentTaskActivity active = taskController.ActiveActivity!;
        CharacterIndependentTaskTravelPlan outboundPlan = outboundController.Plan!;
        CharacterIndependentTaskTravelProgress initialOutboundProgress = outboundController.Progress!;
        CharacterIndependentTaskActivitySnapshot activeActivities = taskController.Activities!;
        CharacterPartySnapshot separatedParty = taskController.Party!;
        WorldEventLogSnapshot initialEventLog = worldController.EventLog!;
        WorldTimeSnapshot initialWorldTime = worldController.WorldTime!;
        CharacterTurnTranscriptSnapshot initialTranscript = transcriptController.Transcript!;
        Vector2 healerPosition = healer.GlobalPosition;
        bool healerVisibility = healer.Visible;
        if (clockController.Cadence is not { } cadence
            || cadence.Interval != ExpectedCadence
            || clockController.Clock is not { } initialClock
            || initialClock.ElapsedTowardNextStep >= cadence.Interval
            || initialOutboundProgress.IsComplete
            || returnController.Plan is not null
            || returnController.Progress is not null)
        {
            Fail("The live scene did not initialize the authored independent-task cadence.");
            return;
        }

        main._Process(cadence.Interval.TotalSeconds);

        if (outboundController.Progress is not { IsComplete: true } completedOutboundProgress
            || ReferenceEquals(completedOutboundProgress, initialOutboundProgress)
            || taskController.ActiveActivity is not null
            || taskController.Activities is not { } completedActivities
            || ReferenceEquals(completedActivities, activeActivities)
            || !ReferenceEquals(taskController.Party, separatedParty)
            || occurrenceController.LastArrivalOccurrenceResult is not { } arrival
            || returnController.Plan is not { } returnPlan
            || returnController.Progress is not { IsComplete: false } initialReturnProgress
            || worldController.EventLog is not { } occurredEventLog
            || ReferenceEquals(occurredEventLog, initialEventLog)
            || worldController.WorldTime is not { } occurredWorldTime
            || ReferenceEquals(occurredWorldTime, initialWorldTime)
            || transcriptController.Transcript is not { } occurredTranscript
            || ReferenceEquals(occurredTranscript, initialTranscript)
            || !ReferenceEquals(arrival.TravelProgress, completedOutboundProgress)
            || healer.GlobalPosition != healerPosition
            || healer.Visible != healerVisibility)
        {
            Fail("One live cadence interval did not complete only the exact outbound occurrence.");
            return;
        }

        main._Process(cadence.Interval.TotalSeconds);

        if (returnController.Progress is not { IsComplete: true } completedReturnProgress
            || ReferenceEquals(completedReturnProgress, initialReturnProgress)
            || !ReferenceEquals(completedReturnProgress.Plan, returnPlan)
            || taskController.Party is not { } rejoinedParty
            || ReferenceEquals(rejoinedParty, separatedParty)
            || !ReferenceEquals(
                rejoinedParty.MemberCharacterIds[^1],
                active.Assignment.CharacterId)
            || !ReferenceEquals(taskController.Activities, completedActivities)
            || !ReferenceEquals(worldController.EventLog, occurredEventLog)
            || !ReferenceEquals(worldController.WorldTime, occurredWorldTime)
            || !ReferenceEquals(transcriptController.Transcript, occurredTranscript)
            || clockController.Clock is not { } completedClock
            || ReferenceEquals(completedClock, initialClock)
            || healer.GlobalPosition != healerPosition
            || healer.Visible != healerVisibility)
        {
            Fail("The next live cadence interval did not complete only the exact return rejoin.");
            return;
        }

        main._Process(cadence.Interval.TotalSeconds);
        if (!ReferenceEquals(returnController.Progress, completedReturnProgress)
            || !ReferenceEquals(taskController.Party, rejoinedParty)
            || !ReferenceEquals(clockController.Clock, completedClock)
            || healer.GlobalPosition != healerPosition
            || healer.Visible != healerVisibility)
        {
            Fail("Completed live round-trip processing changed task, timing, or body state again.");
            return;
        }

        GD.Print(
            "Independent-task live round-trip flow smoke passed: two authored cadence intervals "
            + "completed the exact outbound occurrence and return rejoin once while leaving the "
            + "healer body visible and unmoved.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
