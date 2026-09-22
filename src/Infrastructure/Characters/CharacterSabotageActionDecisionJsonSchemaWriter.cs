using System.Text.Json;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Writes one exact closed sabotage-action decision schema.</summary>
internal static class CharacterSabotageActionDecisionJsonSchemaWriter
{
    internal static void Write(
        Utf8JsonWriter writer,
        CharacterSabotageActionConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);

        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        WriteActorId(writer, request);
        WriteSelectedActionKind(writer, request);
        writer.WriteEndObject();
        writer.WritePropertyName("required");
        writer.WriteStartArray();
        writer.WriteStringValue("actorId");
        writer.WriteStringValue("selectedActionKind");
        writer.WriteEndArray();
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }

    private static void WriteActorId(
        Utf8JsonWriter writer,
        CharacterSabotageActionConsiderationRequest request)
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

    private static void WriteSelectedActionKind(
        Utf8JsonWriter writer,
        CharacterSabotageActionConsiderationRequest request)
    {
        writer.WritePropertyName("selectedActionKind");
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStartArray();
        writer.WriteStringValue("string");
        writer.WriteStringValue("null");
        writer.WriteEndArray();
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        foreach (CharacterSabotageActionKind actionKind in request.ActionKinds)
        {
            writer.WriteStringValue(actionKind.Value);
        }

        writer.WriteNullValue();
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
