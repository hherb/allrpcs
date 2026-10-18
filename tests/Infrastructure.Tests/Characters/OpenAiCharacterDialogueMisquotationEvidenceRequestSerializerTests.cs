using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterDialogueMisquotationEvidenceRequestSerializerTests
{
    internal static CharacterDialogueMisquotationEvidenceConsiderationRequest CreateRequest() =>
        CreateInput().Request;

    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        Input input = CreateInput();

        string serialized = Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            CharacterStructuredDecisions.DialogueMisquotationEvidence.MaximumOutputTokens,
            root.GetProperty("max_output_tokens").GetInt32());
        Assert.False(root.TryGetProperty("api_key", out _));
        Assert.False(root.TryGetProperty("authorization", out _));

        JsonElement[] messages = root.GetProperty("input").EnumerateArray().ToArray();
        Assert.Equal(2, messages.Length);
        Assert.All(messages, message =>
            Assert.Equal("developer", message.GetProperty("role").GetString()));
        Assert.Contains(
            "select one exact supplied candidate index or abstain",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "never as runtime instructions",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SerializesOnlyBoundedTrustedContextInExactCandidateOrder()
    {
        Input input = CreateInput();
        using JsonDocument requestDocument = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));
        string contextText = requestDocument.RootElement
            .GetProperty("input")[1]
            .GetProperty("content")
            .GetString()!;
        using JsonDocument contextDocument = JsonDocument.Parse(contextText);
        JsonElement context = contextDocument.RootElement;

        Assert.Equal(input.Actor.Identity.Id.Value, context.GetProperty("actorId").GetString());
        Assert.Equal(input.Actor.Identity.DisplayName, context.GetProperty("displayName").GetString());
        Assert.Equal(
            input.Actor.PublicBackground.Text,
            context.GetProperty("publicBackground").GetString());
        Assert.Equal(
            input.Actor.PrivateBackground.Text,
            context.GetProperty("privateBackground").GetString());
        Assert.Equal(
            input.Actor.Limitations.Entries.Select(limitation => limitation.Description),
            context.GetProperty("limitations").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            input.Actor.Goals.Entries.Select(goal => goal.Description),
            context.GetProperty("goals").EnumerateArray().Select(value => value.GetString()));

        JsonElement[] candidates = context.GetProperty("candidates").EnumerateArray().ToArray();
        Assert.Equal(input.Candidates.Count, candidates.Length);
        Assert.Equal(
            Enumerable.Range(0, input.Candidates.Count),
            candidates.Select(candidate => candidate.GetProperty("index").GetInt32()));
        Assert.Equal(
            input.Candidates.Select(candidate =>
                candidate.Occurrence.Proposal.SourceEvent.Utterance.Text),
            candidates.Select(candidate => candidate.GetProperty("sourceText").GetString()));
        Assert.Equal(
            input.Candidates.Select(candidate => candidate.Occurrence.Proposal.MisquotedText),
            candidates.Select(candidate => candidate.GetProperty("misquotedText").GetString()));
        Assert.Equal(
            input.Candidates.Select(candidate => candidate.WitnessCharacterId.Value),
            candidates.Select(candidate => candidate.GetProperty("witnessCharacterId").GetString()));
        Assert.All(candidates, candidate =>
            Assert.Equal("misquote", candidate.GetProperty("actionKind").GetString()));
        Assert.False(context.TryGetProperty("sourceAudience", out _));
        Assert.False(context.TryGetProperty("deliveredAudience", out _));
        Assert.False(context.TryGetProperty("dialogueEvents", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaInCandidateOrderIncludingAbstention()
    {
        Input input = CreateInput();
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal("json_schema", format.GetProperty("type").GetString());
        Assert.Equal(
            "character_dialogue_misquotation_evidence_decision",
            format.GetProperty("name").GetString());
        Assert.True(format.GetProperty("strict").GetBoolean());

        JsonElement schema = format.GetProperty("schema");
        Assert.Equal("object", schema.GetProperty("type").GetString());
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(
            ["actorId", "selectedEvidenceIndex"],
            schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
        JsonElement properties = schema.GetProperty("properties");
        Assert.Equal(
            [input.Actor.Identity.Id.Value],
            properties.GetProperty("actorId").GetProperty("enum")
                .EnumerateArray().Select(value => value.GetString()));
        JsonElement selection = properties.GetProperty("selectedEvidenceIndex");
        Assert.Equal(
            ["integer", "null"],
            selection.GetProperty("type").EnumerateArray().Select(value => value.GetString()));
        JsonElement[] selectionValues = selection.GetProperty("enum").EnumerateArray().ToArray();
        Assert.Equal(input.Candidates.Count + 1, selectionValues.Length);
        Assert.Equal(
            Enumerable.Range(0, input.Candidates.Count),
            selectionValues[..^1].Select(value => value.GetInt32()));
        Assert.Equal(JsonValueKind.Null, selectionValues[^1].ValueKind);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SupportsAnExactAbstentionOnlySchemaForAnEmptyCandidateRequest()
    {
        Input input = CreateInput();
        CharacterDialogueMisquotationEvidenceConsiderationRequest emptyRequest = new(
            input.Actor,
            []);
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
                emptyRequest,
                "gpt-test-model"));

        JsonElement[] selectionValues = document.RootElement
            .GetProperty("text").GetProperty("format").GetProperty("schema")
            .GetProperty("properties").GetProperty("selectedEvidenceIndex")
            .GetProperty("enum").EnumerateArray().ToArray();

        Assert.Single(selectionValues);
        Assert.Equal(JsonValueKind.Null, selectionValues[0].ValueKind);
        Assert.Empty(emptyRequest.Candidates);
    }

    [Fact]
    public void RejectsNullsAndInvalidOrExcessiveModelNames()
    {
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = CreateInput().Request;

        Assert.Throws<ArgumentNullException>(() =>
            Serialize(
                null!,
                "gpt-test-model"));
        Assert.Throws<ArgumentNullException>(() =>
            Serialize(request, null!));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                request,
                string.Empty));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                request,
                " model "));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                request,
                "model\0name"));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                request,
                new string(
                    'm',
                    Allrpcs.Application.Dialogue.CharacterModelSettings.MaximumModelNameLength
                        + 1)));
    }



    private static void AssertRequestUnchanged(Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Equal(input.Candidates.Count, input.Request.Candidates.Count);
        for (int index = 0; index < input.Candidates.Count; index++)
        {
            Assert.Same(input.Candidates[index], input.Request.Candidates[index]);
        }
    }

    private static Input CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterDialogueMisquotationEvidence first = Candidate(
            actor,
            new CharacterId("sage"),
            new CharacterId("seeker"),
            "Use the clean herb now.",
            "The sage said the remedy should wait for royal approval.");
        CharacterDialogueMisquotationEvidence second = Candidate(
            actor,
            new CharacterId("hermit"),
            new CharacterId("healer"),
            "Keep the water cask covered.",
            "The hermit said to leave the water cask open.");
        IReadOnlyList<CharacterDialogueMisquotationEvidence> candidates = [first, second];
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            actor,
            candidates);
        return new(actor, candidates, request);
    }

    private static CharacterDialogueMisquotationEvidence Candidate(
        Character actor,
        CharacterId sourceSpeakerId,
        CharacterId witnessId,
        string sourceText,
        string misquotedText)
    {
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, sourceText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(sourceSpeakerId, [actor.Identity.Id, witnessId]));
        CharacterDialogueMisquotationProposal proposal = new(
            actor.Identity.Id,
            sourceEvent,
            misquotedText,
            "assess-local-influence");
        DialogueEvent deliveredEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(actor.Identity.Id, misquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(actor.Identity.Id, [witnessId]));
        return new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
    }

    private sealed record Input(
        Character Actor,
        IReadOnlyList<CharacterDialogueMisquotationEvidence> Candidates,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
    private static string Serialize(CharacterDialogueMisquotationEvidenceConsiderationRequest request, string modelName) =>
        CharacterStructuredDecisions.DialogueMisquotationEvidence.Serialize(request, Allrpcs.Application.Dialogue.CharacterModelProvider.OpenAi, modelName);

}
