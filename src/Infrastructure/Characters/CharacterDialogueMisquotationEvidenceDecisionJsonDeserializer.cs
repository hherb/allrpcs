using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Strictly materializes provider-neutral misquotation-evidence decisions from JSON.</summary>
public static class CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer
{
    private const int MaximumJsonDepth = 4;

    /// <summary>Gets the maximum accepted transport document length in UTF-16 code units.</summary>
    public const int MaximumDocumentLength = 4_096;

    /// <summary>Materializes one accepted decision for the exact consideration request.</summary>
    public static CharacterDialogueMisquotationEvidenceDecision Deserialize(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        string serialized)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(serialized);

        if (serialized.Length > MaximumDocumentLength)
        {
            throw InvalidResponse(nameof(serialized));
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                serialized,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth,
                });
            return ReadDecision(request, document.RootElement);
        }
        catch (Exception exception) when (
            exception is JsonException
                or InvalidDataException
                or ArgumentException
                or InvalidOperationException)
        {
            throw InvalidResponse(nameof(serialized), exception);
        }
    }

    private static CharacterDialogueMisquotationEvidenceDecision ReadDecision(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        JsonElement element)
    {
        RequireObject(element, "actorId", "selectedEvidenceIndex");

        CharacterId parsedActorId = CharacterId.Parse(
            ReadString(element.GetProperty("actorId")));
        CharacterId actorId = parsedActorId == request.Actor.Identity.Id
            ? request.Actor.Identity.Id
            : parsedActorId;
        int? selectedEvidenceIndex = ReadNullableInt32(
            element.GetProperty("selectedEvidenceIndex"));
        CharacterDialogueMisquotationEvidence? selectedEvidence =
            selectedEvidenceIndex is null
                ? null
                : ResolveEvidence(request, selectedEvidenceIndex.Value);

        CharacterDialogueMisquotationEvidenceDecision decision = new(actorId, selectedEvidence);
        if (!CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(request, decision))
        {
            throw InvalidDocument();
        }

        return decision;
    }

    private static CharacterDialogueMisquotationEvidence ResolveEvidence(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        int selectedEvidenceIndex)
    {
        if ((uint)selectedEvidenceIndex >= (uint)request.Candidates.Count)
        {
            throw InvalidDocument();
        }

        return request.Candidates[selectedEvidenceIndex];
    }

    private static string ReadString(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return element.GetString() ?? throw InvalidDocument();
    }

    private static int? ReadNullableInt32(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (element.ValueKind != JsonValueKind.Number || !element.TryGetInt32(out int value))
        {
            throw InvalidDocument();
        }

        return value;
    }

    private static void RequireObject(JsonElement element, params string[] propertyNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw InvalidDocument();
        }

        HashSet<string> expected = new(propertyNames, StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !actual.Add(property.Name))
            {
                throw InvalidDocument();
            }
        }

        if (!actual.SetEquals(expected))
        {
            throw InvalidDocument();
        }
    }

    private static InvalidDataException InvalidDocument() =>
        new("Unexpected misquotation-evidence-decision JSON structure or value.");

    private static ArgumentException InvalidResponse(
        string parameterName,
        Exception? innerException = null) => new(
            "Value must be a complete supported misquotation-evidence-decision JSON document.",
            parameterName,
            innerException);
}
