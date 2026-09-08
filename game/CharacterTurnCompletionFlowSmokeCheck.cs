using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Movement;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs focused observed-speech to completed-turn checks when enabled.</summary>
public partial class CharacterTurnCompletionFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_TURN_COMPLETION_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Please wait while we decide what to examine.";

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private LineEdit input = null!;
    private int stage;
    private CharacterTurnActivity? speakingActivity;
    private CharacterTurnTranscriptSnapshot? spokenTranscript;
    private DialogueEvent? healerSpeechEvent;
    private object? initialWorldEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;
    private HealerCommandMode initialHealerMode;

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
            if (turnController.Activity is not { Response: null } awaiting
                || !ReferenceEquals(awaiting.State, CharacterTurnActivityState.AwaitingResponse))
            {
                Fail("Completion flow did not preserve the awaiting activity boundary.");
                return;
            }

            stage = 2;
            return;
        }

        if (stage == 2)
        {
            CharacterTurnActivity? activity = turnController.Activity;
            DialogueUtterance? speech = activity?.Response?.Speech;
            CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
            if (activity is null
                || speech is null
                || !ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking)
                || !ReferenceEquals(eventController.LastOccurredEvent?.Utterance, speech)
                || transcript is not { Entries.Count: 2 }
                || !ReferenceEquals(transcript.Entries[1].SpokenStatement, speech))
            {
                Fail("Completion flow did not observe the exact speaking response occurrence.");
                return;
            }

            speakingActivity = activity;
            spokenTranscript = transcript;
            healerSpeechEvent = eventController.LastOccurredEvent;
            stage = 3;
            return;
        }

        SetProcess(false);
        CharacterTurnActivity? completed = turnController.Activity;
        if (speakingActivity is null
            || completed is null
            || ReferenceEquals(completed, speakingActivity)
            || !ReferenceEquals(completed.Request, speakingActivity.Request)
            || !ReferenceEquals(completed.Response, speakingActivity.Response)
            || !ReferenceEquals(completed.State, CharacterTurnActivityState.Completed)
            || !ReferenceEquals(transcriptController.Transcript, spokenTranscript)
            || !ReferenceEquals(eventController.LastOccurredEvent, healerSpeechEvent)
            || !UnrelatedStateIsPreserved())
        {
            Fail("Observed speaking activity did not complete without executing its intention.");
            return;
        }

        GD.Print(
            "Character-turn completion flow smoke passed: the exact speaking activity remained "
            + "cancellable for one frame, completed only after its exact speech occurrence, "
            + "retained its request and response, and did not execute the wait intention.");
        GetTree().Quit();
    }

    private void CaptureInitialState()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        initialWorldEventLog = worldEvents.EventLog;
        initialWorldTime = worldEvents.WorldTime;
        initialWorldObjects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects;
        initialPerception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
        initialHealerMode = GetNode<Healer>("../Healer").CommandMode;
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
                initialPerception)
            && GetNode<Healer>("../Healer").CommandMode == initialHealerMode;
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
