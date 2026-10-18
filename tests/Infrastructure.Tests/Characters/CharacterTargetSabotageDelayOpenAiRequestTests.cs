using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageDelayOpenAiRequestTests
{
    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();

        string serialized = Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            CharacterStructuredDecisions.TargetSabotageDelay.MaximumOutputTokens,
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
    public void SerializesOnlyBoundedTrustedWorldAndCandidateContext()
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
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
        Assert.Equal(JsonValueKind.Null, context.GetProperty("currentWorldEventId").ValueKind);
        Assert.Empty(context.GetProperty("occurredWorldEventIds").EnumerateArray());
        JsonElement candidate = Assert.Single(
            context.GetProperty("candidates").EnumerateArray().ToArray());
        Assert.Equal(0, candidate.GetProperty("index").GetInt32());
        Assert.Equal("delay", candidate.GetProperty("actionKind").GetString());
        Assert.Equal(
            input.Occurrence.Target.Identity.Id.Value,
            candidate.GetProperty("targetCharacterId").GetString());
        Assert.Equal(input.WorldEvent.EventId.Value, candidate.GetProperty("worldEventId").GetString());
        Assert.Equal(input.WorldEvent.TimeEffect.Value, candidate.GetProperty("timeEffect").GetString());
        Assert.False(context.TryGetProperty("interactionAccess", out _));
        Assert.False(context.TryGetProperty("worldEvents", out _));
        Assert.False(context.TryGetProperty("knowledge", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaIncludingAbstention()
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal(
            "character_target_sabotage_delay_decision",
            format.GetProperty("name").GetString());
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
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
        CharacterTargetSabotageDelayConsiderationRequest empty = new(
            input.Actor,
            input.InteractionAccess,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
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
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.InteractionAccess, input.Request.InteractionAccess);
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }
    private static string Serialize(CharacterTargetSabotageDelayConsiderationRequest request, string modelName) =>
        CharacterStructuredDecisions.TargetSabotageDelay.Serialize(request, Allrpcs.Application.Dialogue.CharacterModelProvider.OpenAi, modelName);

}
