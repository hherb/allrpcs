using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns explicit redacted OpenAI credential operations for the current OS.</summary>
public partial class OpenAiCredentialController : Node
{
    internal const string TestStoreEnvironmentVariable =
        "ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE";

    private const string EnabledEnvironmentValue = "1";

    private IOpenAiCredentialStore? credentialStore;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (DebugTestEnvironment.GetEnvironment(TestStoreEnvironmentVariable) != EnabledEnvironmentValue)
        {
            credentialStore = CreateCredentialStore();
        }
    }

    /// <summary>Saves or replaces one bounded API key without retaining it in scene state.</summary>
    public ValueTask<OpenAiCredentialWriteStatus> SaveApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        cancellationToken.ThrowIfCancellationRequested();
        return credentialStore is null || !OpenAiCredentialPolicy.IsValid(apiKey)
            ? ValueTask.FromResult(OpenAiCredentialWriteStatus.Failed)
            : credentialStore.SaveApiKeyAsync(apiKey, cancellationToken);
    }

    /// <summary>Deletes the OS-stored API key through one explicit redacted operation.</summary>
    public ValueTask<OpenAiCredentialDeleteStatus> DeleteApiKeyAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return credentialStore is null
            ? ValueTask.FromResult(OpenAiCredentialDeleteStatus.Failed)
            : credentialStore.DeleteApiKeyAsync(cancellationToken);
    }

    internal bool TryInitialize(IOpenAiCredentialStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (credentialStore is not null)
        {
            return false;
        }

        credentialStore = store;
        return true;
    }

    private static IOpenAiCredentialStore? CreateCredentialStore()
    {
        if (OperatingSystem.IsMacOS())
        {
            return new MacOsOpenAiCredentialStore(new MacOsKeychainProcessRunner());
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSecretServiceOpenAiCredentialStore(
                new LinuxSecretServiceProcessRunner());
        }

        return OperatingSystem.IsWindows()
            ? new WindowsOpenAiCredentialStore(new WindowsCredentialManager())
            : null;
    }
}
