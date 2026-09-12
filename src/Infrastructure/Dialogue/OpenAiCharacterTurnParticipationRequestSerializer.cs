using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Writes an OpenAI Responses request for one participation consideration.</summary>
public static class OpenAiCharacterTurnParticipationRequestSerializer
{
    /// <summary>Gets the maximum supported OpenAI model-name length.</summary>
    public const int MaximumModelNameLength = CharacterModelSettings.MaximumModelNameLength;

    /// <summary>Gets the bounded provider output-token request.</summary>
    public const int MaximumOutputTokens = 128;

    /// <summary>Serializes one stateless streaming request with strict structured output.</summary>
    public static string Serialize(
        CharacterTurnParticipationConsiderationRequest request,
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
            writer.WritePropertyName("text");
            writer.WriteStartObject();
            writer.WritePropertyName("format");
            writer.WriteStartObject();
            writer.WriteString("type", "json_schema");
            writer.WriteString("name", "character_turn_participation_decision");
            writer.WriteBoolean("strict", true);
            writer.WritePropertyName("schema");
            CharacterTurnParticipationDecisionJsonSchemaWriter.Write(
                writer,
                includeBounds: true);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteInput(
        Utf8JsonWriter writer,
        CharacterTurnParticipationConsiderationRequest request)
    {
        writer.WritePropertyName("input");
        writer.WriteStartArray();
        WriteMessage(writer, "developer", CharacterTurnParticipationRequestContent.DeveloperInstructions);
        WriteMessage(
            writer,
            "developer",
            CharacterTurnParticipationRequestContent.SerializeTrustedContext(request));
        WriteMessage(writer, "user", request.HeardDialogueEvent.Utterance.Text);
        writer.WriteEndArray();
    }

    private static void WriteMessage(Utf8JsonWriter writer, string role, string content)
    {
        writer.WriteStartObject();
        writer.WriteString("role", role);
        writer.WriteString("content", content);
        writer.WriteEndObject();
    }
}
