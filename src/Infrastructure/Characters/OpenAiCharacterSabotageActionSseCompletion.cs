using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterSabotageActionSseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterSabotageActionDecision> CompleteAsync(
        CharacterSabotageActionConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest,
            CharacterSabotageActionDecision> accumulator = new(request,
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize,
            CharacterSabotageActionDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
