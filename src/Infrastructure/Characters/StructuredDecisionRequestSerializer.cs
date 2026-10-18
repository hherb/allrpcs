using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Maps trusted decision content to the three existing provider wire protocols.</summary>
internal static class StructuredDecisionRequestSerializer
{
    /// <summary>Bounds Anthropic output like the existing Anthropic dialogue request does.</summary>
    /// <remarks>Current Claude models think by default and thinking counts against <c>max_tokens</c>;
    /// the schema-constrained decision itself still ends at <c>end_turn</c>.</remarks>
    internal const int AnthropicMaximumOutputTokens = 4_096;

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
            if (provider == CharacterModelProvider.OpenAi) WriteOpenAi(writer, request, name, instructions, context, writeSchema, maximumOutputTokens);
            else if (provider == CharacterModelProvider.Anthropic) WriteAnthropic(writer, request, instructions, context, writeSchema);
            else if (provider == CharacterModelProvider.Ollama) WriteOllama(writer, request, instructions, context, writeSchema, maximumOutputTokens);
            else throw new ArgumentOutOfRangeException(nameof(provider), "Unsupported character-model provider.");
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteOpenAi<TRequest>(Utf8JsonWriter writer, TRequest request, string name, string instructions,
        Func<TRequest, string> context, Action<Utf8JsonWriter, TRequest> writeSchema, int maximumOutputTokens)
    {
        writer.WriteBoolean("store", false);
        writer.WriteBoolean("stream", true);
        writer.WriteNumber("max_output_tokens", maximumOutputTokens);
        writer.WritePropertyName("input");
        writer.WriteStartArray();
        WriteMessage(writer, "developer", instructions);
        WriteMessage(writer, "developer", context(request));
        writer.WriteEndArray();
        writer.WritePropertyName("text");
        writer.WriteStartObject();
        writer.WritePropertyName("format");
        writer.WriteStartObject();
        writer.WriteString("type", "json_schema");
        writer.WriteString("name", name);
        writer.WriteBoolean("strict", true);
        writer.WritePropertyName("schema");
        writeSchema(writer, request);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteAnthropic<TRequest>(Utf8JsonWriter writer, TRequest request, string instructions,
        Func<TRequest, string> context, Action<Utf8JsonWriter, TRequest> writeSchema)
    {
        writer.WriteBoolean("stream", true);
        writer.WriteNumber("max_tokens", AnthropicMaximumOutputTokens);
        writer.WriteString("system", instructions);
        writer.WritePropertyName("messages");
        writer.WriteStartArray();
        WriteMessage(writer, "user", context(request));
        writer.WriteEndArray();
        writer.WritePropertyName("output_config");
        writer.WriteStartObject();
        writer.WritePropertyName("format");
        writer.WriteStartObject();
        writer.WriteString("type", "json_schema");
        writer.WritePropertyName("schema");
        writeSchema(writer, request);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteOllama<TRequest>(Utf8JsonWriter writer, TRequest request, string instructions,
        Func<TRequest, string> context, Action<Utf8JsonWriter, TRequest> writeSchema, int maximumOutputTokens)
    {
        writer.WriteBoolean("stream", true);
        writer.WriteBoolean("think", false);
        writer.WritePropertyName("messages");
        writer.WriteStartArray();
        WriteMessage(writer, "system", instructions);
        // Some chat templates render system text only inside a user turn.
        WriteMessage(writer, "user", context(request));
        writer.WriteEndArray();
        writer.WritePropertyName("format");
        writeSchema(writer, request);
        writer.WritePropertyName("options");
        writer.WriteStartObject();
        writer.WriteNumber("temperature", 0);
        writer.WriteNumber("num_predict", maximumOutputTokens);
        writer.WriteEndObject();
    }

    private static void WriteMessage(Utf8JsonWriter writer, string role, string text)
    {
        writer.WriteStartObject();
        writer.WriteString("role", role);
        writer.WriteString("content", text);
        writer.WriteEndObject();
    }
}
