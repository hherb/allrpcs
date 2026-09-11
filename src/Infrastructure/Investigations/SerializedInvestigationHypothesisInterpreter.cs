using System.Text.Json;
using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Investigations;

/// <summary>Interprets one bounded request through a caller-supplied serialized transport.</summary>
public sealed class SerializedInvestigationHypothesisInterpreter
    : IInvestigationHypothesisInterpreter
{
    /// <summary>Gets the maximum accepted response length in UTF-16 code units.</summary>
    public const int MaximumResponseDocumentLength = 512;

    private const string InvalidResponseMessage =
        "The hypothesis-recognition response does not match the strict schema.";

    private readonly IInvestigationHypothesisTransport transport;

    /// <summary>Creates an offline-safe interpreter over one supplied transport.</summary>
    public SerializedInvestigationHypothesisInterpreter(IInvestigationHypothesisTransport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        this.transport = transport;
    }

    /// <inheritdoc />
    public async ValueTask<InvestigationHypothesisProposal?> InterpretAsync(
        InvestigationHypothesisRecognitionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string requestDocument = InvestigationHypothesisRequestJsonSerializer.Serialize(request);
        string? responseDocument = await transport
            .CompleteAsync(requestDocument, cancellationToken)
            .ConfigureAwait(false);

        return Deserialize(responseDocument);
    }

    private static InvestigationHypothesisProposal? Deserialize(string? responseDocument)
    {
        if (string.IsNullOrWhiteSpace(responseDocument)
            || responseDocument.Length > MaximumResponseDocumentLength)
        {
            throw InvalidResponse();
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(responseDocument);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw InvalidResponse();
            }

            Dictionary<string, JsonElement> properties = ReadUniqueProperties(root);
            string status = ReadRequiredString(properties, "status");
            if (string.Equals(status, "no-match", StringComparison.Ordinal))
            {
                if (properties.Count != 1)
                {
                    throw InvalidResponse();
                }

                return null;
            }

            if (!string.Equals(status, "match", StringComparison.Ordinal)
                || properties.Count != 3)
            {
                throw InvalidResponse();
            }

            string questionId = ReadRequiredString(properties, "questionId");
            string hypothesisId = ReadRequiredString(properties, "hypothesisId");
            return new InvestigationHypothesisProposal(
                InvestigationQuestionId.Parse(questionId),
                InvestigationHypothesisId.Parse(hypothesisId));
        }
        catch (JsonException)
        {
            throw InvalidResponse();
        }
        catch (ArgumentException exception) when (
            !string.Equals(exception.Message, InvalidResponseMessage, StringComparison.Ordinal))
        {
            throw InvalidResponse();
        }
    }

    private static Dictionary<string, JsonElement> ReadUniqueProperties(JsonElement root)
    {
        Dictionary<string, JsonElement> properties = new(StringComparer.Ordinal);
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!properties.TryAdd(property.Name, property.Value))
            {
                throw InvalidResponse();
            }
        }

        return properties;
    }

    private static string ReadRequiredString(
        Dictionary<string, JsonElement> properties,
        string propertyName)
    {
        if (!properties.TryGetValue(propertyName, out JsonElement value)
            || value.ValueKind != JsonValueKind.String)
        {
            throw InvalidResponse();
        }

        return value.GetString() ?? throw InvalidResponse();
    }

    private static ArgumentException InvalidResponse() => new(InvalidResponseMessage);
}
