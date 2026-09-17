using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one target-delay decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterTargetSabotageDelaySseCompletion
{
    /// <summary>Reads and applies complete semantic events for one exact consideration request.</summary>
    public static async Task<CharacterTargetSabotageDelayDecision> CompleteAsync(
        CharacterTargetSabotageDelayConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        IReadOnlyList<string> documents = await OpenAiServerSentEventReader
            .ReadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        OpenAiCharacterTargetSabotageDelayStreamAccumulator accumulator = new(request);
        CharacterTargetSabotageDelayDecision? decision = null;
        foreach (string document in documents)
        {
            if (decision is not null)
            {
                throw new InvalidDataException(
                    "The OpenAI target-delay stream contains events after completion.");
            }

            decision = accumulator.Apply(document);
        }

        return decision ?? throw new InvalidDataException(
            "The OpenAI target-delay stream did not complete.");
    }
}
