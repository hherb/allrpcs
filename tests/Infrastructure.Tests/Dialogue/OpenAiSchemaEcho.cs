using System.Text;
using System.Text.Json;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public static class OpenAiSchemaEcho
{
    /// <summary>A Responses lifecycle event that echoes the real strict character-turn schema.</summary>
    public static string RealEvent(string eventType, string status)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("type", eventType);
            writer.WriteNumber("sequence_number", 0);
            writer.WriteStartObject("response");
            writer.WriteString("id", "resp_fixture");
            writer.WriteString("object", "response");
            writer.WriteString("status", status);
            writer.WriteString("model", "fixture-model");
            writer.WriteStartArray("output");
            writer.WriteEndArray();
            writer.WriteStartObject("text");
            writer.WriteStartObject("format");
            writer.WriteString("type", "json_schema");
            writer.WriteString("name", "character_turn_response");
            writer.WriteBoolean("strict", true);
            writer.WritePropertyName("schema");
            CharacterTurnResponseJsonSchemaWriter.Write(writer, includeBounds: true);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string Event(string eventType, int depth)
    {
        string schema = "{}";
        for (int level = 5; level < depth; level++)
        {
            schema = "{\"properties\":" + schema + "}";
        }

        return "{\"type\":\"" + eventType
            + "\",\"response\":{\"text\":{\"format\":{\"schema\":" + schema + "}}}}";
    }

    public static TheoryData<string> InvalidEvents => new()
    {
        Event("response.created", 65),
        "{\"type\":\"response.created\",/*comment*/\"response\":{}}",
        "{\"type\":\"response.created\",\"response\":{},}",
    };
}
