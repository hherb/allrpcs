using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Completes one provider-neutral character response from bounded OpenAI SSE input.
/// </summary>
public static class OpenAiCharacterModelSseCompletion
{
    /// <summary>
    /// Reads and applies complete semantic event documents for one exact turn request.
    /// </summary>
    public static async Task<CharacterModelStreamTermination> CompleteAsync(
        CharacterTurnRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        IReadOnlyList<string> documents;
        try
        {
            documents = await OpenAiServerSentEventReader.ReadAsync(stream, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (InvalidDataException)
        {
            return Failure(CharacterModelFailureCode.InvalidResponse);
        }
        catch (IOException)
        {
            return Failure(CharacterModelFailureCode.Transport);
        }

        OpenAiCharacterModelStreamAccumulator accumulator = new(request);
        CharacterModelStreamTermination? termination = null;
        foreach (string document in documents)
        {
            if (termination is not null)
            {
                return Failure(CharacterModelFailureCode.InvalidResponse);
            }

            termination = accumulator.Apply(document);
        }

        return termination ?? Failure(CharacterModelFailureCode.InvalidResponse);
    }

    private static CharacterModelStreamTermination Failure(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));
}
