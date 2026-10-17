using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one misquotation-evidence decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterDialogueMisquotationEvidenceSseCompletion
{
    /// <summary>Stops at the first terminal event for one exact consideration request.</summary>
    public static async Task<CharacterDialogueMisquotationEvidenceDecision> CompleteAsync(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator accumulator = new(request);
        CharacterDialogueMisquotationEvidenceDecision? decision = null;
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
            "The OpenAI misquotation-evidence stream did not complete.");
    }
}
