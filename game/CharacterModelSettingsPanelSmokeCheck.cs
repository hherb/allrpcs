using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit provider-settings panel apply and persistence when enabled.</summary>
public partial class CharacterModelSettingsPanelSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_SETTINGS_PANEL_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private CharacterModelSettingsController controller = null!;
    private CharacterModelSettingsStorageController storage = null!;
    private TemporaryCharacterModelSource modelSource = null!;
    private OptionButton provider = null!;
    private LineEdit modelName = null!;
    private Button apply = null!;
    private Button remove = null!;
    private Button save = null!;
    private Button activate = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<CharacterModelSettingsController>(
            "../CharacterModelSettingsController");
        storage = GetNode<CharacterModelSettingsStorageController>(
            "../CharacterModelSettingsStorageController");
        modelSource = GetNode<TemporaryCharacterModelSource>(
            "../TemporaryCharacterModelSource");
        provider = GetNode<OptionButton>("../SettingsOverlay/Panel/Provider");
        modelName = GetNode<LineEdit>("../SettingsOverlay/Panel/ModelName");
        apply = GetNode<Button>("../SettingsOverlay/Panel/Apply");
        remove = GetNode<Button>("../SettingsOverlay/Panel/Remove");
        save = GetNode<Button>("../SettingsOverlay/Panel/Save");
        activate = GetNode<Button>("../SettingsOverlay/Panel/Activate");
        status = GetNode<Label>("../SettingsOverlay/Panel/Status");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        try
        {
            RunCheck();
        }
        finally
        {
            File.Delete(StoragePath());
        }
    }

    private void RunCheck()
    {
        ICharacterModel offline = modelSource.TakeModel();
        activate.EmitSignal(BaseButton.SignalName.Pressed);
        if (!ReferenceEquals(modelSource.TakeModel(), offline)
            || status.Text
                != TemporaryCharacterModelSettingsPanel.ActivationFailedStatusText)
        {
            Fail("Provider-settings panel did not fail closed without current settings.");
            return;
        }

        save.EmitSignal(BaseButton.SignalName.Pressed);
        if (controller.Snapshot is not null
            || File.Exists(StoragePath())
            || provider.ItemCount != 4
            || provider.Selected != -1
            || provider.GetItemText(0) != "Choose provider"
            || provider.GetItemText(1) != "OpenAI"
            || provider.GetItemText(2) != "Anthropic"
            || provider.GetItemText(3) != "Ollama"
            || modelName.Text.Length != 0
            || status.Text != TemporaryCharacterModelSettingsPanel.SaveFailedStatusText)
        {
            Fail("Provider-settings panel did not begin empty and fail closed while uninitialized.");
            return;
        }

        provider.Select(3);
        modelName.Text = "gemma3";
        apply.EmitSignal(BaseButton.SignalName.Pressed);
        CharacterModelSettingsSelectionSnapshot? applied = controller.Snapshot;
        if (applied is null
            || !ReferenceEquals(applied.SelectedProvider, CharacterModelProvider.Ollama)
            || applied.Settings.Count != 1
            || !ReferenceEquals(applied.Settings[0].Provider, CharacterModelProvider.Ollama)
            || applied.Settings[0].ModelName != "gemma3"
            || File.Exists(StoragePath())
            || status.Text != TemporaryCharacterModelSettingsPanel.AppliedStatusText)
        {
            Fail("Provider-settings panel did not apply one valid unsaved Ollama setting.");
            return;
        }

        activate.EmitSignal(BaseButton.SignalName.Pressed);
        ICharacterModel initialModel = modelSource.TakeModel();
        if (initialModel is not OllamaCharacterModel
            || ReferenceEquals(initialModel, offline)
            || !ReferenceEquals(modelSource.TakeModel(), initialModel)
            || status.Text != TemporaryCharacterModelSettingsPanel.ActivatedStatusText
            || File.Exists(StoragePath()))
        {
            Fail("Provider-settings panel did not activate its first unsaved Ollama binding.");
            return;
        }

        modelName.Text = "invalid model";
        apply.EmitSignal(BaseButton.SignalName.Pressed);
        if (!ReferenceEquals(controller.Snapshot, applied)
            || File.Exists(StoragePath())
            || status.Text != TemporaryCharacterModelSettingsPanel.RejectedStatusText)
        {
            Fail("Provider-settings panel changed current state after invalid input.");
            return;
        }

        modelName.Text = "gemma3-live";
        apply.EmitSignal(BaseButton.SignalName.Pressed);
        CharacterModelSettingsSelectionSnapshot? activatedSelection = controller.Snapshot;
        activate.EmitSignal(BaseButton.SignalName.Pressed);
        ICharacterModel activatedModel = modelSource.TakeModel();
        if (activatedSelection is null
            || ReferenceEquals(activatedSelection, applied)
            || activatedSelection.Settings[0].ModelName != "gemma3-live"
            || status.Text != TemporaryCharacterModelSettingsPanel.ActivatedStatusText
            || File.Exists(StoragePath())
            || activatedModel is not OllamaCharacterModel
            || ReferenceEquals(activatedModel, initialModel)
            || !ReferenceEquals(modelSource.TakeModel(), activatedModel))
        {
            Fail("Provider-settings panel did not activate one unsaved future Ollama binding.");
            return;
        }

        provider.Select(1);
        modelName.Text = "gpt-test";
        apply.EmitSignal(BaseButton.SignalName.Pressed);
        CharacterModelSettingsSelectionSnapshot? withOpenAi = controller.Snapshot;
        if (withOpenAi is null
            || !ReferenceEquals(withOpenAi.SelectedProvider, CharacterModelProvider.OpenAi)
            || withOpenAi.Settings.Count != 2
            || !ReferenceEquals(withOpenAi.Settings[0], activatedSelection!.Settings[0])
            || !ReferenceEquals(withOpenAi.Settings[1].Provider, CharacterModelProvider.OpenAi)
            || withOpenAi.Settings[1].ModelName != "gpt-test"
            || File.Exists(StoragePath()))
        {
            Fail("Provider-settings panel did not append and select an unsaved setting.");
            return;
        }

        provider.Select(3);
        provider.EmitSignal(OptionButton.SignalName.ItemSelected, 3);
        remove.EmitSignal(BaseButton.SignalName.Pressed);
        CharacterModelSettingsSelectionSnapshot? removed = controller.Snapshot;
        if (removed is null
            || !ReferenceEquals(removed.SelectedProvider, CharacterModelProvider.OpenAi)
            || removed.Settings.Count != 1
            || !ReferenceEquals(removed.Settings[0], withOpenAi.Settings[1])
            || modelName.Text.Length != 0
            || File.Exists(StoragePath())
            || status.Text != TemporaryCharacterModelSettingsPanel.RemovedStatusText)
        {
            Fail("Provider-settings panel did not remove one configured unselected provider.");
            return;
        }

        save.EmitSignal(BaseButton.SignalName.Pressed);
        CharacterModelSettingsDocumentLoadResult persisted = storage.LoadCurrent();
        if (!ReferenceEquals(controller.Snapshot, removed)
            || status.Text != TemporaryCharacterModelSettingsPanel.SavedStatusText
            || persisted.Status != CharacterModelSettingsDocumentLoadStatus.Loaded
            || persisted.Selection is not { } loaded
            || !ReferenceEquals(loaded.SelectedProvider, CharacterModelProvider.OpenAi)
            || loaded.Settings.Count != 1
            || !ReferenceEquals(loaded.Settings[0].Provider, CharacterModelProvider.OpenAi)
            || loaded.Settings[0].ModelName != "gpt-test")
        {
            Fail("Provider-settings panel did not explicitly persist its exact current setting.");
            return;
        }

        GD.Print(
            "Character-model settings panel smoke passed: empty-state presentation, canonical "
            + "provider choices, valid unsaved apply, invalid-input preservation, explicit "
            + "future-turn activation, unselected removal, and explicit save were correct.");
        GetTree().Quit();
    }

    private static string StoragePath() =>
        CharacterModelSettingsStorageController.CurrentFilePath();

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
