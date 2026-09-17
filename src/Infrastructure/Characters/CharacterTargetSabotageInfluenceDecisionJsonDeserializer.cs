using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Strictly materializes provider-neutral influence decisions from JSON.</summary>
public static class CharacterTargetSabotageInfluenceDecisionJsonDeserializer
{
    private const int MaximumJsonDepth = 4;

    /// <summary>Gets the maximum accepted transport document length in UTF-16 code units.</summary>
    public const int MaximumDocumentLength = 4_096;

    /// <summary>Materializes one accepted decision for the exact consideration request.</summary>
    public static CharacterTargetSabotageInfluenceDecision Deserialize(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
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

    private static CharacterTargetSabotageInfluenceDecision ReadDecision(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        JsonElement element)
    {
        RequireObject(element, "actorId", "selectedCandidateIndex");

        CharacterId parsedActorId = CharacterId.Parse(ReadString(element.GetProperty("actorId")));
        CharacterId actorId = parsedActorId == request.Actor.Identity.Id
            ? request.Actor.Identity.Id
            : parsedActorId;
        int? selectedCandidateIndex = ReadNullableInt32(
            element.GetProperty("selectedCandidateIndex"));
        CharacterTargetSabotageInfluenceCandidate? selectedCandidate = selectedCandidateIndex is null
            ? null
            : ResolveCandidate(request, selectedCandidateIndex.Value);

        CharacterTargetSabotageInfluenceDecision decision = new(actorId, selectedCandidate);
        if (!CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(request, decision))
        {
            throw InvalidDocument();
        }

        return decision;
    }

    private static CharacterTargetSabotageInfluenceCandidate ResolveCandidate(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        int selectedCandidateIndex)
    {
        if ((uint)selectedCandidateIndex >= (uint)request.Candidates.Count)
        {
            throw InvalidDocument();
        }

        return request.Candidates[selectedCandidateIndex];
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
        new("Unexpected influence-decision JSON structure or value.");

    private static ArgumentException InvalidResponse(
        string parameterName,
        Exception? innerException = null) => new(
            "Value must be a complete supported influence-decision JSON document.",
            parameterName,
            innerException);
}
