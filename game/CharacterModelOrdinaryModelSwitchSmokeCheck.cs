using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Switches exact caller-composed provider bindings across ordinary healer turns.</summary>
public partial class CharacterModelOrdinaryModelSwitchSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ORDINARY_MODEL_SWITCH_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string FirstPlayerText = "Will you wait beside the stream?";
    private const string SecondPlayerText = "Will you also wait near the garden?";
    private const string FirstSpeechText = "I will wait beside the stream.";
    private const string SecondSpeechText = "I will also wait near the garden.";
    private const int MaximumProcessFrames = 240;

    private readonly ScriptedWaitModel firstModel = new(FirstSpeechText);
    private readonly ScriptedWaitModel secondModel = new(SecondSpeechText);
    private TemporaryCharacterModelSource modelSource = null!;
    private CharacterModelGenerationFlowController flow = null!;
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
    private CharacterTurnRequest? firstRequest;
    private CharacterTurnRequest? secondRequest;
    private CharacterModelStreamSnapshot? firstResult;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        modelSource = GetNode<TemporaryCharacterModelSource>(
            "../TemporaryCharacterModelSource");
        CharacterModelSettings openAiSettings = new(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        CharacterModelSettings anthropicSettings = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelBinding openAi = Binding(openAiSettings, firstModel);
        CharacterModelBinding anthropic = Binding(anthropicSettings, secondModel);
        CharacterModelBindingSnapshot bindings = new([openAi, anthropic]);
        CharacterModelSettingsSelectionSnapshot selection = new(
            [anthropicSettings, openAiSettings],
            CharacterModelProvider.OpenAi);
        CharacterModelBinding mismatchedOpenAi = Binding(
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "gpt-test"),
            firstModel);
        if (modelSource.TryInitializeBindings(
                selection,
                new CharacterModelBindingSnapshot([mismatchedOpenAi]))
            || !modelSource.TryInitializeBindings(selection, bindings)
            || modelSource.TryInitializeBindings(selection, bindings)
            || modelSource.TrySelectProvider(CharacterModelProvider.Ollama)
            || !ReferenceEquals(modelSource.TakeModel(), firstModel)
            || !ReferenceEquals(modelSource.TakeModel(), firstModel))
        {
            Fail("The model-switch smoke could not retain its exact initial provider binding.");
            return;
        }

        flow = GetNode<CharacterModelGenerationFlowController>(
            "../CharacterModelGenerationFlowController");
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
            Fail("The ordinary model switch did not resolve within its bound.");
            return;
        }

        switch (stage)
        {
            case 0:
                CaptureInitialStateAndSubmit();
                stage = 1;
                break;
            case 1:
                CaptureFirstRequest();
                break;
            case 2:
                ObserveFirstCompletionAndSwitch();
                break;
            case 3:
                CaptureSecondRequest();
                break;
            default:
                ObserveSecondCompletion();
                break;
        }
    }

    private void CaptureInitialStateAndSubmit()
    {
        initialEventLog = worldEventController.EventLog;
        initialWorldTime = worldEventController.WorldTime;
        initialWorldObjects = worldObjectController.WorldObjects;
        initialPerception = searchController.Perception;
        Submit(FirstPlayerText);
    }

    private void CaptureFirstRequest()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? utterance = dialogueInputController.LastSubmittedUtterance;
        if (activity is null
            || utterance?.Text != FirstPlayerText
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
        {
            return;
        }

        firstRequest = activity.Request;
        stage = 2;
    }

    private void ObserveFirstCompletionAndSwitch()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        CharacterModelStreamSnapshot? result = flow.Result;
        if (activity is null
            || result is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed))
        {
            return;
        }

        CharacterModelStreamTermination? termination = result.Updates[^1]
            as CharacterModelStreamTermination;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (firstModel.CallCount != 1
            || secondModel.CallCount != 0
            || !ReferenceEquals(firstModel.ReceivedRequest?.TurnRequest, firstRequest)
            || !HasCurrentUntrustedDialogue(firstModel.ReceivedRequest, FirstPlayerText)
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Response.Speech.Text: FirstSpeechText,
            }
            || !ReferenceEquals(activity.Response, termination.Response)
            || transcript?.Entries.Count != 2
            || transcript.Entries.Count(entry =>
                ReferenceEquals(entry.SpokenStatement, termination.Response.Speech)) != 1
            || transcript.Entries.Any(entry => entry.PhysicalAction is not null)
            || !HasUnchangedAuthoritativeState())
        {
            Fail("The first caller-composed model escaped its exact ordinary turn.");
            return;
        }

        if (!modelSource.TrySelectProvider(CharacterModelProvider.Anthropic)
            || !ReferenceEquals(modelSource.TakeModel(), secondModel))
        {
            Fail("The model-switch smoke could not select its second exact provider binding.");
            return;
        }

        firstResult = result;
        Submit(SecondPlayerText);
        stage = 3;
    }

    private void CaptureSecondRequest()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? utterance = dialogueInputController.LastSubmittedUtterance;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (activity is null
            || utterance?.Text != SecondPlayerText
            || transcript?.Entries.Count != 3
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(activity.Request.DialogueEvent.Utterance, utterance)
            || !ReferenceEquals(transcript.Entries[^1].SpokenStatement, utterance))
        {
            return;
        }

        secondRequest = activity.Request;
        if (ReferenceEquals(secondRequest, firstRequest))
        {
            Fail("The later ordinary turn reused the first turn request instance.");
            return;
        }

        stage = 4;
    }

    private void ObserveSecondCompletion()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        CharacterModelStreamSnapshot? result = flow.Result;
        Label response = GetNode<Label>("../DialogueOverlay/Panel/Response");
        if (activity is null
            || result is null
            || ReferenceEquals(result, firstResult)
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            || response.Text != SecondSpeechText)
        {
            return;
        }

        CharacterModelStreamTermination? termination = result.Updates[^1]
            as CharacterModelStreamTermination;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        TemporaryCharacterModelStreamTextLabel text =
            GetNode<TemporaryCharacterModelStreamTextLabel>(
                "../DialogueOverlay/Panel/ModelStreamText");
        if (firstModel.CallCount != 1
            || secondModel.CallCount != 1
            || !ReferenceEquals(firstModel.ReceivedRequest?.TurnRequest, firstRequest)
            || !ReferenceEquals(secondModel.ReceivedRequest?.TurnRequest, secondRequest)
            || ReferenceEquals(firstModel.ReceivedRequest, secondModel.ReceivedRequest)
            || !HasCurrentUntrustedDialogue(secondModel.ReceivedRequest, SecondPlayerText)
            || result.Updates.Count != 2
            || result.Updates[0] is not CharacterModelTextDelta { Text: SecondSpeechText }
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Response.Speech.Text: SecondSpeechText,
            }
            || !ReferenceEquals(activity.Response, termination.Response)
            || transcript?.Entries.Count != 4
            || transcript.Entries.Count(entry => entry.SpokenStatement?.Text == FirstSpeechText) != 1
            || transcript.Entries.Count(entry => entry.SpokenStatement?.Text == SecondSpeechText) != 1
            || transcript.Entries.Any(entry => entry.PhysicalAction is not null)
            || !text.Visible
            || text.Text != SecondSpeechText
            || examinationController.LastExaminationResult is not null
            || examinationController.LastCollectionOccurrence is not null
            || !HasUnchangedAuthoritativeState())
        {
            Fail("The second caller-composed model crossed an ordinary turn boundary.");
            return;
        }

        SetProcess(false);
        GD.Print(
            "Ordinary character-model provider-switch smoke passed: exact caller-composed "
            + "OpenAI and Anthropic bindings served distinct consecutive requests, unavailable "
            + "Ollama selection did not substitute a model, each speech occurred once in order, "
            + "and no presentation, action, or authoritative state crossed turn boundaries.");
        GetTree().Quit();
    }

    private static CharacterModelBinding Binding(
        CharacterModelSettings settings,
        ICharacterModel model) =>
        new(
            settings.Provider,
            settings,
            model,
            new UnusedConnectionTester());

    private static bool HasCurrentUntrustedDialogue(
        CharacterModelGenerationRequest? request,
        string expectedText) =>
        request?.Context.Entries.Count(entry =>
            entry.Kind == CharacterModelContextEntryKind.UntrustedPlayerDialogue
            && entry.Text == expectedText) == 1;

    private bool HasUnchangedAuthoritativeState() =>
        ReferenceEquals(worldEventController.EventLog, initialEventLog)
        && ReferenceEquals(worldEventController.WorldTime, initialWorldTime)
        && ReferenceEquals(worldObjectController.WorldObjects, initialWorldObjects)
        && ReferenceEquals(searchController.Perception, initialPerception);

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

    private sealed class ScriptedWaitModel(string speechText) : ICharacterModel
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
                new DialogueUtterance(request.TurnRequest.RespondingCharacterId, speechText),
                new CharacterIntention(CharacterIntentionType.Wait, null),
                [],
                []);
            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(speechText);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                response,
                null);
        }
    }

    private sealed class UnusedConnectionTester : ICharacterModelConnectionTester
    {
        public ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
            CharacterModelSettings settings,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException(
                "Ordinary provider selection must not invoke a connection test.");
    }
}
