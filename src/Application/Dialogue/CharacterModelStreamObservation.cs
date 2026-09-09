namespace Allrpcs.Application.Dialogue;

/// <summary>Pairs an observational stream-update sink with independent cancellation.</summary>
public sealed class CharacterModelStreamObservation
{
    /// <summary>Creates an opt-in observer boundary over caller-owned dependencies.</summary>
    public CharacterModelStreamObservation(
        ICharacterModelStreamUpdateSink sink,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sink);
        Sink = sink;
        CancellationToken = cancellationToken;
    }

    /// <summary>Gets the caller-owned exact update sink.</summary>
    public ICharacterModelStreamUpdateSink Sink { get; }

    /// <summary>Gets cancellation that is independent of model generation.</summary>
    public CancellationToken CancellationToken { get; }
}
