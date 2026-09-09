namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Receives redacted generation diagnostics through caller-owned observational infrastructure.
/// </summary>
public interface ICharacterModelGenerationDiagnosticSink
{
    /// <summary>
    /// Records one diagnostic while observing cancellation. Callers must keep sink failure
    /// separate from the generation outcome.
    /// </summary>
    ValueTask RecordAsync(
        CharacterModelGenerationDiagnostic diagnostic,
        CancellationToken cancellationToken);
}
