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
        OpenAiCharacterModelSseResult result = await CompleteWithUsageAsync(
                request,
                stream,
                cancellationToken)
            .ConfigureAwait(false);
        return result.Termination;
    }

    /// <summary>
    /// Reads and applies complete semantic events while retaining only bounded terminal usage.
    /// </summary>
    public static async Task<OpenAiCharacterModelSseResult> CompleteWithUsageAsync(
        CharacterTurnRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        OpenAiCharacterModelStreamAccumulator accumulator = new(request);
        CharacterModelStreamTermination? termination = null;
        try
        {
            await foreach (string document in OpenAiServerSentEventReader.ReadDocumentsAsync(stream, cancellationToken)
                .ConfigureAwait(false))
            {
                termination = accumulator.Apply(document);
                if (termination is not null)
                {
                    break;
                }
            }
        }
        catch (InvalidDataException)
        {
            return Result(Failure(CharacterModelFailureCode.InvalidResponse));
        }
        catch (IOException)
        {
            return Result(Failure(CharacterModelFailureCode.Transport));
        }

        return new OpenAiCharacterModelSseResult(
            termination ?? Failure(CharacterModelFailureCode.InvalidResponse),
            accumulator.InputTokenCount,
            accumulator.OutputTokenCount,
            accumulator.TotalTokenCount);
    }

    private static CharacterModelStreamTermination Failure(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));

    private static OpenAiCharacterModelSseResult Result(
        CharacterModelStreamTermination termination) => new(termination, null, null, null);
}
