using System.Text.Json;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Accumulates one bounded OpenAI Responses semantic stream into an exact participation decision.
/// </summary>
public sealed class OpenAiCharacterTurnParticipationStreamAccumulator
{
    private readonly CharacterTurnParticipationConsiderationRequest request;
    private CharacterTurnParticipationDecision? completedDecision;
    private int responseDeltaLength;
    private bool hasVerifiedOutputTextDone;
    private bool isTerminated;

    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    /// <summary>Creates an accumulator bound to one exact participation request.</summary>
    public OpenAiCharacterTurnParticipationStreamAccumulator(
        CharacterTurnParticipationConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        this.request = request;
    }

    /// <summary>
    /// Applies one semantic event and returns the exact decision only at successful completion.
    /// </summary>
    public CharacterTurnParticipationDecision? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (isTerminated)
        {
            throw new InvalidOperationException("The OpenAI participation stream is already terminal.");
        }

        if (serializedEvent.Length > MaximumEventLength)
        {
            throw Fail();
        }

        try
        {
            using JsonDocument document = OpenAiStreamDocument.Parse(serializedEvent);
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

    private CharacterTurnParticipationDecision? ApplyTypedEvent(
        string type,
        JsonElement root) => type switch
        {
            "response.created" or
            "response.in_progress" or
            "response.output_item.added" or
            "response.output_item.done" or
            "response.content_part.added" or
            "response.content_part.done" or
            "response.reasoning_summary_part.added" or
            "response.reasoning_summary_part.done" or
            "response.reasoning_summary_text.delta" or
            "response.reasoning_summary_text.done" => null,
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

    private CharacterTurnParticipationDecision? ApplyDelta(JsonElement root)
    {
        if (hasVerifiedOutputTextDone
            || !root.TryGetProperty("delta", out JsonElement deltaElement)
            || deltaElement.ValueKind != JsonValueKind.String)
        {
            throw Fail();
        }

        string delta = deltaElement.GetString()!;
        if (responseDeltaLength + delta.Length
            > CharacterTurnParticipationDecisionJsonDeserializer.MaximumDocumentLength)
        {
            throw Fail();
        }

        responseDeltaLength += delta.Length;
        return null;
    }

    private CharacterTurnParticipationDecision? ApplyDone(JsonElement root)
    {
        if (hasVerifiedOutputTextDone
            || !root.TryGetProperty("text", out JsonElement textElement)
            || textElement.ValueKind != JsonValueKind.String)
        {
            throw Fail();
        }

        string responseDocument = textElement.GetString()!;
        if (responseDocument.Length
            > CharacterTurnParticipationDecisionJsonDeserializer.MaximumDocumentLength)
        {
            throw Fail();
        }

        try
        {
            completedDecision = CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(
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

    private CharacterTurnParticipationDecision Complete()
    {
        if (!hasVerifiedOutputTextDone || completedDecision is null)
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
            "The OpenAI participation stream is not a complete supported response.");
    }
}
