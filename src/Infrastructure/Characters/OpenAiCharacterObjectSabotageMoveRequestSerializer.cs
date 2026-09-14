using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Writes an OpenAI Responses request for one move consideration.</summary>
public static class OpenAiCharacterObjectSabotageMoveRequestSerializer
{
    /// <summary>Gets the maximum supported OpenAI model-name length.</summary>
    public const int MaximumModelNameLength = CharacterModelSettings.MaximumModelNameLength;

    /// <summary>Gets the bounded provider output-token request.</summary>
    public const int MaximumOutputTokens = 128;

    /// <summary>Serializes one stateless streaming request with strict structured output.</summary>
    public static string Serialize(
        CharacterObjectSabotageMoveConsiderationRequest request,
        string modelName)
    {
        ArgumentNullException.ThrowIfNull(request);
        OpenAiCharacterModelRequestSerializer.ValidateModelName(modelName);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", modelName);
            writer.WriteBoolean("store", false);
            writer.WriteBoolean("stream", true);
            writer.WriteNumber("max_output_tokens", MaximumOutputTokens);
            WriteInput(writer, request);
            WriteTextFormat(writer, request);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteInput(
        Utf8JsonWriter writer,
        CharacterObjectSabotageMoveConsiderationRequest request)
    {
        writer.WritePropertyName("input");
        writer.WriteStartArray();
        WriteMessage(writer, CharacterObjectSabotageMoveRequestContent.DeveloperInstructions);
        WriteMessage(
            writer,
            CharacterObjectSabotageMoveRequestContent.SerializeTrustedContext(request));
        writer.WriteEndArray();
    }

    private static void WriteTextFormat(
        Utf8JsonWriter writer,
        CharacterObjectSabotageMoveConsiderationRequest request)
    {
        writer.WritePropertyName("text");
        writer.WriteStartObject();
        writer.WritePropertyName("format");
        writer.WriteStartObject();
        writer.WriteString("type", "json_schema");
        writer.WriteString("name", "character_object_sabotage_move_decision");
        writer.WriteBoolean("strict", true);
        writer.WritePropertyName("schema");
        CharacterObjectSabotageMoveDecisionJsonSchemaWriter.Write(writer, request);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteMessage(Utf8JsonWriter writer, string content)
    {
        writer.WriteStartObject();
        writer.WriteString("role", "developer");
        writer.WriteString("content", content);
        writer.WriteEndObject();
    }
}
