using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Retains a provider-neutral termination and optional bounded Ollama usage.</summary>
public sealed record OllamaCharacterModelNdjsonResult
{
    /// <summary>Creates a completed NDJSON projection.</summary>
    public OllamaCharacterModelNdjsonResult(
        CharacterModelStreamTermination termination,
        int? inputTokenCount,
        int? outputTokenCount,
        int? totalTokenCount)
    {
        ArgumentNullException.ThrowIfNull(termination);
        Termination = termination;
        InputTokenCount = inputTokenCount;
        OutputTokenCount = outputTokenCount;
        TotalTokenCount = totalTokenCount;
    }

    /// <summary>Gets the provider-neutral terminal update.</summary>
    public CharacterModelStreamTermination Termination { get; }

    /// <summary>Gets the bounded prompt-token count, or unknown.</summary>
    public int? InputTokenCount { get; }

    /// <summary>Gets the bounded generated-token count, or unknown.</summary>
    public int? OutputTokenCount { get; }

    /// <summary>Gets the bounded derived total-token count, or unknown.</summary>
    public int? TotalTokenCount { get; }
}
