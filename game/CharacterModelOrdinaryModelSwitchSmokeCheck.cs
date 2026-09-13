using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
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
    private const string RestoredCommitmentText =
        "kind=promise;counterparty=seeker;source-turn=turn-one;"
        + "description=Inspect the moonwort before collection.";
    private const int MaximumProcessFrames = 240;

    private readonly ScriptedWaitModel firstModel = new(FirstSpeechText, waitsForRelease: true);
    private readonly ScriptedWaitModel secondModel = new(SecondSpeechText);
    private CharacterModelSettings anthropicSettings = null!;
    private CharacterModelBinding anthropicBinding = null!;
    private TemporaryCharacterModelSource modelSource = null!;
    private CharacterModelGenerationFlowController flow = null!;
    private CharacterMemoryController memoryController = null!;
    private DialogueInputController dialogueInputController = null!;
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryCharacterExaminationFlowController examinationController = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController searchController = null!;
    private SaveSnapshotJsonController saveController = null!;
    private LineEdit inputControl = null!;
    private int processFrames;
    private int stage;
    private object? initialEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;
    private CharacterMemorySnapshot? initialMemory;
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
        anthropicSettings = new CharacterModelSettings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelBinding openAi = Binding(openAiSettings, firstModel);
        anthropicBinding = Binding(anthropicSettings, secondModel);
        CharacterModelBindingSnapshot bindings = new([openAi, anthropicBinding]);
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
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
        saveController = GetNode<SaveSnapshotJsonController>("../SaveSnapshotJsonController");
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
        CharacterMemorySnapshot promised =
            CharacterMemoryControllerSmokeCheck.CreateMemory("healer");
        memoryController.Install(promised);
        string? serialized = saveController.CaptureSerialized();
        memoryController.Install(new CharacterMemorySnapshot([]));
        if (serialized is null
            || !saveController.TryRestoreSerialized(serialized)
            || ReferenceEquals(memoryController.Memory, promised)
            || !HasCanonicalPromise(memoryController.Memory))
        {
            Fail("The model-switch smoke could not restore its canonical healer promise.");
            return;
        }

        initialEventLog = worldEventController.EventLog;
        initialWorldTime = worldEventController.WorldTime;
        initialWorldObjects = worldObjectController.WorldObjects;
        initialPerception = searchController.Perception;
        initialMemory = memoryController.Memory;
        Submit(FirstPlayerText);
    }

    private void CaptureFirstRequest()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? utterance = dialogueInputController.LastSubmittedUtterance;
        if (activity is null
            || utterance?.Text != FirstPlayerText
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || firstModel.CallCount == 0)
        {
            return;
        }

        firstRequest = activity.Request;
        CharacterModelSettingsSelectionSnapshot replacementSelection = new(
            [anthropicSettings],
            CharacterModelProvider.Anthropic);
        if (!modelSource.TryReplaceBindings(
                replacementSelection,
                new CharacterModelBindingSnapshot([anthropicBinding]))
            || secondModel.CallCount != 0)
        {
            Fail("The active-turn model replacement was not retained only for a future request.");
            return;
        }

        firstModel.Release();
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
            || !HasRestoredCommitment(firstModel.ReceivedRequest)
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
            || !HasRestoredCommitment(secondModel.ReceivedRequest)
            || !HasMatchingMemoryAcrossProviderSwitch()
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
            + "OpenAI and Anthropic bindings served distinct consecutive requests, active-turn "
            + "replacement affected only the future request, one serialized and restored healer "
            + "promise retained its source in both bounded requests without the campaign "
            + "transcript, unavailable Ollama selection did not substitute a model, and no state "
            + "crossed turn boundaries.");
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

    private static bool HasRestoredCommitment(CharacterModelGenerationRequest? request) =>
        request?.Context.Entries.Count(entry =>
            entry.Kind == CharacterModelContextEntryKind.Memory
            && entry.Text == RestoredCommitmentText) == 1;

    private bool HasMatchingMemoryAcrossProviderSwitch()
    {
        CharacterModelContextEntry? firstMemory = firstModel.ReceivedRequest?.Context.Entries.Single(
            entry => entry.Kind == CharacterModelContextEntryKind.Memory);
        CharacterModelContextEntry? secondMemory = secondModel.ReceivedRequest?.Context.Entries.Single(
            entry => entry.Kind == CharacterModelContextEntryKind.Memory);
        return firstMemory is not null
            && secondMemory is not null
            && firstMemory.Text == secondMemory.Text
            && !ReferenceEquals(firstMemory, secondMemory);
    }

    private static bool HasCanonicalPromise(CharacterMemorySnapshot memory)
    {
        CharacterMemoryAggregate aggregate = memory.Characters.Single();
        CharacterCommitmentMemory promise = aggregate.Commitments.Commitments.Single();
        return aggregate.CharacterId.Value == "healer"
            && ReferenceEquals(promise.Kind, CharacterCommitmentKind.Promise)
            && promise.CounterpartyCharacterId.Value == "seeker"
            && promise.SourceTurnId.Value == "turn-one"
            && promise.Description == "Inspect the moonwort before collection.";
    }

    private bool HasUnchangedAuthoritativeState() =>
        ReferenceEquals(worldEventController.EventLog, initialEventLog)
        && ReferenceEquals(worldEventController.WorldTime, initialWorldTime)
        && ReferenceEquals(worldObjectController.WorldObjects, initialWorldObjects)
        && ReferenceEquals(searchController.Perception, initialPerception)
        && ReferenceEquals(memoryController.Memory, initialMemory);

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

    private sealed class ScriptedWaitModel(
        string speechText,
        bool waitsForRelease = false) : ICharacterModel
    {
        private readonly TaskCompletionSource release = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }

        public CharacterModelGenerationRequest? ReceivedRequest { get; private set; }

        public void Release() => release.TrySetResult();

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            ReceivedRequest = request;

            if (waitsForRelease)
            {
                await release.Task.WaitAsync(cancellationToken);
            }

            CharacterTurnResponse response = new(
                request.TurnRequest,
                CharacterTurnDisposition.Accept,
                new DialogueDelivery(
                    new DialogueUtterance(request.TurnRequest.RespondingCharacterId, speechText),
                    DialogueDeliveryMode.OrdinarySpeech),
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
