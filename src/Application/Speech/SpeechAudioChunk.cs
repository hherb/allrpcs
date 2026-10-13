namespace Allrpcs.Application.Speech;

/// <summary>Owns a bounded immutable copy of finite mono float PCM samples.</summary>
public sealed class SpeechAudioChunk
{
    /// <summary>Limits buffering to one second at the maximum supported rate.</summary>
    public const int MaximumSamples = 48000;

    /// <summary>Rejects malformed PCM and copies it before exposing the chunk to playback.</summary>
    public SpeechAudioChunk(int sampleRate, IReadOnlyList<float> samples)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (sampleRate is < 8000 or > 48000)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }
        if (samples.Count is < 1 or > MaximumSamples || samples.Any(sample =>
            !float.IsFinite(sample) || sample is < -1 or > 1))
        {
            throw new ArgumentException("PCM must be finite, normalized and bounded.", nameof(samples));
        }
        SampleRate = sampleRate;
        Samples = Array.AsReadOnly(samples.ToArray());
    }

    /// <summary>Gets the sample rate in hertz.</summary>
    public int SampleRate { get; }

    /// <summary>Gets immutable mono samples in playback order.</summary>
    public IReadOnlyList<float> Samples { get; }
}
