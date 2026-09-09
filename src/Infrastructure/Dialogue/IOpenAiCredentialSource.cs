namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Loads one OpenAI API key from a caller-owned credential facility for immediate use.
/// </summary>
public interface IOpenAiCredentialSource
{
    /// <summary>
    /// Loads the current API key, or returns <see langword="null"/> when none is available.
    /// </summary>
    ValueTask<string?> LoadApiKeyAsync(CancellationToken cancellationToken);
}
