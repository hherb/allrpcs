using System.Security.Cryptography;
using System.Text;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Stores one OpenAI API key under a fixed Windows generic credential identity.</summary>
public sealed class WindowsOpenAiCredentialStore : IOpenAiCredentialStore
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly IWindowsCredentialManager credentialManager;

    /// <summary>Gets the exact generic credential target.</summary>
    public const string TargetName = "Allrpcs.OpenAI";

    /// <summary>Gets the exact generic credential user identity.</summary>
    public const string UserName = "api-key";

    /// <summary>Creates a store behind a caller-owned native boundary.</summary>
    public WindowsOpenAiCredentialStore(IWindowsCredentialManager credentialManager)
    {
        ArgumentNullException.ThrowIfNull(credentialManager);
        this.credentialManager = credentialManager;
    }

    /// <summary>Gets whether Windows Credential Manager is available.</summary>
    public bool IsAvailable => credentialManager.IsAvailable;

    /// <inheritdoc />
    public async ValueTask<OpenAiCredentialWriteStatus> SaveApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        cancellationToken.ThrowIfCancellationRequested();
        if (!OpenAiCredentialPolicy.IsValid(apiKey) || !IsAvailable)
        {
            return OpenAiCredentialWriteStatus.Failed;
        }

        byte[] secretBytes = StrictUtf8.GetBytes(apiKey);
        try
        {
            WindowsCredentialManagerResult result = await credentialManager.WriteAsync(
                    TargetName,
                    UserName,
                    secretBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            try
            {
                return result.Status == WindowsCredentialManagerStatus.Succeeded
                    ? OpenAiCredentialWriteStatus.Saved
                    : OpenAiCredentialWriteStatus.Failed;
            }
            finally
            {
                result.ClearSecretBytes();
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretBytes);
        }
    }

    /// <inheritdoc />
    public async ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsAvailable)
        {
            return OpenAiCredentialLoadResult.Failed();
        }

        WindowsCredentialManagerResult result = await credentialManager.ReadAsync(
                TargetName,
                UserName,
                OpenAiCredentialPolicy.MaximumApiKeyLength,
                cancellationToken)
            .ConfigureAwait(false);
        if (result.Status == WindowsCredentialManagerStatus.Missing)
        {
            return OpenAiCredentialLoadResult.Missing();
        }

        if (result.Status != WindowsCredentialManagerStatus.Succeeded)
        {
            return OpenAiCredentialLoadResult.Failed();
        }

        try
        {
            string apiKey = StrictUtf8.GetString(result.SecretBytes.Span);
            return OpenAiCredentialPolicy.IsValid(apiKey)
                ? OpenAiCredentialLoadResult.Loaded(apiKey)
                : OpenAiCredentialLoadResult.Failed();
        }
        catch (DecoderFallbackException)
        {
            return OpenAiCredentialLoadResult.Failed();
        }
        finally
        {
            result.ClearSecretBytes();
        }
    }

    /// <inheritdoc />
    public async ValueTask<OpenAiCredentialDeleteStatus> DeleteApiKeyAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsAvailable)
        {
            return OpenAiCredentialDeleteStatus.Failed;
        }

        WindowsCredentialManagerResult result = await credentialManager.DeleteAsync(
                TargetName,
                cancellationToken)
            .ConfigureAwait(false);
        try
        {
            return result.Status switch
            {
                WindowsCredentialManagerStatus.Succeeded => OpenAiCredentialDeleteStatus.Deleted,
                WindowsCredentialManagerStatus.Missing => OpenAiCredentialDeleteStatus.Missing,
                _ => OpenAiCredentialDeleteStatus.Failed,
            };
        }
        finally
        {
            result.ClearSecretBytes();
        }
    }
}
