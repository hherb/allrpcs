using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Accumulates the bounded structured text from OpenAI Responses API semantic stream events.
/// </summary>
public sealed class OpenAiCharacterModelStreamAccumulator
{
    private const int MaximumEventDepth = 8;
    private readonly StringBuilder responseDocument = new();
    private readonly CharacterTurnRequest request;
    private bool hasVerifiedOutputTextDone;
    private bool isTerminated;

    /// <summary>Gets the maximum accepted serialized event length.</summary>
    public const int MaximumEventLength = 64_000;

    /// <summary>Creates an accumulator bound to one exact character-turn request.</summary>
    public OpenAiCharacterModelStreamAccumulator(CharacterTurnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        this.request = request;
    }

    /// <summary>Gets the bounded terminal input-token count, or unknown.</summary>
    public int? InputTokenCount { get; private set; }

    /// <summary>Gets the bounded terminal output-token count, or unknown.</summary>
    public int? OutputTokenCount { get; private set; }

    /// <summary>Gets the bounded terminal total-token count, or unknown.</summary>
    public int? TotalTokenCount { get; private set; }

    /// <summary>
    /// Applies one serialized semantic event and returns a terminal update when the stream ends.
    /// Structured JSON deltas remain buffered and are never exposed as display text.
    /// </summary>
    public CharacterModelStreamTermination? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (isTerminated)
        {
            throw new InvalidOperationException("The OpenAI response stream is already terminal.");
        }

        if (serializedEvent.Length > MaximumEventLength)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                serializedEvent,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumEventDepth,
                });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("type", out JsonElement typeElement)
                || typeElement.ValueKind != JsonValueKind.String)
            {
                return Terminate(CharacterModelFailureCode.InvalidResponse);
            }

            return ApplyTypedEvent(typeElement.GetString()!, root);
        }
        catch (JsonException)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }
    }

    private CharacterModelStreamTermination? ApplyTypedEvent(
        string type,
        JsonElement root) => type switch
        {
            "response.created" or
            "response.in_progress" or
            "response.output_item.added" or
            "response.output_item.done" or
            "response.content_part.added" or
            "response.content_part.done" => null,
            "response.output_text.delta" => ApplyDelta(root),
            "response.output_text.done" => ApplyDone(root),
            "response.completed" => Complete(root),
            "response.incomplete" => Terminate(CharacterModelFailureCode.InvalidResponse),
            "response.refusal.delta" or
            "response.refusal.done" or
            "response.failed" or
            "error" => Terminate(CharacterModelFailureCode.ProviderRejected),
            _ => Terminate(CharacterModelFailureCode.InvalidResponse),
        };

    private CharacterModelStreamTermination? ApplyDelta(JsonElement root)
    {
        if (hasVerifiedOutputTextDone
            || !root.TryGetProperty("delta", out JsonElement deltaElement)
            || deltaElement.ValueKind != JsonValueKind.String)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        string delta = deltaElement.GetString()!;
        if (responseDocument.Length + delta.Length
            > CharacterTurnResponseJsonDeserializer.MaximumDocumentLength)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        responseDocument.Append(delta);
        return null;
    }

    private CharacterModelStreamTermination? ApplyDone(JsonElement root)
    {
        if (hasVerifiedOutputTextDone
            || !root.TryGetProperty("text", out JsonElement textElement)
            || textElement.ValueKind != JsonValueKind.String
            || !string.Equals(
                responseDocument.ToString(),
                textElement.GetString(),
                StringComparison.Ordinal))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        hasVerifiedOutputTextDone = true;
        return null;
    }

    private CharacterModelStreamTermination Complete(JsonElement root)
    {
        if (!hasVerifiedOutputTextDone || !TryReadUsage(root))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        isTerminated = true;
        return CharacterModelJsonResponseCompletion.Create(request, responseDocument.ToString());
    }

    private bool TryReadUsage(JsonElement root)
    {
        if (!root.TryGetProperty("response", out JsonElement response))
        {
            return true;
        }

        if (response.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!response.TryGetProperty("usage", out JsonElement usage)
            || usage.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (usage.ValueKind != JsonValueKind.Object
            || !TryReadTokenCount(usage, "input_tokens", out int inputTokenCount)
            || !TryReadTokenCount(usage, "output_tokens", out int outputTokenCount)
            || !TryReadTokenCount(usage, "total_tokens", out int totalTokenCount)
            || (long)inputTokenCount + outputTokenCount != totalTokenCount)
        {
            return false;
        }

        InputTokenCount = inputTokenCount;
        OutputTokenCount = outputTokenCount;
        TotalTokenCount = totalTokenCount;
        return true;
    }

    private static bool TryReadTokenCount(
        JsonElement usage,
        string propertyName,
        out int tokenCount)
    {
        tokenCount = 0;
        int matchingPropertyCount = 0;
        foreach (JsonProperty property in usage.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.Ordinal))
            {
                continue;
            }

            matchingPropertyCount++;
            if (matchingPropertyCount > 1
                || property.Value.ValueKind != JsonValueKind.Number
                || !property.Value.TryGetInt32(out tokenCount)
                || tokenCount < 0
                || tokenCount > CharacterModelGenerationDiagnostic.MaximumTokenCount)
            {
                return false;
            }
        }

        return matchingPropertyCount == 1;
    }

    private CharacterModelStreamTermination Terminate(CharacterModelFailureCode code)
    {
        isTerminated = true;
        return new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));
    }
}
