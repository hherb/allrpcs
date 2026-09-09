using System.Globalization;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one bounded provider-neutral generation diagnostic without model content or provider
/// request details.
/// </summary>
public sealed class CharacterModelGenerationDiagnostic
{
    /// <summary>Gets the largest accepted count for one token-count field.</summary>
    public const int MaximumTokenCount = 1_000_000;

    /// <summary>Gets the largest accepted elapsed generation duration.</summary>
    public static TimeSpan MaximumElapsed { get; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Creates a redacted terminal diagnostic. Usage must be wholly absent or contain consistent
    /// input, output, and total counts.
    /// </summary>
    public CharacterModelGenerationDiagnostic(
        CharacterModelProvider provider,
        CharacterModelStreamTerminationStatus status,
        CharacterModelFailureCode? failureCode,
        TimeSpan elapsed,
        int? inputTokenCount,
        int? outputTokenCount,
        int? totalTokenCount)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if (failureCode is { } definedFailureCode && !Enum.IsDefined(definedFailureCode))
        {
            throw new ArgumentOutOfRangeException(nameof(failureCode));
        }

        if (status == CharacterModelStreamTerminationStatus.Failed)
        {
            if (failureCode is null)
            {
                throw new ArgumentNullException(nameof(failureCode));
            }
        }
        else if (failureCode is not null)
        {
            throw new ArgumentException(
                "Only a failed generation diagnostic can retain a failure code.",
                nameof(failureCode));
        }

        if (elapsed < TimeSpan.Zero || elapsed > MaximumElapsed)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed));
        }

        ValidateUsage(inputTokenCount, outputTokenCount, totalTokenCount);

        Provider = provider;
        Status = status;
        FailureCode = failureCode;
        Elapsed = elapsed;
        InputTokenCount = inputTokenCount;
        OutputTokenCount = outputTokenCount;
        TotalTokenCount = totalTokenCount;
    }

    /// <summary>Gets the exact canonical configured provider.</summary>
    public CharacterModelProvider Provider { get; }

    /// <summary>Gets the terminal generation outcome.</summary>
    public CharacterModelStreamTerminationStatus Status { get; }

    /// <summary>Gets the provider-neutral failure code for a failed outcome.</summary>
    public CharacterModelFailureCode? FailureCode { get; }

    /// <summary>Gets the bounded elapsed generation duration.</summary>
    public TimeSpan Elapsed { get; }

    /// <summary>Gets the bounded input-token count, or <see langword="null"/> when unknown.</summary>
    public int? InputTokenCount { get; }

    /// <summary>Gets the bounded output-token count, or <see langword="null"/> when unknown.</summary>
    public int? OutputTokenCount { get; }

    /// <summary>Gets the bounded total-token count, or <see langword="null"/> when unknown.</summary>
    public int? TotalTokenCount { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        string outcome = FailureCode is null ? Status.ToString() : $"{Status}:{FailureCode}";
        string elapsedMilliseconds = Elapsed.TotalMilliseconds.ToString(
            "0.#######",
            CultureInfo.InvariantCulture);
        string usage = TotalTokenCount is null
            ? "unknown"
            : $"{InputTokenCount}/{OutputTokenCount}/{TotalTokenCount}";
        return $"{Provider}:{outcome}:{elapsedMilliseconds}ms:tokens={usage}";
    }

    private static void ValidateUsage(
        int? inputTokenCount,
        int? outputTokenCount,
        int? totalTokenCount)
    {
        bool hasAnyCount = inputTokenCount is not null
            || outputTokenCount is not null
            || totalTokenCount is not null;
        bool hasAllCounts = inputTokenCount is not null
            && outputTokenCount is not null
            && totalTokenCount is not null;
        if (hasAnyCount && !hasAllCounts)
        {
            throw new ArgumentException("Token usage must be wholly present or absent.");
        }

        if (!hasAllCounts)
        {
            return;
        }

        int inputCount = inputTokenCount.GetValueOrDefault();
        int outputCount = outputTokenCount.GetValueOrDefault();
        int totalCount = totalTokenCount.GetValueOrDefault();

        if (inputCount < 0 || inputCount > MaximumTokenCount)
        {
            throw new ArgumentOutOfRangeException(nameof(inputTokenCount));
        }

        if (outputCount < 0 || outputCount > MaximumTokenCount)
        {
            throw new ArgumentOutOfRangeException(nameof(outputTokenCount));
        }

        if (totalCount < 0 || totalCount > MaximumTokenCount)
        {
            throw new ArgumentOutOfRangeException(nameof(totalTokenCount));
        }

        if ((long)inputCount + outputCount != totalCount)
        {
            throw new ArgumentException("Total token count must equal input plus output counts.");
        }
    }
}
