using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Writes a bounded Ollama chat request from provider-neutral character-turn context.</summary>
public static class OllamaCharacterModelRequestSerializer
{
    /// <summary>Gets the maximum supported Ollama model-name length.</summary>
    public const int MaximumModelNameLength = CharacterModelSettings.MaximumModelNameLength;

    /// <summary>Serializes one stateless streaming structured-output request.</summary>
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
            writer.WriteBoolean("stream", true);
            writer.WriteBoolean("think", false);
            writer.WritePropertyName("messages");
            writer.WriteStartArray();
            WriteMessage(writer, "system", CharacterModelRequestContent.DeveloperInstructions);
            WriteMessage(
                writer,
                "system",
                CharacterModelRequestContent.SerializeTrustedContext(request));
            WriteMessage(writer, "user", request.TurnRequest.DialogueEvent.Utterance.Text);
            writer.WriteEndArray();
            writer.WritePropertyName("format");
            CharacterTurnResponseJsonSchemaWriter.Write(writer, includeBounds: false);
            writer.WritePropertyName("options");
            writer.WriteStartObject();
            writer.WriteNumber("temperature", 0);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
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
