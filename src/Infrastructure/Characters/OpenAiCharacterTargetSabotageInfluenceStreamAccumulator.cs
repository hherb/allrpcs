using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed stream contract over the shared decision lifecycle.</summary>
public sealed class OpenAiCharacterTargetSabotageInfluenceStreamAccumulator
{
    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    private readonly OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceConsiderationRequest,
        CharacterTargetSabotageInfluenceDecision> stream;

    /// <summary>Binds the stream to the exact request and existing decision policy.</summary>
    public OpenAiCharacterTargetSabotageInfluenceStreamAccumulator(
        CharacterTargetSabotageInfluenceConsiderationRequest request)
    {
        stream = new(request,
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize,
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
    }

    /// <summary>Returns the exact decision only after verified successful completion.</summary>
    public CharacterTargetSabotageInfluenceDecision? Apply(string serializedEvent) => stream.Apply(serializedEvent);
}
