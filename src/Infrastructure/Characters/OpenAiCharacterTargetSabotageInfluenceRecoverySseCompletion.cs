using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterTargetSabotageInfluenceRecoveryDecision> CompleteAsync(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest,
            CharacterTargetSabotageInfluenceRecoveryDecision> accumulator = new(request,
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize,
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
