using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Completes one provider-neutral response from bounded Anthropic Messages SSE.</summary>
public static class AnthropicCharacterModelSseCompletion
{
    /// <summary>Stops at the first terminal document for the exact turn request.</summary>
    public static async Task<CharacterModelStreamTermination> CompleteAsync(
        CharacterTurnRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        AnthropicCharacterModelSseResult result = await CompleteWithUsageAsync(
                request,
                stream,
                cancellationToken)
            .ConfigureAwait(false);
        return result.Termination;
    }

    /// <summary>Reads one stream while retaining only bounded terminal usage.</summary>
    public static async Task<AnthropicCharacterModelSseResult> CompleteWithUsageAsync(
        CharacterTurnRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        AnthropicCharacterModelStreamAccumulator accumulator = new(request);
        CharacterModelStreamTermination? termination = null;
        try
        {
            await foreach (string document in AnthropicServerSentEventReader
                .ReadDocumentsAsync(stream, cancellationToken)
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
            return FailureResult(CharacterModelFailureCode.InvalidResponse);
        }
        catch (IOException)
        {
            return FailureResult(CharacterModelFailureCode.Transport);
        }

        if (termination is null)
        {
            return FailureResult(CharacterModelFailureCode.InvalidResponse);
        }

        return new AnthropicCharacterModelSseResult(
            termination,
            accumulator.InputTokenCount,
            accumulator.OutputTokenCount,
            accumulator.TotalTokenCount);
    }

    private static AnthropicCharacterModelSseResult FailureResult(
        CharacterModelFailureCode code) =>
        new(
            new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Failed,
                null,
                new CharacterModelFailure(code)),
            null,
            null,
            null);
}
