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
    private AudioSettingsPanel audioPanel = null!;
    private Button audioEntry = null!;
    private bool closingAll;

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogueOverlay = GetNode<CanvasLayer>("../DialogueOverlay");
        settingsOverlay = GetNode<CanvasLayer>("../SettingsOverlay");
        audioPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        audioEntry = GetNode<Button>("../SettingsOverlay/AudioEntry");
        audioEntry.Pressed += OnAudioPressed;
        audioPanel.Closed += OnAudioClosed;
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

        SetVisible(!(dialogueOverlay.Visible || audioPanel.Visible));
        GetViewport().SetInputAsHandled();
    }

    private void SetVisible(bool visible)
    {
        closingAll = true;
        if (audioPanel.Visible)
        {
            audioPanel.Close(restoreFocus: false);
        }
        closingAll = false;
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

    private void OnAudioPressed()
    {
        dialogueOverlay.Visible = false;
        settingsOverlay.Visible = false;
        audioPanel.Open(audioEntry);
    }

    private void OnAudioClosed(bool restoreFocus)
    {
        if (!closingAll)
        {
            dialogueOverlay.Visible = restoreFocus;
            settingsOverlay.Visible = restoreFocus;
        }
    }
}
