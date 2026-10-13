namespace Allrpcs.Application.Speech;

/// <summary>Streams character speech independently of a synthesis provider or playback device.</summary>
public interface ISpeechSynthesizer
{
    /// <summary>Produces mono PCM for the supplied text; cancellation stops obsolete generation.</summary>
    IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(
        SpeechSynthesisRequest request,
        CancellationToken cancellationToken);
}
