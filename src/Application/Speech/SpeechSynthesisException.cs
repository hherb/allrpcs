namespace Allrpcs.Application.Speech;

/// <summary>Signals an unavailable synthesis path while the caller retains text and subtitles.</summary>
public sealed class SpeechSynthesisException : Exception
{
    /// <summary>Creates a failure message without embedding dialogue, paths or worker output.</summary>
    public SpeechSynthesisException(string message) : base(message)
    {
    }
}
