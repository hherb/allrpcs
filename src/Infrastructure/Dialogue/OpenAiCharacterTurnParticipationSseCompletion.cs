using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Completes one participation decision from bounded OpenAI Responses SSE input.
/// </summary>
public static class OpenAiCharacterTurnParticipationSseCompletion
{
    /// <summary>Reads and applies complete semantic events for one exact participation request.</summary>
    public static async Task<CharacterTurnParticipationDecision> CompleteAsync(
        CharacterTurnParticipationConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterTurnParticipationStreamAccumulator accumulator = new(request);
        CharacterTurnParticipationDecision? decision = null;
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
            "The OpenAI participation stream did not complete.");
    }
}
