using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves an unterminated ordinary healer model stream fails closed.</summary>
public partial class CharacterModelOrdinaryMalformedStreamSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ORDINARY_MALFORMED_MODEL_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Tell me what you can before the message ends.";
    private const string PartialText = "I can only say this much";
    private const string NoResponseText = "Character response: NONE";
    private const int MaximumProcessFrames = 120;

    private readonly UnterminatedModel model = new();
    private CharacterModelGenerationFlowController flow = null!;
    private DialogueEventController dialogueController = null!;
    private DialogueInputController dialogueInputController = null!;
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryCharacterExaminationFlowController examinationController = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController searchController = null!;
    private LineEdit inputControl = null!;
    private int processFrames;
    private int stage;
    private object? initialEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;
    private DialogueEvent? acceptedDialogue;
    private CharacterTurnTranscriptSnapshot? acceptedTranscript;
    private CharacterTurnRequest? acceptedRequest;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        TemporaryCharacterModelSource modelSource = GetNode<TemporaryCharacterModelSource>(
            "../TemporaryCharacterModelSource");
        if (!modelSource.TrySupplyModel(model))
        {
            Fail("The malformed-stream smoke could not supply its unterminated model.");
            return;
        }

        flow = GetNode<CharacterModelGenerationFlowController>(
            "../CharacterModelGenerationFlowController");
        dialogueController = GetNode<DialogueEventController>("../DialogueEventController");
        dialogueInputController = GetNode<DialogueInputController>("../DialogueInputController");
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        examinationController = GetNode<TemporaryCharacterExaminationFlowController>(
            "../TemporaryCharacterExaminationFlowController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        searchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        inputControl = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("The malformed ordinary model stream did not resolve within its bound.");
            return;
        }

        if (stage == 0)
        {
            CaptureInitialStateAndSubmit();
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            CaptureAcceptedTurn();
            return;
        }

        ObserveRejectedStream();
    }

    private void CaptureInitialStateAndSubmit()
    {
        initialEventLog = worldEventController.EventLog;
        initialWorldTime = worldEventController.WorldTime;
        initialWorldObjects = worldObjectController.WorldObjects;
        initialPerception = searchController.Perception;
        inputControl.Text = PlayerText;
        inputControl.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
    }

    private void CaptureAcceptedTurn()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? utterance = dialogueInputController.LastSubmittedUtterance;
        DialogueEvent? dialogue = dialogueController.LastOccurredEvent;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (activity is null
            || utterance is null
            || dialogue is null
            || transcript?.Entries.Count != 1
            || utterance.Text != PlayerText
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(activity.Request.DialogueEvent, dialogue)
            || !ReferenceEquals(dialogue.Utterance, utterance)
            || !ReferenceEquals(transcript.Entries[0].SpokenStatement, utterance))
        {
            return;
        }

        acceptedDialogue = dialogue;
        acceptedTranscript = transcript;
        acceptedRequest = activity.Request;
        stage = 2;
    }

    private void ObserveRejectedStream()
    {
        CharacterModelStreamSnapshot? result = flow.Result;
        CharacterTurnActivity? activity = turnController.Activity;
        if (result is null
            || activity is null
            || ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
        {
            return;
        }

        CharacterModelStreamTermination? termination = result.Updates[^1]
            as CharacterModelStreamTermination;
        TemporaryCharacterModelGenerationStatusLabel status =
            GetNode<TemporaryCharacterModelGenerationStatusLabel>(
                "../DialogueOverlay/Panel/ModelStatus");
        TemporaryCharacterModelStreamTextLabel text =
            GetNode<TemporaryCharacterModelStreamTextLabel>(
                "../DialogueOverlay/Panel/ModelStreamText");
        Label response = GetNode<Label>("../DialogueOverlay/Panel/Response");
        if (model.CallCount != 1
            || !ReferenceEquals(model.ReceivedRequest?.TurnRequest, acceptedRequest)
            || result.Updates.Count != 2
            || result.Updates[0] is not CharacterModelTextDelta { Text: PartialText }
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Failed,
                Response: null,
                Failure.Code: CharacterModelFailureCode.InvalidResponse,
            }
            || !ReferenceEquals(activity.Request, acceptedRequest)
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Cancelled)
            || activity.Response is not null
            || !ReferenceEquals(dialogueController.LastOccurredEvent, acceptedDialogue)
            || !ReferenceEquals(transcriptController.Transcript, acceptedTranscript)
            || acceptedTranscript?.Entries.Any(entry => entry.PhysicalAction is not null) != false
            || response.Text != NoResponseText
            || !ReferenceEquals(
                status.Presentation,
                CharacterModelGenerationStatusPresentation.Failed)
            || !text.Visible
            || text.Text != PartialText
            || examinationController.LastExaminationResult is not null
            || examinationController.LastCollectionOccurrence is not null
            || !HasUnchangedAuthoritativeState())
        {
            Fail("The malformed model stream escaped the ordinary fail-closed boundary.");
            return;
        }

        SetProcess(false);
        GD.Print(
            "Malformed ordinary character-model stream smoke passed: one accepted text delta "
            + "prevented retry, missing termination became bounded invalid-response failure, "
            + "and no dialogue, action, fallback, or authoritative mutation occurred.");
        GetTree().Quit();
    }

    private bool HasUnchangedAuthoritativeState() =>
        ReferenceEquals(worldEventController.EventLog, initialEventLog)
        && ReferenceEquals(worldEventController.WorldTime, initialWorldTime)
        && ReferenceEquals(worldObjectController.WorldObjects, initialWorldObjects)
        && ReferenceEquals(searchController.Perception, initialPerception);

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class UnterminatedModel : ICharacterModel
    {
        public int CallCount { get; private set; }

        public CharacterModelGenerationRequest? ReceivedRequest { get; private set; }

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            ReceivedRequest = request;
            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(PartialText);
        }
    }
}
