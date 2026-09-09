namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Loads one Anthropic API key for immediate caller-owned use.</summary>
public interface IAnthropicCredentialSource
{
    /// <summary>Loads the current API key with an explicit redacted outcome.</summary>
    ValueTask<AnthropicCredentialLoadResult> LoadApiKeyAsync(
        CancellationToken cancellationToken);
}
