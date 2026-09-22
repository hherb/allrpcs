using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Completes one delay-recovery decision from bounded OpenAI Responses SSE.</summary>
public static class OpenAiCharacterTargetSabotageDelayRecoverySseCompletion
{
    /// <summary>Reads and applies complete semantic events for one exact consideration request.</summary>
    public static async Task<CharacterTargetSabotageDelayRecoveryDecision> CompleteAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        IReadOnlyList<string> documents = await OpenAiServerSentEventReader
            .ReadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator accumulator = new(request);
        CharacterTargetSabotageDelayRecoveryDecision? decision = null;
        foreach (string document in documents)
        {
            if (decision is not null)
            {
                throw new InvalidDataException(
                    "The OpenAI delay-recovery stream contains events after completion.");
            }

            decision = accumulator.Apply(document);
        }

        return decision ?? throw new InvalidDataException(
            "The OpenAI delay-recovery stream did not complete.");
    }
}
