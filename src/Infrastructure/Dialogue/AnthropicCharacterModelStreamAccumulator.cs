using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Accumulates one bounded structured response from Anthropic Messages events.</summary>
public sealed class AnthropicCharacterModelStreamAccumulator
{
    private const int MaximumEventDepth = 8;
    private readonly StringBuilder responseDocument = new();
    private readonly CharacterTurnRequest request;
    private bool hasContentBlockStarted;
    private bool hasContentBlockStopped;
    private bool hasMessageDelta;
    private bool hasMessageStarted;
    private bool isTerminated;

    /// <summary>Gets the maximum accepted serialized event length.</summary>
    public const int MaximumEventLength = 64_000;

    /// <summary>Creates an accumulator bound to one exact character-turn request.</summary>
    public AnthropicCharacterModelStreamAccumulator(CharacterTurnRequest request)
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

    /// <summary>Applies one Messages event and returns a terminal update when complete.</summary>
    public CharacterModelStreamTermination? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (isTerminated)
        {
            throw new InvalidOperationException("The Anthropic response stream is already terminal.");
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
            "ping" => null,
            "message_start" => ApplyMessageStart(root),
            "content_block_start" => ApplyContentBlockStart(root),
            "content_block_delta" => ApplyContentBlockDelta(root),
            "content_block_stop" => ApplyContentBlockStop(root),
            "message_delta" => ApplyMessageDelta(root),
            "message_stop" => Complete(),
            "error" => Terminate(CharacterModelFailureCode.ProviderRejected),
            _ => Terminate(CharacterModelFailureCode.InvalidResponse),
        };

    private CharacterModelStreamTermination? ApplyMessageStart(JsonElement root)
    {
        if (hasMessageStarted
            || !root.TryGetProperty("message", out JsonElement message)
            || message.ValueKind != JsonValueKind.Object
            || !message.TryGetProperty("content", out JsonElement content)
            || content.ValueKind != JsonValueKind.Array
            || content.GetArrayLength() != 0
            || !message.TryGetProperty("stop_reason", out JsonElement stopReason)
            || stopReason.ValueKind != JsonValueKind.Null
            || !TryReadInputUsage(message))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        hasMessageStarted = true;
        return null;
    }

    private CharacterModelStreamTermination? ApplyContentBlockStart(JsonElement root)
    {
        if (!hasMessageStarted
            || hasContentBlockStarted
            || !HasZeroIndex(root)
            || !root.TryGetProperty("content_block", out JsonElement block)
            || block.ValueKind != JsonValueKind.Object
            || !HasStringProperty(block, "type", "text")
            || !HasStringProperty(block, "text", string.Empty))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        hasContentBlockStarted = true;
        return null;
    }

    private CharacterModelStreamTermination? ApplyContentBlockDelta(JsonElement root)
    {
        if (!hasContentBlockStarted
            || hasContentBlockStopped
            || !HasZeroIndex(root)
            || !root.TryGetProperty("delta", out JsonElement delta)
            || delta.ValueKind != JsonValueKind.Object
            || !HasStringProperty(delta, "type", "text_delta")
            || !delta.TryGetProperty("text", out JsonElement textElement)
            || textElement.ValueKind != JsonValueKind.String)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        string text = textElement.GetString()!;
        if (responseDocument.Length + text.Length
            > CharacterTurnResponseJsonDeserializer.MaximumDocumentLength)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        responseDocument.Append(text);
        return null;
    }

    private CharacterModelStreamTermination? ApplyContentBlockStop(JsonElement root)
    {
        if (!hasContentBlockStarted || hasContentBlockStopped || !HasZeroIndex(root))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        hasContentBlockStopped = true;
        return null;
    }

    private CharacterModelStreamTermination? ApplyMessageDelta(JsonElement root)
    {
        if (!hasContentBlockStopped
            || hasMessageDelta
            || !root.TryGetProperty("delta", out JsonElement delta)
            || delta.ValueKind != JsonValueKind.Object
            || !delta.TryGetProperty("stop_reason", out JsonElement stopReason)
            || stopReason.ValueKind != JsonValueKind.String)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        string reason = stopReason.GetString()!;
        if (reason == "refusal")
        {
            return Terminate(CharacterModelFailureCode.ProviderRejected);
        }

        if (reason != "end_turn" || !TryReadOutputUsage(root))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        hasMessageDelta = true;
        return null;
    }

    private CharacterModelStreamTermination Complete()
    {
        if (!hasMessageStarted || !hasContentBlockStopped || !hasMessageDelta)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        isTerminated = true;
        return CharacterModelJsonResponseCompletion.Create(request, responseDocument.ToString());
    }

    private bool TryReadInputUsage(JsonElement message)
    {
        if (!message.TryGetProperty("usage", out JsonElement usage)
            || usage.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        return usage.ValueKind == JsonValueKind.Object
            && TryReadTokenCount(usage, "input_tokens", out int inputTokens)
            && SetInputTokenCount(inputTokens);
    }

    private bool TryReadOutputUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usage", out JsonElement usage)
            || usage.ValueKind == JsonValueKind.Null)
        {
            return InputTokenCount is null;
        }

        if (usage.ValueKind != JsonValueKind.Object
            || !TryReadTokenCount(usage, "output_tokens", out int outputTokens)
            || InputTokenCount is not int inputTokens
            || (long)inputTokens + outputTokens > CharacterModelGenerationDiagnostic.MaximumTokenCount)
        {
            return false;
        }

        OutputTokenCount = outputTokens;
        TotalTokenCount = inputTokens + outputTokens;
        return true;
    }

    private bool SetInputTokenCount(int tokenCount)
    {
        InputTokenCount = tokenCount;
        return true;
    }

    private static bool TryReadTokenCount(
        JsonElement usage,
        string propertyName,
        out int tokenCount)
    {
        tokenCount = 0;
        int matchCount = 0;
        foreach (JsonProperty property in usage.EnumerateObject())
        {
            if (property.Name != propertyName)
            {
                continue;
            }

            matchCount++;
            if (matchCount > 1
                || property.Value.ValueKind != JsonValueKind.Number
                || !property.Value.TryGetInt32(out tokenCount)
                || tokenCount < 0
                || tokenCount > CharacterModelGenerationDiagnostic.MaximumTokenCount)
            {
                return false;
            }
        }

        return matchCount == 1;
    }

    private static bool HasZeroIndex(JsonElement root) =>
        root.TryGetProperty("index", out JsonElement index)
        && index.ValueKind == JsonValueKind.Number
        && index.TryGetInt32(out int value)
        && value == 0;

    private static bool HasStringProperty(
        JsonElement root,
        string propertyName,
        string expected) =>
        root.TryGetProperty(propertyName, out JsonElement property)
        && property.ValueKind == JsonValueKind.String
        && property.GetString() == expected;

    private CharacterModelStreamTermination Terminate(CharacterModelFailureCode code)
    {
        isTerminated = true;
        InputTokenCount = null;
        OutputTokenCount = null;
        TotalTokenCount = null;
        return new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));
    }
}
