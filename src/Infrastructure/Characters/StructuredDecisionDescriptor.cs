using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Pairs one decision kind's trusted content and schema with its exact acceptance policy.</summary>
public sealed class StructuredDecisionDescriptor<TRequest, TDecision>
    where TRequest : class
    where TDecision : class
{
    private readonly string instructions;
    private readonly Func<TRequest, string> context;
    private readonly Action<Utf8JsonWriter, TRequest> writeSchema;
    private readonly Func<TRequest, string, TDecision> deserialize;
    private readonly bool requireLifecycle;
    private readonly bool allowMetadataAfterTextDone;

    /// <summary>Creates an inert contract without serializing or obtaining a decision.</summary>
    /// <param name="maximumOutputTokens">The OpenAI and Ollama output-token bound; Anthropic uses its own.</param>
    /// <param name="requireLifecycle">The OpenAI-only legacy rule requiring created/in-progress events.</param>
    /// <param name="allowMetadataAfterTextDone">The OpenAI-only legacy rule accepting metadata after text completion.</param>
    public StructuredDecisionDescriptor(
        string name, string instructions, Func<TRequest, string> context,
        Action<Utf8JsonWriter, TRequest> writeSchema, Func<TRequest, string, TDecision> deserialize,
        int maximumOutputTokens, int maximumDocumentLength,
        bool requireLifecycle, bool allowMetadataAfterTextDone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(instructions);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(writeSchema);
        ArgumentNullException.ThrowIfNull(deserialize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumOutputTokens);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumDocumentLength);
        Name = name;
        this.instructions = instructions;
        this.context = context;
        this.writeSchema = writeSchema;
        this.deserialize = deserialize;
        MaximumOutputTokens = maximumOutputTokens;
        MaximumDocumentLength = maximumDocumentLength;
        this.requireLifecycle = requireLifecycle;
        this.allowMetadataAfterTextDone = allowMetadataAfterTextDone;
    }

    /// <summary>Gets the stable schema name.</summary>
    public string Name { get; }
    /// <summary>Gets the existing OpenAI and Ollama output-token bound.</summary>
    public int MaximumOutputTokens { get; }
    /// <summary>Gets the decision policy's document bound.</summary>
    public int MaximumDocumentLength { get; }

    /// <summary>Serializes the same trusted content/schema for the selected provider.</summary>
    internal string Serialize(TRequest request, CharacterModelProvider provider, string modelName) =>
        StructuredDecisionRequestSerializer.Serialize(request, provider, modelName,
            Name, instructions, context, writeSchema, MaximumOutputTokens);

    /// <summary>Creates an isolated exact-request lifecycle with the strict default event order.</summary>
    /// <remarks>The per-kind legacy lifecycle flags apply only to <see cref="CreateOpenAiAccumulator"/>.</remarks>
    internal StructuredDecisionPipeline<TRequest, TDecision> CreatePipeline(TRequest request) =>
        new(request, deserialize, MaximumDocumentLength);

    /// <summary>Creates an OpenAI accumulator preserving the decision kind's legacy stream policies.</summary>
    internal OpenAiStructuredDecisionAccumulator<TRequest, TDecision> CreateOpenAiAccumulator(TRequest request) =>
        new(request, deserialize, MaximumDocumentLength, requireLifecycle, allowMetadataAfterTextDone);
}
