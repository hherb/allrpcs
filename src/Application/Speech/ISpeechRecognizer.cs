namespace Allrpcs.Application.Speech;

/// <summary>Transforms caller-owned mono PCM into editable text without submitting dialogue.</summary>
public interface ISpeechRecognizer
{
    /// <summary>Recognizes one bounded input session; cancellation invalidates every remaining event.</summary>
    IAsyncEnumerable<SpeechRecognitionEvent> RecognizeAsync(SpeechLanguage language,
        IAsyncEnumerable<SpeechAudioChunk> input, CancellationToken cancellationToken);
}

/// <summary>Captures local mono PCM only when explicitly enumerated by an opted-in caller.</summary>
public interface IMicrophoneCapture
{
    /// <summary>Opens the platform microphone and retires it when cancelled or enumeration ends.</summary>
    IAsyncEnumerable<SpeechAudioChunk> CaptureAsync(CancellationToken cancellationToken);
}

/// <summary>Presentation-only recognition lifecycle; no kind represents a dialogue action.</summary>
public enum SpeechRecognitionKind { CaptureStarted, VoiceStarted, Partial, Final }

/// <summary>Retains a bounded utterance identifier and text for a single recognition generation.</summary>
public sealed record SpeechRecognitionEvent
{
    /// <summary>Rejects unbounded identities, malformed text and unknown event kinds.</summary>
    public SpeechRecognitionEvent(SpeechRecognitionKind kind, string utteranceId = "", string text = "")
    {
        ArgumentNullException.ThrowIfNull(utteranceId); ArgumentNullException.ThrowIfNull(text);
        if (!Enum.IsDefined(kind) || utteranceId.Length > 64 || text.Length > 2000 || text.Contains('\0')
            || (kind is SpeechRecognitionKind.Partial or SpeechRecognitionKind.Final && utteranceId.Length == 0))
        { throw new ArgumentException("Invalid recognition event."); }
        Kind = kind; UtteranceId = utteranceId; Text = text;
    }
    /// <summary>Gets the presentation event kind.</summary>
    public SpeechRecognitionKind Kind { get; }
    /// <summary>Gets an opaque provider-neutral utterance identity.</summary>
    public string UtteranceId { get; }
    /// <summary>Gets draft recognition text, never a submitted command.</summary>
    public string Text { get; }
}
