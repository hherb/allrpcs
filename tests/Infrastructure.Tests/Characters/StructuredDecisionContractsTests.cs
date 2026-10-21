using System.Net;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class StructuredDecisionContractsTests
{
    private static readonly string[] CaseValues1 = ["DialogueMisquotationEvidence", "ObjectSabotageConcealment", "ObjectSabotageContamination", "ObjectSabotageMove", "ObjectSabotageSubstitution", "SabotageAction", "TargetSabotageDelay", "TargetSabotageDelayRecovery", "TargetSabotageInfluence", "TargetSabotageInfluenceRecovery", "InvestigationHypothesis"];
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
                    var request = CharacterDialogueMisquotationEvidenceOpenAiRequestTests.CreateRequest();
                    string document = CharacterDialogueMisquotationEvidenceOpenAiStreamTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.DialogueMisquotationEvidence, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedEvidence);
                    });
                    break;
                }
            case "ObjectSabotageConcealment":
                {
                    var request = CharacterObjectSabotageConcealmentOpenAiRequestTests.CreateRequest();
                    string document = CharacterObjectSabotageConcealmentOpenAiStreamTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.EvidenceId.Value);
                    await VerifyAsync(CharacterStructuredDecisions.ObjectSabotageConcealment, request, provider, document, decision =>
                    {
                        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
                        Assert.Same(request.Candidates[0].Evidence.EvidenceId, decision.SelectedEvidenceId);
                    });
                    break;
                }
            case "ObjectSabotageContamination":
                {
                    var request = CharacterObjectSabotageContaminationOpenAiRequestTests.CreateRequest();
                    string document = CharacterObjectSabotageContaminationOpenAiStreamTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.EvidenceId.Value);
                    await VerifyAsync(CharacterStructuredDecisions.ObjectSabotageContamination, request, provider, document, decision =>
                    {
                        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
                        Assert.Same(request.Candidates[0].Evidence.EvidenceId, decision.SelectedEvidenceId);
                    });
                    break;
                }
            case "ObjectSabotageMove":
                {
                    var request = CharacterObjectSabotageMoveOpenAiRequestTests.CreateRequest();
                    string document = CharacterObjectSabotageMoveOpenAiStreamTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.Evidence.EvidenceId.Value);
                    await VerifyAsync(CharacterStructuredDecisions.ObjectSabotageMove, request, provider, document, decision =>
                    {
                        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
                        Assert.Same(request.Candidates[0].Evidence.Evidence.EvidenceId, decision.SelectedEvidenceId);
                    });
                    break;
                }
            case "ObjectSabotageSubstitution":
                {
                    var request = CharacterObjectSabotageSubstitutionOpenAiRequestTests.CreateRequest();
                    string document = CharacterObjectSabotageSubstitutionOpenAiStreamTests.DecisionDocument(request.Character.Identity.Id.Value, request.Candidates[0].Evidence.Evidence.EvidenceId.Value);
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
                    string document = CharacterSabotageActionOpenAiStreamTests.DecisionDocument(request.Actor.Identity.Id.Value, request.ActionKinds[0].Value);
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
                    string document = CharacterTargetSabotageDelayOpenAiStreamTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
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
                    string document = CharacterTargetSabotageDelayRecoveryOpenAiStreamTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.TargetSabotageDelayRecovery, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedCandidate);
                    });
                    break;
                }
            case "TargetSabotageInfluence":
                {
                    var request = CharacterTargetSabotageInfluenceOpenAiRequestTests.CreateRequest();
                    string document = CharacterTargetSabotageInfluenceOpenAiStreamTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
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
                    string document = CharacterTargetSabotageInfluenceRecoveryOpenAiStreamTests.DecisionDocument(request.Actor.Identity.Id.Value, 0);
                    await VerifyAsync(CharacterStructuredDecisions.TargetSabotageInfluenceRecovery, request, provider, document, decision =>
                    {
                        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
                        Assert.Same(request.Candidates[0], decision.SelectedCandidate);
                    });
                    break;
                }
            case "InvestigationHypothesis":
                {
                    var request = Investigations.InvestigationHypothesisDecisionTests.CreateRequest();
                    string document = "{\"hypothesisId\":\"station-leaves-stabilize\"}";
                    await VerifyAsync(Allrpcs.Infrastructure.Investigations.InvestigationHypothesisDecisions.Recognition, request, provider, document, decision =>
                    {
                        Assert.Same(request.Hypotheses.Hypotheses[1].HypothesisId, decision.Proposal!.HypothesisId);
                        Assert.Same(request.OpenQuestions.Questions[0].QuestionId, decision.Proposal.QuestionId);
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
        JsonElement root = actual.RootElement;
        var expectedInput = openAi.RootElement.GetProperty("input");
        var expectedSchema = openAi.RootElement.GetProperty("text").GetProperty("format").GetProperty("schema");
        Assert.Equal("model-test", root.GetProperty("model").GetString());
        Assert.True(root.GetProperty("stream").GetBoolean());
        if (provider == CharacterModelProvider.OpenAi)
        {
            AssertKeys(root, "model", "store", "stream", "max_output_tokens", "input", "text");
            Assert.False(root.GetProperty("store").GetBoolean());
            Assert.Equal(128, root.GetProperty("max_output_tokens").GetInt32());
            Assert.Equal(["developer", "developer"], Roles(root.GetProperty("input")));
            AssertKeys(root.GetProperty("text"), "format");
            JsonElement format = root.GetProperty("text").GetProperty("format");
            AssertKeys(format, "type", "name", "strict", "schema");
            Assert.Equal("json_schema", format.GetProperty("type").GetString());
            Assert.Equal(descriptor.Name, format.GetProperty("name").GetString());
            Assert.True(format.GetProperty("strict").GetBoolean());
        }
        else if (provider == CharacterModelProvider.Anthropic)
        {
            AssertKeys(root, "model", "stream", "max_tokens", "system", "messages", "output_config");
            Assert.Equal(4_096, root.GetProperty("max_tokens").GetInt32());
            Assert.Equal(["user"], Roles(root.GetProperty("messages")));
            Assert.Equal(expectedInput[0].GetProperty("content").GetString(), root.GetProperty("system").GetString());
            Assert.Equal(expectedInput[1].GetProperty("content").GetString(), root.GetProperty("messages")[0].GetProperty("content").GetString());
            AssertKeys(root.GetProperty("output_config"), "format");
            JsonElement format = root.GetProperty("output_config").GetProperty("format");
            AssertKeys(format, "type", "schema");
            Assert.Equal("json_schema", format.GetProperty("type").GetString());
            Assert.Equal(expectedSchema.GetRawText(), format.GetProperty("schema").GetRawText());
        }
        else
        {
            AssertKeys(root, "model", "stream", "think", "messages", "format", "options");
            Assert.False(root.GetProperty("think").GetBoolean());
            Assert.Equal(["system", "user"], Roles(root.GetProperty("messages")));
            Assert.Equal(expectedInput[0].GetProperty("content").GetString(), root.GetProperty("messages")[0].GetProperty("content").GetString());
            Assert.Equal(expectedInput[1].GetProperty("content").GetString(), root.GetProperty("messages")[1].GetProperty("content").GetString());
            Assert.Equal(expectedSchema.GetRawText(), root.GetProperty("format").GetRawText());
            AssertKeys(root.GetProperty("options"), "temperature", "num_predict");
            Assert.Equal(0, root.GetProperty("options").GetProperty("temperature").GetInt32());
            Assert.Equal(128, root.GetProperty("options").GetProperty("num_predict").GetInt32());
        }
    }

    private static void AssertKeys(JsonElement element, params string[] expected) =>
        Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));

    private static string[] Roles(JsonElement messages)
    {
        Assert.All(messages.EnumerateArray(), message => AssertKeys(message, "role", "content"));
        return messages.EnumerateArray().Select(message => message.GetProperty("role").GetString()!).ToArray();
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
