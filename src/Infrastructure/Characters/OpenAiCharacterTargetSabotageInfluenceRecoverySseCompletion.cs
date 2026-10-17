using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one influence-recovery decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion
{
    /// <summary>Reads and applies complete semantic events for one exact consideration request.</summary>
    public static async Task<CharacterTargetSabotageInfluenceRecoveryDecision> CompleteAsync(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterTargetSabotageInfluenceRecoveryStreamAccumulator accumulator = new(request);
        CharacterTargetSabotageInfluenceRecoveryDecision? decision = null;
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
            "The OpenAI influence-recovery stream did not complete.");
    }
}
