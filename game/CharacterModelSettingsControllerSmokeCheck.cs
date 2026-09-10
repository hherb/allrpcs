using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves scene-owned provider-settings updates remain immutable and isolated.</summary>
public partial class CharacterModelSettingsControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_SETTINGS_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (isEnabled)
        {
            DeleteStoragePath();
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        try
        {
            RunCheck();
        }
        finally
        {
            DeleteStoragePath();
        }
    }

    private void RunCheck()
    {
        CharacterModelSettingsController controller = GetNode<CharacterModelSettingsController>(
            "../CharacterModelSettingsController");
        CharacterModelSettingsStorageController storageController =
            GetNode<CharacterModelSettingsStorageController>(
                "../CharacterModelSettingsStorageController");
        CharacterModelSettings openAi = new(CharacterModelProvider.OpenAi, "gpt-before");
        CharacterModelSettings anthropic = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettings replacement = new(CharacterModelProvider.OpenAi, "gpt-after");
        CharacterModelSettings ollama = new(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettings selectedOllama = new(
            CharacterModelProvider.Ollama,
            "granite-selected");
        CharacterModelSettingsSelectionSnapshot initial = new(
            [openAi, anthropic],
            CharacterModelProvider.OpenAi);

        if (controller.Snapshot is not null
            || controller.SaveCurrent() != CharacterModelSettingsDocumentWriteResult.Failed
            || controller.TryApply(replacement)
            || controller.TryUpdate(replacement)
            || controller.TrySelect(CharacterModelProvider.OpenAi)
            || controller.TryRemove(CharacterModelProvider.Anthropic)
            || !controller.TryInitialize(initial)
            || !ReferenceEquals(controller.Snapshot, initial)
            || controller.TryInitialize(initial))
        {
            Fail("Provider-settings controller did not initialize exactly once.");
            return;
        }

        if (!controller.TryUpdate(replacement)
            || controller.Snapshot is not { } replaced
            || ReferenceEquals(replaced, initial)
            || !ReferenceEquals(replaced.SelectedProvider, CharacterModelProvider.OpenAi)
            || replaced.Settings.Count != 2
            || !ReferenceEquals(replaced.Settings[0], replacement)
            || !ReferenceEquals(replaced.Settings[1], anthropic)
            || !controller.TryUpdate(ollama)
            || controller.Snapshot is not { } appended
            || appended.Settings.Count != 3
            || !ReferenceEquals(appended.Settings[0], replacement)
            || !ReferenceEquals(appended.Settings[1], anthropic)
            || !ReferenceEquals(appended.Settings[2], ollama))
        {
            Fail("Provider-settings controller did not retain exact immutable updates.");
            return;
        }

        if (!controller.TrySelect(CharacterModelProvider.Anthropic)
            || controller.Snapshot is not { } selected
            || !ReferenceEquals(selected.SelectedProvider, CharacterModelProvider.Anthropic)
            || !controller.TryRemove(CharacterModelProvider.OpenAi)
            || controller.Snapshot is not { } removed
            || removed.Settings.Count != 2
            || !ReferenceEquals(removed.Settings[0], anthropic)
            || !ReferenceEquals(removed.Settings[1], ollama)
            || controller.TryRemove(CharacterModelProvider.Anthropic)
            || controller.TryRemove(CharacterModelProvider.OpenAi)
            || !ReferenceEquals(controller.Snapshot, removed))
        {
            Fail("Provider-settings controller did not preserve valid selection and removal state.");
            return;
        }

        try
        {
            controller.TryUpdate(null!);
            Fail("Provider-settings controller accepted null settings.");
            return;
        }
        catch (ArgumentNullException)
        {
        }

        try
        {
            controller.TrySelect(null!);
            Fail("Provider-settings controller accepted a null selection.");
            return;
        }
        catch (ArgumentNullException)
        {
        }

        try
        {
            controller.TryRemove(null!);
            Fail("Provider-settings controller accepted a null removal.");
            return;
        }
        catch (ArgumentNullException)
        {
        }

        try
        {
            controller.TryApply(null!);
            Fail("Provider-settings controller accepted a null atomic apply.");
            return;
        }
        catch (ArgumentNullException)
        {
        }

        if (!controller.TryApply(selectedOllama)
            || controller.Snapshot is not { } applied
            || ReferenceEquals(applied, removed)
            || !ReferenceEquals(applied.SelectedProvider, CharacterModelProvider.Ollama)
            || applied.Settings.Count != 2
            || !ReferenceEquals(applied.Settings[0], anthropic)
            || !ReferenceEquals(applied.Settings[1], selectedOllama)
            || !ReferenceEquals(removed.SelectedProvider, CharacterModelProvider.Anthropic)
            || !ReferenceEquals(removed.Settings[1], ollama)
            || File.Exists(StoragePath())
            || !controller.TrySelect(CharacterModelProvider.Anthropic)
            || controller.Snapshot is not { } saveable
            || !ReferenceEquals(saveable.Settings[1], selectedOllama)
            || File.Exists(StoragePath())
            || controller.SaveCurrent() != CharacterModelSettingsDocumentWriteResult.Saved)
        {
            Fail("Provider-settings controller did not atomically apply without automatic save.");
            return;
        }

        CharacterModelSettingsDocumentLoadResult persisted = storageController.LoadCurrent();
        if (persisted.Status != CharacterModelSettingsDocumentLoadStatus.Loaded
            || persisted.Selection is not { } saved
            || !ReferenceEquals(saved.SelectedProvider, CharacterModelProvider.Anthropic)
            || saved.Settings.Count != 2
            || !ReferenceEquals(saved.Settings[0].Provider, CharacterModelProvider.Anthropic)
            || saved.Settings[0].ModelName != "claude-test"
            || !ReferenceEquals(saved.Settings[1].Provider, CharacterModelProvider.Ollama)
            || saved.Settings[1].ModelName != "granite-selected")
        {
            Fail("Provider-settings controller did not save its exact current snapshot.");
            return;
        }

        File.Delete(StoragePath());
        Directory.CreateDirectory(StoragePath());
        if (controller.SaveCurrent() != CharacterModelSettingsDocumentWriteResult.Failed
            || !ReferenceEquals(controller.Snapshot, saveable))
        {
            Fail("Provider-settings controller did not preserve state after a failed save.");
            return;
        }

        GD.Print(
            "Character-model settings controller smoke passed: exact one-time initialization, "
            + "immutable replacement, append, selection, removal, and atomic apply, fail-closed "
            + "rejected state, explicit exact-current persistence, failed-save preservation, and "
            + "null rejection were correct.");
        GetTree().Quit();
    }

    private static string StoragePath() =>
        CharacterModelSettingsStorageController.CurrentFilePath();

    private static void DeleteStoragePath()
    {
        string path = StoragePath();
        if (Directory.Exists(path))
        {
            Directory.Delete(path);
            return;
        }

        File.Delete(path);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
