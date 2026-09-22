using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Strictly materializes provider-neutral sabotage-action decisions from JSON.</summary>
public static class CharacterSabotageActionDecisionJsonDeserializer
{
    private const int MaximumJsonDepth = 4;

    /// <summary>Gets the maximum accepted transport document length in UTF-16 code units.</summary>
    public const int MaximumDocumentLength = 4_096;

    /// <summary>Materializes one accepted decision for the exact consideration request.</summary>
    public static CharacterSabotageActionDecision Deserialize(
        CharacterSabotageActionConsiderationRequest request,
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

    private static CharacterSabotageActionDecision ReadDecision(
        CharacterSabotageActionConsiderationRequest request,
        JsonElement element)
    {
        RequireObject(element, "actorId", "selectedActionKind");

        CharacterId parsedActorId = CharacterId.Parse(ReadString(element.GetProperty("actorId")));
        CharacterId actorId = parsedActorId == request.Actor.Identity.Id
            ? request.Actor.Identity.Id
            : parsedActorId;
        CharacterSabotageActionKind? selectedActionKind = ReadSelectedActionKind(
            request,
            element.GetProperty("selectedActionKind"));

        CharacterSabotageActionDecision decision = new(actorId, selectedActionKind);
        if (!CharacterSabotageActionDecisionPolicy.CanAccept(request, decision))
        {
            throw InvalidDocument();
        }

        return decision;
    }

    private static CharacterSabotageActionKind? ReadSelectedActionKind(
        CharacterSabotageActionConsiderationRequest request,
        JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        string selectedActionKindText = ReadString(element);
        CharacterSabotageActionKind parsedActionKind = CharacterSabotageActionKind.Parse(
            selectedActionKindText);
        return request.ActionKinds.FirstOrDefault(actionKind =>
                ReferenceEquals(actionKind, parsedActionKind))
            ?? throw InvalidDocument();
    }

    private static string ReadString(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return element.GetString() ?? throw InvalidDocument();
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
        new("Unexpected sabotage-action decision JSON structure or value.");

    private static ArgumentException InvalidResponse(
        string parameterName,
        Exception? innerException = null) => new(
            "Value must be a complete supported sabotage-action decision JSON document.",
            parameterName,
            innerException);
}
