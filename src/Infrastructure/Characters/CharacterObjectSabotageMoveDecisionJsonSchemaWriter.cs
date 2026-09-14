using System.Text.Json;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Writes one exact closed move-decision schema.</summary>
internal static class CharacterObjectSabotageMoveDecisionJsonSchemaWriter
{
    internal static void Write(
        Utf8JsonWriter writer,
        CharacterObjectSabotageMoveConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);

        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        WriteCharacterId(writer, request);
        WriteSelectedEvidenceId(writer, request);
        writer.WriteEndObject();
        writer.WritePropertyName("required");
        writer.WriteStartArray();
        writer.WriteStringValue("characterId");
        writer.WriteStringValue("selectedEvidenceId");
        writer.WriteEndArray();
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }

    private static void WriteCharacterId(
        Utf8JsonWriter writer,
        CharacterObjectSabotageMoveConsiderationRequest request)
    {
        writer.WritePropertyName("characterId");
        writer.WriteStartObject();
        writer.WriteString("type", "string");
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        writer.WriteStringValue(request.Character.Identity.Id.Value);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteSelectedEvidenceId(
        Utf8JsonWriter writer,
        CharacterObjectSabotageMoveConsiderationRequest request)
    {
        writer.WritePropertyName("selectedEvidenceId");
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStartArray();
        writer.WriteStringValue("string");
        writer.WriteStringValue("null");
        writer.WriteEndArray();
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        foreach (CharacterObjectSabotageMoveCandidate candidate in request.Candidates)
        {
            writer.WriteStringValue(candidate.Evidence.Evidence.EvidenceId.Value);
        }

        writer.WriteNullValue();
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
