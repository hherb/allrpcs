using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Dialogue;
using Godot;
using NetHttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>
/// Proves one explicitly enabled live provider response through the ordinary healer flow.
/// </summary>
public partial class LiveCharacterModelOrdinaryFlowSmokeCheck : Node
{
    private const string EnabledEnvironmentValue = "1";
    private const string LiveOllamaCheckEnvironmentVariable =
        "ALLRPCS_LIVE_OLLAMA_ORDINARY_ACTION";
    private const string LiveOllamaWaitCheckEnvironmentVariable =
        "ALLRPCS_LIVE_OLLAMA_ORDINARY_WAIT";
    private const string LiveOllamaModelEnvironmentVariable = "ALLRPCS_LIVE_OLLAMA_MODEL";
    private const string LiveOpenAiApiBaseEnvironmentVariable = "ALLRPCS_LIVE_OPENAI_API_BASE";
    private const string LiveOpenAiCheckEnvironmentVariable =
        "ALLRPCS_LIVE_OPENAI_ORDINARY_ACTION";
    private const string LiveOpenAiWaitCheckEnvironmentVariable =
        "ALLRPCS_LIVE_OPENAI_ORDINARY_WAIT";
    private const string LiveOpenAiModelEnvironmentVariable = "ALLRPCS_LIVE_OPENAI_MODEL";
    private const string ActionPlayerText = "Please examine the herb patch.";
    private const string WaitPlayerText = "Please wait here for a moment.";
    private const ulong MaximumElapsedMilliseconds = 120_000;

    private NetHttpClient? client;
    private RequestCapturingModel? model;
    private CharacterModelGenerationFlowController modelFlow = null!;
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryCharacterExaminationFlowController examinationController = null!;
    private WorldEventController worldEventController = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController searchController = null!;
    private LineEdit input = null!;
    private object? initialEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;
    private ulong startedAtMilliseconds;
    private string playerText = null!;
    private bool actionMode;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool ollamaActionEnabled =
            OS.GetEnvironment(LiveOllamaCheckEnvironmentVariable) == EnabledEnvironmentValue;
        // This legacy fixture explicitly exercises the conditional collection scaffold.
        GetParent<Main>().ConditionalCollectionScaffolding = OS.GetEnvironment(LiveOllamaCheckEnvironmentVariable) == EnabledEnvironmentValue
                || OS.GetEnvironment(LiveOpenAiCheckEnvironmentVariable) == EnabledEnvironmentValue;
        bool ollamaWaitEnabled =
            OS.GetEnvironment(LiveOllamaWaitCheckEnvironmentVariable) == EnabledEnvironmentValue;
        bool openAiActionEnabled =
            OS.GetEnvironment(LiveOpenAiCheckEnvironmentVariable) == EnabledEnvironmentValue;
        bool openAiWaitEnabled =
            OS.GetEnvironment(LiveOpenAiWaitCheckEnvironmentVariable) == EnabledEnvironmentValue;
        int enabledCount = Convert.ToInt32(ollamaActionEnabled)
            + Convert.ToInt32(ollamaWaitEnabled)
            + Convert.ToInt32(openAiActionEnabled)
            + Convert.ToInt32(openAiWaitEnabled);
        SetProcess(enabledCount > 0);
        if (enabledCount == 0)
        {
            return;
        }

        if (enabledCount != 1)
        {
            Fail("Live ordinary flow requires exactly one explicitly enabled provider and mode.");
            return;
        }

        bool ollamaEnabled = ollamaActionEnabled || ollamaWaitEnabled;
        actionMode = ollamaActionEnabled || openAiActionEnabled;
        playerText = actionMode ? ActionPlayerText : WaitPlayerText;
        client = new NetHttpClient
        {
            Timeout = TimeSpan.FromMinutes(3),
        };
        ICharacterModel? selectedModel = ollamaEnabled
            ? CreateOllamaModel()
            : CreateOpenAiModel();
        if (selectedModel is null)
        {
            return;
        }

        model = new RequestCapturingModel(selectedModel);
        if (!GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource")
                .TrySupplyModel(model))
        {
            Fail("Live ordinary flow could not supply its exact model.");
            return;
        }

