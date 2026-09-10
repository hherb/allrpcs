using Allrpcs.Application.Dialogue;
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

        CharacterModelSettingsController controller =
            GetNode<CharacterModelSettingsController>("../CharacterModelSettingsController");
        CharacterModelSettings openAi = new(CharacterModelProvider.OpenAi, "gpt-before");
        CharacterModelSettings anthropic = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettings replacement = new(CharacterModelProvider.OpenAi, "gpt-after");
        CharacterModelSettings ollama = new(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettingsSelectionSnapshot initial = new(
            [openAi, anthropic],
            CharacterModelProvider.OpenAi);

        if (controller.Snapshot is not null
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

        if (!ReferenceEquals(controller.Snapshot, removed))
        {
            Fail("Rejected provider-settings input changed the retained snapshot.");
            return;
        }

        GD.Print(
            "Character-model settings controller smoke passed: exact one-time initialization, "
            + "immutable replacement, append, selection, and removal, fail-closed rejected state, "
            + "and null rejection were correct.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
