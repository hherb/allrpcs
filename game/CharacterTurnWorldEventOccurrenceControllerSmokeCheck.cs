using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks atomic live character-action occurrence when explicitly enabled.</summary>
public partial class CharacterTurnWorldEventOccurrenceControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_TURN_WORLD_EVENT_CONTROLLER_CHECK";
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

        CharacterTurnWorldEventOccurrenceController controller =
            GetNode<CharacterTurnWorldEventOccurrenceController>(
                "../CharacterTurnWorldEventOccurrenceController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        WorldEventController worldEventController =
            GetNode<WorldEventController>("../WorldEventController");
        TemporaryWorldEventSource eventSource =
            GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        ObjectId targetObjectId = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id;
        CharacterTurnActivity activity = SpeakingActivity(targetObjectId);
        DialogueUtterance speech = activity.Response!.Speech!;

        if (!transcriptController.TryAppendSpokenStatement(speech)
            || !controller.TryOccurImmediateIntention(
                activity,
                eventSource.HerbPatchExaminedWorldEvent))
        {
            Fail("Character action controller rejected an eligible exact examination.");
            return;
        }

        CharacterTurnWorldEventOccurrence? examination = controller.LastOccurrence;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        WorldTimeSnapshot? worldTime = worldEventController.WorldTime;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (examination is null
            || !ReferenceEquals(examination.WorldEvent, eventSource.HerbPatchExaminedWorldEvent)
            || !ReferenceEquals(
                examination.TranscriptEntry.CharacterId,
                activity.Request.RespondingCharacterId)
            || !ReferenceEquals(
                examination.TranscriptEntry.PhysicalAction,
                activity.Response.ImmediateIntention)
            || eventLog is not { Entries.Count: 2 }
            || !ReferenceEquals(eventLog.Entries[^1], examination.WorldEvent.EventId)
            || !ReferenceEquals(worldTime?.CurrentEventId, examination.WorldEvent.EventId)
            || transcript is not { Entries.Count: 2 }
            || !ReferenceEquals(transcript.Entries[0].SpokenStatement, speech)
            || !ReferenceEquals(transcript.Entries[1], examination.TranscriptEntry))
        {
            Fail("Character action controller failed exact atomic result installation.");
            return;
        }

        if (controller.TryOccurImmediateIntention(
                activity,
                eventSource.HerbPatchExaminedWorldEvent)
            || !ReferenceEquals(controller.LastOccurrence, examination)
            || !ReferenceEquals(worldEventController.EventLog, eventLog)
            || !ReferenceEquals(worldEventController.WorldTime, worldTime)
            || !ReferenceEquals(transcriptController.Transcript, transcript)
            || !RejectsNullInputs(controller, activity, eventSource.HerbPatchExaminedWorldEvent))
        {
            Fail("Character action controller failed closed-state preservation.");
            return;
        }

        CharacterExaminationResult result = new(
            examination,
            CharacterExaminationSafety.SafeToCollect);
        CharacterIntention collection =
            DeterministicPostExaminationIntentionSelector.Select(result);
        if (!controller.TryOccurPostExaminationCollection(
                result,
                collection,
                eventSource.HerbPatchCollectedWorldEvent)
            || controller.LastOccurrence is not { } collected
            || !ReferenceEquals(collected.WorldEvent, eventSource.HerbPatchCollectedWorldEvent)
            || !ReferenceEquals(collected.TranscriptEntry.PhysicalAction, collection)
            || worldEventController.EventLog is not { Entries.Count: 3 } collectedLog
            || !ReferenceEquals(collectedLog.Entries[^1], collected.WorldEvent.EventId)
            || !ReferenceEquals(
                worldEventController.WorldTime?.CurrentEventId,
                collected.WorldEvent.EventId)
            || transcriptController.Transcript is not { Entries.Count: 3 } collectedTranscript
            || !ReferenceEquals(collectedTranscript.Entries[^1], collected.TranscriptEntry))
        {
            Fail("Character action controller rejected exact safe ordered collection.");
            return;
        }

        GD.Print(
            "Character action occurrence controller smoke passed: exact speaking response action, "
            + "authoritative examination and safe collection events, meaningful time, and "
            + "transcript occurrences changed atomically; "
            + "rejected repeats and null inputs preserved state.");
        GetTree().Quit();
    }

    private static CharacterTurnActivity SpeakingActivity(ObjectId targetObjectId)
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance playerSpeech = new(seekerId, "Please examine the flowers.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                playerSpeech,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(playerSpeech, DialogueActType.Request));
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueUtterance(healerId, "I will examine them first."),
            new CharacterIntention(CharacterIntentionType.ExamineObject, targetObjectId),
            [],
            []);
        return new CharacterTurnActivity(request, response, CharacterTurnActivityState.Speaking);
    }

    private static bool RejectsNullInputs(
        CharacterTurnWorldEventOccurrenceController controller,
        CharacterTurnActivity activity,
        WorldEvent worldEvent)
    {
        try
        {
            _ = controller.TryOccurImmediateIntention(null!, worldEvent);
            return false;
        }
        catch (ArgumentNullException)
        {
        }

        try
        {
            _ = controller.TryOccurImmediateIntention(activity, null!);
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
        GetTree().Quit(exitCode: 1);
    }
}
