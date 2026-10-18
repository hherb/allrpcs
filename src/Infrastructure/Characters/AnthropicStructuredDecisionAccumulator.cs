using System.Text;
using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Maps one bounded Anthropic text block and successful stop to the shared lifecycle.</summary>
public sealed class AnthropicStructuredDecisionAccumulator<TRequest, TDecision>
    where TRequest : class where TDecision : class
{
    private readonly StructuredDecisionPipeline<TRequest, TDecision> pipeline;
    private readonly StringBuilder text = new();
    private bool started;
    private bool blockStarted;
    private bool blockStopped;
    private bool stopReceived;
    private bool terminal;

    /// <summary>Binds a new stream to the exact request and its decision policy.</summary>
    public AnthropicStructuredDecisionAccumulator(TRequest request,
        StructuredDecisionDescriptor<TRequest, TDecision> descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        pipeline = descriptor.CreatePipeline(request);
    }

    /// <summary>Returns the decision only at message_stop after a complete block and end_turn.</summary>
    public TDecision? Apply(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        if (terminal) throw new InvalidOperationException("The structured-decision stream is already terminal.");
        if (serializedEvent.Length > OpenAiStructuredDecisionAccumulator<TRequest, TDecision>.MaximumEventLength)
            return Fail();
        try
        {
            using JsonDocument document = OpenAiStreamDocument.Parse(serializedEvent);
            JsonElement root = document.RootElement;
            switch (ReadString(root, "type"))
            {
                case "ping": return null;
                case "message_start":
                    if (started || !TryObject(root, "message", out JsonElement message)
                        || !message.TryGetProperty("content", out JsonElement content)
                        || content.ValueKind != JsonValueKind.Array || content.GetArrayLength() != 0
                        || !message.TryGetProperty("stop_reason", out JsonElement reason)
                        || reason.ValueKind != JsonValueKind.Null) return Fail();
                    started = true;
                    pipeline.Apply(StructuredDecisionEvent.Started);
                    return pipeline.Apply(StructuredDecisionEvent.InProgress);
                case "content_block_start":
                    if (!started || blockStarted || !HasZeroIndex(root)
                        || !TryObject(root, "content_block", out JsonElement block)
                        || ReadString(block, "type") != "text" || ReadString(block, "text") != "") return Fail();
                    blockStarted = true;
                    return null;
                case "content_block_delta":
                    if (!blockStarted || blockStopped || !HasZeroIndex(root)
                        || !TryObject(root, "delta", out JsonElement delta)
                        || ReadString(delta, "type") != "text_delta"
                        || ReadString(delta, "text") is not { } fragment) return Fail();
                    pipeline.Apply(StructuredDecisionEvent.TextDelta, fragment);
                    text.Append(fragment);
                    return null;
                case "content_block_stop":
                    if (!blockStarted || blockStopped || !HasZeroIndex(root)) return Fail();
                    blockStopped = true;
                    return pipeline.Apply(StructuredDecisionEvent.TextDone, text.ToString());
                case "message_delta":
                    if (!blockStopped || stopReceived || !TryObject(root, "delta", out JsonElement stop)
                        || ReadString(stop, "stop_reason") != "end_turn") return Fail();
                    stopReceived = true;
                    return null;
                case "message_stop":
                    if (!stopReceived) return Fail();
                    terminal = true;
                    return pipeline.Apply(StructuredDecisionEvent.Completed);
                default: return Fail();
            }
        }
        catch (JsonException) { return Fail(); }
        catch (InvalidDataException) { terminal = true; throw; }
    }

    /// <summary>Stops before any unread tail and leaves the caller's stream open.</summary>
    public async Task<TDecision> CompleteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await foreach (string document in OpenAiServerSentEventReader.ReadDocumentsAsync(stream, cancellationToken)
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

    private static string? ReadString(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out JsonElement value)
        && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static bool TryObject(JsonElement root, string name, out JsonElement value)
    {
        value = default;
        return root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out value)
            && value.ValueKind == JsonValueKind.Object;
    }

    private static bool HasZeroIndex(JsonElement root) =>
        root.TryGetProperty("index", out JsonElement index) && index.ValueKind == JsonValueKind.Number
        && index.TryGetInt32(out int value) && value == 0;
}
