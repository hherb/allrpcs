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
    public StructuredDecisionDescriptor(
        string name, string instructions, Func<TRequest, string> context,
        Action<Utf8JsonWriter, TRequest> writeSchema, Func<TRequest, string, TDecision> deserialize,
        int maximumOutputTokens, int maximumDocumentLength,
        bool requireLifecycle = true, bool allowMetadataAfterTextDone = true)
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
    /// <summary>Gets the existing bounded token request.</summary>
    public int MaximumOutputTokens { get; }
    /// <summary>Gets the decision policy's document bound.</summary>
    public int MaximumDocumentLength { get; }

    /// <summary>Serializes the same trusted content/schema for the selected provider.</summary>
    public string Serialize(TRequest request, CharacterModelProvider provider, string modelName) =>
        StructuredDecisionRequestSerializer.Serialize(request, provider, modelName,
            Name, instructions, context, writeSchema, MaximumOutputTokens);

    /// <summary>Creates an isolated exact-request lifecycle for a non-OpenAI stream.</summary>
    public StructuredDecisionPipeline<TRequest, TDecision> CreatePipeline(TRequest request) =>
        new(request, deserialize, MaximumDocumentLength);

    /// <summary>Creates an OpenAI accumulator preserving the decision kind's legacy stream policies.</summary>
    public OpenAiStructuredDecisionAccumulator<TRequest, TDecision> CreateOpenAiAccumulator(TRequest request) =>
        new(request, deserialize, MaximumDocumentLength, requireLifecycle, allowMetadataAfterTextDone);
}
