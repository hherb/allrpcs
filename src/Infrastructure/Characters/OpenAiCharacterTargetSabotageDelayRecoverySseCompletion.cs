using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one delay-recovery decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterTargetSabotageDelayRecoverySseCompletion
{
    /// <summary>Stops at the first terminal event for one exact consideration request.</summary>
    public static async Task<CharacterTargetSabotageDelayRecoveryDecision> CompleteAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator accumulator = new(request);
        CharacterTargetSabotageDelayRecoveryDecision? decision = null;
        await foreach (string document in OpenAiServerSentEventReader.ReadDocumentsAsync(stream, cancellationToken)
                .ConfigureAwait(false))
        {

            decision = accumulator.Apply(document);
            if (decision is not null)
            {
                return decision;
            }
        }

        return decision ?? throw new InvalidDataException(
            "The OpenAI delay-recovery stream did not complete.");
    }
}
