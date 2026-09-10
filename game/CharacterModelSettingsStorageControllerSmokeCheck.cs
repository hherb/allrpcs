using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free local provider-settings persistence checks when enabled.</summary>
public partial class CharacterModelSettingsStorageControllerSmokeCheck : Node
{
    private const string EnabledEnvironmentValue = "1";

    private CharacterModelSettingsStorageController controller = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(
            CharacterModelSettingsStorageController.SmokeCheckEnvironmentVariable)
            == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<CharacterModelSettingsStorageController>(
            "../CharacterModelSettingsStorageController");
        File.Delete(StoragePath());
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
        CharacterModelSettings openAi = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings ollama = new(CharacterModelProvider.Ollama, "gemma3");
        CharacterModelSettingsSelectionSnapshot initial = new(
            [openAi, ollama],
            CharacterModelProvider.OpenAi);
        CharacterModelSettingsSelectionSnapshot replacement = new(
            [
                new CharacterModelSettings(CharacterModelProvider.OpenAi, "gpt-replacement"),
                new CharacterModelSettings(CharacterModelProvider.Ollama, "llama3.2"),
            ],
            CharacterModelProvider.Ollama);

        CharacterModelSettingsDocumentLoadResult missing = controller.LoadCurrent();
        if (missing.Status != CharacterModelSettingsDocumentLoadStatus.Missing
            || missing.Selection is not null
            || controller.SaveCurrent(initial)
                != CharacterModelSettingsDocumentWriteResult.Saved)
        {
            Fail("Provider-settings storage smoke failed missing or initial persistence.");
            return;
        }

        string saved = File.ReadAllText(StoragePath());
        CharacterModelSettingsDocumentLoadResult loaded = controller.LoadCurrent();
        if (saved.Contains("credential", StringComparison.OrdinalIgnoreCase)
            || loaded.Status != CharacterModelSettingsDocumentLoadStatus.Loaded
            || !Matches(loaded.Selection, CharacterModelProvider.OpenAi, "gpt-test", "gemma3"))
        {
            Fail("Provider-settings storage smoke failed secret-free exact loading.");
            return;
        }

        File.WriteAllText(StoragePath(), "{");
        CharacterModelSettingsDocumentLoadResult invalid = controller.LoadCurrent();
        if (invalid.Status != CharacterModelSettingsDocumentLoadStatus.InvalidDocument
            || invalid.Selection is not null)
        {
            Fail("Provider-settings storage smoke failed malformed-document rejection.");
            return;
        }

        File.WriteAllBytes(StoragePath(), [0xc3, 0x28]);
        CharacterModelSettingsDocumentLoadResult failed = controller.LoadCurrent();
        if (failed.Status != CharacterModelSettingsDocumentLoadStatus.StorageFailed
            || failed.Selection is not null
            || controller.SaveCurrent(replacement)
                != CharacterModelSettingsDocumentWriteResult.Saved)
        {
            Fail("Provider-settings storage smoke failed read failure or replacement recovery.");
            return;
        }

        CharacterModelSettingsDocumentLoadResult replaced = controller.LoadCurrent();
        if (replaced.Status != CharacterModelSettingsDocumentLoadStatus.Loaded
            || !Matches(
                replaced.Selection,
                CharacterModelProvider.Ollama,
                "gpt-replacement",
                "llama3.2"))
        {
            Fail("Provider-settings storage smoke failed exact replacement loading.");
            return;
        }

        File.Delete(StoragePath());
        CharacterModelSettingsDocumentLoadResult deleted = controller.LoadCurrent();
        if (deleted.Status != CharacterModelSettingsDocumentLoadStatus.Missing
            || deleted.Selection is not null)
        {
            Fail("Provider-settings storage smoke failed final missing outcome.");
            return;
        }

        GD.Print(
            "Character-model settings storage controller smoke passed: isolated local "
            + "persistence, secret-free exact loading, malformed and invalid UTF-8 outcomes, "
            + "replacement recovery, and missing-document handling were correct.");
        GetTree().Quit();
    }

    private static bool Matches(
        CharacterModelSettingsSelectionSnapshot? selection,
        CharacterModelProvider selectedProvider,
        string openAiModelName,
        string ollamaModelName) =>
        selection is not null
        && ReferenceEquals(selection.SelectedProvider, selectedProvider)
        && selection.Settings.Count == 2
        && ReferenceEquals(selection.Settings[0].Provider, CharacterModelProvider.OpenAi)
        && selection.Settings[0].ModelName == openAiModelName
        && ReferenceEquals(selection.Settings[1].Provider, CharacterModelProvider.Ollama)
        && selection.Settings[1].ModelName == ollamaModelName;

    private static string StoragePath() =>
        CharacterModelSettingsStorageController.CurrentFilePath();

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
