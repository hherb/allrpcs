using System.Text.Json;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Accumulates one bounded OpenAI stream into an exact delay-recovery decision.</summary>
public sealed class OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator
{
    private const int MaximumEventDepth = 8;
    private readonly CharacterTargetSabotageDelayRecoveryConsiderationRequest request;
    private CharacterTargetSabotageDelayRecoveryDecision? completedDecision;
    private int responseDeltaLength;
    private bool hasStarted;
    private bool hasVerifiedOutputTextDone;
    private bool isInProgress;
    private bool isTerminated;

    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    /// <summary>Creates an accumulator bound to one exact delay-recovery request.</summary>
    public OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        this.request = request;
    }

    /// <summary>Applies one event and returns the exact decision only at successful completion.</summary>
    public CharacterTargetSabotageDelayRecoveryDecision? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (isTerminated)
        {
            throw new InvalidOperationException(
                "The OpenAI delay-recovery stream is already terminal.");
        }

        if (serializedEvent.Length > MaximumEventLength)
        {
            throw Fail();
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
                throw Fail();
            }

            return ApplyTypedEvent(typeElement.GetString()!, root);
        }
        catch (JsonException)
        {
            throw Fail();
        }
    }

    private CharacterTargetSabotageDelayRecoveryDecision? ApplyTypedEvent(
        string type,
        JsonElement root) => type switch
        {
            "response.created" => Start(),
            "response.in_progress" => MarkInProgress(),
            "response.output_item.added" or
            "response.output_item.done" or
            "response.content_part.added" or
            "response.content_part.done" or
            "response.reasoning_summary_part.added" or
            "response.reasoning_summary_part.done" or
            "response.reasoning_summary_text.delta" or
            "response.reasoning_summary_text.done" => IgnoreMetadata(),
            "response.output_text.delta" => ApplyDelta(root),
            "response.output_text.done" => ApplyDone(root),
            "response.completed" => Complete(),
            "response.incomplete" or
            "response.refusal.delta" or
            "response.refusal.done" or
            "response.failed" or
            "error" => throw Fail(),
            _ => throw Fail(),
        };

    private CharacterTargetSabotageDelayRecoveryDecision? Start()
    {
        if (hasStarted)
        {
            throw Fail();
        }

        hasStarted = true;
        return null;
    }

    private CharacterTargetSabotageDelayRecoveryDecision? MarkInProgress()
    {
        if (!hasStarted || isInProgress)
        {
            throw Fail();
        }

        isInProgress = true;
        return null;
    }

    private CharacterTargetSabotageDelayRecoveryDecision? IgnoreMetadata()
    {
        if (!isInProgress || hasVerifiedOutputTextDone)
        {
            throw Fail();
        }

        return null;
    }

    private CharacterTargetSabotageDelayRecoveryDecision? ApplyDelta(JsonElement root)
    {
        if (!isInProgress
            || hasVerifiedOutputTextDone
            || !root.TryGetProperty("delta", out JsonElement deltaElement)
            || deltaElement.ValueKind != JsonValueKind.String)
        {
            throw Fail();
        }

        string delta = deltaElement.GetString()!;
        if (responseDeltaLength + delta.Length
            > CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.MaximumDocumentLength)
        {
            throw Fail();
        }

        responseDeltaLength += delta.Length;
        return null;
    }

    private CharacterTargetSabotageDelayRecoveryDecision? ApplyDone(JsonElement root)
    {
        if (!isInProgress
            || hasVerifiedOutputTextDone
            || !root.TryGetProperty("text", out JsonElement textElement)
            || textElement.ValueKind != JsonValueKind.String)
        {
            throw Fail();
        }

        string responseDocument = textElement.GetString()!;
        if (responseDocument.Length
            > CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.MaximumDocumentLength)
        {
            throw Fail();
        }

        try
        {
            completedDecision =
                CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                    request,
                    responseDocument);
        }
        catch (ArgumentException)
        {
            throw Fail();
        }

        hasVerifiedOutputTextDone = true;
        return null;
    }

    private CharacterTargetSabotageDelayRecoveryDecision Complete()
    {
        if (!isInProgress || !hasVerifiedOutputTextDone || completedDecision is null)
        {
            throw Fail();
        }

        isTerminated = true;
        return completedDecision;
    }

    private InvalidDataException Fail()
    {
        isTerminated = true;
        return new InvalidDataException(
            "The OpenAI delay-recovery stream is not a complete supported response.");
    }
}
