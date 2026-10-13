namespace Allrpcs.Infrastructure.Speech;

/// <summary>Starts a local worker without giving application code paths or process ownership.</summary>
public interface IPocketSpeechWorkerFactory
{
    /// <summary>Starts one process; the synthesizer validates its ready handshake.</summary>
    ValueTask<IPocketSpeechWorker> StartAsync(CancellationToken cancellationToken);
}
