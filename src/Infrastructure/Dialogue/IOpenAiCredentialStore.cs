namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Owns the save, load, and delete lifecycle for one OpenAI API key.</summary>
public interface IOpenAiCredentialStore : IOpenAiCredentialSource
{
    /// <summary>Saves or replaces the bounded API key.</summary>
    ValueTask<OpenAiCredentialWriteStatus> SaveApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken);

    /// <summary>Deletes the API key when present.</summary>
    ValueTask<OpenAiCredentialDeleteStatus> DeleteApiKeyAsync(
        CancellationToken cancellationToken);
}
