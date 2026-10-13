using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Checks that optional settings and dialogue overlays stay out of the playfield until requested.
/// </summary>
public partial class OptionalOverlayVisibilitySmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPTIONAL_OVERLAY_VISIBILITY_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ToggleAction = "optional_overlays_toggle";

    /// <inheritdoc />
    public override void _Ready()
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

        GD.Print(
            "Optional overlay visibility smoke passed: settings and dialogue started hidden, "
            + "the retained toggle action toggled settings, the compact HUD remained visible, hidden UI "
            + "released focus, and the controller surface stayed closed.");
        GetTree().Quit();
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
