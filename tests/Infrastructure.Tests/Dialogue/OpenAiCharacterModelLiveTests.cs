using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterModelLiveTests
{
    private const string LiveApiBaseEnvironmentVariable = "ALLRPCS_LIVE_OPENAI_API_BASE";
    private const string LiveModelEnvironmentVariable = "ALLRPCS_LIVE_OPENAI_MODEL";

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

    private static CharacterModelGenerationRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        const string playerText = "Please wait here for a moment.";
        DialogueUtterance utterance = new(seekerId, playerText);
        DialogueEvent dialogueEvent = new(
            utterance,
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
                        "type=wait;target=none"),
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                        playerText),
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
