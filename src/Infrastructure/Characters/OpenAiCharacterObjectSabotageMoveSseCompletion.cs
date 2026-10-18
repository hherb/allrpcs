using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterObjectSabotageMoveSseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterObjectSabotageMoveDecision> CompleteAsync(
        CharacterObjectSabotageMoveConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageMoveConsiderationRequest,
            CharacterObjectSabotageMoveDecision> accumulator = new(request,
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize,
            CharacterObjectSabotageMoveDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
