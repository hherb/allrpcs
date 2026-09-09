namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Supplies caller-owned observational diagnostics for generation outcomes synthesized by the
/// application controller.
/// </summary>
public sealed class CharacterModelGenerationControllerDiagnostics
{
    /// <summary>
    /// Creates an opt-in diagnostic boundary with an independent recording cancellation token.
    /// </summary>
    public CharacterModelGenerationControllerDiagnostics(
        CharacterModelProvider provider,
        TimeProvider timeProvider,
        ICharacterModelGenerationDiagnosticSink sink,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(sink);

        Provider = provider;
        TimeProvider = timeProvider;
        Sink = sink;
        CancellationToken = cancellationToken;
    }

    /// <summary>Gets the exact canonical provider for the controlled generation.</summary>
    public CharacterModelProvider Provider { get; }

    /// <summary>Gets the caller-owned monotonic elapsed-time source.</summary>
    public TimeProvider TimeProvider { get; }

    /// <summary>Gets the caller-owned observational sink.</summary>
    public ICharacterModelGenerationDiagnosticSink Sink { get; }

    /// <summary>Gets cancellation for diagnostic recording, independent of generation.</summary>
    public CancellationToken CancellationToken { get; }
}
