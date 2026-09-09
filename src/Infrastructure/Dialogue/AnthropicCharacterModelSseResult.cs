using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Retains one redacted Anthropic stream termination and optional bounded usage.</summary>
public sealed record AnthropicCharacterModelSseResult
{
    /// <summary>Creates one result from a terminal update and wholly present or absent usage.</summary>
    public AnthropicCharacterModelSseResult(
        CharacterModelStreamTermination termination,
        int? inputTokenCount,
        int? outputTokenCount,
        int? totalTokenCount)
    {
        ArgumentNullException.ThrowIfNull(termination);
        bool hasAllCounts = inputTokenCount.HasValue
            && outputTokenCount.HasValue
            && totalTokenCount.HasValue;
        if (hasAllCounts)
        {
            _ = new CharacterModelGenerationDiagnostic(
                CharacterModelProvider.Anthropic,
                termination.Status,
                termination.Failure?.Code,
                TimeSpan.Zero,
                inputTokenCount,
                outputTokenCount,
                totalTokenCount);
        }
        else if (inputTokenCount.HasValue
            || outputTokenCount.HasValue
            || totalTokenCount.HasValue)
        {
            throw new ArgumentException("Anthropic usage counts must be wholly present or absent.");
        }

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
}
