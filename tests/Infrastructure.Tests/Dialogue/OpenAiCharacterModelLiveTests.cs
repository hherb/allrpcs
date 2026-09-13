using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterModelLiveTests
{
    private const string LiveApiBaseEnvironmentVariable = "ALLRPCS_LIVE_OPENAI_API_BASE";
    private const string LiveActionConformanceEnvironmentVariable =
        "ALLRPCS_LIVE_OPENAI_ACTION_CONFORMANCE";
    private const string LiveConformanceEnvironmentVariable = "ALLRPCS_LIVE_OPENAI_CONFORMANCE";
    private const string LiveModelEnvironmentVariable = "ALLRPCS_LIVE_OPENAI_MODEL";

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
    public async Task ReachesAnExplicitlyEnabledApiAndAcceptsOnlyLocallyValidContent()
    {
        string? apiBase = Environment.GetEnvironmentVariable(LiveApiBaseEnvironmentVariable);
        string? modelName = Environment.GetEnvironmentVariable(LiveModelEnvironmentVariable);
        if (string.IsNullOrEmpty(apiBase) || string.IsNullOrEmpty(modelName))
        {
            return;
        }

        OpenAiEndpoint endpoint = new(new Uri(apiBase, UriKind.Absolute));
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, modelName);
        LocalCredentialSource credentialSource = new();
        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(3),
        };
        CharacterModelBinding binding = OpenAiCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource,
            endpoint);
        using CancellationTokenSource cancellation = new(TimeSpan.FromMinutes(2));

        CharacterModelGenerationRequest request = CreateRequest();
        List<CharacterModelStreamUpdate> updates = [];
        await foreach (CharacterModelStreamUpdate update in
                       binding.Model.StreamResponseAsync(request, cancellation.Token))
        {
            updates.Add(update);
        }

        CharacterModelStreamTermination termination =
            Assert.IsType<CharacterModelStreamTermination>(updates[^1]);
        Assert.True(
            termination.Status == CharacterModelStreamTerminationStatus.Completed
                || termination.Failure?.Code == CharacterModelFailureCode.InvalidResponse,
            $"Unexpected live OpenAI-compatible outcome: {termination.Failure?.Code}.");
        if (termination.Status == CharacterModelStreamTerminationStatus.Completed)
        {
            Assert.Same(request.TurnRequest, termination.Response?.Request);
            Assert.Same(
                CharacterIntentionType.Wait,
                termination.Response?.ImmediateIntention.Type);
            Assert.Null(termination.Response?.ImmediateIntention.TargetObjectId);
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

        string? apiBase = Environment.GetEnvironmentVariable(LiveApiBaseEnvironmentVariable);
        string? modelName = Environment.GetEnvironmentVariable(LiveModelEnvironmentVariable);
        Assert.False(
            string.IsNullOrEmpty(apiBase) || string.IsNullOrEmpty(modelName),
            "Live conformance requires an explicitly selected API base and model.");

        OpenAiEndpoint endpoint = new(new Uri(apiBase!, UriKind.Absolute));
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, modelName!);
        LocalCredentialSource credentialSource = new();
        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(3),
        };
        CharacterModelBinding binding = OpenAiCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource,
            endpoint);
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

        string? apiBase = Environment.GetEnvironmentVariable(LiveApiBaseEnvironmentVariable);
        string? modelName = Environment.GetEnvironmentVariable(LiveModelEnvironmentVariable);
        Assert.False(
            string.IsNullOrEmpty(apiBase) || string.IsNullOrEmpty(modelName),
            "Live action conformance requires an explicitly selected API base and model.");

        OpenAiEndpoint endpoint = new(new Uri(apiBase!, UriKind.Absolute));
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, modelName!);
        LocalCredentialSource credentialSource = new();
        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(3),
        };
        CharacterModelBinding binding = OpenAiCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource,
            endpoint);
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
                        playerText,
                        DialogueDeliveryMode.OrdinarySpeech),
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
                        request.TurnRequest.DialogueEvent.Utterance.Text,
                        request.TurnRequest.DialogueEvent.Delivery.Mode),
                ]));
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
