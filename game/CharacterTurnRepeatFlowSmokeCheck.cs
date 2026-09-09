using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs focused consecutive completed character-turn checks when enabled.</summary>
public partial class CharacterTurnRepeatFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_TURN_REPEAT_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string FirstText = "Please examine the flowers first.";
    private const string SecondText = "Now report whether they are safe.";

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryCharacterExaminationFlowController examinationFlowController = null!;
    private DialogueEventController eventController = null!;
    private DialogueInputController inputController = null!;
    private TemporaryCharacterTurnResponseSource responseSource = null!;
    private LineEdit input = null!;
    private int stage;
    private CharacterTurnActivity? firstCompleted;
    private CharacterTurnRequest? firstRequest;
    private DialogueUtterance? firstSpeech;
    private CharacterTurnRequest? secondRequest;
    private DialogueUtterance? secondSpeech;
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
        examinationFlowController = GetNode<TemporaryCharacterExaminationFlowController>(
            "../TemporaryCharacterExaminationFlowController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        inputController = GetNode<DialogueInputController>("../DialogueInputController");
        responseSource = GetNode<TemporaryCharacterTurnResponseSource>(
            "../TemporaryCharacterTurnResponseSource");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            CaptureInitialState();
            Submit(FirstText);
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            if (!IsExactAwaitingTurn(FirstText))
            {
                return;
            }

            firstRequest = turnController.Activity!.Request;
            if (!TryInstallDeterministicTestResponse(firstRequest))
            {
                return;
            }

            stage = 2;
            return;
        }

        if (stage == 2)
        {
            if (!IsExactSpeakingTurn(firstRequest))
            {
                return;
            }

            firstSpeech = turnController.Activity!.Response!.Speech;
            stage = 3;
            return;
        }

        if (stage == 3)
        {
            firstCompleted = turnController.Activity;
            if (firstCompleted is null
                || !ReferenceEquals(firstCompleted.Request, firstRequest)
                || !ReferenceEquals(firstCompleted.Response?.Speech, firstSpeech)
                || !ReferenceEquals(firstCompleted.State, CharacterTurnActivityState.Completed))
            {
                Fail("First exact live turn did not complete before the second submission.");
                return;
            }

            if (examinationFlowController.LastCollectionOccurrence is null)
            {
                return;
            }

            Submit(SecondText);
            stage = 4;
            return;
        }

        if (stage == 4)
        {
            if (!IsExactAwaitingTurn(SecondText))
            {
                return;
            }

            if (ReferenceEquals(turnController.Activity!.Request, firstRequest))
            {
                Fail("Second player utterance reused the first turn request instance.");
                return;
            }

            secondRequest = turnController.Activity.Request;
            if (!TryInstallDeterministicTestResponse(secondRequest))
            {
                return;
            }

            stage = 5;
            return;
        }

        if (stage == 5)
        {
            if (!IsExactSpeakingTurn(secondRequest))
            {
                return;
            }

            secondSpeech = turnController.Activity!.Response!.Speech;
            stage = 6;
            return;
        }

        SetProcess(false);
        CharacterTurnActivity? secondCompleted = turnController.Activity;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (firstCompleted is null
            || secondCompleted is null
            || ReferenceEquals(secondCompleted, firstCompleted)
            || !ReferenceEquals(secondCompleted.Request, secondRequest)
            || !ReferenceEquals(secondCompleted.Response?.Speech, secondSpeech)
            || !ReferenceEquals(secondCompleted.State, CharacterTurnActivityState.Completed)
            || !ReferenceEquals(firstCompleted.State, CharacterTurnActivityState.Completed)
            || transcript is not { Entries.Count: 6 }
            || transcript.Entries[0].SpokenStatement?.Text != FirstText
            || !ReferenceEquals(transcript.Entries[1].SpokenStatement, firstSpeech)
            || transcript.Entries[2].PhysicalAction?.Type != CharacterIntentionType.ExamineObject
            || transcript.Entries[3].PhysicalAction?.Type != CharacterIntentionType.CollectObject
            || transcript.Entries[4].SpokenStatement?.Text != SecondText
            || !ReferenceEquals(transcript.Entries[5].SpokenStatement, secondSpeech)
            || !UnrelatedStateIsPreserved())
        {
            Fail("Second exact live turn did not complete without replacing prior history.");
            return;
        }

        GD.Print(
            "Character-turn repeat flow smoke passed: two distinct heard player utterances "
            + "started consecutive exact awaiting, speaking, and completed turns, retained both "
            + "speech pairs around one exact examination and collection sequence.");
        GetTree().Quit();
    }

    private bool IsExactAwaitingTurn(string expectedText)
    {
        DialogueUtterance? submitted = inputController.LastSubmittedUtterance;
        CharacterTurnActivity? activity = turnController.Activity;
        bool matches = submitted?.Text == expectedText
            && activity is not null
            && ReferenceEquals(activity.Request.DialogueEvent.Utterance, submitted)
            && ReferenceEquals(eventController.LastOccurredEvent?.Utterance, submitted)
            && ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            && activity.Response is null;
        if (!matches)
        {
            Fail("Player utterance did not start its exact fresh awaiting turn.");
        }

        return matches;
    }

    private bool IsExactSpeakingTurn(CharacterTurnRequest? expectedRequest)
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? speech = activity?.Response?.Speech;
        bool matches = expectedRequest is not null
            && activity is not null
            && speech is not null
            && ReferenceEquals(activity.Request, expectedRequest)
            && ReferenceEquals(activity.Response!.Request, expectedRequest)
            && ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking)
            && ReferenceEquals(eventController.LastOccurredEvent?.Utterance, speech)
            && transcriptController.Transcript?.Entries.Any(
                entry => ReferenceEquals(entry.SpokenStatement, speech)) == true;
        if (!matches)
        {
            Fail("Fresh awaiting turn did not receive and record its exact response speech.");
        }

        return matches;
    }

    private void Submit(string text)
    {
        input.Text = text;
        input.EmitSignal(LineEdit.SignalName.TextSubmitted, text);
    }

    private bool TryInstallDeterministicTestResponse(CharacterTurnRequest request)
    {
        CharacterTurnResponse response = responseSource.CreateResponse(
            request,
            GetNode<TemporaryWorldObjectSource>(
                "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id);
        if (!turnController.TryReceiveResponse(response))
        {
            Fail("Focused repeat-flow scaffolding could not install its deterministic response.");
            return false;
        }

        return true;
    }

    private void CaptureInitialState()
    {
        initialWorldObjects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects;
        initialPerception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
    }

    private bool UnrelatedStateIsPreserved()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        return worldEvents.EventLog is { Entries.Count: 3 } eventLog
            && ReferenceEquals(
                eventLog.Entries[^1],
                GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource")
                    .HerbPatchCollectedWorldEvent.EventId)
            && ReferenceEquals(worldEvents.WorldTime?.CurrentEventId, eventLog.Entries[^1])
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