        modelFlow = GetNode<CharacterModelGenerationFlowController>(
            "../CharacterModelGenerationFlowController");
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        examinationController = GetNode<TemporaryCharacterExaminationFlowController>(
            "../TemporaryCharacterExaminationFlowController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        searchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
        startedAtMilliseconds = Time.GetTicksMsec();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (Time.GetTicksMsec() - startedAtMilliseconds > MaximumElapsedMilliseconds)
        {
            Fail("Live ordinary flow did not resolve within its bound.");
            return;
        }

        if (stage == 0)
        {
            CaptureInitialStateAndSubmit();
            stage = 1;
            return;
        }

        if (actionMode && examinationController.LastCollectionOccurrence is not null)
        {
            VerifyCompletedActionFlow();
            return;
        }

        if (!actionMode
            && ReferenceEquals(
                turnController.Activity?.State,
                CharacterTurnActivityState.Completed))
        {
            VerifyCompletedWaitFlow();
            return;
        }

        if (modelFlow.Result?.Updates[^1] is CharacterModelStreamTermination termination
            && termination.Status != CharacterModelStreamTerminationStatus.Completed)
        {
            Fail("Live ordinary flow returned a non-completed bounded outcome.");
        }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        client?.Dispose();
    }

    private void CaptureInitialStateAndSubmit()
    {
        initialEventLog = worldEventController.EventLog;
        initialWorldTime = worldEventController.WorldTime;
        initialWorldObjects = worldObjectController.WorldObjects;
        initialPerception = searchController.Perception;
        input.Text = playerText;
        input.EmitSignal(LineEdit.SignalName.TextSubmitted, playerText);
    }

    private void VerifyCompletedActionFlow()
    {
        SetProcess(false);
        CharacterModelGenerationRequest? request = model?.ReceivedRequest;
        CharacterModelStreamTermination? termination = modelFlow.Result?.Updates[^1]
            as CharacterModelStreamTermination;
        CharacterTurnResponse? response = termination?.Response;
        CharacterTurnActivity? activity = turnController.Activity;
        CharacterExaminationResult? result = examinationController.LastExaminationResult;
        CharacterTurnWorldEventOccurrence collection =
            examinationController.LastCollectionOccurrence!;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        ObjectId authoritativeTarget = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id;
        ObjectId? proposedTarget = response?.ImmediateIntention.TargetObjectId;
        CharacterModelContextEntry[] affordances = request?.Context.Entries
            .Where(entry => entry.Kind == CharacterModelContextEntryKind.ValidAffordance)
            .ToArray() ?? [];

        if (model?.CallCount != 1
            || request is null
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Failure: null,
            }
            || response is null
            || activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            || !ReferenceEquals(activity.Request, request.TurnRequest)
            || !ReferenceEquals(activity.Response, response)
            || !ReferenceEquals(response.Request, request.TurnRequest)
            || response.Speech is null
            || !ReferenceEquals(
                response.Speech.SpeakerCharacterId,
                request.TurnRequest.RespondingCharacterId)
            || !ReferenceEquals(
                response.ImmediateIntention.Type,
                CharacterIntentionType.ExamineObject)
            || proposedTarget != authoritativeTarget
            || ReferenceEquals(proposedTarget, authoritativeTarget)
            || response.Commitments.Count != 0
            || response.StateProposals.Count != 0
            || result is null
            || !ReferenceEquals(result.Safety, CharacterExaminationSafety.SafeToCollect)
            || !ReferenceEquals(result.TargetObjectId, proposedTarget)
            || !ReferenceEquals(
                result.Occurrence.WorldEvent,
                worldEventSource.HerbPatchExaminedWorldEvent)
            || !ReferenceEquals(collection.WorldEvent, worldEventSource.HerbPatchCollectedWorldEvent)
            || transcript is not { Entries.Count: 4 }
            || transcript.Entries[0].SpokenStatement?.Text != playerText
            || !ReferenceEquals(transcript.Entries[1].SpokenStatement, response.Speech)
            || !ReferenceEquals(transcript.Entries[2], result.Occurrence.TranscriptEntry)
            || !ReferenceEquals(transcript.Entries[3], collection.TranscriptEntry)
            || eventLog is not { Entries.Count: 3 }
            || !ReferenceEquals(eventLog.Entries[1], result.Occurrence.WorldEvent.EventId)
            || !ReferenceEquals(eventLog.Entries[2], collection.WorldEvent.EventId)
            || !ReferenceEquals(
                worldEventController.WorldTime?.CurrentEventId,
                collection.WorldEvent.EventId)
            || ReferenceEquals(initialEventLog, eventLog)
            || ReferenceEquals(initialWorldTime, worldEventController.WorldTime)
            || !ReferenceEquals(initialWorldObjects, worldObjectController.WorldObjects)
            || !ReferenceEquals(initialPerception, searchController.Perception)
            || affordances.Length != 1
            || affordances[0].Text != "type=examine-object;target=herb-patch"
            || request.Context.Entries.Any(
                entry => entry.Text.Contains("stone", StringComparison.OrdinalIgnoreCase)
                    || entry.Text.Contains("secret", StringComparison.OrdinalIgnoreCase)))
        {
            Fail("Live ordinary action escaped its bounded scene contract.");
            return;
        }

