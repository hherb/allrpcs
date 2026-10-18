using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterObjectSabotageContaminationSseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterObjectSabotageContaminationDecision> CompleteAsync(
        CharacterObjectSabotageContaminationConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageContaminationConsiderationRequest,
            CharacterObjectSabotageContaminationDecision> accumulator = new(request,
            CharacterObjectSabotageContaminationDecisionJsonDeserializer.Deserialize,
            CharacterObjectSabotageContaminationDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
