using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;
using Allrpcs.Infrastructure.Tests.Characters;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class ProviderDecisionDeadlineTests
{
    private static readonly bool[] Flags = [false, true];
    private static readonly string[] Names = ["CharacterTargetSabotageDelayModel",
        "CharacterObjectSabotageSubstitutionModel",
        "CharacterObjectSabotageMoveModel",
        "CharacterDialogueMisquotationEvidenceModel",
        "CharacterSabotageActionModel",
        "CharacterTargetSabotageInfluenceModel",
        "CharacterObjectSabotageContaminationModel",
        "CharacterTargetSabotageInfluenceRecoveryModel",
        "CharacterTargetSabotageDelayRecoveryModel",
        "CharacterObjectSabotageConcealmentModel",
        "OpenAiParticipation",
        "OllamaParticipation"];

    public static IEnumerable<object[]> Cases =>
        from name in Names
        from headers in Flags
        from cancel in Flags
        select new object[] { name, headers, cancel };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task DecisionConsumersBoundHeadersAndBodyAndPreserveCallerCancellation(
        string name, bool headers, bool cancel)
    {
        using ProviderDeadlineTestTransport transport = new()
        {
            StallHeaders = headers,
            MediaType = name == "OllamaParticipation" ? "application/x-ndjson" : "text/event-stream"
        };
        using HttpClient client = new(transport);
        using CancellationTokenSource caller = new();
        Credential credential = new();
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "model");
        Func<Task> invoke = name switch
        {
            "CharacterTargetSabotageDelayModel" => async () =>
            {
                _ = await CreateCharacterTargetSabotageDelay(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request, caller.Token);
            }
            ,
            "CharacterObjectSabotageSubstitutionModel" => async () =>
            {
                _ = await CreateCharacterObjectSabotageSubstitution(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterObjectSabotageSubstitutionOpenAiRequestTests.CreateRequest(), caller.Token);
            }
            ,
            "CharacterObjectSabotageMoveModel" => async () =>
            {
                _ = await CreateCharacterObjectSabotageMove(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterObjectSabotageMoveOpenAiRequestTests.CreateRequest(), caller.Token);
            }
            ,
            "CharacterDialogueMisquotationEvidenceModel" => async () =>
            {
                _ = await CreateCharacterDialogueMisquotationEvidence(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterDialogueMisquotationEvidenceOpenAiRequestTests.CreateRequest(), caller.Token);
            }
            ,
            "CharacterSabotageActionModel" => async () =>
            {
                _ = await CreateCharacterSabotageAction(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput().Request, caller.Token);
            }
            ,
            "CharacterTargetSabotageInfluenceModel" => async () =>
            {
                _ = await CreateCharacterTargetSabotageInfluence(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterTargetSabotageInfluenceOpenAiRequestTests.CreateRequest(), caller.Token);
            }
            ,
            "CharacterObjectSabotageContaminationModel" => async () =>
            {
                _ = await CreateCharacterObjectSabotageContamination(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterObjectSabotageContaminationOpenAiRequestTests.CreateRequest(), caller.Token);
            }
            ,
            "CharacterTargetSabotageInfluenceRecoveryModel" => async () =>
            {
                _ = await CreateCharacterTargetSabotageInfluenceRecovery(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create().Request, caller.Token);
            }
            ,
            "CharacterTargetSabotageDelayRecoveryModel" => async () =>
            {
                _ = await CreateCharacterTargetSabotageDelayRecovery(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create().Request, caller.Token);
            }
            ,
            "CharacterObjectSabotageConcealmentModel" => async () =>
            {
                _ = await CreateCharacterObjectSabotageConcealment(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterObjectSabotageConcealmentOpenAiRequestTests.CreateRequest(), caller.Token);
            }
            ,
            "OpenAiParticipation" => async () =>
            {
                _ = await new OpenAiCharacterTurnParticipationModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options).DecideAsync(
                OpenAiCharacterTurnParticipationStreamAccumulatorTests.CreateRequest(true), caller.Token);
            }
            ,
            "OllamaParticipation" => async () =>
            {
                _ = await new OllamaCharacterTurnParticipationModel(
                client, new CharacterModelSettings(CharacterModelProvider.Ollama, "model"),
                OllamaEndpoint.Default, transport.Options).DecideAsync(
                OpenAiCharacterTurnParticipationStreamAccumulatorTests.CreateRequest(true), caller.Token);
            }
            ,
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };
        Task task = invoke();
        await transport.ExpireAsync(cancel, caller);
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        else await Assert.ThrowsAsync<IOException>(() => task);
        Assert.Equal(cancel, caller.IsCancellationRequested);
        Assert.False(transport.Disposed);
        if (!headers) Assert.True(transport.Body.Disposed);
        Assert.Equal(0, transport.Clock.ActiveTimerCount);
    }

    private sealed class Credential : IOpenAiCredentialSource
    {
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(CancellationToken cancellationToken) =>
            ValueTask.FromResult(OpenAiCredentialLoadResult.Loaded("test-secret"));
    }
}
