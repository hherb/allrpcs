using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
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
        TemporaryCharacterIndependentTaskOccurrenceSource occurrenceSource =
            GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>(
                "../TemporaryCharacterIndependentTaskOccurrenceSource");
        WorldEventController worldController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");

        CharacterIndependentTaskActivity active = taskController.ActiveActivity!;
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
            || !ReferenceEquals(taskController.Activities, activities)
            || !ReferenceEquals(worldController.EventLog, eventLog)
            || !ReferenceEquals(worldController.WorldTime, worldTime)
            || !ReferenceEquals(transcriptController.Transcript, transcript)
            || !RejectsInvalidSourceInputs(occurrenceSource, active)
            || !HasClosedSourceSurface())
        {
            Fail("The authored task occurrence candidate changed state or lost exact values.");
            return;
        }

        WorldEventLogSnapshot staleEventLog = new(eventLog.Entries);
        if (occurrenceController.TryOccur(
                candidate,
                activities,
                staleEventLog,
                worldTime,
                transcript)
            || !HasExactState(
                taskController,
                worldController,
                transcriptController,
                active,
                activities,
                party,
                eventLog,
                worldTime,
                transcript))
        {
            Fail("The task occurrence controller changed state after stale input.");
            return;
        }

        if (!occurrenceController.TryOccur(
                candidate,
                activities,
                eventLog,
                worldTime,
                transcript)
            || taskController.ActiveActivity is not null
            || taskController.Activities is not { } completedActivities
            || AssertSingle(completedActivities.Activities) is not { } completed
            || !ReferenceEquals(completed.Assignment, active.Assignment)
            || !ReferenceEquals(completed.State, CharacterIndependentTaskActivityState.Completed)
            || !ReferenceEquals(taskController.Party, party)
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
            || !ReferenceEquals(occurrenceController.LastOccurrence, candidate))
        {
            Fail("The task occurrence controller failed atomic authoritative completion.");
            return;
        }

        WorldEventLogSnapshot completedLog = worldController.EventLog!;
        WorldTimeSnapshot completedTime = worldController.WorldTime!;
        CharacterTurnTranscriptSnapshot completedTranscript = transcriptController.Transcript!;
        if (occurrenceController.TryOccur(
                candidate,
                completedActivities,
                completedLog,
                completedTime,
                completedTranscript)
            || !ReferenceEquals(taskController.Activities, completedActivities)
            || !ReferenceEquals(taskController.Party, party)
            || !ReferenceEquals(worldController.EventLog, completedLog)
            || !ReferenceEquals(worldController.WorldTime, completedTime)
            || !ReferenceEquals(transcriptController.Transcript, completedTranscript)
            || !ReferenceEquals(occurrenceController.LastOccurrence, candidate)
            || !RejectsNulls(
                occurrenceController,
                candidate,
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
            + "completed with its authoritative event, meaningful time, and physical-action "
            + "transcript as one fail-closed scene operation.");
        GetTree().Quit();
    }

    private static bool HasExactState(
        CharacterIndependentTaskDepartureController taskController,
        WorldEventController worldController,
        CharacterTurnTranscriptController transcriptController,
        CharacterIndependentTaskActivity active,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        ReferenceEquals(taskController.ActiveActivity, active)
        && ReferenceEquals(taskController.Activities, activities)
        && ReferenceEquals(taskController.Party, party)
        && ReferenceEquals(worldController.EventLog, eventLog)
        && ReferenceEquals(worldController.WorldTime, worldTime)
        && ReferenceEquals(transcriptController.Transcript, transcript);

    private static bool RejectsNulls(
        CharacterIndependentTaskOccurrenceController controller,
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        ThrowsArgumentNull(() => controller.TryOccur(
            null!, activities, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, null!, eventLog, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, activities, null!, worldTime, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, activities, eventLog, null!, transcript))
        && ThrowsArgumentNull(() => controller.TryOccur(
            candidate, activities, eventLog, worldTime, null!));

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
