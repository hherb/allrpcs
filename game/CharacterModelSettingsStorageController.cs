using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes strict provider-settings documents with isolated local storage.</summary>
public partial class CharacterModelSettingsStorageController : Node
{
    internal const string SmokeCheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_SETTINGS_STORAGE_CHECK";
    internal const string TestStorageEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE";

    private const string EnabledEnvironmentValue = "1";
    private const string SettingsDocumentVirtualPath =
        "user://character-model-settings-v1.json";

    private CharacterModelSettingsDocumentController documentController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        FileCharacterModelSettingsDocumentStore documentStore = new(CurrentFilePath());
        documentController = new CharacterModelSettingsDocumentController(documentStore);
    }

    /// <summary>Serializes and stores one complete provider-settings selection.</summary>
    public CharacterModelSettingsDocumentWriteResult SaveCurrent(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return documentController.Save(selection);
    }

    /// <summary>Loads and strictly materializes the current provider-settings document.</summary>
    public CharacterModelSettingsDocumentLoadResult LoadCurrent() =>
        documentController.Load();

    internal static string CurrentFilePath() =>
        DebugTestEnvironment.GetEnvironment(TestStorageEnvironmentVariable) == EnabledEnvironmentValue
            ? Path.Combine(
                Path.GetTempPath(),
                $"allrpcs-character-model-settings-v1-smoke-{OS.GetProcessId()}.json")
            : ProjectSettings.GlobalizePath(SettingsDocumentVirtualPath);
}
