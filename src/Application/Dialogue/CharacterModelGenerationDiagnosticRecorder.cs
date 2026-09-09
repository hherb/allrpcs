namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Isolates best-effort diagnostic recording from the authoritative generation outcome.
/// </summary>
public static class CharacterModelGenerationDiagnosticRecorder
{
    /// <summary>
    /// Attempts to record one diagnostic and reports only whether the observational sink accepted
    /// it. Sink failure and cancellation never replace or reinterpret the generation outcome.
    /// </summary>
    public static async ValueTask<bool> TryRecordAsync(
        ICharacterModelGenerationDiagnosticSink sink,
        CharacterModelGenerationDiagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentNullException.ThrowIfNull(diagnostic);

        try
        {
            await sink.RecordAsync(diagnostic, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception exception) when (IsObservationalFailure(exception))
        {
            return false;
        }
    }

    private static bool IsObservationalFailure(Exception exception) =>
        exception is not OutOfMemoryException
        and not StackOverflowException
        and not AccessViolationException;
}
