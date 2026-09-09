using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterModelLiveTests
{
    private const string LiveModelEnvironmentVariable = "ALLRPCS_LIVE_OLLAMA_MODEL";

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
}
