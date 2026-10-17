using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one concealment decision from bounded OpenAI Responses SSE input.</summary>
public static class OpenAiCharacterObjectSabotageConcealmentSseCompletion
{
    /// <summary>Stops at the first terminal event for one exact consideration request.</summary>
    public static async Task<CharacterObjectSabotageConcealmentDecision> CompleteAsync(
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator accumulator = new(request);
        CharacterObjectSabotageConcealmentDecision? decision = null;
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
            "The OpenAI concealment stream did not complete.");
    }
}
