using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterTargetSabotageInfluenceSseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterTargetSabotageInfluenceDecision> CompleteAsync(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceConsiderationRequest,
            CharacterTargetSabotageInfluenceDecision> accumulator = new(request,
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize,
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
