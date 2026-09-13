using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks atomic scene completion of one authored independent task.</summary>
public partial class CharacterIndependentTaskOccurrenceControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_OCCURRENCE_CONTROLLER_CHECK";
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

        CharacterIndependentTaskDepartureController taskController =
            GetNode<CharacterIndependentTaskDepartureController>(
                "../CharacterIndependentTaskDepartureController");
        CharacterIndependentTaskOccurrenceController occurrenceController =
            GetNode<CharacterIndependentTaskOccurrenceController>(
                "../CharacterIndependentTaskOccurrenceController");
        CharacterIndependentTaskTravelController travelController =
            GetNode<CharacterIndependentTaskTravelController>(
                "../CharacterIndependentTaskTravelController");
        CharacterIndependentTaskReturnTravelController returnTravelController =
            GetNode<CharacterIndependentTaskReturnTravelController>(
                "../CharacterIndependentTaskReturnTravelController");
        TemporaryCharacterIndependentTaskOccurrenceSource occurrenceSource =
            GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>(
                "../TemporaryCharacterIndependentTaskOccurrenceSource");
        WorldEventController worldController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");

        CharacterIndependentTaskActivity active = taskController.ActiveActivity!;
        CharacterIndependentTaskTravelPlan travelPlan = travelController.Plan!;
        CharacterIndependentTaskTravelProgress initialTravelProgress = travelController.Progress!;
        CharacterIndependentTaskActivitySnapshot activities = taskController.Activities!;
        CharacterPartySnapshot party = taskController.Party!;
        WorldEventLogSnapshot eventLog = worldController.EventLog!;
        WorldTimeSnapshot worldTime = worldController.WorldTime!;
        CharacterTurnTranscriptSnapshot transcript = transcriptController.Transcript!;
        CharacterIndependentTaskWorldEventOccurrence candidate =
            occurrenceSource.CreateOccurrence(active);

        if (!ReferenceEquals(candidate.Activity, active)
            || !ReferenceEquals(
                candidate.Occurrence.WorldEvent,
                GetNode<TemporaryWorldEventSource>(
                    "../TemporaryWorldEventSource").HerbPatchExaminedWorldEvent)
            || !ReferenceEquals(
                candidate.Occurrence.TranscriptEntry.CharacterId,
                active.Assignment.CharacterId)
            || !ReferenceEquals(
                candidate.Occurrence.TranscriptEntry.PhysicalAction,
                active.Assignment.Intention)
            || !ReferenceEquals(travelPlan.Activity, active)
            || !ReferenceEquals(initialTravelProgress.Plan, travelPlan)
            || initialTravelProgress.IsComplete
            || !ReferenceEquals(taskController.Activities, activities)
            || !ReferenceEquals(worldController.EventLog, eventLog)
            || !ReferenceEquals(worldController.WorldTime, worldTime)
            || !ReferenceEquals(transcriptController.Transcript, transcript)
            || returnTravelController.Plan is not null
            || returnTravelController.Progress is not null
            || !RejectsInvalidSourceInputs(occurrenceSource, active)
            || !HasClosedSourceSurface())
        {
            Fail("The authored task occurrence candidate changed state or lost exact values.");
            return;
        }

        WorldEventLogSnapshot staleEventLog = new(eventLog.Entries);
        if (occurrenceController.TryArriveAndOccur(
                candidate,
                travelPlan,
                initialTravelProgress,
                travelPlan.Waypoints.Single(),
                activities,
                staleEventLog,
                worldTime,
                transcript)
            || !HasExactState(
                taskController,
                travelController,
                worldController,
                transcriptController,
                active,
                travelPlan,
                initialTravelProgress,
                activities,
                party,
                eventLog,
                worldTime,
                transcript))
        {
            Fail("The task occurrence controller changed state after stale input.");
            return;
        }

        if (occurrenceController.TryOccur(
                candidate,
                travelPlan,
                initialTravelProgress,
                activities,
                eventLog,
                worldTime,
                transcript)
            || !HasExactState(
                taskController,
                travelController,
                worldController,
                transcriptController,
                active,
                travelPlan,
                initialTravelProgress,
                activities,
                party,
                eventLog,
                worldTime,
                transcript))
        {
            Fail("The task occurred before the exact authored travel plan completed.");
            return;
        }

        CharacterIndependentTaskTravelPlan mismatchedTravelPlan = new(
            active,
            travelPlan.WorldBounds,
            travelPlan.Origin,
            travelPlan.Waypoints);
        CharacterIndependentTaskTravelProgress mismatchedTravelProgress =
            CharacterIndependentTaskTravelProgressTransition.Advance(
                new CharacterIndependentTaskTravelProgress(mismatchedTravelPlan),
                mismatchedTravelPlan,
                mismatchedTravelPlan.Waypoints.Single());
        if (occurrenceController.TryArriveAndOccur(
                candidate,
                mismatchedTravelPlan,
                mismatchedTravelProgress,
                mismatchedTravelPlan.Waypoints.Single(),
                activities,
                eventLog,
                worldTime,
                transcript)
            || !HasExactState(
                taskController,
                travelController,
                worldController,
                transcriptController,
                active,
                travelPlan,
                initialTravelProgress,
                activities,
                party,
                eventLog,
                worldTime,
                transcript))
        {
            Fail("Mismatched completed travel state changed authoritative occurrence state.");
            return;
        }

        WorldEvent ineligibleEvent = new(
            new WorldEventId("unrecognized-independent-task-event"),
            WorldEventTimeEffect.Advance);
        CharacterIndependentTaskWorldEventOccurrence ineligibleCandidate = new(
            active,
            new CharacterTurnWorldEventOccurrence(
                ineligibleEvent,
                candidate.Occurrence.TranscriptEntry));
        if (occurrenceController.TryArriveAndOccur(
                ineligibleCandidate,
                travelPlan,
                initialTravelProgress,
                travelPlan.Waypoints.Single(),
                activities,
                eventLog,
                worldTime,
                transcript)
            || !HasExactState(
                taskController,
                travelController,
                worldController,
                transcriptController,
                active,
                travelPlan,
                initialTravelProgress,
                activities,
                party,
                eventLog,
                worldTime,
                transcript))
        {
            Fail("An ineligible arrival occurrence changed authoritative scene state.");
            return;
        }

        if (!occurrenceController.TryArriveAndOccur(
                candidate,
                travelPlan,
                initialTravelProgress,
                travelPlan.Waypoints.Single(),
                activities,
                eventLog,
                worldTime,
                transcript)
            || travelController.Progress is not { IsComplete: true } completedTravelProgress
            || ReferenceEquals(completedTravelProgress, initialTravelProgress)
            || !ReferenceEquals(completedTravelProgress.Plan, travelPlan)
            || taskController.ActiveActivity is not null
            || taskController.Activities is not { } completedActivities
            || AssertSingle(completedActivities.Activities) is not { } completed
            || !ReferenceEquals(completed.Assignment, active.Assignment)
            || !ReferenceEquals(completed.State, CharacterIndependentTaskActivityState.Completed)
            || !ReferenceEquals(taskController.Party, party)
            || !ReferenceEquals(travelController.Plan, travelPlan)
            || !ReferenceEquals(travelController.Progress, completedTravelProgress)
            || worldController.EventLog is not { Entries.Count: 2 } resultingLog
            || !ReferenceEquals(
                resultingLog.Entries[^1],
                candidate.Occurrence.WorldEvent.EventId)
            || !ReferenceEquals(
                worldController.WorldTime?.CurrentEventId,
                candidate.Occurrence.WorldEvent.EventId)
            || transcriptController.Transcript is not { Entries.Count: 1 } resultingTranscript
            || !ReferenceEquals(
                resultingTranscript.Entries[0],
                candidate.Occurrence.TranscriptEntry)
            || !ReferenceEquals(occurrenceController.LastOccurrence, candidate)
            || occurrenceController.LastArrivalOccurrenceResult is null)
        {
            Fail("The task occurrence controller failed atomic authoritative completion.");
            return;
        }

        WorldEventLogSnapshot completedLog = worldController.EventLog!;
        WorldTimeSnapshot completedTime = worldController.WorldTime!;
        CharacterTurnTranscriptSnapshot completedTranscript = transcriptController.Transcript!;
        CharacterIndependentTaskArrivalOccurrenceResult arrivalResult =
            occurrenceController.LastArrivalOccurrenceResult!;
        CharacterIndependentTaskReturnTravelPlan returnPlan = returnTravelController.Plan!;
        CharacterIndependentTaskReturnTravelProgress returnProgress = returnTravelController.Progress!;
        if (!ReferenceEquals(arrivalResult.TravelProgress, completedTravelProgress)
            || !ReferenceEquals(arrivalResult.TaskOccurrenceResult.Activities, completedActivities)
            || !ReferenceEquals(returnPlan.ArrivalOccurrenceResult, arrivalResult)
            || !ReferenceEquals(returnPlan.CompletedActivity, completed)
            || returnPlan.WorldBounds != travelPlan.WorldBounds
            || returnPlan.Origin != travelPlan.Waypoints[^1]
            || returnPlan.Waypoints.Count != 1
            || returnPlan.Waypoints[0] != travelPlan.Origin
            || !ReferenceEquals(returnProgress.Plan, returnPlan)
            || returnProgress.CompletedWaypointCount != 0
            || returnProgress.IsComplete)
        {
            Fail("The final arrival did not initialize the exact authored return route.");
            return;
        }

        if (occurrenceController.TryArriveAndOccur(
                candidate,
                travelPlan,
                initialTravelProgress,
                travelPlan.Waypoints.Single(),
                completedActivities,
                completedLog,
                completedTime,
                completedTranscript)
            || !ReferenceEquals(taskController.Activities, completedActivities)
            || !ReferenceEquals(taskController.Party, party)
            || !ReferenceEquals(travelController.Plan, travelPlan)
            || !ReferenceEquals(travelController.Progress, completedTravelProgress)
            || !ReferenceEquals(worldController.EventLog, completedLog)
            || !ReferenceEquals(worldController.WorldTime, completedTime)
            || !ReferenceEquals(transcriptController.Transcript, completedTranscript)
            || !ReferenceEquals(occurrenceController.LastOccurrence, candidate)
            || !ReferenceEquals(
                occurrenceController.LastArrivalOccurrenceResult,
                arrivalResult)
            || !ReferenceEquals(returnTravelController.Plan, returnPlan)
            || !ReferenceEquals(returnTravelController.Progress, returnProgress)
            || !RejectsNulls(
                occurrenceController,
                candidate,
                travelPlan,
                completedTravelProgress,
                completedActivities,
                completedLog,
                completedTime,
                completedTranscript)
            || !RejectsArrivalNulls(
                occurrenceController,
                candidate,
                travelPlan,
                initialTravelProgress,
                completedActivities,
                completedLog,
                completedTime,
                completedTranscript))
        {
            Fail("Repeated or null task occurrence input did not preserve completed state.");
            return;
        }

        GD.Print(
            "Independent-task occurrence controller smoke passed: the exact active authored task "
            + "installed exact final travel arrival, authoritative event, meaningful time, "
            + "physical-action transcript, and an authored return route as one fail-closed scene "
            + "operation.");
        GetTree().Quit();
    }

    private static bool HasExactState(
        CharacterIndependentTaskDepartureController taskController,
        CharacterIndependentTaskTravelController travelController,
        WorldEventController worldController,
        CharacterTurnTranscriptController transcriptController,
        CharacterIndependentTaskActivity active,
        CharacterIndependentTaskTravelPlan travelPlan,
        CharacterIndependentTaskTravelProgress travelProgress,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        ReferenceEquals(taskController.ActiveActivity, active)
        && ReferenceEquals(travelController.Plan, travelPlan)
        && ReferenceEquals(travelController.Progress, travelProgress)
        && ReferenceEquals(taskController.Activities, activities)
        && ReferenceEquals(taskController.Party, party)
        && ReferenceEquals(worldController.EventLog, eventLog)
        && ReferenceEquals(worldController.WorldTime, worldTime)
        && ReferenceEquals(transcriptController.Transcript, transcript);

    private static bool RejectsNulls(
        CharacterIndependentTaskOccurrenceController controller,
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskTravelPlan travelPlan,
        CharacterIndependentTaskTravelProgress travelProgress,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        ThrowsArgumentNull(() => controller.TryOccur(
            null!, travelPlan, travelProgress, activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, null!, travelProgress, activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, travelPlan, null!, activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, travelPlan, travelProgress, null!, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, travelPlan, travelProgress, activities, null!, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, travelPlan, travelProgress, activities, eventLog, null!, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, travelPlan, travelProgress, activities, eventLog, worldTime, null!));

    private static bool RejectsArrivalNulls(
        CharacterIndependentTaskOccurrenceController controller,
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskTravelPlan travelPlan,
        CharacterIndependentTaskTravelProgress travelProgress,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        ThrowsArgumentNull(() => controller.TryArriveAndOccur(
            null!, travelPlan, travelProgress, travelPlan.Waypoints[0],
            activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryArriveAndOccur(
            candidate, null!, travelProgress, travelPlan.Waypoints[0],
            activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryArriveAndOccur(
            candidate, travelPlan, null!, travelPlan.Waypoints[0],
            activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryArriveAndOccur(
            candidate, travelPlan, travelProgress, travelPlan.Waypoints[0],
            null!, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryArriveAndOccur(
            candidate, travelPlan, travelProgress, travelPlan.Waypoints[0],
            activities, null!, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryArriveAndOccur(
            candidate, travelPlan, travelProgress, travelPlan.Waypoints[0],
            activities, eventLog, null!, transcript))
        && ThrowsArgumentNull(() => controller.TryArriveAndOccur(
            candidate, travelPlan, travelProgress, travelPlan.Waypoints[0],
            activities, eventLog, worldTime, null!));

    private static bool RejectsInvalidSourceInputs(
        TemporaryCharacterIndependentTaskOccurrenceSource source,
        CharacterIndependentTaskActivity active) =>
        ThrowsArgumentNull(() => source.CreateOccurrence(null!))
        && ThrowsInvalidOperation(() => source.CreateOccurrence(
            new CharacterIndependentTaskActivity(
                active.Assignment,
                CharacterIndependentTaskActivityState.Assigned)));

    private static bool HasClosedSourceSurface()
    {
        Type sourceType = typeof(TemporaryCharacterIndependentTaskOccurrenceSource);
        string[] methodNames = sourceType.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        return sourceType.GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly).Length == 0
            && methodNames.Length == 2
            && methodNames[0] == "CreateOccurrence"
            && methodNames[1] == nameof(_Ready);
    }

    private static T AssertSingle<T>(IEnumerable<T> values)
    {
        T[] retained = values.ToArray();
        return retained.Length == 1
            ? retained[0]
            : throw new InvalidOperationException("Expected exactly one task activity.");
    }

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

    private static bool ThrowsInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
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
