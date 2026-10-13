namespace Allrpcs.Application.Speech;

/// <summary>Retains exact submitted text and a presentation-owned voice profile, without provider IDs.</summary>
public sealed class SpeechSynthesisRequest
{
    /// <summary>Bounds one request; longer dialogue must be explicitly segmented by its presentation owner.</summary>
    public const int MaximumTextLength = 2000;

    /// <summary>Validates a bounded request without trimming or rewriting the spoken words.</summary>
    public SpeechSynthesisRequest(string text, SpeechLanguage language, string voiceProfile)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaximumTextLength || text.Contains('\0'))
        {
            throw new ArgumentException("Speech text must be nonempty and bounded.", nameof(text));
        }
        if (!Enum.IsDefined(language))
        {
            throw new ArgumentOutOfRangeException(nameof(language));
        }
        ArgumentNullException.ThrowIfNull(voiceProfile);
        if (voiceProfile.Length is < 1 or > 64 || voiceProfile.Any(
            character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
        {
            throw new ArgumentException("Voice profile must be a bounded opaque identifier.", nameof(voiceProfile));
        }
        Text = text;
        Language = language;
        VoiceProfile = voiceProfile;
    }

    /// <summary>Gets the exact requested dialogue, never a provider-generated response.</summary>
    public string Text { get; }

    /// <summary>Gets the explicitly selected synthesis language.</summary>
    public SpeechLanguage Language { get; }

    /// <summary>Gets the presentation profile; it is not an actor's domain identity or provider voice ID.</summary>
    public string VoiceProfile { get; }

    /// <summary>Renders no dialogue content in diagnostics.</summary>
    public override string ToString() => nameof(SpeechSynthesisRequest);
}
