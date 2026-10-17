using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Accumulates one bounded OpenAI semantic stream into an exact contamination decision.</summary>
public sealed class OpenAiCharacterObjectSabotageContaminationStreamAccumulator
{
    private readonly CharacterObjectSabotageContaminationConsiderationRequest request;
    private CharacterObjectSabotageContaminationDecision? completedDecision;
    private int responseDeltaLength;
    private bool hasStarted;
    private bool hasVerifiedOutputTextDone;
    private bool isInProgress;
    private bool isTerminated;

    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    /// <summary>Creates an accumulator bound to one exact contamination request.</summary>
    public OpenAiCharacterObjectSabotageContaminationStreamAccumulator(
        CharacterObjectSabotageContaminationConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        this.request = request;
    }

    /// <summary>Applies one event and returns the exact decision only at successful completion.</summary>
    public CharacterObjectSabotageContaminationDecision? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (isTerminated)
        {
            throw new InvalidOperationException("The OpenAI contamination stream is already terminal.");
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

    private CharacterObjectSabotageContaminationDecision? ApplyTypedEvent(
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

    private CharacterObjectSabotageContaminationDecision? Start()
    {
        if (hasStarted)
        {
            throw Fail();
        }

        hasStarted = true;
        return null;
    }

    private CharacterObjectSabotageContaminationDecision? MarkInProgress()
    {
        if (!hasStarted || isInProgress)
        {
            throw Fail();
        }

        isInProgress = true;
        return null;
    }

    private CharacterObjectSabotageContaminationDecision? IgnoreMetadata()
    {
        if (!isInProgress)
        {
            throw Fail();
        }

        return null;
    }

    private CharacterObjectSabotageContaminationDecision? ApplyDelta(JsonElement root)
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
            > CharacterObjectSabotageContaminationDecisionJsonDeserializer.MaximumDocumentLength)
        {
            throw Fail();
        }

        responseDeltaLength += delta.Length;
        return null;
    }

    private CharacterObjectSabotageContaminationDecision? ApplyDone(JsonElement root)
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
            > CharacterObjectSabotageContaminationDecisionJsonDeserializer.MaximumDocumentLength)
        {
            throw Fail();
        }

        try
        {
            completedDecision = CharacterObjectSabotageContaminationDecisionJsonDeserializer.Deserialize(
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

    private CharacterObjectSabotageContaminationDecision Complete()
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
            "The OpenAI contamination stream is not a complete supported response.");
    }
}
