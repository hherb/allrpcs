using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed stream contract over the shared decision lifecycle.</summary>
public sealed class OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator
{
    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    private readonly OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayRecoveryConsiderationRequest,
        CharacterTargetSabotageDelayRecoveryDecision> stream;

    /// <summary>Binds the stream to the exact request and existing decision policy.</summary>
    public OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request)
    {
        stream = new(request,
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize,
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: false);
    }

    /// <summary>Returns the exact decision only after verified successful completion.</summary>
    public CharacterTargetSabotageDelayRecoveryDecision? Apply(string serializedEvent) => stream.Apply(serializedEvent);
}
