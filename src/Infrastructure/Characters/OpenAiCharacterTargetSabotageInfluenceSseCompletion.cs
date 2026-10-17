using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one influence decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterTargetSabotageInfluenceSseCompletion
{
    /// <summary>Stops at the first terminal event for one exact consideration request.</summary>
    public static async Task<CharacterTargetSabotageInfluenceDecision> CompleteAsync(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterTargetSabotageInfluenceStreamAccumulator accumulator = new(request);
        CharacterTargetSabotageInfluenceDecision? decision = null;
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
            "The OpenAI influence stream did not complete.");
    }
}
