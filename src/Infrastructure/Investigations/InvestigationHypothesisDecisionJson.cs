using System.Text.Json;
using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Infrastructure.Investigations;

/// <summary>Writes the closed hypothesis-recognition schema and strictly reads its decisions.</summary>
internal static class InvestigationHypothesisDecisionJson
{
    internal const int MaximumDocumentLength = 512;
    private const string Property = "hypothesisId";

    internal static void WriteSchema(Utf8JsonWriter writer, InvestigationHypothesisRecognitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);

        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        writer.WritePropertyName(Property);
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStartArray();
        writer.WriteStringValue("string");
        writer.WriteStringValue("null");
        writer.WriteEndArray();
        writer.WritePropertyName("enum");
        writer.WriteStartArray();
        foreach (InvestigationHypothesis hypothesis in request.Hypotheses.Hypotheses)
        {
            writer.WriteStringValue(hypothesis.HypothesisId.Value);
        }

        writer.WriteNullValue();
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.WritePropertyName("required");
        writer.WriteStartArray();
        writer.WriteStringValue(Property);
        writer.WriteEndArray();
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }

    /// <remarks>Throws <see cref="ArgumentException"/> for anything but one offered id or null.</remarks>
    internal static InvestigationHypothesisDecision Deserialize(
        InvestigationHypothesisRecognitionRequest request,
        string serialized)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(serialized);
        if (serialized.Length > MaximumDocumentLength)
        {
            throw Invalid();
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(serialized, new JsonDocumentOptions { MaxDepth = 2 });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || root.EnumerateObject().Count() != 1
                || !root.TryGetProperty(Property, out JsonElement value))
            {
                throw Invalid();
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                return new InvestigationHypothesisDecision(null);
            }

            string? id = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
            InvestigationHypothesis hypothesis = request.Hypotheses.Hypotheses.SingleOrDefault(candidate =>
                string.Equals(candidate.HypothesisId.Value, id, StringComparison.Ordinal)) ?? throw Invalid();
            return new InvestigationHypothesisDecision(
                new InvestigationHypothesisProposal(hypothesis.Question.QuestionId, hypothesis.HypothesisId));
        }
        catch (JsonException)
        {
            throw Invalid();
        }
    }

    private static ArgumentException Invalid() =>
        new("The hypothesis-recognition decision does not match the strict schema.");
}
