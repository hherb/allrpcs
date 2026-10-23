using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Keeps the temporary settings and dialogue overlays hidden until the player requests them.
/// </summary>
public partial class OptionalOverlayVisibilityController : Node
{
    private const string ToggleAction = "optional_overlays_toggle";
    private const string TalkAction = "dialogue_open";

    private CanvasLayer dialogueOverlay = null!;
    private CanvasLayer settingsOverlay = null!;
    private AudioSettingsPanel audioPanel = null!;
    private Button audioEntry = null!;
    private bool closingAll;
    // Dialogue opened straight from the world returns to the world, not to the settings entries.
    private bool openedDirectly;
    private TemporaryDialogueInputPanel dialoguePanel = null!;
    private Button dialogueEntry = null!;
    private Button settingsClose = null!;
    private Control[] entryCycle = [];
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
        dialoguePanel = GetNode<TemporaryDialogueInputPanel>("../DialogueOverlay/Panel");
        dialogueEntry = GetNode<Button>("../SettingsOverlay/DialogueEntry");
        settingsClose = GetNode<Button>("../SettingsOverlay/SettingsClose");
        dialogueEntry.Pressed += OnDialoguePressed;
        settingsClose.Pressed += OnSettingsClosePressed;
        dialoguePanel.Closed += OnDialogueClosed;
        entryCycle = controlsEntry is null ? [audioEntry, dialogueEntry, settingsClose]
            : [controlsEntry, audioEntry, dialogueEntry, settingsClose];
        foreach (Control entry in entryCycle)
        {
            // Entry surfaces remain readable over the independent caption layer.
            foreach (string state in new[] { "normal", "hover", "pressed" })
            {
                entry.AddThemeStyleboxOverride(state, new StyleBoxFlat
                {
                    BgColor = state == "hover" ? new Color(0.09f, 0.11f, 0.14f) : new Color(0.055f, 0.067f, 0.082f),
                    ContentMarginLeft = 12,
                    ContentMarginRight = 12,
                    ContentMarginTop = 6,
                    ContentMarginBottom = 6,
                    CornerRadiusTopLeft = 3,
                    CornerRadiusTopRight = 3,
                    CornerRadiusBottomLeft = 3,
                    CornerRadiusBottomRight = 3
                });
            }
        }
        for (int i = 0; i < entryCycle.Length; i++)
        {
            entryCycle[i].FocusNext = entryCycle[i].GetPathTo(entryCycle[(i + 1) % entryCycle.Length]);
            entryCycle[i].FocusPrevious = entryCycle[i].GetPathTo(entryCycle[(i + entryCycle.Length - 1) % entryCycle.Length]);
        }
        dialogueEntry.AccessibilityName = "Dialogue";
        dialogueEntry.AccessibilityDescription = "Open dialogue as Seeker.";
        settingsClose.AccessibilityName = "Close settings";
        settingsClose.AccessibilityDescription = "Return to the world.";
        RefreshControlsEntry();
        SetVisible(visible: false);
    }

    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        // A remapped primary button never steals native GUI pointer activation.
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left }
            && GetViewport().GuiGetHoveredControl() is Control hovered && hovered.IsVisibleInTree()) { return; }
        Control? focus = GetViewport().GuiGetFocusOwner();
        if (settingsOverlay.Visible && focus is not null && entryCycle.Contains(focus))
        {
            if (ControlInputMatcher.Pressed(@event, "ui_cancel"))
            { SetVisible(false); GetViewport().SetInputAsHandled(); return; }
            bool next = ControlInputMatcher.Pressed(@event, "ui_focus_next");
            bool previous = ControlInputMatcher.Pressed(@event, "ui_focus_prev");
            if (next || previous)
            {
                int index = Array.IndexOf(entryCycle, focus);
                entryCycle[(index + (previous ? entryCycle.Length - 1 : 1)) % entryCycle.Length].GrabFocus();
                GetViewport().SetInputAsHandled(); return;
            }
        }
        // Contained focus traversal takes priority over a coincident overlay toggle binding.
        if ((settingsOverlay.Visible || dialogueOverlay.Visible) && focus is not null
            && (ControlInputMatcher.Pressed(@event, "ui_focus_next")
                || ControlInputMatcher.Pressed(@event, "ui_focus_prev"))) { return; }
        if (ControlInputContext.TogglesAllowed(this) && ControlInputMatcher.Pressed(@event, ToggleAction))
        { _UnhandledInput(@event); }
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.TogglesAllowed(this)) { return; }
        ArgumentNullException.ThrowIfNull(@event);
        // Unhandled only: a focused button or text field keeps its own Enter.
        if (ControlInputMatcher.Pressed(@event, TalkAction) && !AnyOverlayVisible())
        {
            openedDirectly = true;
            OnDialoguePressed();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (!ControlInputMatcher.Pressed(@event, ToggleAction))
        {
            return;
        }

        SetVisible(!AnyOverlayVisible());
        GetViewport().SetInputAsHandled();
    }

    private bool AnyOverlayVisible() =>
        settingsOverlay.Visible || dialogueOverlay.Visible || audioPanel.Visible || controlsPanel?.Visible == true;

    private void SetVisible(bool visible)
    {
        openedDirectly = false;
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

        dialogueOverlay.Visible = false;
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
            dialogueOverlay.Visible = false;
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
        if (!closingAll) { dialogueOverlay.Visible = false; settingsOverlay.Visible = restoreFocus; }
        RefreshControlsEntry();
    }
    private void OnDialoguePressed()
    {
        settingsOverlay.Visible = false;
        dialogueOverlay.Visible = true;
        dialoguePanel.FocusInput();
    }
    private void OnDialogueClosed()
    {
        if (openedDirectly) { SetVisible(false); return; }
        dialogueOverlay.Visible = false;
        settingsOverlay.Visible = true;
        dialogueEntry.GrabFocus();
    }
    private void OnSettingsClosePressed() => SetVisible(false);
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
        dialogueEntry.Pressed -= OnDialoguePressed;
        settingsClose.Pressed -= OnSettingsClosePressed;
        dialoguePanel.Closed -= OnDialogueClosed;
        audioEntry.Pressed -= OnAudioPressed;
        audioPanel.Closed -= OnAudioClosed;
        if (controlsController is not null) { controlsController.Changed -= RefreshControlsEntry; }
        if (controlsPanel is not null) { controlsPanel.Closed -= OnControlsClosed; }
        if (controlsEntry is not null) { controlsEntry.Pressed -= OnControlsPressed; }
    }

}
