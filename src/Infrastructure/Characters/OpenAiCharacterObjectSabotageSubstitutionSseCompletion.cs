using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes the typed decision through shared bounded Responses event handling.</summary>
public static class OpenAiCharacterObjectSabotageSubstitutionSseCompletion
{
    /// <summary>Stops at the first terminal event without disposing the caller-owned stream.</summary>
    public static async Task<CharacterObjectSabotageSubstitutionDecision> CompleteAsync(
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageSubstitutionConsiderationRequest,
            CharacterObjectSabotageSubstitutionDecision> accumulator = new(request,
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize,
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false,
            allowMetadataAfterTextDone: true);
        return await accumulator.CompleteAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
