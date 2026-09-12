using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Strictly materializes provider-neutral participation decisions from JSON.</summary>
public static class CharacterTurnParticipationDecisionJsonDeserializer
{
    private const int MaximumJsonDepth = 4;

    /// <summary>Gets the maximum accepted transport document length in UTF-16 code units.</summary>
    public const int MaximumDocumentLength = 4_096;

    /// <summary>Materializes one accepted decision for the exact consideration request.</summary>
    public static CharacterTurnParticipationDecision Deserialize(
        CharacterTurnParticipationConsiderationRequest request,
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

    private static CharacterTurnParticipationDecision ReadDecision(
        CharacterTurnParticipationConsiderationRequest request,
        JsonElement element)
    {
        RequireObject(element, "kind", "motiveKey");
        CharacterTurnParticipationKind kind = CharacterTurnParticipationKind.Parse(
            ReadString(element.GetProperty("kind")));
        string? motiveKey = ReadNullableString(element.GetProperty("motiveKey"));
        CharacterGoal? motive = motiveKey is null
            ? null
            : request.ScheduledCharacter.Goals.Entries.SingleOrDefault(
                goal => string.Equals(goal.Key, motiveKey, StringComparison.Ordinal));
        if (motiveKey is not null && motive is null)
        {
            throw InvalidDocument();
        }

        CharacterTurnParticipationDecision decision = new(
            request.ScheduledCharacter.Identity.Id,
            kind,
            motive);

        if (!CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision))
        {
            throw InvalidDocument();
        }

        return decision;
    }

    private static string ReadString(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return element.GetString() ?? throw InvalidDocument();
    }

    private static string? ReadNullableString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => element.GetString() ?? throw InvalidDocument(),
        _ => throw InvalidDocument(),
    };

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
        new("Unexpected participation-decision JSON structure or value.");

    private static ArgumentException InvalidResponse(
        string parameterName,
        Exception? innerException = null) => new(
            "Value must be a complete supported participation-decision JSON document.",
            parameterName,
            innerException);
}
