using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Accumulates one bounded participation decision from native Ollama chat documents.</summary>
public sealed class OllamaCharacterTurnParticipationStreamAccumulator
{
    private const int MaximumDocumentDepth = 8;
    private readonly StringBuilder responseDocument = new();
    private readonly CharacterTurnParticipationConsiderationRequest request;
    private bool isTerminated;

    /// <summary>Gets the maximum accepted serialized update length.</summary>
    public const int MaximumEventLength = 64_000;

    /// <summary>Creates an accumulator bound to one exact character-turn request.</summary>
    public OllamaCharacterTurnParticipationStreamAccumulator(CharacterTurnParticipationConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        this.request = request;
    }

    /// <summary>Gets the bounded terminal prompt-token count, or unknown.</summary>
    public int? InputTokenCount { get; private set; }

    /// <summary>Gets the bounded terminal generated-token count, or unknown.</summary>
    public int? OutputTokenCount { get; private set; }

    /// <summary>Gets the bounded derived total-token count, or unknown.</summary>
    public int? TotalTokenCount { get; private set; }

    /// <summary>Applies one native chat document and returns a terminal update when complete.</summary>
    public CharacterTurnParticipationDecision? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (isTerminated)
        {
            throw new InvalidOperationException("The Ollama response stream is already terminal.");
        }

