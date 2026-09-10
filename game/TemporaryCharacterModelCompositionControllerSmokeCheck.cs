using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks persisted provider composition without credentials or network input/output.</summary>
public partial class TemporaryCharacterModelCompositionControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_COMPOSITION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporaryCharacterModelCompositionController controller = null!;
    private CharacterModelSettingsController settingsController = null!;
    private CharacterModelSettingsStorageController storage = null!;
    private TemporaryCharacterModelSource modelSource = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<TemporaryCharacterModelCompositionController>(
            "../TemporaryCharacterModelCompositionController");
        settingsController = GetNode<CharacterModelSettingsController>(
            "../CharacterModelSettingsController");
        storage = GetNode<CharacterModelSettingsStorageController>(
            "../CharacterModelSettingsStorageController");
        modelSource = GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        CharacterModelSettings anthropicSettings = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettingsSelectionSnapshot unsupported = new(
            [anthropicSettings],
            CharacterModelProvider.Anthropic);
        ICharacterModel offline = modelSource.TakeModel();
        if (controller.TryInitialize(unsupported)
            || !ReferenceEquals(modelSource.TakeModel(), offline))
        {
            Fail("Provider composition did not preserve offline behavior for Anthropic.");
            return;
        }

        CharacterModelSettings ollamaSettings = new(
            CharacterModelProvider.Ollama,
            "gemma3");
        CharacterModelSettingsSelectionSnapshot ollama = new(
            [ollamaSettings],
            CharacterModelProvider.Ollama);
        if (storage.SaveCurrent(ollama) != CharacterModelSettingsDocumentWriteResult.Saved
            || !controller.TryLoadCurrent()
            || settingsController.Snapshot is not { } loaded
            || !ReferenceEquals(loaded.SelectedProvider, CharacterModelProvider.Ollama)
            || loaded.Settings.Count != 1
            || controller.TryInitialize(ollama))
        {
            Fail("Provider composition did not initialize the persisted Ollama selection once.");
            return;
        }

        ICharacterModel first = modelSource.TakeModel();
        ICharacterModel second = modelSource.TakeModel();
        if (first is not OllamaCharacterModel
            || !ReferenceEquals(first, second))
        {
            Fail("Provider composition did not retain its exact persisted Ollama model.");
            return;
        }

        GD.Print(
            "Temporary character-model composition smoke passed: unsupported Anthropic remained "
            + "offline, one exact retained persisted Ollama snapshot composed once without "
            + "credential or network input/output, and the scene retained one stable selected "
            + "model.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
