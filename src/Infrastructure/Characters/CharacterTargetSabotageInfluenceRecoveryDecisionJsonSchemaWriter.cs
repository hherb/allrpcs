using System.Text.Json;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Writes one exact closed influence-recovery-decision schema.</summary>
internal static class CharacterTargetSabotageInfluenceRecoveryDecisionJsonSchemaWriter
{
    internal static void Write(
        Utf8JsonWriter writer,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);

        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        WriteActorId(writer, request);
        WriteSelectedCandidateIndex(writer, request);
        writer.WriteEndObject();
        writer.WritePropertyName("required");
        writer.WriteStartArray();
        writer.WriteStringValue("actorId");
        writer.WriteStringValue("selectedCandidateIndex");
        writer.WriteEndArray();
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }

    private static void WriteActorId(
        Utf8JsonWriter writer,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request)
    {
        writer.WritePropertyName("actorId");
        writer.WriteStartObject();
        writer.WriteString("type", "string");
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        writer.WriteStringValue(request.Actor.Identity.Id.Value);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteSelectedCandidateIndex(
        Utf8JsonWriter writer,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request)
    {
        writer.WritePropertyName("selectedCandidateIndex");
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStartArray();
        writer.WriteStringValue("integer");
        writer.WriteStringValue("null");
        writer.WriteEndArray();
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        for (int index = 0; index < request.Candidates.Count; index++)
        {
            writer.WriteNumberValue(index);
        }

        writer.WriteNullValue();
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
