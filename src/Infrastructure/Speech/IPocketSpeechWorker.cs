namespace Allrpcs.Infrastructure.Speech;

/// <summary>Provides a bounded line-oriented channel to one local speech process.</summary>
public interface IPocketSpeechWorker : IAsyncDisposable
{
    /// <summary>Writes one protocol message without logging its dialogue content.</summary>
    ValueTask SendAsync(string message, CancellationToken cancellationToken);

    /// <summary>Reads one bounded protocol message, or null when the process closes output.</summary>
    ValueTask<string?> ReceiveAsync(CancellationToken cancellationToken);
}
