using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Retains one provider-neutral stream termination plus bounded terminal usage, without provider
/// response metadata.
/// </summary>
public sealed class OpenAiCharacterModelSseResult
{
    internal OpenAiCharacterModelSseResult(
        CharacterModelStreamTermination termination,
        int? inputTokenCount,
        int? outputTokenCount,
        int? totalTokenCount)
    {
        Termination = termination;
        InputTokenCount = inputTokenCount;
        OutputTokenCount = outputTokenCount;
        TotalTokenCount = totalTokenCount;
    }

    /// <summary>Gets the exact provider-neutral terminal update.</summary>
    public CharacterModelStreamTermination Termination { get; }

    /// <summary>Gets the bounded input-token count, or unknown.</summary>
    public int? InputTokenCount { get; }

    /// <summary>Gets the bounded output-token count, or unknown.</summary>
    public int? OutputTokenCount { get; }

    /// <summary>Gets the bounded total-token count, or unknown.</summary>
    public int? TotalTokenCount { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        string outcome = Termination.Failure is null
            ? Termination.Status.ToString()
            : $"{Termination.Status}:{Termination.Failure.Code}";
        string usage = TotalTokenCount is null
            ? "unknown"
            : $"{InputTokenCount}/{OutputTokenCount}/{TotalTokenCount}";
        return $"{outcome}:tokens={usage}";
    }
}
