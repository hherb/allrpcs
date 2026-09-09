using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Writes a bounded OpenAI Responses API request from provider-neutral character-turn context.
/// </summary>
public static class OpenAiCharacterModelRequestSerializer
{
    /// <summary>Gets the maximum supported OpenAI model-name length.</summary>
    public const int MaximumModelNameLength = CharacterModelSettings.MaximumModelNameLength;

    /// <summary>Gets the bounded provider output-token request.</summary>
    public const int MaximumOutputTokens = 4_096;

    /// <summary>
    /// Serializes one stateless streaming request with strict structured output and no credentials.
    /// </summary>
    public static string Serialize(
        CharacterModelGenerationRequest request,
        string modelName)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateModelName(modelName);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", modelName);
            writer.WriteBoolean("store", false);
            writer.WriteBoolean("stream", true);
            writer.WriteNumber("max_output_tokens", MaximumOutputTokens);
            WriteInput(writer, request);
            writer.WritePropertyName("text");
            writer.WriteStartObject();
            writer.WritePropertyName("format");
            writer.WriteStartObject();
            writer.WriteString("type", "json_schema");
            writer.WriteString("name", "character_turn_response");
            writer.WriteBoolean("strict", true);
            writer.WritePropertyName("schema");
            CharacterTurnResponseJsonSchemaWriter.Write(
                writer,
                includeBounds: true);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    internal static void ValidateModelName(string modelName)
    {
        CharacterModelSettings.ValidateModelName(modelName);
    }

    private static void WriteInput(
        Utf8JsonWriter writer,
        CharacterModelGenerationRequest request)
    {
        writer.WritePropertyName("input");
        writer.WriteStartArray();
        WriteMessage(writer, "developer", CharacterModelRequestContent.DeveloperInstructions);
        WriteMessage(
            writer,
            "developer",
            CharacterModelRequestContent.SerializeTrustedContext(request));
        WriteMessage(writer, "user", request.TurnRequest.DialogueEvent.Utterance.Text);
        writer.WriteEndArray();
    }

    private static void WriteMessage(
        Utf8JsonWriter writer,
        string role,
        string content)
    {
        writer.WriteStartObject();
        writer.WriteString("role", role);
        writer.WriteString("content", content);
        writer.WriteEndObject();
    }
}
