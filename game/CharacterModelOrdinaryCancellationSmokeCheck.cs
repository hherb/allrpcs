using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Cancels one waiting caller-supplied model through ordinary dialogue input.</summary>
public partial class CharacterModelOrdinaryCancellationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ORDINARY_MODEL_CANCELLATION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Please consider this, but stop when I ask.";
    private const string PartialText = "I am considering your request.";
    private const string NoResponseText = "Character response: NONE";
    private const int MaximumProcessFrames = 120;
    private static readonly StringName CancelAction = new("character_turn_cancel");

    private readonly WaitingModel model = new();
    private Main main = null!;
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
            Fail("The ordinary cancellation smoke could not supply its waiting model.");
            return;
        }

        main = GetNode<Main>("..");
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
            Fail("Ordinary caller cancellation did not finish within its bound.");
            return;
        }

        if (stage == 0)
        {
            initialEventLog = worldEventController.EventLog;
            initialWorldTime = worldEventController.WorldTime;
            initialWorldObjects = worldObjectController.WorldObjects;
            initialPerception = searchController.Perception;
            inputControl.Text = PlayerText;
            GetNode<Healer>("../Healer").Visible = true; // Explicit present listener in this disposable fixture.
            inputControl.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            ObserveGeneratingAndCancel();
            return;
        }

        ObserveCancelled();
    }

    private void ObserveGeneratingAndCancel()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? utterance = dialogueInputController.LastSubmittedUtterance;
        CharacterModelStreamSnapshot? snapshot = GetNode<CharacterModelStreamController>(
            "../CharacterModelStreamController").Snapshot;
        TemporaryCharacterModelStreamTextLabel text =
            GetNode<TemporaryCharacterModelStreamTextLabel>(
                "../DialogueOverlay/Panel/ModelStreamText");
        if (activity is null
            || utterance is null
            || !flow.IsGenerating
            || model.CallCount != 1
            || snapshot is not { Updates.Count: 1 }
            || snapshot.Updates[0] is not CharacterModelTextDelta { Text: PartialText }
            || !text.Visible
            || text.Text != PartialText)
        {
            return;
        }

        DialogueEvent? dialogue = dialogueController.LastOccurredEvent;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (dialogue is null
            || transcript?.Entries.Count != 1
            || utterance.Text != PlayerText
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(activity.Request.DialogueEvent, dialogue)
            || !ReferenceEquals(dialogue.Utterance, utterance)
            || !ReferenceEquals(transcript.Entries[0].SpokenStatement, utterance)
            || !ReferenceEquals(model.ReceivedRequest?.TurnRequest, activity.Request))
        {
            Fail("The waiting model did not receive the exact ordinary pending turn.");
            return;
        }

        acceptedDialogue = dialogue;
        acceptedTranscript = transcript;
        acceptedRequest = activity.Request;
        main._UnhandledInput(new InputEventAction
        {
            Action = CancelAction,
            Pressed = true,
        });
        stage = 2;
    }

    private void ObserveCancelled()
    {
        CharacterModelStreamSnapshot? result = flow.Result;
        CharacterTurnActivity? activity = turnController.Activity;
        if (result is null || activity is null)
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
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Cancelled,
                Response: null,
                Failure: null,
            }
            || result.Updates.Count != 2
            || result.Updates[0] is not CharacterModelTextDelta { Text: PartialText }
            || !ReferenceEquals(activity.Request, acceptedRequest)
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Cancelled)
            || activity.Response is not null
            || !ReferenceEquals(dialogueController.LastOccurredEvent, acceptedDialogue)
            || !ReferenceEquals(transcriptController.Transcript, acceptedTranscript)
            || acceptedTranscript?.Entries.Any(entry => entry.PhysicalAction is not null) != false
            || response.Text != NoResponseText
            || !ReferenceEquals(
                status.Presentation,
                CharacterModelGenerationStatusPresentation.Cancelled)
            || !text.Visible
            || text.Text != PartialText
            || examinationController.LastExaminationResult is not null
            || examinationController.LastCollectionOccurrence is not null
            || !HasUnchangedAuthoritativeState())
        {
            Fail("Ordinary cancellation changed response or authoritative scene state.");
            return;
        }

        SetProcess(false);
        GD.Print(
            "Ordinary character-model cancellation smoke passed: the exact waiting turn "
            + "preserved its accepted streamed text, ended in canonical caller cancellation, "
            + "and produced no speech, action, fallback, or authoritative state mutation.");
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

    private sealed class WaitingModel : ICharacterModel
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
            yield return new CharacterModelTextDelta(PartialText);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
    }
}
