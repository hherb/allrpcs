using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one sabotage-action decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterSabotageActionSseCompletion
{
    /// <summary>Stops at the first terminal event for one exact consideration request.</summary>
    public static async Task<CharacterSabotageActionDecision> CompleteAsync(
        CharacterSabotageActionConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterSabotageActionStreamAccumulator accumulator = new(request);
        CharacterSabotageActionDecision? decision = null;
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
            "The OpenAI sabotage-action stream did not complete.");
    }
}
