using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageInfluenceRequestSerializerTests
{
    internal static CharacterTargetSabotageInfluenceConsiderationRequest CreateRequest() =>
        CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.CreateInput().Request;

    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.CreateInput();

        string serialized = Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            CharacterStructuredDecisions.TargetSabotageInfluence.MaximumOutputTokens,
            root.GetProperty("max_output_tokens").GetInt32());
        Assert.False(root.TryGetProperty("api_key", out _));
        JsonElement[] messages = root.GetProperty("input").EnumerateArray().ToArray();
        Assert.Equal(2, messages.Length);
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
    public void SerializesOnlyBoundedTrustedRelationshipAndCandidateContext()
    {
        CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.CreateInput();
        using JsonDocument requestDocument = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));
        string contextText = requestDocument.RootElement.GetProperty("input")[1]
            .GetProperty("content").GetString()!;
        using JsonDocument contextDocument = JsonDocument.Parse(contextText);
        JsonElement context = contextDocument.RootElement;

        Assert.Equal(input.Actor.Identity.Id.Value, context.GetProperty("actorId").GetString());
        Assert.Equal(input.Actor.Identity.DisplayName, context.GetProperty("displayName").GetString());
        Assert.Equal(
            input.Relationship.CharacterId.Value,
            context.GetProperty("relationship").GetProperty("characterId").GetString());
        Assert.Equal(
            input.Relationship.RelatedCharacterId.Value,
            context.GetProperty("relationship").GetProperty("relatedCharacterId").GetString());
        JsonElement[] candidates = context.GetProperty("candidates").EnumerateArray().ToArray();
        JsonElement candidate = Assert.Single(candidates);
        Assert.Equal(0, candidate.GetProperty("index").GetInt32());
        Assert.Equal("influence", candidate.GetProperty("actionKind").GetString());
        Assert.Equal(
            input.Occurrence.Target.Identity.Id.Value,
            candidate.GetProperty("targetCharacterId").GetString());
        Assert.Equal(input.Change.Delta, candidate.GetProperty("delta").GetInt32());
        Assert.Equal(
            input.Change.Reason.Dimension.Value,
            candidate.GetProperty("dimension").GetString());
        Assert.False(context.TryGetProperty("interactionAccess", out _));
        Assert.False(context.TryGetProperty("knowledge", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaIncludingAbstention()
    {
        CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.CreateInput();
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal("character_target_sabotage_influence_decision", format.GetProperty("name").GetString());
        Assert.True(format.GetProperty("strict").GetBoolean());
        JsonElement schema = format.GetProperty("schema");
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(
            ["actorId", "selectedCandidateIndex"],
            schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
        JsonElement properties = schema.GetProperty("properties");
        Assert.Equal(
            [input.Actor.Identity.Id.Value],
            properties.GetProperty("actorId").GetProperty("enum")
                .EnumerateArray().Select(value => value.GetString()));
        JsonElement[] values = properties.GetProperty("selectedCandidateIndex")
            .GetProperty("enum").EnumerateArray().ToArray();
        Assert.Equal(0, values[0].GetInt32());
        Assert.Equal(JsonValueKind.Null, values[1].ValueKind);
    }

    [Fact]
    public void SupportsAnAbstentionOnlySchemaAndRejectsInvalidInputs()
    {
        CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.CreateInput();
        CharacterTargetSabotageInfluenceConsiderationRequest empty = new(
            input.Actor,
            input.InteractionAccess,
            input.Relationship,
            []);
        using JsonDocument document = JsonDocument.Parse(
            Serialize(empty, "gpt-test"));
        JsonElement[] values = document.RootElement.GetProperty("text").GetProperty("format")
            .GetProperty("schema").GetProperty("properties")
            .GetProperty("selectedCandidateIndex").GetProperty("enum").EnumerateArray().ToArray();

        Assert.Single(values);
        Assert.Equal(JsonValueKind.Null, values[0].ValueKind);
        Assert.Throws<ArgumentNullException>(() =>
            Serialize(null!, "gpt-test"));
        Assert.Throws<ArgumentNullException>(() =>
            Serialize(input.Request, null!));
        Assert.Throws<ArgumentException>(() =>
            Serialize(input.Request, " model "));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                input.Request,
                new string(
                    'm',
                    Allrpcs.Application.Dialogue.CharacterModelSettings.MaximumModelNameLength + 1)));
    }



    private static void AssertRequestUnchanged(
        CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.InteractionAccess, input.Request.InteractionAccess);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }
    private static string Serialize(CharacterTargetSabotageInfluenceConsiderationRequest request, string modelName) =>
        CharacterStructuredDecisions.TargetSabotageInfluence.Serialize(request, Allrpcs.Application.Dialogue.CharacterModelProvider.OpenAi, modelName);

}
