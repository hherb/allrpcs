using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one substitution decision from bounded OpenAI Responses SSE input.</summary>
public static class OpenAiCharacterObjectSabotageSubstitutionSseCompletion
{
    /// <summary>Reads and applies complete semantic events for one exact consideration request.</summary>
    public static async Task<CharacterObjectSabotageSubstitutionDecision> CompleteAsync(
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        IReadOnlyList<string> documents = await OpenAiServerSentEventReader
            .ReadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        OpenAiCharacterObjectSabotageSubstitutionStreamAccumulator accumulator = new(request);
        CharacterObjectSabotageSubstitutionDecision? decision = null;
        foreach (string document in documents)
        {
            if (decision is not null)
            {
                throw new InvalidDataException(
                    "The OpenAI substitution stream contains events after completion.");
            }

            decision = accumulator.Apply(document);
        }

        return decision ?? throw new InvalidDataException(
            "The OpenAI substitution stream did not complete.");
    }
}
