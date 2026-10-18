using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Maps trusted decision content to the three existing provider wire protocols.</summary>
internal static class StructuredDecisionRequestSerializer
{
    internal static string Serialize<TRequest>(
        TRequest request, CharacterModelProvider provider, string modelName,
        string name, string instructions, Func<TRequest, string> context,
        Action<Utf8JsonWriter, TRequest> writeSchema, int maximumOutputTokens) where TRequest : class
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(provider);
        CharacterModelSettings.ValidateModelName(modelName);
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", modelName);
            if (provider == CharacterModelProvider.OpenAi) writer.WriteBoolean("store", false);
            writer.WriteBoolean("stream", true);
            if (provider == CharacterModelProvider.Ollama) writer.WriteBoolean("think", false);
            else writer.WriteNumber(provider == CharacterModelProvider.OpenAi ? "max_output_tokens" : "max_tokens",
                maximumOutputTokens);

            if (provider == CharacterModelProvider.Anthropic)
            {
                writer.WriteString("system", instructions);
                writer.WritePropertyName("messages");
                writer.WriteStartArray();
                WriteMessage(writer, "user", context(request));
            }
            else
            {
                writer.WritePropertyName(provider == CharacterModelProvider.OpenAi ? "input" : "messages");
                writer.WriteStartArray();
                string role = provider == CharacterModelProvider.OpenAi ? "developer" : "system";
                WriteMessage(writer, role, instructions);
                WriteMessage(writer, role, context(request));
            }
            writer.WriteEndArray();

            if (provider == CharacterModelProvider.Ollama)
            {
                writer.WritePropertyName("format");
                writeSchema(writer, request);
                writer.WritePropertyName("options");
                writer.WriteStartObject();
                writer.WriteNumber("temperature", 0);
                writer.WriteNumber("num_predict", maximumOutputTokens);
                writer.WriteEndObject();
            }
            else
            {
                writer.WritePropertyName(provider == CharacterModelProvider.OpenAi ? "text" : "output_config");
                writer.WriteStartObject();
                writer.WritePropertyName("format");
                writer.WriteStartObject();
                writer.WriteString("type", "json_schema");
                if (provider == CharacterModelProvider.OpenAi)
                {
                    writer.WriteString("name", name);
                    writer.WriteBoolean("strict", true);
                }
                writer.WritePropertyName("schema");
                writeSchema(writer, request);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteMessage(Utf8JsonWriter writer, string role, string text)
    {
        writer.WriteStartObject();
        writer.WriteString("role", role);
        writer.WriteString("content", text);
        writer.WriteEndObject();
    }
}