        GD.Print(
            "Live ordinary character-model action smoke passed: one explicitly selected model "
            + "completed the validated herb-patch examination and safe collection flow without changing "
            + "world objects or perception.");
        GetTree().Quit();
    }

    private void VerifyCompletedWaitFlow()
    {
        SetProcess(false);
        CharacterModelGenerationRequest? request = model?.ReceivedRequest;
        CharacterModelStreamTermination? termination = modelFlow.Result?.Updates[^1]
            as CharacterModelStreamTermination;
        CharacterTurnResponse? response = termination?.Response;
        CharacterTurnActivity? activity = turnController.Activity;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        CharacterModelContextEntry[] affordances = request?.Context.Entries
            .Where(entry => entry.Kind == CharacterModelContextEntryKind.ValidAffordance)
            .ToArray() ?? [];

        if (model?.CallCount != 1
            || request is null
            || termination is not
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Failure: null,
            }
            || response is null
            || activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            || !ReferenceEquals(activity.Request, request.TurnRequest)
            || !ReferenceEquals(activity.Response, response)
            || !ReferenceEquals(response.Request, request.TurnRequest)
            || response.Speech is null
            || !ReferenceEquals(
                response.Speech.SpeakerCharacterId,
                request.TurnRequest.RespondingCharacterId)
            || !ReferenceEquals(response.ImmediateIntention.Type, CharacterIntentionType.Wait)
            || response.ImmediateIntention.TargetObjectId is not null
            || response.Commitments.Count != 0
            || response.StateProposals.Count != 0
            || transcript is not { Entries.Count: 2 }
            || transcript.Entries[0].SpokenStatement?.Text != playerText
            || !ReferenceEquals(transcript.Entries[1].SpokenStatement, response.Speech)
            || transcript.Entries.Any(entry => entry.PhysicalAction is not null)
            || !ReferenceEquals(initialEventLog, worldEventController.EventLog)
            || !ReferenceEquals(initialWorldTime, worldEventController.WorldTime)
            || !ReferenceEquals(initialWorldObjects, worldObjectController.WorldObjects)
            || !ReferenceEquals(initialPerception, searchController.Perception)
            || affordances.Length != 1
            || affordances[0].Text != "type=examine-object;target=herb-patch"
            || request.Context.Entries.Any(
                entry => entry.Text.Contains("stone", StringComparison.OrdinalIgnoreCase)
                    || entry.Text.Contains("secret", StringComparison.OrdinalIgnoreCase)))
        {
            Fail("Live ordinary wait escaped its bounded scene contract.");
            return;
        }

        GD.Print(
            "Live ordinary character-model wait smoke passed: one explicitly selected model "
            + "completed bounded healer speech without physical action or authoritative mutation.");
        GetTree().Quit();
    }

    private ICharacterModel? CreateOllamaModel()
    {
        string modelName = OS.GetEnvironment(LiveOllamaModelEnvironmentVariable);
        if (string.IsNullOrEmpty(modelName))
        {
            Fail("Live Ollama ordinary flow requires an explicitly selected model.");
            return null;
        }

        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, modelName);
        return OllamaCharacterModelBindingFactory.Create(
            settings,
            client!,
            OllamaEndpoint.Default).Model;
    }

    private ICharacterModel? CreateOpenAiModel()
    {
        string apiBase = OS.GetEnvironment(LiveOpenAiApiBaseEnvironmentVariable);
        string modelName = OS.GetEnvironment(LiveOpenAiModelEnvironmentVariable);
        if (string.IsNullOrEmpty(apiBase) || string.IsNullOrEmpty(modelName))
        {
            Fail(
                "Live OpenAI-compatible ordinary flow requires an explicitly selected API "
                + "base and model.");
            return null;
        }

        OpenAiEndpoint endpoint = new(new Uri(apiBase, UriKind.Absolute));
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, modelName);
        return OpenAiCharacterModelBindingFactory.Create(
            settings,
            client!,
            new LocalCredentialSource(),
            endpoint).Model;
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class RequestCapturingModel : ICharacterModel
    {
        private readonly ICharacterModel inner;

        internal RequestCapturingModel(ICharacterModel inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        internal int CallCount { get; private set; }

        internal CharacterModelGenerationRequest? ReceivedRequest { get; private set; }

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            ReceivedRequest = request;
            await foreach (CharacterModelStreamUpdate update in
                           inner.StreamResponseAsync(request, cancellationToken))
            {
                yield return update;
            }
        }
    }

    private sealed class LocalCredentialSource : IOpenAiCredentialSource
    {
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(OpenAiCredentialLoadResult.Loaded("local-test"));
        }
    }
}
