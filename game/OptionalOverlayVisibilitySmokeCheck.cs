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
        LineEdit dialogueInput = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");

        if (dialogueOverlay.Visible || settingsOverlay.Visible || !commandOverlay.Visible)
        {
            Fail(
                "Optional overlays did not start hidden while the compact HUD remained visible. "
                + $"Dialogue={dialogueOverlay.Visible}; settings={settingsOverlay.Visible}; "
                + $"HUD={commandOverlay.Visible}.");
            return;
        }

        if (!HasOnlyPhysicalKey(ToggleAction, Key.Tab))
        {
            Godot.Collections.Array<InputEvent> events =
                InputMap.ActionGetEvents(new StringName(ToggleAction));
            Key actualKey = events.Count == 1 && events[0] is InputEventKey key
                ? key.PhysicalKeycode
                : Key.None;
            Fail(
                "Optional overlays did not use only physical Tab. "
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
        dialogueInput.GrabFocus();
        if (!dialogueOverlay.Visible
            || !settingsOverlay.Visible
            || !commandOverlay.Visible
            || !dialogueInput.HasFocus())
        {
            Fail("Physical Tab did not reveal both optional overlays above the retained HUD.");
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
            + "physical Tab alone toggled both, the compact HUD remained visible, hidden UI "
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
        return methods.SequenceEqual(["_Ready", "_UnhandledInput"])
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

    private static bool HasOnlyPhysicalKey(string actionName, Key expectedKey)
    {
        StringName action = new(actionName);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey keyEvent
            && keyEvent.PhysicalKeycode == expectedKey;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
