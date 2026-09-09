using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the complete deterministic herb-patch examination flow when enabled.</summary>
public partial class CharacterExaminationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_EXAMINATION_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Please examine the flowers and collect them only if safe.";

    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryCharacterExaminationFlowController flowController = null!;
    private CharacterTurnActivityController turnController = null!;
    private TemporaryCharacterTurnResponseSource responseSource = null!;
    private WorldEventController worldEventController = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private LineEdit input = null!;
    private object? initialWorldObjects;
    private object? initialPerception;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool enabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(enabled);
        if (!enabled)
        {
            return;
        }

        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        flowController = GetNode<TemporaryCharacterExaminationFlowController>(
            "../TemporaryCharacterExaminationFlowController");
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        responseSource = GetNode<TemporaryCharacterTurnResponseSource>(
            "../TemporaryCharacterTurnResponseSource");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            initialWorldObjects = GetNode<WorldObjectController>(
                "../WorldObjectController").WorldObjects;
            initialPerception = GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception;
            input.Text = PlayerText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            CharacterTurnActivity? activity = turnController.Activity;
            if (activity is null
                || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
            {
                Fail("Focused examination scaffolding did not observe its awaiting turn.");
                return;
            }

            CharacterTurnResponse response = responseSource.CreateResponse(
                activity.Request,
                GetNode<TemporaryWorldObjectSource>(
                    "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id);
            if (!turnController.TryReceiveResponse(response))
            {
                Fail(
                    "Focused examination scaffolding could not install its deterministic "
                    + "response.");
                return;
            }

            stage = 2;
            return;
        }

        if (flowController.LastCollectionOccurrence is null)
        {
            return;
        }

        SetProcess(false);
        CharacterExaminationResult? result = flowController.LastExaminationResult;
        CharacterTurnWorldEventOccurrence collection = flowController.LastCollectionOccurrence;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        if (result is null
            || !ReferenceEquals(result.Safety, CharacterExaminationSafety.SafeToCollect)
            || !ReferenceEquals(result.Occurrence.WorldEvent, worldEventSource.HerbPatchExaminedWorldEvent)
            || !ReferenceEquals(collection.WorldEvent, worldEventSource.HerbPatchCollectedWorldEvent)
            || !ReferenceEquals(result.TargetObjectId, collection.TranscriptEntry.PhysicalAction?.TargetObjectId)
            || collection.TranscriptEntry.PhysicalAction?.Type != CharacterIntentionType.CollectObject
            || transcript is not { Entries.Count: 4 }
            || transcript.Entries[0].SpokenStatement?.Text != PlayerText
            || transcript.Entries[1].SpokenStatement?.Text
                != DeterministicExaminationResponseSelector.ExaminationSpeech
            || !ReferenceEquals(transcript.Entries[2], result.Occurrence.TranscriptEntry)
            || !ReferenceEquals(transcript.Entries[3], collection.TranscriptEntry)
            || eventLog is not { Entries.Count: 3 }
            || !ReferenceEquals(eventLog.Entries[1], result.Occurrence.WorldEvent.EventId)
            || !ReferenceEquals(eventLog.Entries[2], collection.WorldEvent.EventId)
            || !ReferenceEquals(worldEventController.WorldTime?.CurrentEventId, collection.WorldEvent.EventId)
            || !ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            || !ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception))
        {
            Fail("Live examination flow failed exact ordered safe collection containment.");
            return;
        }

        GD.Print(
            "Character examination flow smoke passed: exact player and healer speech, examination, "
            + "authoritative safe result, and collection occurred in order while world objects and "
            + "perception remained unchanged.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
