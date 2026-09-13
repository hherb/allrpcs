using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterModelLiveTests
{
    private const string LiveActionConformanceEnvironmentVariable =
        "ALLRPCS_LIVE_OLLAMA_ACTION_CONFORMANCE";
    private const string LiveConformanceEnvironmentVariable = "ALLRPCS_LIVE_OLLAMA_CONFORMANCE";
    private const string LiveModelEnvironmentVariable = "ALLRPCS_LIVE_OLLAMA_MODEL";

    private static readonly string[] ConformancePlayerTexts =
    [
        "Please wait here for a moment.",
        "Stay here while I check the path.",
        "Do not go anywhere yet.",
    ];

    private static readonly string[] ActionConformancePlayerTexts =
    [
        "Please examine the herb patch.",
        "Could you inspect those healing herbs?",
        "Take a close look at the herb patch for me.",
    ];

    [Fact]
    public async Task ReachesAnExplicitlyEnabledModelAndAcceptsOnlyLocallyValidContent()
    {
        string? modelName = Environment.GetEnvironmentVariable(LiveModelEnvironmentVariable);
        if (string.IsNullOrEmpty(modelName))
        {
            return;
        }

        CharacterModelGenerationRequest request = CreateRequest();
        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(3),
        };
        OllamaCharacterModel model = new(
            client,
            new CharacterModelSettings(CharacterModelProvider.Ollama, modelName));
        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(45));

        OllamaCharacterModelConnectionTester tester = new(client);
        CharacterModelConnectionTestResult connection = await tester.TestConnectionAsync(
            new CharacterModelSettings(CharacterModelProvider.Ollama, modelName),
            cancellation.Token);
        Assert.True(connection.IsConnected, $"Live Ollama connection failed: {connection}.");

        List<CharacterModelStreamUpdate> updates = [];
        await foreach (CharacterModelStreamUpdate update in
                       model.StreamResponseAsync(request, cancellation.Token))
        {
            updates.Add(update);
        }

        CharacterModelStreamTermination termination =
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(updates));
        Assert.True(
            termination.Status == CharacterModelStreamTerminationStatus.Completed
                || termination.Failure?.Code == CharacterModelFailureCode.InvalidResponse,
            $"Unexpected live Ollama outcome: {termination.Failure?.Code}.");
        if (termination.Status == CharacterModelStreamTerminationStatus.Completed)
        {
            Assert.Same(request.TurnRequest, termination.Response?.Request);
        }
        else
        {
            Assert.Null(termination.Response);
        }
    }

    [Fact]
    public async Task ExplicitConformanceSeriesRequiresThreeCompletedSafeWaitResponses()
    {
        string? enabled = Environment.GetEnvironmentVariable(LiveConformanceEnvironmentVariable);
        if (!string.Equals(enabled, "1", StringComparison.Ordinal))
        {
            return;
        }

        string? modelName = Environment.GetEnvironmentVariable(LiveModelEnvironmentVariable);
        Assert.False(
            string.IsNullOrEmpty(modelName),
            "Live Ollama conformance requires an explicitly selected model.");

        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, modelName!);
        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(3),
        };
        CharacterModelBinding binding = OllamaCharacterModelBindingFactory.Create(
            settings,
            client,
            OllamaEndpoint.Default);
        List<CharacterModelLiveConformanceOutcome> outcomes = [];
        foreach (string playerText in ConformancePlayerTexts)
        {
            using CancellationTokenSource cancellation = new(TimeSpan.FromMinutes(2));
            CharacterModelGenerationRequest request = CreateRequest(playerText);
            int updateCount = 0;
            CharacterModelStreamTermination? termination = null;
            await foreach (CharacterModelStreamUpdate update in
                           binding.Model.StreamResponseAsync(request, cancellation.Token))
            {
                updateCount++;
                termination = update as CharacterModelStreamTermination;
            }

            outcomes.Add(CharacterModelLiveConformance.Evaluate(
                request,
                updateCount,
                termination));
        }

        CharacterModelLiveConformanceSummary summary =
            CharacterModelLiveConformanceSummary.Create(outcomes);
        Assert.True(summary.IsConforming, summary.ToString());
    }

    [Fact]
    public async Task ExplicitActionConformanceRequiresThreeValidatedKnownTargetAttempts()
    {
        string? enabled =
            Environment.GetEnvironmentVariable(LiveActionConformanceEnvironmentVariable);
        if (!string.Equals(enabled, "1", StringComparison.Ordinal))
        {
            return;
        }

        string? modelName = Environment.GetEnvironmentVariable(LiveModelEnvironmentVariable);
        Assert.False(
            string.IsNullOrEmpty(modelName),
            "Live Ollama action conformance requires an explicitly selected model.");

        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, modelName!);
        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(3),
        };
        CharacterModelBinding binding = OllamaCharacterModelBindingFactory.Create(
            settings,
            client,
            OllamaEndpoint.Default);
        ObjectId herbPatchId = new("herb-patch");
        WorldObject herbPatch = new(
            new WorldObjectIdentity(herbPatchId, "Herb patch"),
            new WorldObjectDescription("A visible patch of healing herbs."));
        WorldObjectSnapshot worldObjects = new([herbPatch]);
        CharacterIntentionAffordanceSnapshot affordances = new(
            [
                new CharacterIntentionAffordance(
                    CharacterIntentionType.ExamineObject,
                    herbPatchId),
            ]);
        List<CharacterModelLiveConformanceOutcome> outcomes = [];
        foreach (string playerText in ActionConformancePlayerTexts)
        {
            using CancellationTokenSource cancellation = new(TimeSpan.FromMinutes(2));
            CharacterModelGenerationRequest request = CreateActionRequest(playerText);
            int updateCount = 0;
            CharacterModelStreamTermination? termination = null;
            await foreach (CharacterModelStreamUpdate update in
                           binding.Model.StreamResponseAsync(request, cancellation.Token))
            {
                updateCount++;
                termination = update as CharacterModelStreamTermination;
            }

            outcomes.Add(CharacterModelLiveConformance.EvaluateTargetedAction(
                request,
                updateCount,
                termination,
                CharacterIntentionType.ExamineObject,
                herbPatchId,
                worldObjects,
                affordances));
        }

        CharacterModelLiveConformanceSummary summary =
            CharacterModelLiveConformanceSummary.Create(outcomes);
        Assert.True(summary.IsConforming, summary.ToString());
        Assert.Same(herbPatch, Assert.Single(worldObjects.Entries));
        Assert.Same(herbPatchId, Assert.Single(affordances.Entries).TargetObjectId);
    }

    private static CharacterModelGenerationRequest CreateRequest(
        string playerText = "Please wait here for a moment.")
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, playerText);
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(seekerId, [healerId]));
        CharacterTurnRequest turnRequest = new(
            healerId,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
        return new CharacterModelGenerationRequest(
            turnRequest,
            new CharacterModelContextSnapshot(
                [
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.ValidAffordance,
                        "type=wait;target=null"),
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                        playerText),
                ]));
    }

    private static CharacterModelGenerationRequest CreateActionRequest(string playerText)
    {
        CharacterModelGenerationRequest request = CreateRequest(playerText);
        return new CharacterModelGenerationRequest(
            request.TurnRequest,
            new CharacterModelContextSnapshot(
                [
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.FilteredPerception,
                        "A visible patch of healing herbs has id herb-patch."),
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.ValidAffordance,
                        "type=examine-object;target=herb-patch"),
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                        request.TurnRequest.DialogueEvent.Utterance.Text),
                ]));
    }
}
