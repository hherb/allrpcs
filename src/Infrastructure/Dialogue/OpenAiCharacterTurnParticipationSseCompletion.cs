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

        IReadOnlyList<string> documents = await OpenAiServerSentEventReader
            .ReadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        OpenAiCharacterTurnParticipationStreamAccumulator accumulator = new(request);
        CharacterTurnParticipationDecision? decision = null;
        foreach (string document in documents)
        {
            if (decision is not null)
            {
                throw new InvalidDataException(
                    "The OpenAI participation stream contains events after completion.");
            }

            decision = accumulator.Apply(document);
        }

        return decision ?? throw new InvalidDataException(
            "The OpenAI participation stream did not complete.");
    }
}
