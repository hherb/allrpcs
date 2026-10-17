using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;
using Allrpcs.Infrastructure.Tests.Characters;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class ProviderDecisionDeadlineTests
{
    private static readonly bool[] Flags = [false, true];
    private static readonly string[] Names = ["OpenAiCharacterTargetSabotageDelayModel",
        "OpenAiCharacterObjectSabotageSubstitutionModel",
        "OpenAiCharacterObjectSabotageMoveModel",
        "OpenAiCharacterDialogueMisquotationEvidenceModel",
        "OpenAiCharacterSabotageActionModel",
        "OpenAiCharacterTargetSabotageInfluenceModel",
        "OpenAiCharacterObjectSabotageContaminationModel",
        "OpenAiCharacterTargetSabotageInfluenceRecoveryModel",
        "OpenAiCharacterTargetSabotageDelayRecoveryModel",
        "OpenAiCharacterObjectSabotageConcealmentModel",
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
            "OpenAiCharacterTargetSabotageDelayModel" => async () =>
            {
                _ = await new OpenAiCharacterTargetSabotageDelayModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request, caller.Token);
            }
            ,
            "OpenAiCharacterObjectSabotageSubstitutionModel" => async () =>
            {
                _ = await new OpenAiCharacterObjectSabotageSubstitutionModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(OpenAiCharacterObjectSabotageSubstitutionRequestSerializerTests.CreateRequest(), caller.Token);
            }
            ,
            "OpenAiCharacterObjectSabotageMoveModel" => async () =>
            {
                _ = await new OpenAiCharacterObjectSabotageMoveModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(OpenAiCharacterObjectSabotageMoveRequestSerializerTests.CreateRequest(), caller.Token);
            }
            ,
            "OpenAiCharacterDialogueMisquotationEvidenceModel" => async () =>
            {
                _ = await new OpenAiCharacterDialogueMisquotationEvidenceModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(OpenAiCharacterDialogueMisquotationEvidenceRequestSerializerTests.CreateRequest(), caller.Token);
            }
            ,
            "OpenAiCharacterSabotageActionModel" => async () =>
            {
                _ = await new OpenAiCharacterSabotageActionModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput().Request, caller.Token);
            }
            ,
            "OpenAiCharacterTargetSabotageInfluenceModel" => async () =>
            {
                _ = await new OpenAiCharacterTargetSabotageInfluenceModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(OpenAiCharacterTargetSabotageInfluenceRequestSerializerTests.CreateRequest(), caller.Token);
            }
            ,
            "OpenAiCharacterObjectSabotageContaminationModel" => async () =>
            {
                _ = await new OpenAiCharacterObjectSabotageContaminationModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest(), caller.Token);
            }
            ,
            "OpenAiCharacterTargetSabotageInfluenceRecoveryModel" => async () =>
            {
                _ = await new OpenAiCharacterTargetSabotageInfluenceRecoveryModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create().Request, caller.Token);
            }
            ,
            "OpenAiCharacterTargetSabotageDelayRecoveryModel" => async () =>
            {
                _ = await new OpenAiCharacterTargetSabotageDelayRecoveryModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create().Request, caller.Token);
            }
            ,
            "OpenAiCharacterObjectSabotageConcealmentModel" => async () =>
            {
                _ = await new OpenAiCharacterObjectSabotageConcealmentModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options)
                .DecideAsync(OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest(), caller.Token);
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
