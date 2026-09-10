using System.Text.Json;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Writes the closed character-turn response schema for one provider dialect.</summary>
internal static class CharacterTurnResponseJsonSchemaWriter
{
    internal static void Write(Utf8JsonWriter writer, bool includeBounds)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        WriteStringEnum(writer, "disposition", CharacterTurnDisposition.All.Select(value => value.Value));
        WriteNullableString(
            writer,
            "speech",
            DialogueUtterance.MaximumTextLength,
            includeBounds);
        writer.WritePropertyName("immediateIntention");
        WriteIntentionSchema(writer, includeBounds);
        WriteCommitmentsSchema(writer, includeBounds);
        WriteStateProposalsSchema(writer, includeBounds);
        writer.WriteEndObject();
        WriteRequired(
            writer,
            "disposition",
            "speech",
            "immediateIntention",
            "commitments",
            "stateProposals");
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }

    private static void WriteIntentionSchema(
        Utf8JsonWriter writer,
        bool includeBounds)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("anyOf");
        writer.WriteStartArray();
        WriteIntentionVariant(
            writer,
            [CharacterIntentionType.Wait.Value],
            targetIsNull: true,
            includeBounds);
        WriteIntentionVariant(
            writer,
            [CharacterIntentionType.ExamineObject.Value, CharacterIntentionType.CollectObject.Value],
            targetIsNull: false,
            includeBounds);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteIntentionVariant(
        Utf8JsonWriter writer,
        IEnumerable<string> intentionTypes,
        bool targetIsNull,
        bool includeBounds)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        WriteStringEnum(writer, "type", intentionTypes);
        if (targetIsNull)
        {
            writer.WritePropertyName("targetObjectId");
            writer.WriteStartObject();
            writer.WriteString("type", "null");
            writer.WriteEndObject();
        }
        else
        {
            WriteBoundedString(
                writer,
                "targetObjectId",
                CanonicalIdentifierText.MaximumLength,
                includeBounds);
        }

        writer.WriteEndObject();
        WriteRequired(writer, "type", "targetObjectId");
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }

    private static void WriteCommitmentsSchema(
        Utf8JsonWriter writer,
        bool includeBounds)
    {
        writer.WritePropertyName("commitments");
        writer.WriteStartObject();
        writer.WriteString("type", "array");
        if (includeBounds)
        {
            writer.WriteNumber(
                "maxItems",
                CharacterTurnResponseJsonDeserializer.MaximumCommitmentCount);
        }

        writer.WritePropertyName("items");
        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        writer.WritePropertyName("proposedIntentions");
        writer.WriteStartObject();
        writer.WriteString("type", "array");
        writer.WriteNumber("minItems", 1);
        if (includeBounds)
        {
            writer.WriteNumber(
                "maxItems",
                CharacterTurnResponseJsonDeserializer.MaximumIntentionsPerCommitment);
        }

        writer.WritePropertyName("items");
        WriteIntentionSchema(writer, includeBounds);
        writer.WriteEndObject();
        writer.WriteEndObject();
        WriteRequired(writer, "proposedIntentions");
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteStateProposalsSchema(
        Utf8JsonWriter writer,
        bool includeBounds)
    {
        writer.WritePropertyName("stateProposals");
        writer.WriteStartObject();
        writer.WriteString("type", "array");
        if (includeBounds)
        {
            writer.WriteNumber("maxItems", CharacterTurnResponse.MaximumStateProposalCount);
        }

        writer.WritePropertyName("items");
        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        WriteBoundedString(
            writer,
            "key",
            CanonicalIdentifierText.MaximumLength,
            includeBounds);
        WriteBoundedString(
            writer,
            "value",
            CharacterStateProposal.MaximumValueLength,
            includeBounds);
        writer.WriteEndObject();
        WriteRequired(writer, "key", "value");
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteStringEnum(
        Utf8JsonWriter writer,
        string propertyName,
        IEnumerable<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartObject();
        writer.WriteString("type", "string");
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        foreach (string value in values)
        {
            writer.WriteStringValue(value);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteNullableString(
        Utf8JsonWriter writer,
        string propertyName,
        int maximumLength,
        bool includeBounds)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStartArray();
        writer.WriteStringValue("string");
        writer.WriteStringValue("null");
        writer.WriteEndArray();
        if (includeBounds)
        {
            writer.WriteNumber("maxLength", maximumLength);
        }

        writer.WriteEndObject();
    }

    private static void WriteBoundedString(
        Utf8JsonWriter writer,
        string propertyName,
        int maximumLength,
        bool includeBounds)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartObject();
        writer.WriteString("type", "string");
        if (includeBounds)
        {
            writer.WriteNumber("minLength", 1);
            writer.WriteNumber("maxLength", maximumLength);
        }

        writer.WriteEndObject();
    }

    private static void WriteRequired(Utf8JsonWriter writer, params string[] propertyNames)
    {
        writer.WritePropertyName("required");
        writer.WriteStartArray();
        foreach (string propertyName in propertyNames)
        {
            writer.WriteStringValue(propertyName);
        }

        writer.WriteEndArray();
    }
}
