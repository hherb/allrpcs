using System.Text.Json;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Writes the closed participation-decision schema for one provider dialect.</summary>
internal static class CharacterTurnParticipationDecisionJsonSchemaWriter
{
    internal static void Write(Utf8JsonWriter writer, bool includeBounds)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        writer.WritePropertyName("kind");
        writer.WriteStartObject();
        writer.WriteString("type", "string");
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        foreach (CharacterTurnParticipationKind kind in CharacterTurnParticipationKind.All)
        {
            writer.WriteStringValue(kind.Value);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WritePropertyName("motiveKey");
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStartArray();
        writer.WriteStringValue("string");
        writer.WriteStringValue("null");
        writer.WriteEndArray();
        if (includeBounds)
        {
            writer.WriteNumber("minLength", 1);
            writer.WriteNumber("maxLength", CanonicalIdentifierText.MaximumLength);
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.WritePropertyName("required");
        writer.WriteStartArray();
        writer.WriteStringValue("kind");
        writer.WriteStringValue("motiveKey");
        writer.WriteEndArray();
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }
}
