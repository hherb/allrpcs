using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one influence decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterTargetSabotageInfluenceSseCompletion
{
    /// <summary>Reads and applies complete semantic events for one exact consideration request.</summary>
    public static async Task<CharacterTargetSabotageInfluenceDecision> CompleteAsync(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        IReadOnlyList<string> documents = await OpenAiServerSentEventReader
            .ReadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        OpenAiCharacterTargetSabotageInfluenceStreamAccumulator accumulator = new(request);
        CharacterTargetSabotageInfluenceDecision? decision = null;
        foreach (string document in documents)
        {
            if (decision is not null)
            {
                throw new InvalidDataException(
                    "The OpenAI influence stream contains events after completion.");
            }

            decision = accumulator.Apply(document);
        }

        return decision ?? throw new InvalidDataException(
            "The OpenAI influence stream did not complete.");
    }
}
