using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one substitution decision from bounded OpenAI Responses SSE input.</summary>
public static class OpenAiCharacterObjectSabotageSubstitutionSseCompletion
{
    /// <summary>Stops at the first terminal event for one exact consideration request.</summary>
    public static async Task<CharacterObjectSabotageSubstitutionDecision> CompleteAsync(
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterObjectSabotageSubstitutionStreamAccumulator accumulator = new(request);
        CharacterObjectSabotageSubstitutionDecision? decision = null;
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
            "The OpenAI substitution stream did not complete.");
    }
}
