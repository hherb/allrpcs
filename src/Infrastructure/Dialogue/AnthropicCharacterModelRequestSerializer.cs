using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Writes a bounded Anthropic Messages request from provider-neutral turn context.</summary>
public static class AnthropicCharacterModelRequestSerializer
{
    /// <summary>Gets the maximum supported Anthropic model-name length.</summary>
    public const int MaximumModelNameLength = CharacterModelSettings.MaximumModelNameLength;

    /// <summary>Gets the bounded provider output-token request.</summary>
    public const int MaximumOutputTokens = 4_096;

    /// <summary>Serializes one stateless streaming request with no credentials.</summary>
    public static string Serialize(
        CharacterModelGenerationRequest request,
        string modelName)
    {
        ArgumentNullException.ThrowIfNull(request);
        CharacterModelSettings.ValidateModelName(modelName);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", modelName);
            writer.WriteNumber("max_tokens", MaximumOutputTokens);
            writer.WriteBoolean("stream", true);
            WriteSystem(writer, request);
            writer.WritePropertyName("messages");
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WriteString("role", "user");
            writer.WriteString("content", request.TurnRequest.DialogueEvent.Utterance.Text);
            writer.WriteEndObject();
            writer.WriteEndArray();
            writer.WritePropertyName("output_config");
            writer.WriteStartObject();
            writer.WritePropertyName("format");
            writer.WriteStartObject();
            writer.WriteString("type", "json_schema");
            writer.WritePropertyName("schema");
            CharacterTurnResponseJsonSchemaWriter.Write(writer, includeBounds: false);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteSystem(
        Utf8JsonWriter writer,
        CharacterModelGenerationRequest request)
    {
        writer.WritePropertyName("system");
        writer.WriteStartArray();
        WriteTextBlock(writer, CharacterModelRequestContent.DeveloperInstructions);
        WriteTextBlock(writer, CharacterModelRequestContent.SerializeTrustedContext(request));
        writer.WriteEndArray();
    }

    private static void WriteTextBlock(Utf8JsonWriter writer, string text)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "text");
        writer.WriteString("text", text);
        writer.WriteEndObject();
    }
}
