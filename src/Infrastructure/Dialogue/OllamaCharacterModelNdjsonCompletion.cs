using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Completes one provider-neutral response from bounded native Ollama NDJSON.</summary>
public static class OllamaCharacterModelNdjsonCompletion
{
    /// <summary>Reads one complete native stream for the exact turn request.</summary>
    public static async Task<CharacterModelStreamTermination> CompleteAsync(
        CharacterTurnRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        OllamaCharacterModelNdjsonResult result = await CompleteWithUsageAsync(
                request,
                stream,
                cancellationToken)
            .ConfigureAwait(false);
        return result.Termination;
    }

    /// <summary>Reads one stream while retaining only bounded terminal usage.</summary>
    public static async Task<OllamaCharacterModelNdjsonResult> CompleteWithUsageAsync(
        CharacterTurnRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        IReadOnlyList<string> documents;
        try
        {
            documents = await OllamaNewlineDelimitedJsonReader.ReadAsync(
                    stream,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (InvalidDataException)
        {
            return FailureResult(CharacterModelFailureCode.InvalidResponse);
        }
        catch (IOException)
        {
            return FailureResult(CharacterModelFailureCode.Transport);
        }

        OllamaCharacterModelStreamAccumulator accumulator = new(request);
        CharacterModelStreamTermination? termination = null;
        foreach (string document in documents)
        {
            if (termination is not null)
            {
                return FailureResult(CharacterModelFailureCode.InvalidResponse);
            }

            termination = accumulator.Apply(document);
        }

        if (termination is null)
        {
            return FailureResult(CharacterModelFailureCode.InvalidResponse);
        }

        return new OllamaCharacterModelNdjsonResult(
            termination,
            accumulator.InputTokenCount,
            accumulator.OutputTokenCount,
            accumulator.TotalTokenCount);
    }

    private static OllamaCharacterModelNdjsonResult FailureResult(
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
