using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs one scripted success followed by default-offline ordinary healer flow.</summary>
public partial class TemporaryCharacterModelSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SCRIPTED_CHARACTER_MODEL_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string FirstPlayerText = "Please wait here while I think.";
    private const string SecondPlayerText = "Can you answer another question?";
    private const string HealerSpeechText = "I will wait here with you.";
    private const int MaximumProcessFrames = 180;

    private readonly ScriptedWaitModel scriptedModel = new();
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
    private LineEdit inputControl = null!;
    private int processFrames;
    private int stage;
    private object? initialEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;
    private TemporaryCharacterModelGenerationInput? firstInput;
    private CharacterModelStreamSnapshot? firstResult;
    private CharacterTurnTranscriptSnapshot? secondAcceptedTranscript;

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
        if (!modelSource.TrySupplyModel(scriptedModel)
            || modelSource.TrySupplyModel(new ScriptedWaitModel()))
        {
            Fail("The scene model source did not retain exactly one caller-supplied model.");
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
            Fail("Scripted ordinary character-model flow did not finish within its bound.");
            return;
        }

        switch (stage)
        {
            case 0:
                CaptureInitialStateAndSubmit(FirstPlayerText);
                stage = 1;
                break;
            case 1:
                CaptureFirstAcceptedTurn();
                break;
            case 2:
                ObserveScriptedCompletion();
                break;
            case 3:
                CaptureSecondAcceptedTurn();
                break;
            default:
                ObserveDefaultOfflineCompletion();
                break;
        }
    }

    private void CaptureInitialStateAndSubmit(string text)
    {
        initialEventLog = worldEventController.EventLog;
        initialWorldTime = worldEventController.WorldTime;
        initialWorldObjects = worldObjectController.WorldObjects;
        initialPerception = searchController.Perception;
        Submit(text);
    }

    private void CaptureFirstAcceptedTurn()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? utterance = dialogueInputController.LastSubmittedUtterance;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (activity is null
            || utterance is null
            || transcript?.Entries.Count != 1
            || utterance.Text != FirstPlayerText
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(activity.Request.DialogueEvent.Utterance, utterance)
            || !ReferenceEquals(transcript.Entries[0].SpokenStatement, utterance))
        {
            return;
        }

        firstInput = inputSource.CreateInput(activity);
        if (firstInput is null
            || !ReferenceEquals(firstInput.GenerationRequest.TurnRequest, activity.Request))
        {
            Fail("The scripted ordinary turn did not retain its exact bounded generation input.");
            return;
        }

        stage = 2;
    }

    private void ObserveScriptedCompletion()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        CharacterModelStreamSnapshot? result = flow.Result;
        Label responseLabel = GetNode<Label>("../DialogueOverlay/Panel/Response");
        if (activity is null
            || result is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            || responseLabel.Text != HealerSpeechText)
        {
            return;
        }

        CharacterModelStreamTermination? termination = result.Updates[^1]
            as CharacterModelStreamTermination;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        CharacterModelGenerationRequest? receivedRequest = scriptedModel.ReceivedRequest;
        if (firstInput is null
            || scriptedModel.CallCount != 1
            || receivedRequest is null
            || !ReferenceEquals(
                receivedRequest.TurnRequest,
                firstInput.GenerationRequest.TurnRequest)
            || !HasSameExactContext(
                receivedRequest.Context,
                firstInput.GenerationRequest.Context)
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Response: not null,
                Failure: null,
            }
            || !ReferenceEquals(activity.Request, firstInput.GenerationRequest.TurnRequest)
            || !ReferenceEquals(activity.Response, termination.Response)
            || termination.Response.Speech?.Text != HealerSpeechText
            || !ReferenceEquals(
                termination.Response.ImmediateIntention.Type,
                CharacterIntentionType.Wait)
            || termination.Response.ImmediateIntention.TargetObjectId is not null
            || result.Updates.Count != 2
            || result.Updates[0] is not CharacterModelTextDelta { Text: HealerSpeechText }
            || transcript?.Entries.Count != 2
            || transcript.Entries.Count(entry =>
                ReferenceEquals(entry.SpokenStatement, termination.Response.Speech)) != 1
            || transcript.Entries.Any(entry => entry.PhysicalAction is not null)
            || !HasOnlyFilteredProviderContext(receivedRequest)
            || examinationController.LastExaminationResult is not null
            || examinationController.LastCollectionOccurrence is not null
            || !HasUnchangedAuthoritativeState())
        {
            Fail("The scripted Wait response escaped its exact speech-only ordinary boundary.");
            return;
        }

        firstResult = result;
        Submit(SecondPlayerText);
        stage = 3;
    }

    private void CaptureSecondAcceptedTurn()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? utterance = dialogueInputController.LastSubmittedUtterance;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (activity is null
            || utterance is null
            || transcript?.Entries.Count != 3
            || utterance.Text != SecondPlayerText
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(activity.Request.DialogueEvent.Utterance, utterance)
            || !ReferenceEquals(transcript.Entries[^1].SpokenStatement, utterance))
        {
            return;
        }

        secondAcceptedTranscript = transcript;
        stage = 4;
    }

    private void ObserveDefaultOfflineCompletion()
    {
        CharacterModelStreamSnapshot? result = flow.Result;
        CharacterTurnActivity? activity = turnController.Activity;
        if (result is null
            || ReferenceEquals(result, firstResult)
            || ReferenceEquals(activity?.State, CharacterTurnActivityState.AwaitingResponse))
        {
            return;
        }

        CharacterModelStreamTermination? termination = result.Updates.SingleOrDefault()
            as CharacterModelStreamTermination;
        if (scriptedModel.CallCount != 1
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Failed,
                Response: null,
                Failure.Code: CharacterModelFailureCode.Unavailable,
            }
            || activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Cancelled)
            || activity.Response is not null
            || !ReferenceEquals(transcriptController.Transcript, secondAcceptedTranscript)
            || transcriptController.Transcript?.Entries.Any(
                entry => entry.PhysicalAction is not null) != false
            || examinationController.LastExaminationResult is not null
            || examinationController.LastCollectionOccurrence is not null
            || !HasUnchangedAuthoritativeState())
        {
            Fail("The later default-offline request did not fail closed after terminal reset.");
            return;
        }

        SetProcess(false);
        GD.Print(
            "Scripted ordinary character-model smoke passed: one exact caller-supplied model "
            + "completed a validated Wait response with one healer speech occurrence and no "
            + "physical or hidden-state authority; a later default-offline request reset and "
            + "failed closed without deterministic fallback or world-state mutation.");
        GetTree().Quit();
    }

    private bool HasUnchangedAuthoritativeState() =>
        ReferenceEquals(worldEventController.EventLog, initialEventLog)
        && ReferenceEquals(worldEventController.WorldTime, initialWorldTime)
        && ReferenceEquals(worldObjectController.WorldObjects, initialWorldObjects)
        && ReferenceEquals(searchController.Perception, initialPerception);

    private static bool HasOnlyFilteredProviderContext(CharacterModelGenerationRequest request) =>
        request.Context.Entries.Any(
            entry => entry.Kind == CharacterModelContextEntryKind.UntrustedPlayerDialogue)
        && request.Context.Entries.All(
            entry => !entry.Text.Contains("stone", StringComparison.OrdinalIgnoreCase)
                && !entry.Text.Contains("secret", StringComparison.OrdinalIgnoreCase));

    private static bool HasSameExactContext(
        CharacterModelContextSnapshot left,
        CharacterModelContextSnapshot right) =>
        left.Entries.Count == right.Entries.Count
        && left.Entries.Zip(right.Entries).All(
            pair => pair.First.Kind == pair.Second.Kind
                && ReferenceEquals(pair.First.Text, pair.Second.Text));

    private void Submit(string text)
    {
        inputControl.Text = text;
        inputControl.EmitSignal(LineEdit.SignalName.TextSubmitted, text);
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class ScriptedWaitModel : ICharacterModel
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
