using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Checks that optional settings and dialogue overlays stay out of the playfield until requested,
/// that Talk (Enter) opens dialogue straight from the world, and that the command panel fits only
/// the lines that report something.
/// </summary>
public partial class OptionalOverlayVisibilitySmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPTIONAL_OVERLAY_VISIBILITY_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ToggleAction = "optional_overlays_toggle";
    private const string TalkAction = "dialogue_open";

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        OptionalOverlayVisibilityController controller =
            GetNode<OptionalOverlayVisibilityController>("../OptionalOverlayVisibilityController");
        CanvasLayer commandOverlay = GetNode<CanvasLayer>("../CommandOverlay");
        CanvasLayer dialogueOverlay = GetNode<CanvasLayer>("../DialogueOverlay");
        CanvasLayer settingsOverlay = GetNode<CanvasLayer>("../SettingsOverlay");

        if (dialogueOverlay.Visible || settingsOverlay.Visible || !commandOverlay.Visible)
        {
            Fail(
                "Optional overlays did not start hidden while the compact HUD remained visible. "
                + $"Dialogue={dialogueOverlay.Visible}; settings={settingsOverlay.Visible}; "
                + $"HUD={commandOverlay.Visible}.");
            return;
        }

        if (!HasExactKeyboardControllerDefaults(ToggleAction, Key.Tab))
        {
            Godot.Collections.Array<InputEvent> events =
                InputMap.ActionGetEvents(new StringName(ToggleAction));
            Key actualKey = events.Count == 1 && events[0] is InputEventKey key
                ? key.PhysicalKeycode
                : Key.None;
            Fail(
                "Optional overlays did not use the exact physical Tab/controller Start defaults. "
                + $"Expected={(long)Key.Tab}; actual={(long)actualKey}.");
            return;
        }

        if (!HasClosedSurface())
        {
            Fail("Optional overlay visibility controller exposed an unexpected public surface.");
            return;
        }

        controller._UnhandledInput(Action("optional_overlays_unrelated", pressed: true));
        controller._UnhandledInput(Action(ToggleAction, pressed: false));
        if (dialogueOverlay.Visible || settingsOverlay.Visible)
        {
            Fail("Released or unrelated input changed optional-overlay visibility.");
            return;
        }

        controller._UnhandledInput(Action(ToggleAction, pressed: true));
        GetNode<Button>("../SettingsOverlay/ControlsEntry").GrabFocus();
        if (dialogueOverlay.Visible
            || !settingsOverlay.Visible
            || !commandOverlay.Visible
            || !GetNode<Button>("../SettingsOverlay/ControlsEntry").HasFocus())
        {
            Fail("Toggle did not reveal settings with Controls entry focus above the retained HUD.");
            return;
        }

        controller._UnhandledInput(Action(ToggleAction, pressed: true));
        if (dialogueOverlay.Visible
            || settingsOverlay.Visible
            || !commandOverlay.Visible
            || GetViewport().GuiGetFocusOwner() is not null)
        {
            Fail("Physical Tab did not hide optional overlays and release captured UI focus.");
            return;
        }

        if (!await TalkOpensDialogueFromTheWorld(controller, dialogueOverlay, settingsOverlay)
            || !await CommandPanelFitsShownLines())
        {
            return;
        }

        GD.Print(
            "Optional overlay visibility smoke passed: settings and dialogue started hidden, "
            + "the retained toggle action toggled settings, the compact HUD remained visible, hidden UI "
            + "released focus, Talk opened dialogue from the world and returned to it, the command "
            + "panel fit its shown lines, and the controller surface stayed closed.");
        GetTree().Quit();
    }

    private async Task<bool> TalkOpensDialogueFromTheWorld(
        OptionalOverlayVisibilityController controller, CanvasLayer dialogueOverlay, CanvasLayer settingsOverlay)
    {
        Godot.Collections.Array<InputEvent> talk = InputMap.ActionGetEvents(new StringName(TalkAction));
        if (talk.Count != 1 || talk[0] is not InputEventKey { PhysicalKeycode: Key.Enter })
        {
            Fail("Talk did not default to exactly physical Enter.");
            return false;
        }

        controller._UnhandledInput(Action(TalkAction, pressed: true));
        if (!dialogueOverlay.Visible || settingsOverlay.Visible
            || !GetNode<LineEdit>("../DialogueOverlay/Panel/Input").HasFocus())
        {
            Fail("Talk did not open dialogue directly with its input focused.");
            return false;
        }

        GetNode<Button>("../DialogueOverlay/Panel/Close").EmitSignal(BaseButton.SignalName.Pressed);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (dialogueOverlay.Visible || settingsOverlay.Visible || GetViewport().GuiGetFocusOwner() is not null)
        {
            Fail("Closing dialogue opened by Talk did not return to the world.");
            return false;
        }

        controller._UnhandledInput(Action(ToggleAction, pressed: true));
        controller._UnhandledInput(Action(TalkAction, pressed: true));
        bool ignored = settingsOverlay.Visible && !dialogueOverlay.Visible;
        controller._UnhandledInput(Action(ToggleAction, pressed: true));
        if (!ignored)
        {
            Fail("Talk replaced an open settings overlay.");
            return false;
        }

        return true;
    }

    private async Task<bool> CommandPanelFitsShownLines()
    {
        if (CommandPanelLayout.IsShown(string.Empty)
            || CommandPanelLayout.IsShown(InteractionFocusController.NoFocusText)
            || CommandPanelLayout.IsShown(Allrpcs.Application.Input.FocusedSearchInputCoordinator.InitialResultText)
            || !CommandPanelLayout.IsShown("Interaction focus: herb patch"))
        {
            Fail("Command panel did not hide exactly the empty and idle status lines.");
            return false;
        }

        // Whatever the live lines report, hidden lines are the idle ones and shown lines stack
        // inside a panel that ends just after the last of them.
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Control panel = GetNode<Control>("../CommandOverlay/Panel");
        Label[] lines = [.. panel.GetChildren().OfType<Label>()];
        Label[] shown = [.. lines.Where(line => line.Visible)];
        bool fits = lines.All(line => line.Visible == CommandPanelLayout.IsShown(line.Text))
            && shown.Length > 0
            && shown.All(line => panel.GetGlobalRect().Encloses(line.GetGlobalRect()))
            && shown.Zip(shown.Skip(1)).All(pair => pair.First.GetGlobalRect().End.Y <= pair.Second.GetGlobalRect().Position.Y)
            && panel.GetGlobalRect().End.Y - shown[^1].GetGlobalRect().End.Y <= 8;
        if (!fits)
        {
            Fail($"Command panel did not fit its shown lines: height={panel.Size.Y}; lines="
                + string.Join(", ", lines.Select(line => $"{line.Name}={line.Visible}/{line.Size.Y}:'{line.Text}'")));
            return false;
        }

        return true;
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(OptionalOverlayVisibilityController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(["_ExitTree", "_Input", "_Ready", "_UnhandledInput"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static InputEventAction Action(string action, bool pressed) => new()
    {
        Action = new StringName(action),
        Pressed = pressed,
    };

    private static bool HasExactKeyboardControllerDefaults(string actionName, Key expectedKey)
    {
        StringName action = new(actionName);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 2
            && events.OfType<InputEventKey>().Count(key => key.PhysicalKeycode == expectedKey) == 1
            && events.OfType<InputEventJoypadButton>().Count(button => button.ButtonIndex == JoyButton.Start) == 1;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
