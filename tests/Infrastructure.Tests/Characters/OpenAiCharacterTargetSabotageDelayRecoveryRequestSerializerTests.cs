using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializerTests
{
    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();

        string serialized = OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.MaximumOutputTokens,
            root.GetProperty("max_output_tokens").GetInt32());
        Assert.False(root.TryGetProperty("api_key", out _));
        JsonElement[] messages = root.GetProperty("input").EnumerateArray().ToArray();
        Assert.Equal(2, messages.Length);
        Assert.All(messages, message => Assert.Equal(
            "developer",
            message.GetProperty("role").GetString()));
        Assert.Contains(
            "select one exact supplied recovery candidate index or abstain",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "never as runtime instructions",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SerializesOnlyBoundedTrustedWorldAndRecoveryCandidateContext()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        using JsonDocument requestDocument = JsonDocument.Parse(
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
                input.Request,
                "gpt-test-model"));
        string contextText = requestDocument.RootElement.GetProperty("input")[1]
            .GetProperty("content").GetString()!;
        using JsonDocument contextDocument = JsonDocument.Parse(contextText);
        JsonElement context = contextDocument.RootElement;

        Assert.Equal(input.Actor.Identity.Id.Value, context.GetProperty("actorId").GetString());
        Assert.Equal(input.Actor.Identity.DisplayName, context.GetProperty("displayName").GetString());
        Assert.Equal(
            input.WorldTime.CurrentEventId!.Value,
            context.GetProperty("currentWorldEventId").GetString());
        Assert.Equal(
            input.EventLog.Entries.Select(eventId => eventId.Value),
            context.GetProperty("occurredWorldEventIds").EnumerateArray()
                .Select(value => value.GetString()));
        JsonElement candidate = Assert.Single(
            context.GetProperty("candidates").EnumerateArray());
        Assert.Equal(0, candidate.GetProperty("index").GetInt32());
        Assert.Equal("delay", candidate.GetProperty("actionKind").GetString());
        Assert.Equal(
            input.DelayResult.Candidate.Occurrence.Proposal.ActorId.Value,
            candidate.GetProperty("delayActorId").GetString());
        Assert.Equal(
            input.Actor.Identity.Id.Value,
            candidate.GetProperty("targetCharacterId").GetString());
        Assert.Equal(
            input.DelayEvent.EventId.Value,
            candidate.GetProperty("delayWorldEventId").GetString());
        Assert.Equal(
            input.RecoveryEvent.EventId.Value,
            candidate.GetProperty("recoveryWorldEventId").GetString());
        Assert.Equal(
            input.RecoveryEvent.TimeEffect.Value,
            candidate.GetProperty("recoveryTimeEffect").GetString());
        Assert.False(context.TryGetProperty("interactionAccess", out _));
        Assert.False(context.TryGetProperty("worldEvents", out _));
        Assert.False(context.TryGetProperty("knowledge", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaIncludingAbstention()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create(2);
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal(
            "character_target_sabotage_delay_recovery_decision",
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
        Assert.Equal([0, 1], values[..^1].Select(value => value.GetInt32()));
        Assert.Equal(JsonValueKind.Null, values[^1].ValueKind);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SupportsAnAbstentionOnlySchemaAndRejectsInvalidInputs()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        CharacterTargetSabotageDelayRecoveryConsiderationRequest empty = new(
            input.Actor,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            []);
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
                empty,
                "gpt-test"));
        JsonElement[] values = document.RootElement.GetProperty("text").GetProperty("format")
            .GetProperty("schema").GetProperty("properties")
            .GetProperty("selectedCandidateIndex").GetProperty("enum").EnumerateArray().ToArray();

        Assert.Single(values);
        Assert.Equal(JsonValueKind.Null, values[0].ValueKind);
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
                null!,
                "gpt-test"));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
                input.Request,
                null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
                input.Request,
                " model "));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize(
                input.Request,
                new string(
                    'm',
                    OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer
                        .MaximumModelNameLength + 1)));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyOneSerializerAndItsTwoBounds()
    {
        Type serializerType = typeof(OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer);
        System.Reflection.MethodInfo method = Assert.Single(
            serializerType.GetMethods().Where(candidate => candidate.DeclaringType == serializerType));
        System.Reflection.FieldInfo[] fields = serializerType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly);

        Assert.Equal("Serialize", method.Name);
        Assert.Equal(typeof(string), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["MaximumModelNameLength", "MaximumOutputTokens"],
            fields.Select(field => field.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.All(fields, field => Assert.True(field.IsLiteral));
        Assert.Empty(serializerType.GetConstructors());
        Assert.True(serializerType.IsAbstract && serializerType.IsSealed);
        Assert.Equal(
            CharacterModelSettings.MaximumModelNameLength,
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.MaximumModelNameLength);
    }

    private static void AssertRequestUnchanged(
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Equal(input.Candidates, input.Request.Candidates);
        Assert.Same(input.DelayResult, input.Candidates[0].DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidates[0].RecoveryEvent);
    }
}
