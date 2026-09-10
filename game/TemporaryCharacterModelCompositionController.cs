using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;
using NetHttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>
/// Composes one persisted selected provider binding while preserving offline fallback behavior.
/// </summary>
public partial class TemporaryCharacterModelCompositionController : Node
{
    internal const string AutomaticCompositionDisabledEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED";

    private const string DisabledEnvironmentValue = "1";

    private CharacterModelSettingsStorageController storageController = null!;
    private TemporaryCharacterModelSource modelSource = null!;
    private NetHttpClient client = null!;
    private bool isInitialized;

    /// <inheritdoc />
    public override void _Ready()
    {
        storageController = GetNode<CharacterModelSettingsStorageController>(
            "../CharacterModelSettingsStorageController");
        modelSource = GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource");
        client = new NetHttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };
        if (OS.GetEnvironment(AutomaticCompositionDisabledEnvironmentVariable)
            != DisabledEnvironmentValue)
        {
            TryLoadCurrent();
        }
    }

    /// <summary>Loads and composes the current supported selected provider once.</summary>
    public bool TryLoadCurrent()
    {
        CharacterModelSettingsDocumentLoadResult result = storageController.LoadCurrent();
        return result.Status == CharacterModelSettingsDocumentLoadStatus.Loaded
            && TryInitialize(result.Selection!);
    }

    /// <summary>Composes one exact supported selected provider without input/output.</summary>
    public bool TryInitialize(CharacterModelSettingsSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (isInitialized)
        {
            return false;
        }

        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        CharacterModelBinding? binding = CreateBinding(selectedSettings);
        if (binding is null
            || !modelSource.TryInitializeBindings(
                selection,
                new CharacterModelBindingSnapshot([binding])))
        {
            return false;
        }

        isInitialized = true;
        return true;
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        client?.Dispose();
    }

    private CharacterModelBinding? CreateBinding(CharacterModelSettings settings)
    {
        if (ReferenceEquals(settings.Provider, CharacterModelProvider.Ollama))
        {
            return OllamaCharacterModelBindingFactory.Create(
                settings,
                client,
                OllamaEndpoint.Default);
        }

        if (!ReferenceEquals(settings.Provider, CharacterModelProvider.OpenAi))
        {
            return null;
        }

        IOpenAiCredentialSource? credentialSource = CreateOpenAiCredentialSource();
        return credentialSource is null
            ? null
            : OpenAiCharacterModelBindingFactory.Create(settings, client, credentialSource);
    }

    private static IOpenAiCredentialSource? CreateOpenAiCredentialSource()
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
