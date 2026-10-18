using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterTargetSabotageDelayRecoverySseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterTargetSabotageDelayRecoveryDecision> CompleteAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayRecoveryConsiderationRequest,
            CharacterTargetSabotageDelayRecoveryDecision> accumulator = new(request,
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize,
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: false);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
