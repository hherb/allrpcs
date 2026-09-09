namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reports a redacted credential-load outcome and carries a key only on success.</summary>
public sealed class OpenAiCredentialLoadResult
{
    private OpenAiCredentialLoadResult(OpenAiCredentialLoadStatus status, string? apiKey)
    {
        Status = status;
        ApiKey = apiKey;
    }

    /// <summary>Gets the redacted outcome.</summary>
    public OpenAiCredentialLoadStatus Status { get; }

    /// <summary>Gets the exact loaded API key, or <see langword="null"/> otherwise.</summary>
    public string? ApiKey { get; }

    /// <summary>Creates a successful result for one bounded API key.</summary>
    public static OpenAiCredentialLoadResult Loaded(string apiKey)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        if (!OpenAiCredentialPolicy.IsValid(apiKey))
        {
            throw new ArgumentException("Value must be a bounded printable ASCII API key.", nameof(apiKey));
        }

        return new(OpenAiCredentialLoadStatus.Loaded, apiKey);
    }

    /// <summary>Creates an explicit missing-key result.</summary>
    public static OpenAiCredentialLoadResult Missing() =>
        new(OpenAiCredentialLoadStatus.Missing, null);

    /// <summary>Creates a redacted failure result.</summary>
    public static OpenAiCredentialLoadResult Failed() =>
        new(OpenAiCredentialLoadStatus.Failed, null);

    /// <inheritdoc />
    public override string ToString() => Status.ToString();
}
