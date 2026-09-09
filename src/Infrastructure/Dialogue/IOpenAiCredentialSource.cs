namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Loads one OpenAI API key from a caller-owned credential facility for immediate use.
/// </summary>
public interface IOpenAiCredentialSource
{
    /// <summary>
    /// Loads the current API key with an explicit redacted outcome.
    /// </summary>
    ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
        CancellationToken cancellationToken);
}
