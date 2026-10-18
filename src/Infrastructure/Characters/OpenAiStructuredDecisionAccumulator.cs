using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Translates bounded OpenAI Responses events into the shared decision lifecycle.</summary>
public sealed class OpenAiStructuredDecisionAccumulator<TRequest, TDecision>
    where TRequest : class
    where TDecision : class
{
    /// <summary>Gets the existing maximum serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    private readonly StructuredDecisionPipeline<TRequest, TDecision> pipeline;

    /// <summary>Binds the stream to one exact request and its decision-specific document policy.</summary>
    public OpenAiStructuredDecisionAccumulator(
        TRequest request,
        Func<TRequest, string, TDecision> deserialize,
        int maximumDocumentLength,
        bool requireLifecycle = true,
        bool allowMetadataAfterTextDone = true)
    {
        pipeline = new(request, deserialize, maximumDocumentLength, requireLifecycle, allowMetadataAfterTextDone);
    }

    /// <summary>Applies one event, preserving terminal fencing even for malformed input.</summary>
    public TDecision? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (serializedEvent.Length > MaximumEventLength)
            return pipeline.Apply(StructuredDecisionEvent.Failed);

        try
        {
            using JsonDocument document = OpenAiStreamDocument.Parse(serializedEvent);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("type", out JsonElement type)
                || type.ValueKind != JsonValueKind.String)
                return pipeline.Apply(StructuredDecisionEvent.Failed);

            return type.GetString() switch
            {
                "response.created" => pipeline.Apply(StructuredDecisionEvent.Started),
                "response.in_progress" => pipeline.Apply(StructuredDecisionEvent.InProgress),
                "response.output_item.added" or "response.output_item.done" or
                "response.content_part.added" or "response.content_part.done" or
                "response.reasoning_summary_part.added" or "response.reasoning_summary_part.done" or
                "response.reasoning_summary_text.delta" or "response.reasoning_summary_text.done" =>
                    pipeline.Apply(StructuredDecisionEvent.Metadata),
                "response.output_text.delta" => pipeline.Apply(StructuredDecisionEvent.TextDelta, ReadText(root, "delta")),
                "response.output_text.done" => pipeline.Apply(StructuredDecisionEvent.TextDone, ReadText(root, "text")),
                "response.completed" => pipeline.Apply(StructuredDecisionEvent.Completed),
                _ => pipeline.Apply(StructuredDecisionEvent.Failed),
            };
        }
        catch (JsonException)
        {
            return pipeline.Apply(StructuredDecisionEvent.Failed);
        }
    }

    /// <summary>Stops at the first terminal decision without disposing the caller-owned stream.</summary>
    public async Task<TDecision> CompleteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await foreach (string document in OpenAiServerSentEventReader.ReadDocumentsAsync(stream, cancellationToken)
                .ConfigureAwait(false))
        {
            TDecision? decision = Apply(document);
            if (decision is not null) return decision;
        }

        return pipeline.Apply(StructuredDecisionEvent.Failed)!;
    }

    private static string? ReadText(JsonElement root, string property) =>
        root.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
