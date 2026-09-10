using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Writes one deterministic reasoning-free request for a local compatible API.</summary>
public static class OpenAiChatCompletionRequestSerializer
{
    /// <summary>Serializes one bounded non-streaming structured character request.</summary>
    public static string Serialize(
        CharacterModelGenerationRequest request,
        string modelName)
    {
        ArgumentNullException.ThrowIfNull(request);
        OpenAiCharacterModelRequestSerializer.ValidateModelName(modelName);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", modelName);
            writer.WriteBoolean("stream", false);
            writer.WriteNumber("temperature", 0);
            writer.WriteNumber("seed", 0);
            writer.WriteNumber(
                "max_tokens",
                OpenAiCharacterModelRequestSerializer.MaximumOutputTokens);
            writer.WriteNumber("thinking_budget", 0);
            writer.WriteString("reasoning_effort", "none");
            WriteChatTemplateArguments(writer);
            WriteMessages(writer, request);
            WriteResponseFormat(writer);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteChatTemplateArguments(Utf8JsonWriter writer)
    {
        writer.WritePropertyName("chat_template_kwargs");
        writer.WriteStartObject();
        writer.WriteBoolean("enable_thinking", false);
        writer.WriteBoolean("preserve_thinking", false);
        writer.WriteString("reasoning_effort", "none");
        writer.WriteEndObject();
    }

    private static void WriteMessages(
        Utf8JsonWriter writer,
        CharacterModelGenerationRequest request)
    {
        writer.WritePropertyName("messages");
        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WriteString("role", "system");
        writer.WriteString(
            "content",
            CharacterModelRequestContent.DeveloperInstructions
                + "\n\n"
                + CharacterModelRequestContent.SerializeTrustedContext(request));
        writer.WriteEndObject();
        writer.WriteStartObject();
        writer.WriteString("role", "user");
        writer.WriteString("content", request.TurnRequest.DialogueEvent.Utterance.Text);
        writer.WriteEndObject();
        writer.WriteEndArray();
    }

    private static void WriteResponseFormat(Utf8JsonWriter writer)
    {
        writer.WritePropertyName("response_format");
        writer.WriteStartObject();
        writer.WriteString("type", "json_schema");
        writer.WritePropertyName("json_schema");
        writer.WriteStartObject();
        writer.WriteString("name", "character_turn_response");
        writer.WriteBoolean("strict", true);
        writer.WritePropertyName("schema");
        CharacterTurnResponseJsonSchemaWriter.Write(writer, includeBounds: true);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
