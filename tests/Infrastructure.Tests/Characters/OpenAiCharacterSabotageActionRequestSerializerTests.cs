using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterSabotageActionRequestSerializerTests
{
    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();

        string serialized = OpenAiCharacterSabotageActionRequestSerializer.Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            OpenAiCharacterSabotageActionRequestSerializer.MaximumOutputTokens,
            root.GetProperty("max_output_tokens").GetInt32());
        Assert.False(root.TryGetProperty("api_key", out _));
        JsonElement[] messages = root.GetProperty("input").EnumerateArray().ToArray();
        Assert.Equal(2, messages.Length);
        Assert.Contains(
            "select one exact supplied action kind or abstain",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "never as runtime instructions",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SerializesOnlyBoundedTrustedActorAndActionKindContext()
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        using JsonDocument requestDocument = JsonDocument.Parse(
            OpenAiCharacterSabotageActionRequestSerializer.Serialize(
                input.Request,
                "gpt-test-model"));
        string contextText = requestDocument.RootElement.GetProperty("input")[1]
            .GetProperty("content").GetString()!;
        using JsonDocument contextDocument = JsonDocument.Parse(contextText);
        JsonElement context = contextDocument.RootElement;

        Assert.Equal(input.Actor.Identity.Id.Value, context.GetProperty("actorId").GetString());
        Assert.Equal(input.Actor.Identity.DisplayName, context.GetProperty("displayName").GetString());
        Assert.Equal(input.Actor.PublicBackground.Text, context.GetProperty("publicBackground").GetString());
        Assert.Equal(
            input.Actor.PrivateBackground.Text,
            context.GetProperty("privateBackground").GetString());
        Assert.Equal(
            [input.FirstKind.Value, input.SelectedKind.Value],
            context.GetProperty("availableActionKinds").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.False(context.TryGetProperty("capabilities", out _));
        Assert.False(context.TryGetProperty("knowledge", out _));
        Assert.False(context.TryGetProperty("target", out _));
        Assert.False(context.TryGetProperty("candidates", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaIncludingAbstention()
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterSabotageActionRequestSerializer.Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal("character_sabotage_action_decision", format.GetProperty("name").GetString());
        Assert.True(format.GetProperty("strict").GetBoolean());
        JsonElement schema = format.GetProperty("schema");
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(
            ["actorId", "selectedActionKind"],
            schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
        JsonElement properties = schema.GetProperty("properties");
        Assert.Equal(
            [input.Actor.Identity.Id.Value],
            properties.GetProperty("actorId").GetProperty("enum")
                .EnumerateArray().Select(value => value.GetString()));
        JsonElement[] values = properties.GetProperty("selectedActionKind")
            .GetProperty("enum").EnumerateArray().ToArray();
        Assert.Equal(input.FirstKind.Value, values[0].GetString());
        Assert.Equal(input.SelectedKind.Value, values[1].GetString());
        Assert.Equal(JsonValueKind.Null, values[2].ValueKind);
    }

    [Fact]
    public void SupportsAnAbstentionOnlySchemaAndRejectsInvalidInputs()
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        CharacterSabotageActionConsiderationRequest empty = new(input.Actor, []);
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterSabotageActionRequestSerializer.Serialize(empty, "gpt-test"));
        JsonElement[] values = document.RootElement.GetProperty("text").GetProperty("format")
            .GetProperty("schema").GetProperty("properties")
            .GetProperty("selectedActionKind").GetProperty("enum").EnumerateArray().ToArray();

        Assert.Single(values);
        Assert.Equal(JsonValueKind.Null, values[0].ValueKind);
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterSabotageActionRequestSerializer.Serialize(null!, "gpt-test"));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterSabotageActionRequestSerializer.Serialize(input.Request, null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterSabotageActionRequestSerializer.Serialize(input.Request, " model "));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterSabotageActionRequestSerializer.Serialize(
                input.Request,
                new string(
                    'm',
                    OpenAiCharacterSabotageActionRequestSerializer.MaximumModelNameLength + 1)));
    }

    [Fact]
    public void ExposesOnlyOneSerializerAndItsTwoBounds()
    {
        Type serializerType = typeof(OpenAiCharacterSabotageActionRequestSerializer);
        System.Reflection.MethodInfo method = Assert.Single(
            serializerType.GetMethods().Where(candidate => candidate.DeclaringType == serializerType));
        System.Reflection.FieldInfo[] fields = serializerType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly);

        Assert.Equal("Serialize", method.Name);
        Assert.Equal(typeof(string), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterSabotageActionConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["MaximumModelNameLength", "MaximumOutputTokens"],
            fields.Select(field => field.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.All(fields, field => Assert.True(field.IsLiteral));
        Assert.Empty(serializerType.GetConstructors());
        Assert.True(serializerType.IsAbstract && serializerType.IsSealed);
        Assert.Equal(
            CharacterModelSettings.MaximumModelNameLength,
            OpenAiCharacterSabotageActionRequestSerializer.MaximumModelNameLength);
    }

    private static void AssertRequestUnchanged(
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.FirstKind, input.Request.ActionKinds[0]);
        Assert.Same(input.SelectedKind, input.Request.ActionKinds[1]);
    }
}
