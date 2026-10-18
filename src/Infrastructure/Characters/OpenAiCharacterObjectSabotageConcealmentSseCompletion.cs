using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterObjectSabotageConcealmentSseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterObjectSabotageConcealmentDecision> CompleteAsync(
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageConcealmentConsiderationRequest,
            CharacterObjectSabotageConcealmentDecision> accumulator = new(request,
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize,
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
