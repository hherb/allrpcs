using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Keeps the temporary settings and dialogue overlays hidden until the player requests them.
/// </summary>
public partial class OptionalOverlayVisibilityController : Node
{
    private const string ToggleAction = "optional_overlays_toggle";

    private CanvasLayer dialogueOverlay = null!;
    private CanvasLayer settingsOverlay = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogueOverlay = GetNode<CanvasLayer>("../DialogueOverlay");
        settingsOverlay = GetNode<CanvasLayer>("../SettingsOverlay");
        SetVisible(visible: false);
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        if (!@event.IsActionPressed(ToggleAction))
        {
            return;
        }

        SetVisible(!dialogueOverlay.Visible);
        GetViewport().SetInputAsHandled();
    }

    private void SetVisible(bool visible)
    {
        if (!visible
            && GetViewport().GuiGetFocusOwner() is Control focusOwner
            && (dialogueOverlay.IsAncestorOf(focusOwner)
                || settingsOverlay.IsAncestorOf(focusOwner)))
        {
            focusOwner.ReleaseFocus();
        }

        dialogueOverlay.Visible = visible;
        settingsOverlay.Visible = visible;
    }
}
