using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterTargetSabotageDelaySseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterTargetSabotageDelayDecision> CompleteAsync(
        CharacterTargetSabotageDelayConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest,
            CharacterTargetSabotageDelayDecision> accumulator = new(request,
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize,
            CharacterTargetSabotageDelayDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
