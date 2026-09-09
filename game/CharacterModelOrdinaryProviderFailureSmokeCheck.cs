using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves retryable and terminal provider failures through ordinary healer dialogue.</summary>
public partial class CharacterModelOrdinaryProviderFailureSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK";
    private const string RetryMode = "retry";
    private const string TerminalMode = "terminal";
    private const string PlayerText = "Can you wait here despite a temporary interruption?";
    private const string HealerSpeechText = "I can wait here.";
    private const string NoResponseText = "Character response: NONE";
    private const int MaximumProcessFrames = 180;

    private CharacterModelGenerationFlowController flow = null!;
    private DialogueEventController dialogueController = null!;
    private DialogueInputController dialogueInputController = null!;
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryCharacterModelGenerationInputSource inputSource = null!;
    private TemporaryCharacterExaminationFlowController examinationController = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController searchController = null!;
    private ScriptedProviderOutcomeModel model = null!;
    private LineEdit inputControl = null!;
    private string mode = string.Empty;
    private int processFrames;
    private int stage;
    private object? initialEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;
    private DialogueEvent? acceptedDialogue;
    private CharacterTurnTranscriptSnapshot? acceptedTranscript;
    private TemporaryCharacterModelGenerationInput? acceptedInput;

    /// <inheritdoc />
    public override void _Ready()
    {
        mode = OS.GetEnvironment(CheckEnvironmentVariable);
        bool isEnabled = mode is RetryMode or TerminalMode;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        model = new ScriptedProviderOutcomeModel(mode == RetryMode);
        TemporaryCharacterModelSource modelSource = GetNode<TemporaryCharacterModelSource>(
            "../TemporaryCharacterModelSource");
        if (!modelSource.TrySupplyModel(model))
        {
            Fail("The provider-failure smoke could not supply its exact scripted model.");
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
        inputSource = GetNode<TemporaryCharacterModelGenerationInputSource>(
            "../TemporaryCharacterModelGenerationInputSource");
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
            Fail("The ordinary provider-failure flow did not resolve within its bound.");
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

        if (mode == RetryMode)
        {
            ObserveRetryCompletion();
        }
        else
        {
            ObserveTerminalFailure();
        }
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

        acceptedInput = inputSource.CreateInput(activity);
        if (acceptedInput is null
            || !ReferenceEquals(acceptedInput.GenerationRequest.TurnRequest, activity.Request))
        {
            Fail("The provider-failure flow did not retain its exact generation input.");
            return;
        }

        acceptedDialogue = dialogue;
        acceptedTranscript = transcript;
        stage = 2;
    }

    private void ObserveRetryCompletion()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        CharacterModelStreamSnapshot? result = flow.Result;
        Label response = GetNode<Label>("../DialogueOverlay/Panel/Response");
        if (activity is null
            || result is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            || response.Text != HealerSpeechText)
        {
            return;
        }

        CharacterModelStreamTermination? termination = result.Updates[^1]
            as CharacterModelStreamTermination;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        TemporaryCharacterModelGenerationStatusLabel status = GetStatusLabel();
        TemporaryCharacterModelStreamTextLabel text = GetTextLabel();
        if (!HasExactAttemptRequests(expectedCount: 2)
            || result.Updates.Count != 2
            || result.Updates[0] is not CharacterModelTextDelta { Text: HealerSpeechText }
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Response: not null,
                Failure: null,
            }
            || !ReferenceEquals(activity.Request, acceptedInput!.GenerationRequest.TurnRequest)
            || !ReferenceEquals(activity.Response, termination.Response)
            || termination.Response.Speech?.Text != HealerSpeechText
            || !ReferenceEquals(
                termination.Response.ImmediateIntention.Type,
                CharacterIntentionType.Wait)
            || termination.Response.ImmediateIntention.TargetObjectId is not null
            || transcript?.Entries.Count != 2
            || transcript.Entries.Count(entry =>
                ReferenceEquals(entry.SpokenStatement, termination.Response.Speech)) != 1
            || transcript.Entries.Any(entry => entry.PhysicalAction is not null)
            || !ReferenceEquals(
                status.Presentation,
                CharacterModelGenerationStatusPresentation.Completed)
            || !text.Visible
            || text.Text != HealerSpeechText
            || examinationController.LastExaminationResult is not null
            || examinationController.LastCollectionOccurrence is not null
            || !HasUnchangedAuthoritativeState())
        {
            Fail("The retryable provider failure entered scene state or escaped safe retry.");
            return;
        }

        Pass(
            "Ordinary provider retry smoke passed: one hidden text-free unavailable attempt "
            + "retried the same exact request, then one validated Wait response spoke once "
            + "without fallback, physical action, or authoritative mutation.");
    }

    private void ObserveTerminalFailure()
    {
        CharacterModelStreamSnapshot? result = flow.Result;
        CharacterTurnActivity? activity = turnController.Activity;
        if (result is null
            || activity is null
            || ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
        {
            return;
        }

        CharacterModelStreamTermination? termination = result.Updates.SingleOrDefault()
            as CharacterModelStreamTermination;
        TemporaryCharacterModelGenerationStatusLabel status = GetStatusLabel();
        TemporaryCharacterModelStreamTextLabel text = GetTextLabel();
        Label response = GetNode<Label>("../DialogueOverlay/Panel/Response");
        if (!HasExactAttemptRequests(expectedCount: 1)
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Failed,
                Response: null,
                Failure.Code: CharacterModelFailureCode.ProviderRejected,
            }
            || !ReferenceEquals(activity.Request, acceptedInput!.GenerationRequest.TurnRequest)
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Cancelled)
            || activity.Response is not null
            || !ReferenceEquals(dialogueController.LastOccurredEvent, acceptedDialogue)
            || !ReferenceEquals(transcriptController.Transcript, acceptedTranscript)
            || acceptedTranscript?.Entries.Any(entry => entry.PhysicalAction is not null) != false
            || response.Text != NoResponseText
            || !ReferenceEquals(
                status.Presentation,
                CharacterModelGenerationStatusPresentation.Failed)
            || text.Visible
            || text.Text.Length != 0
            || examinationController.LastExaminationResult is not null
            || examinationController.LastCollectionOccurrence is not null
            || !HasUnchangedAuthoritativeState())
        {
            Fail("The terminal provider failure changed dialogue or authoritative scene state.");
            return;
        }

        Pass(
            "Ordinary terminal provider-failure smoke passed: one exact non-retryable failure "
            + "cancelled only its pending turn with canonical content-free status and no "
            + "dialogue, fallback, physical action, or authoritative mutation.");
    }

    private bool HasExactAttemptRequests(int expectedCount)
    {
        if (acceptedInput is null || model.ReceivedRequests.Count != expectedCount)
        {
            return false;
        }

        CharacterModelGenerationRequest first = model.ReceivedRequests[0];
        return ReferenceEquals(first.TurnRequest, acceptedInput.GenerationRequest.TurnRequest)
            && model.ReceivedRequests.All(request => ReferenceEquals(request, first));
    }

    private bool HasUnchangedAuthoritativeState() =>
        ReferenceEquals(worldEventController.EventLog, initialEventLog)
        && ReferenceEquals(worldEventController.WorldTime, initialWorldTime)
        && ReferenceEquals(worldObjectController.WorldObjects, initialWorldObjects)
        && ReferenceEquals(searchController.Perception, initialPerception);

    private TemporaryCharacterModelGenerationStatusLabel GetStatusLabel() =>
        GetNode<TemporaryCharacterModelGenerationStatusLabel>(
            "../DialogueOverlay/Panel/ModelStatus");

    private TemporaryCharacterModelStreamTextLabel GetTextLabel() =>
        GetNode<TemporaryCharacterModelStreamTextLabel>(
            "../DialogueOverlay/Panel/ModelStreamText");

    private void Pass(string message)
    {
        SetProcess(false);
        GD.Print(message);
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class ScriptedProviderOutcomeModel(bool retryThenComplete) : ICharacterModel
    {
        public List<CharacterModelGenerationRequest> ReceivedRequests { get; } = [];

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            ReceivedRequests.Add(request);

            if (!retryThenComplete || ReceivedRequests.Count == 1)
            {
                CharacterModelFailureCode code = retryThenComplete
                    ? CharacterModelFailureCode.Unavailable
                    : CharacterModelFailureCode.ProviderRejected;
                yield return new CharacterModelStreamTermination(
                    CharacterModelStreamTerminationStatus.Failed,
                    null,
                    new CharacterModelFailure(code));
                yield break;
            }

            CharacterTurnResponse response = new(
                request.TurnRequest,
                CharacterTurnDisposition.Accept,
                new DialogueUtterance(request.TurnRequest.RespondingCharacterId, HealerSpeechText),
                new CharacterIntention(CharacterIntentionType.Wait, null),
                [],
                []);
            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(HealerSpeechText);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                response,
                null);
        }
    }
}