        if (serializedEvent.Length == 0 || serializedEvent.Length > MaximumEventLength)
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
                    MaxDepth = MaximumDocumentDepth,
                });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !HasUniqueKnownRootProperties(root))
            {
                return Terminate(CharacterModelFailureCode.InvalidResponse);
            }

            if (root.TryGetProperty("error", out JsonElement error))
            {
                return ApplyError(root, error);
            }

            return ApplyChatDocument(root);
        }
        catch (JsonException)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }
    }

    private CharacterTurnParticipationDecision ApplyError(JsonElement root, JsonElement error)
    {
        if (root.EnumerateObject().Count() != 1
            || error.ValueKind != JsonValueKind.String
            || string.IsNullOrEmpty(error.GetString()))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        return Terminate(CharacterModelFailureCode.ProviderRejected);
    }

    private CharacterTurnParticipationDecision? ApplyChatDocument(JsonElement root)
    {
        if (!HasValidMetadata(root)
            || !TryReadMessageContent(root, out string content)
            || !root.TryGetProperty("done", out JsonElement doneElement)
            || doneElement.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        bool done = doneElement.GetBoolean();
        if (!done && HasTerminalProperty(root))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        if (responseDocument.Length + content.Length
            > CharacterTurnParticipationDecisionJsonDeserializer.MaximumDocumentLength)
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        responseDocument.Append(content);
        if (!done)
        {
            return null;
        }

        if (!HasStringProperty(root, "done_reason", "stop")
            || !TryReadUsage(root, out int? inputTokens, out int? outputTokens, out int? totalTokens))
        {
            return Terminate(CharacterModelFailureCode.InvalidResponse);
        }

        InputTokenCount = inputTokens;
        OutputTokenCount = outputTokens;
        TotalTokenCount = totalTokens;
        isTerminated = true;
        try
        {
            return CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(request, responseDocument.ToString());
        }
        catch (ArgumentException)
        {
            throw new InvalidDataException("The Ollama participation decision is invalid.");
        }
    }

    private static bool TryReadMessageContent(JsonElement root, out string content)
    {
        content = string.Empty;
        if (!root.TryGetProperty("message", out JsonElement message)
            || message.ValueKind != JsonValueKind.Object
            || !HasExactUniqueProperties(message, "role", "content")
            || !HasStringProperty(message, "role", "assistant")
            || !message.TryGetProperty("content", out JsonElement contentElement)
            || contentElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        content = contentElement.GetString()!;
        return true;
    }

    private static bool TryReadUsage(
        JsonElement root,
        out int? inputTokens,
        out int? outputTokens,
        out int? totalTokens)
    {
        inputTokens = null;
        outputTokens = null;
        totalTokens = null;
        bool hasInput = root.TryGetProperty("prompt_eval_count", out JsonElement inputElement);
        bool hasOutput = root.TryGetProperty("eval_count", out JsonElement outputElement);
        bool hasCached = root.TryGetProperty("prompt_eval_cached_count", out JsonElement cachedElement);
        if (!hasInput && !hasOutput)
        {
            return !hasCached;
        }

        if (!hasInput
            || !hasOutput
            || !TryReadTokenCount(inputElement, out int boundedInput)
            || !TryReadTokenCount(outputElement, out int boundedOutput)
            || (hasCached && !TryReadTokenCount(cachedElement, out _))
            || (long)boundedInput + boundedOutput
                > CharacterModelGenerationDiagnostic.MaximumTokenCount)
        {
            return false;
        }

        inputTokens = boundedInput;
        outputTokens = boundedOutput;
        totalTokens = boundedInput + boundedOutput;
        return true;
    }

    private static bool HasValidMetadata(JsonElement root)
    {
        if (root.TryGetProperty("model", out JsonElement model)
            && (model.ValueKind != JsonValueKind.String
                || string.IsNullOrEmpty(model.GetString())))
        {
            return false;
        }

        if (root.TryGetProperty("created_at", out JsonElement createdAt)
            && (createdAt.ValueKind != JsonValueKind.String
                || string.IsNullOrEmpty(createdAt.GetString())))
        {
            return false;
        }

        return HasValidOptionalDuration(root, "total_duration")
            && HasValidOptionalDuration(root, "load_duration")
            && HasValidOptionalDuration(root, "prompt_eval_duration")
            && HasValidOptionalDuration(root, "eval_duration");
    }

    private static bool HasValidOptionalDuration(JsonElement root, string propertyName) =>
        !root.TryGetProperty(propertyName, out JsonElement duration)
        || (duration.ValueKind == JsonValueKind.Number
            && duration.TryGetInt64(out long value)
            && value >= 0);

    private static bool TryReadTokenCount(JsonElement element, out int tokenCount)
    {
        tokenCount = 0;
        return element.ValueKind == JsonValueKind.Number
            && element.TryGetInt32(out tokenCount)
            && tokenCount >= 0
            && tokenCount <= CharacterModelGenerationDiagnostic.MaximumTokenCount;
    }

    private static bool HasTerminalProperty(JsonElement root) =>
        root.TryGetProperty("done_reason", out _)
        || root.TryGetProperty("prompt_eval_count", out _)
        || root.TryGetProperty("eval_count", out _)
        || root.TryGetProperty("prompt_eval_cached_count", out _)
        || root.TryGetProperty("total_duration", out _)
        || root.TryGetProperty("load_duration", out _)
        || root.TryGetProperty("prompt_eval_duration", out _)
        || root.TryGetProperty("eval_duration", out _);

    private static bool HasUniqueKnownRootProperties(JsonElement root)
    {
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!names.Add(property.Name) || !IsKnownRootProperty(property.Name))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsKnownRootProperty(string name) => name is
        "error" or
        "model" or
        "created_at" or
        "message" or
        "done" or
        "done_reason" or
        "total_duration" or
        "load_duration" or
        "prompt_eval_count" or
        "prompt_eval_cached_count" or
        "prompt_eval_duration" or
        "eval_count" or
        "eval_duration";

    private static bool HasExactUniqueProperties(JsonElement element, params string[] names)
    {
        HashSet<string> remaining = new(names, StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!remaining.Remove(property.Name))
            {
                return false;
            }
        }

        return remaining.Count == 0;
    }

    private static bool HasStringProperty(
        JsonElement element,
        string propertyName,
        string expected) =>
        element.TryGetProperty(propertyName, out JsonElement property)
        && property.ValueKind == JsonValueKind.String
        && string.Equals(property.GetString(), expected, StringComparison.Ordinal);

    private CharacterTurnParticipationDecision Terminate(CharacterModelFailureCode code)
    {
        isTerminated = true;
        throw new InvalidDataException($"The Ollama participation response failed: {code}.");
    }
}
