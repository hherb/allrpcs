using System.Text.Json;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Strictly materializes provider-neutral character-turn responses from JSON.</summary>
public static class CharacterTurnResponseJsonDeserializer
{
    private const int MaximumJsonDepth = 12;

    /// <summary>Gets the maximum accepted transport document length in UTF-16 code units.</summary>
    public const int MaximumDocumentLength = 32_000;

    /// <summary>Gets the maximum number of commitments accepted from one transport document.</summary>
    public const int MaximumCommitmentCount = 8;

    /// <summary>Gets the maximum intentions accepted in one transport commitment.</summary>
    public const int MaximumIntentionsPerCommitment = 8;

    /// <summary>
    /// Materializes one complete response for the exact request, rejecting unexpected structure.
    /// </summary>
    public static CharacterTurnResponse Deserialize(
        CharacterTurnRequest request,
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
            return ReadResponse(request, document.RootElement);
        }
        catch (Exception exception) when (
            exception is JsonException
                or InvalidDataException
                or ArgumentException
                or InvalidOperationException
                or OverflowException)
        {
            throw InvalidResponse(nameof(serialized), exception);
        }
    }

    private static CharacterTurnResponse ReadResponse(
        CharacterTurnRequest request,
        JsonElement element)
    {
        RequireObject(
            element,
            "disposition",
            "speech",
            "deliveryMode",
            "immediateIntention",
            "commitments",
            "stateProposals");
        return new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Parse(ReadString(element.GetProperty("disposition"))),
            ReadDelivery(
                request,
                element.GetProperty("speech"),
                element.GetProperty("deliveryMode")),
            ReadIntention(element.GetProperty("immediateIntention")),
            ReadCommitments(element.GetProperty("commitments")),
            ReadStateProposals(element.GetProperty("stateProposals")));
    }

    private static DialogueDelivery? ReadDelivery(
        CharacterTurnRequest request,
        JsonElement speechElement,
        JsonElement modeElement)
    {
        if (speechElement.ValueKind == JsonValueKind.Null
            && modeElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (speechElement.ValueKind != JsonValueKind.String
            || modeElement.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return new DialogueDelivery(
                new DialogueUtterance(
                    request.RespondingCharacterId,
                    ReadString(speechElement)),
                DialogueDeliveryMode.Parse(ReadString(modeElement)));
    }

    private static CharacterIntention ReadIntention(JsonElement element)
    {
        RequireObject(element, "type", "targetObjectId");
        JsonElement targetElement = element.GetProperty("targetObjectId");
        ObjectId? targetObjectId = targetElement.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => new ObjectId(ReadString(targetElement)),
            _ => throw InvalidDocument(),
        };
        CharacterIntention intention = new(
            CharacterIntentionType.Parse(ReadString(element.GetProperty("type"))),
            targetObjectId);
        if (!CharacterIntentionValidationPolicy.Validate(intention).IsValid)
        {
            throw InvalidDocument();
        }

        return intention;
    }

    private static List<CharacterCommitment> ReadCommitments(JsonElement element)
    {
        RequireArray(element);
        List<CharacterCommitment> commitments = new();
        foreach (JsonElement commitmentElement in element.EnumerateArray())
        {
            if (commitments.Count == MaximumCommitmentCount)
            {
                throw InvalidDocument();
            }

            RequireObject(commitmentElement, "proposedIntentions");
            JsonElement intentionsElement = commitmentElement.GetProperty("proposedIntentions");
            RequireArray(intentionsElement);
            List<CharacterIntention> intentions = new();
            foreach (JsonElement intentionElement in intentionsElement.EnumerateArray())
            {
                if (intentions.Count == MaximumIntentionsPerCommitment)
                {
                    throw InvalidDocument();
                }

                intentions.Add(ReadIntention(intentionElement));
            }

            commitments.Add(new CharacterCommitment(intentions));
        }

        return commitments;
    }

    private static List<CharacterStateProposal> ReadStateProposals(JsonElement element)
    {
        RequireArray(element);
        List<CharacterStateProposal> proposals = new();
        foreach (JsonElement proposalElement in element.EnumerateArray())
        {
            RequireObject(proposalElement, "key", "value");
            proposals.Add(new CharacterStateProposal(
                ReadString(proposalElement.GetProperty("key")),
                ReadString(proposalElement.GetProperty("value"))));
        }

        return proposals;
    }

    private static string ReadString(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return element.GetString() ?? throw InvalidDocument();
    }

    private static void RequireArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw InvalidDocument();
        }
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
        new("Unexpected character-turn response JSON structure or value.");

    private static ArgumentException InvalidResponse(
        string parameterName,
        Exception? innerException = null) => new(
            "Value must be a complete supported character-turn response JSON document.",
            parameterName,
            innerException);
}
