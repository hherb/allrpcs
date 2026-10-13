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
    private ControlSettingsPanel? controlsPanel;
    private Button? controlsEntry;
    private ControlSettingsController? controlsController;

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogueOverlay = GetNode<CanvasLayer>("../DialogueOverlay");
        settingsOverlay = GetNode<CanvasLayer>("../SettingsOverlay");
        audioPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        audioEntry = GetNode<Button>("../SettingsOverlay/AudioEntry");
        audioEntry.Pressed += OnAudioPressed;
        audioPanel.Closed += OnAudioClosed;
        controlsPanel = GetNodeOrNull<ControlSettingsPanel>("../ControlSettingsPanel");
        controlsEntry = GetNodeOrNull<Button>("../SettingsOverlay/ControlsEntry");
        controlsController = GetNodeOrNull<ControlSettingsController>("../ControlSettingsController");
        if (controlsPanel is not null && controlsEntry is not null)
        { controlsEntry.Pressed += OnControlsPressed; controlsPanel.Closed += OnControlsClosed; }
        if (controlsController is not null) { controlsController.Changed += RefreshControlsEntry; }
        RefreshControlsEntry();
        SetVisible(visible: false);
    }

    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        // A remapped primary button never steals native GUI pointer activation.
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left }
            && GetViewport().GuiGetHoveredControl() is Control hovered && hovered.IsVisibleInTree()) { return; }
        if (ControlInputContext.TogglesAllowed(this) && ControlInputMatcher.Pressed(@event, ToggleAction))
        { _UnhandledInput(@event); }
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.TogglesAllowed(this)) { return; }
        ArgumentNullException.ThrowIfNull(@event);
        if (!ControlInputMatcher.Pressed(@event, ToggleAction))
        {
            return;
        }

        SetVisible(!(dialogueOverlay.Visible || audioPanel.Visible || controlsPanel?.Visible == true));
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
        if (visible && controlsEntry is not null) { controlsEntry.GrabFocus(); }
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
    private void OnControlsPressed()
    {
        dialogueOverlay.Visible = false; settingsOverlay.Visible = false;
        controlsPanel?.Open(controlsEntry);
    }
    private void OnControlsClosed(bool restoreFocus)
    {
        if (!closingAll) { dialogueOverlay.Visible = restoreFocus; settingsOverlay.Visible = restoreFocus; }
        RefreshControlsEntry();
    }
    private void RefreshControlsEntry()
    {
        if (controlsEntry is null) { return; }
        bool unsaved = controlsController?.Unsaved == true;
        controlsEntry.Text = unsaved ? "Controls settings · unsaved" : "Controls settings";
        controlsEntry.TooltipText = unsaved ? "Applied controls will reset on restart until saved." : "Remap keyboard, mouse and controller inputs.";
    }
    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (controlsController is not null) { controlsController.Changed -= RefreshControlsEntry; }
        if (controlsPanel is not null) { controlsPanel.Closed -= OnControlsClosed; }
        if (controlsEntry is not null) { controlsEntry.Pressed -= OnControlsPressed; }
    }

}
