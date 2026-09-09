using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs focused healer response-speech occurrence checks when enabled.</summary>
public partial class CharacterTurnResponseSpeechFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_TURN_RESPONSE_SPEECH_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Please examine the flowers before collecting them.";

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private DialogueInputController inputController = null!;
    private LineEdit input = null!;
    private int stage;
    private DialogueUtterance? playerUtterance;
    private CharacterTurnTranscriptSnapshot? recordedTranscript;
    private DialogueEvent? recordedHealerEvent;
    private object? initialWorldEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        inputController = GetNode<DialogueInputController>("../DialogueInputController");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            CaptureInitialState();
            input.Text = PlayerText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            playerUtterance = inputController.LastSubmittedUtterance;
            if (playerUtterance is null
                || turnController.Activity is not { Response: null } awaiting
                || !ReferenceEquals(awaiting.State, CharacterTurnActivityState.AwaitingResponse)
                || !ReferenceEquals(eventController.LastOccurredEvent?.Utterance, playerUtterance)
                || transcriptController.Transcript is not { Entries.Count: 1 } transcript
                || !ReferenceEquals(transcript.Entries[0].SpokenStatement, playerUtterance))
            {
                Fail("Healer speech flow did not preserve the exact awaiting player occurrence.");
                return;
            }

            stage = 2;
            return;
        }

        if (stage == 2)
        {
            if (!ProvesExactHealerSpeechOccurrence())
            {
                return;
            }

            recordedTranscript = transcriptController.Transcript;
            recordedHealerEvent = eventController.LastOccurredEvent;
            stage = 3;
            return;
        }

        SetProcess(false);
        if (!ReferenceEquals(transcriptController.Transcript, recordedTranscript)
            || !ReferenceEquals(eventController.LastOccurredEvent, recordedHealerEvent)
            || !UnrelatedStateIsPreserved())
        {
            Fail("Healer response speech was repeated or changed unrelated state.");
            return;
        }

        GD.Print(
            "Character-turn response speech flow smoke passed: the exact complete healer speech "
            + "occurred once with the Seeker as listener, appended once after player speech, "
            + "remained speaking for an interruptible frame without repeating or executing.");
        GetTree().Quit();
    }

    private bool ProvesExactHealerSpeechOccurrence()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? speech = activity?.Response?.Speech;
        DialogueEvent? occurred = eventController.LastOccurredEvent;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        bool matches = playerUtterance is not null
            && activity is not null
            && speech is not null
            && occurred is not null
            && !ReferenceEquals(occurred.Utterance, playerUtterance)
            && ReferenceEquals(occurred.Utterance, speech)
            && ReferenceEquals(occurred.Audience.SpeakerCharacterId, speech.SpeakerCharacterId)
            && occurred.Audience.ListenerCharacterIds.Count == 1
            && ReferenceEquals(
                occurred.Audience.ListenerCharacterIds[0],
                playerUtterance.SpeakerCharacterId)
            && ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking)
            && ReferenceEquals(
                activity.Response!.ImmediateIntention.Type,
                CharacterIntentionType.ExamineObject)
            && ReferenceEquals(
                activity.Response.ImmediateIntention.TargetObjectId,
                GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource")
                    .WorldObjects.Entries[0].Identity.Id)
            && transcript is { Entries.Count: 2 }
            && ReferenceEquals(transcript.Entries[0].SpokenStatement, playerUtterance)
            && ReferenceEquals(transcript.Entries[1].SpokenStatement, speech)
            && transcript.Entries.All(entry => entry.PhysicalAction is null);
        if (!matches)
        {
            Fail("Exact healer speech did not occur and append after the player speech.");
        }

        return matches;
    }

    private void CaptureInitialState()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        initialWorldEventLog = worldEvents.EventLog;
        initialWorldTime = worldEvents.WorldTime;
        initialWorldObjects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects;
        initialPerception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
    }

    private bool UnrelatedStateIsPreserved()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        return ReferenceEquals(worldEvents.EventLog, initialWorldEventLog)
            && ReferenceEquals(worldEvents.WorldTime, initialWorldTime)
            && ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            && ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception);
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
