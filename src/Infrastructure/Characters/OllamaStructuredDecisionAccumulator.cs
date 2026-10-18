using System.Text;
using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Maps bounded Ollama assistant text and a successful done record to the shared lifecycle.</summary>
public sealed class OllamaStructuredDecisionAccumulator<TRequest, TDecision>
    where TRequest : class where TDecision : class
{
    private readonly StructuredDecisionPipeline<TRequest, TDecision> pipeline;
    private readonly StringBuilder text = new();
    private bool started;
    private bool terminal;

    /// <summary>Binds one stream to the exact request and its acceptance policy.</summary>
    public OllamaStructuredDecisionAccumulator(TRequest request, StructuredDecisionDescriptor<TRequest, TDecision> descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        pipeline = descriptor.CreatePipeline(request);
    }

    /// <summary>Returns only at a done record with the successful stop reason.</summary>
    public TDecision? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (terminal) throw new InvalidOperationException("The structured-decision stream is already terminal.");
        if (serializedEvent.Length > OllamaNewlineDelimitedJsonReader.MaximumDocumentLength) return Fail();
        try
        {
            using JsonDocument document = OpenAiStreamDocument.Parse(serializedEvent);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || root.TryGetProperty("error", out _)
                || !root.TryGetProperty("done", out JsonElement done)
                || done.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
                || !root.TryGetProperty("message", out JsonElement message)
                || message.ValueKind != JsonValueKind.Object
                || !message.TryGetProperty("role", out JsonElement role) || role.ValueKind != JsonValueKind.String
                || role.GetString() != "assistant" || message.TryGetProperty("tool_calls", out _)
                || !message.TryGetProperty("content", out JsonElement content)
                || content.ValueKind != JsonValueKind.String) return Fail();
            if (!started)
            {
                pipeline.Apply(StructuredDecisionEvent.Started);
                pipeline.Apply(StructuredDecisionEvent.InProgress);
                started = true;
            }
            string fragment = content.GetString()!;
            pipeline.Apply(StructuredDecisionEvent.TextDelta, fragment);
            text.Append(fragment);
            if (!done.GetBoolean()) return null;
            if (!root.TryGetProperty("done_reason", out JsonElement reason) || reason.ValueKind != JsonValueKind.String
                || reason.GetString() != "stop") return Fail();
            pipeline.Apply(StructuredDecisionEvent.TextDone, text.ToString());
            terminal = true;
            return pipeline.Apply(StructuredDecisionEvent.Completed);
        }
        catch (JsonException) { return Fail(); }
        catch (InvalidDataException) { terminal = true; throw; }
    }

    /// <summary>Stops at the first terminal record and leaves the caller's stream open.</summary>
    public async Task<TDecision> CompleteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await foreach (string document in OllamaNewlineDelimitedJsonReader.ReadDocumentsAsync(stream, cancellationToken)
            .ConfigureAwait(false))
        {
            if (Apply(document) is { } decision) return decision;
        }
        return Fail()!;
    }

    private TDecision? Fail()
    {
        terminal = true;
        return pipeline.Apply(StructuredDecisionEvent.Failed);
    }
}
