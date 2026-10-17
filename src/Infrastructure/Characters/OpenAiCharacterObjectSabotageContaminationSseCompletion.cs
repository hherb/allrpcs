using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one contamination decision from bounded OpenAI Responses SSE input.</summary>
public static class OpenAiCharacterObjectSabotageContaminationSseCompletion
{
    /// <summary>Stops at the first terminal event for one exact consideration request.</summary>
    public static async Task<CharacterObjectSabotageContaminationDecision> CompleteAsync(
        CharacterObjectSabotageContaminationConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator accumulator = new(request);
        CharacterObjectSabotageContaminationDecision? decision = null;
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
            "The OpenAI contamination stream did not complete.");
    }
}
