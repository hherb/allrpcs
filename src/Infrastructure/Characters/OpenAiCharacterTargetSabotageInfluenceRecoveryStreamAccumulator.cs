using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed stream contract over the shared decision lifecycle.</summary>
public sealed class OpenAiCharacterTargetSabotageInfluenceRecoveryStreamAccumulator
{
    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    private readonly OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest,
        CharacterTargetSabotageInfluenceRecoveryDecision> stream;

    /// <summary>Binds the stream to the exact request and existing decision policy.</summary>
    public OpenAiCharacterTargetSabotageInfluenceRecoveryStreamAccumulator(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request)
    {
        stream = new(request,
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize,
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
    }

    /// <summary>Returns the exact decision only after verified successful completion.</summary>
    public CharacterTargetSabotageInfluenceRecoveryDecision? Apply(string serializedEvent) => stream.Apply(serializedEvent);
}
