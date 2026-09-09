namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reports a redacted credential-load outcome and carries a key only on success.</summary>
public sealed class AnthropicCredentialLoadResult
{
    private AnthropicCredentialLoadResult(AnthropicCredentialLoadStatus status, string? apiKey)
    {
        Status = status;
        ApiKey = apiKey;
    }

    /// <summary>Gets the redacted outcome.</summary>
    public AnthropicCredentialLoadStatus Status { get; }

    /// <summary>Gets the exact loaded API key, or <see langword="null"/> otherwise.</summary>
    public string? ApiKey { get; }

    /// <summary>Creates a successful result for one bounded API key.</summary>
    public static AnthropicCredentialLoadResult Loaded(string apiKey)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        if (!AnthropicCredentialPolicy.IsValid(apiKey))
        {
            throw new ArgumentException(
                "Value must be a bounded printable ASCII API key.",
                nameof(apiKey));
        }

        return new(AnthropicCredentialLoadStatus.Loaded, apiKey);
    }

    /// <summary>Creates an explicit missing-key result.</summary>
    public static AnthropicCredentialLoadResult Missing() =>
        new(AnthropicCredentialLoadStatus.Missing, null);

    /// <summary>Creates a redacted failure result.</summary>
    public static AnthropicCredentialLoadResult Failed() =>
        new(AnthropicCredentialLoadStatus.Failed, null);

    /// <inheritdoc />
    public override string ToString() => Status.ToString();
}
