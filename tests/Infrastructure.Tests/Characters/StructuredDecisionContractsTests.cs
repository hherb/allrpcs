using System.Net;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class StructuredDecisionContractsTests
{
    private static readonly string[] CaseValues1 = ["DialogueMisquotationEvidence", "ObjectSabotageConcealment", "ObjectSabotageContamination", "ObjectSabotageMove", "ObjectSabotageSubstitution", "SabotageAction", "TargetSabotageDelay", "TargetSabotageDelayRecovery", "TargetSabotageInfluence", "TargetSabotageInfluenceRecovery"];
    private static readonly string[] CaseValues2 = ["openai", "anthropic", "ollama"];

    public static IEnumerable<object[]> Cases =>
        from kind in CaseValues1
        from provider in CaseValues2
        select new object[] { kind, provider };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task AllKindsPreserveExactSelectionsAndTrustedPayloadsAcrossProviders(string kind, string provider)
    {
        switch (kind)
        {
            case "DialogueMisquotationEvidence":
                {
                    var request = OpenAiCharacterDialogueMisquotationEvidenceRequestSerializerTests.CreateRequest();
                    string document = OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulatorTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.DialogueMisquotationEvidence, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedEvidence);
                    });
                    break;
                }
            case "ObjectSabotageConcealment":
                {
                    var request = OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest();
                    string document = OpenAiCharacterObjectSabotageConcealmentStreamAccumulatorTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.EvidenceId.Value);
                    await VerifyAsync(CharacterStructuredDecisions.ObjectSabotageConcealment, request, provider, document, decision =>
                    {
                        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
                        Assert.Same(request.Candidates[0].Evidence.EvidenceId, decision.SelectedEvidenceId);
                    });
                    break;
                }
            case "ObjectSabotageContamination":
                {
                    var request = OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest();
                    string document = OpenAiCharacterObjectSabotageContaminationStreamAccumulatorTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.EvidenceId.Value);
                    await VerifyAsync(CharacterStructuredDecisions.ObjectSabotageContamination, request, provider, document, decision =>
                    {
                        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
                        Assert.Same(request.Candidates[0].Evidence.EvidenceId, decision.SelectedEvidenceId);
                    });
                    break;
                }
            case "ObjectSabotageMove":
                {
                    var request = OpenAiCharacterObjectSabotageMoveRequestSerializerTests.CreateRequest();
                    string document = OpenAiCharacterObjectSabotageMoveStreamAccumulatorTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.Evidence.EvidenceId.Value);
                    await VerifyAsync(CharacterStructuredDecisions.ObjectSabotageMove, request, provider, document, decision =>
                    {
                        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
                        Assert.Same(request.Candidates[0].Evidence.Evidence.EvidenceId, decision.SelectedEvidenceId);
                    });
                    break;
                }
            case "ObjectSabotageSubstitution":
                {
                    var request = OpenAiCharacterObjectSabotageSubstitutionRequestSerializerTests.CreateRequest();
                    string document = OpenAiCharacterObjectSabotageSubstitutionStreamAccumulatorTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.Evidence.EvidenceId.Value);
                    await VerifyAsync(CharacterStructuredDecisions.ObjectSabotageSubstitution, request, provider, document, decision =>
                    {
                        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
                        Assert.Same(request.Candidates[0].Evidence.Evidence.EvidenceId, decision.SelectedEvidenceId);
                    });
                    break;
                }
            case "SabotageAction":
                {
                    var request = CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput().Request;
                    string document = OpenAiCharacterSabotageActionStreamAccumulatorTests.DecisionDocument(request.Actor.Identity.Id.Value, request.ActionKinds[0].Value);
                    await VerifyAsync(CharacterStructuredDecisions.SabotageAction, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.ActionKinds[0], decision.SelectedActionKind);
                    });
                    break;
                }
            case "TargetSabotageDelay":
                {
                    var request = CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request;
                    string document = OpenAiCharacterTargetSabotageDelayStreamAccumulatorTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.TargetSabotageDelay, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedCandidate);
                    });
                    break;
                }
            case "TargetSabotageDelayRecovery":
                {
                    var request = CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create().Request;
                    string document = OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulatorTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.TargetSabotageDelayRecovery, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedCandidate);
                    });
                    break;
                }
            case "TargetSabotageInfluence":
                {
                    var request = OpenAiCharacterTargetSabotageInfluenceRequestSerializerTests.CreateRequest();
                    string document = OpenAiCharacterTargetSabotageInfluenceStreamAccumulatorTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.TargetSabotageInfluence, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedCandidate);
                    });
                    break;
                }
            case "TargetSabotageInfluenceRecovery":
                {
                    var request = CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create().Request;
                    string document = OpenAiCharacterTargetSabotageInfluenceRecoveryStreamAccumulatorTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.TargetSabotageInfluenceRecovery, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedCandidate);
                    });
                    break;
                }
        }
    }

    private static async Task VerifyAsync<TRequest, TDecision>(
        StructuredDecisionDescriptor<TRequest, TDecision> descriptor, TRequest request,
        string providerName, string document, Action<TDecision> assertDecision)
        where TRequest : class where TDecision : class
    {
        var provider = CharacterModelProvider.Parse(providerName);
        var credentials = new ThrowingCredentials();
        using HttpClient http = new(new Handler(StructuredDecisionProvidersTests.Events(providerName)
            .Replace(JsonSerializer.Serialize("{}"), JsonSerializer.Serialize(document), StringComparison.Ordinal)));
        // Construction neither loads either credential source nor performs HTTP I/O.
        var client = StructuredDecisionClientFactory.Create(http, new(provider, "model-test"), descriptor,
            credentials, credentials);
        Assert.Equal(0, credentials.LoadCount);
        credentials.AllowLoad = true;
        assertDecision(await client.DecideAsync(request, default));
        Assert.Equal(provider == CharacterModelProvider.Ollama ? 0 : 1, credentials.LoadCount);

        using var openAi = JsonDocument.Parse(descriptor.Serialize(request, CharacterModelProvider.OpenAi, "model-test"));
        using var actual = JsonDocument.Parse(descriptor.Serialize(request, provider, "model-test"));
        var expectedInput = openAi.RootElement.GetProperty("input");
        var expectedSchema = openAi.RootElement.GetProperty("text").GetProperty("format").GetProperty("schema");
        if (provider == CharacterModelProvider.Anthropic)
        {
            Assert.Equal(expectedInput[0].GetProperty("content").GetString(), actual.RootElement.GetProperty("system").GetString());
            Assert.Equal(expectedInput[1].GetProperty("content").GetString(), actual.RootElement.GetProperty("messages")[0].GetProperty("content").GetString());
            Assert.Equal(expectedSchema.GetRawText(), actual.RootElement.GetProperty("output_config").GetProperty("format").GetProperty("schema").GetRawText());
        }
        else if (provider == CharacterModelProvider.Ollama)
        {
            Assert.Equal(expectedInput[0].GetProperty("content").GetString(), actual.RootElement.GetProperty("messages")[0].GetProperty("content").GetString());
            Assert.Equal(expectedInput[1].GetProperty("content").GetString(), actual.RootElement.GetProperty("messages")[1].GetProperty("content").GetString());
            Assert.Equal(expectedSchema.GetRawText(), actual.RootElement.GetProperty("format").GetRawText());
            Assert.False(actual.RootElement.GetProperty("think").GetBoolean());
        }
    }

    private sealed class ThrowingCredentials : IOpenAiCredentialSource, IAnthropicCredentialSource
    {
        internal bool AllowLoad { get; set; }
        internal int LoadCount { get; private set; }
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(CancellationToken cancellationToken)
        {
            Assert.True(AllowLoad);
            LoadCount++;
            return ValueTask.FromResult(OpenAiCredentialLoadResult.Loaded("fictional-key"));
        }
        ValueTask<AnthropicCredentialLoadResult> IAnthropicCredentialSource.LoadApiKeyAsync(CancellationToken cancellationToken)
        {
            Assert.True(AllowLoad);
            LoadCount++;
            return ValueTask.FromResult(AnthropicCredentialLoadResult.Loaded("fictional-key"));
        }
    }

    private sealed class Handler(string events) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(events,
                Encoding.UTF8, request.RequestUri!.AbsolutePath == "/api/chat" ? "application/x-ndjson" : "text/event-stream")
            });
    }
}
