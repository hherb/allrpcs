using Allrpcs.Application.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>Accessible draft editor with finite device capture and explicit apply/save/reset.</summary>
public partial class ControlSettingsPanel : CanvasLayer
{
    private ControlSettingsController controller = null!;
    private ControlPreferences draft = ControlPreferences.Defaults();
    private readonly ControlBindingCapture capture = new();
    private readonly List<Control> focusOrder = [];
    private ItemList actions = null!;
    private ItemList bindings = null!;
    private OptionButton device = null!;
    private HSlider deadzone = null!;
    private Label details = null!, status = null!, zoneValue = null!;
    private Button add = null!, replace = null!, remove = null!, rowDefault = null!, cancelCapture = null!;
    private Button restoreDefaults = null!, close = null!;
    private HBoxContainer confirmation = null!;
    private Button discard = null!, keep = null!;
    private Control? origin;
    private bool refreshing;
    private string actionId = "move_left";
    private int? replacing;
    private bool awaitingArm;
    /// <summary>Restores the optional-overlay entry after this view closes.</summary>
    public event Action<bool>? Closed;
    /// <summary>Complete draft, exposed internally to the bounded runtime fixture.</summary>
    internal ControlPreferences Draft => draft;
    internal ControlBindingCapture Capture => capture;
    internal bool HasUnapplied => !ControlSettingsController.Same(draft, controller.Preferences);
    /// <inheritdoc />
    public override void _Ready()
    {
        controller = GetNode<ControlSettingsController>("../ControlSettingsController");
        Build(); controller.Changed += RefreshStatus; Input.JoyConnectionChanged += ConnectionChanged;
    }
    /// <summary>Opens a complete active snapshot with predictable first focus.</summary>
    public void Open(Control? entry = null)
    {
        origin = entry; draft = controller.Preferences; Visible = true; confirmation.Visible = false;
        cancelCapture.Visible = false; Refresh(); actions.GrabFocus(); ControlInputContext.Current?.Fence();
    }
    /// <summary>Offers explicit discard when unapplied edits exist.</summary>
    public void Close(bool restoreFocus = true)
    {
        CancelCapture();
        if (HasUnapplied && !confirmation.Visible)
        {
            confirmation.Visible = true;
            status.Text = "Unapplied draft · Discard to close (Escape again), or Keep editing.";
            WireFocus(); keep.GrabFocus(); return;
        }
        FinishClose(restoreFocus);
    }
    private void FinishClose(bool restoreFocus)
    {
        CancelCapture(); draft = controller.Preferences; confirmation.Visible = false;
        GetViewport().GuiGetFocusOwner()?.ReleaseFocus(); Visible = false;
        ControlInputContext.Current?.Fence(); Closed?.Invoke(restoreFocus);
        if (restoreFocus && origin is not null && GodotObject.IsInstanceValid(origin) && origin.IsVisibleInTree()) { origin.GrabFocus(); }
    }
    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        InputEvent value = @event;
        if (!Visible) { return; }
        ControlInputContext.Current?.Observe(value);
        bool escape = value is InputEventKey key && key.Pressed && !key.Echo
            && (key.Keycode == Key.Escape || key.PhysicalKeycode == Key.Escape);
        if (escape)
        {
            if (capture.Active) { CancelCapture(); add.GrabFocus(); } else { Close(); }
            GetViewport().SetInputAsHandled(); return;
        }
        if (capture.Active)
        {
            if (value is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left
                && HandleCapturePointer(mouse.Position)) { }
            else if (value is InputEventJoypadMotion axis)
            { AcceptCandidate(capture.ObserveAxis((int)axis.Axis, axis.AxisValue, axis.Device)); }
            else if (ControlInputMapAdapter.FromEvent(value) is { } binding)
            { AcceptCandidate(capture.Observe(binding, value.IsPressed(), value is InputEventKey echoed && echoed.Echo, value.Device)); }
            else if (value.IsPressed()) { status.Text = "Unsupported input · Waiting for selected device class"; }
            GetViewport().SetInputAsHandled(); return;
        }
        // Primary pointer hit testing keeps visible controls usable even when Left is UI cancel.
        if (value is InputEventMouseButton { ButtonIndex: MouseButton.Left } pointer
            && focusOrder.Any(control => control.IsVisibleInTree() && control.GetGlobalRect().HasPoint(pointer.Position))) { return; }
        if (ControlInputMatcher.Pressed(value, "ui_cancel")) { Close(); GetViewport().SetInputAsHandled(); return; }
        // Tab is both the default UI focus key and the existing overlay toggle. This view owns focus.
        if (ControlInputMatcher.Pressed(value, "optional_overlays_toggle")
            && !ControlInputMatcher.Pressed(value, "ui_focus_next") && !ControlInputMatcher.Pressed(value, "ui_focus_prev"))
        { GetViewport().SetInputAsHandled(); }
        // GUI gets native navigation/activation; unmatched world actions never reach _UnhandledInput.
    }
    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        InputEvent value = @event; if (Visible) { GetViewport().SetInputAsHandled(); }
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Visible || !capture.Active) { return; }
        if (awaitingArm && !Input.IsMouseButtonPressed(MouseButton.Left) && !Input.IsActionPressed("ui_accept"))
        { capture.ArmAfterRelease(); awaitingArm = false; status.Text = capture.Status; }
        if (!awaitingArm && capture.Device is int connected && Input.GetConnectedJoypads().Contains(connected))
        {
            for (int axis = 0; axis <= 5 && capture.Active; axis++)
            { AcceptCandidate(capture.ObserveAxis(axis, Input.GetJoyAxis(connected, (JoyAxis)axis), connected)); }
        }
        capture.Advance(double.IsFinite(delta) && delta >= 0 ? delta : 0);
        if (!capture.Active) { if (capture.Candidate is null) { status.Text = capture.Status; } cancelCapture.Visible = false; EnableEdits(); WireFocus(); add.GrabFocus(); }
    }
    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut)
        { if (capture.Active) { CancelCapture("Window lost focus"); } ControlInputContext.Current?.Fence(); }
    }
    private bool HandleCapturePointer(Vector2 point)
    {
        if (cancelCapture.GetGlobalRect().HasPoint(point)) { CancelCapture(); add.GrabFocus(); return true; }
        if (close.GetGlobalRect().HasPoint(point)) { Close(); return true; }
        if (restoreDefaults.GetGlobalRect().HasPoint(point)) { RestoreDefaults(); restoreDefaults.GrabFocus(); return true; }
        return false;
    }
    private void RestoreDefaults()
    {
        CancelCapture(); draft = ControlPreferences.Defaults(); RefreshBindings(); WireFocus();
        status.Text = "Default draft restored · Apply or Save";
    }
    private void ConnectionChanged(long deviceId, bool connected)
    {
        if (connected) { return; }
        capture.Disconnect((int)deviceId); ControlInputContext.Current?.Disconnect((int)deviceId);
        if (Visible && !capture.Active) { status.Text = capture.Status; cancelCapture.Visible = false; EnableEdits(); WireFocus(); }
    }
    internal void BeginCapture(string deviceClass, int? index = null)
    {
        for (int item = 0; item < 3; item++) { if (device.GetItemText(item) == deviceClass) { device.Select(item); } }
        replacing = index; int[] connected = Input.GetConnectedJoypads().ToArray();
        capture.Begin(deviceClass, deviceClass == "Controller" && connected.Length > 0 ? connected[0] : null);
        awaitingArm = true; status.Text = capture.Status; cancelCapture.Visible = true;
        add.Disabled = replace.Disabled = remove.Disabled = rowDefault.Disabled = true; WireFocus();
    }
    private void AcceptCandidate(ControlBinding? candidate)
    {
        if (candidate is null) { return; }
        Edit(() => ControlBindingEdit.Set(draft, actionId, candidate, replacing));
        cancelCapture.Visible = false; EnableEdits(); WireFocus(); add.GrabFocus();
    }
    internal void CancelCapture(string reason = "Capture cancelled")
    {
        if (capture.Active) { capture.Cancel(reason); status.Text = capture.Status; }
        awaitingArm = false;
        if (cancelCapture is not null) { cancelCapture.Visible = false; EnableEdits(); WireFocus(); }
    }
    internal void SelectAction(string id)
    {
        actionId = ControlActionCatalog.Get(id).Id;
        actions.Select(ControlActionCatalog.Actions.ToList().FindIndex(a => a.Id == id)); actions.EnsureCurrentIsVisible(); RefreshBindings();
    }
    internal bool ApplyDraft(bool save)
    {
        bool applied = save ? controller.Save(draft) : controller.Apply(draft);
        if (applied) { draft = controller.Preferences; }
        RefreshStatus(); return applied;
    }
    internal void SetDraft(ControlPreferences value) { draft = value; Refresh(); status.Text = HasUnapplied ? "Draft changed · Apply or Save when ready" : controller.Status; }
    private void Edit(Func<ControlPreferences> edit)
    {
        try { draft = edit(); status.Text = "Draft changed · Apply or Save when ready"; RefreshBindings(); }
        catch (ArgumentException error) { status.Text = error.Message; }
    }
    private void Refresh()
    {
        actions.Clear();
        foreach (ControlActionDefinition action in ControlActionCatalog.Actions)
        { int index = actions.AddItem(action.Label + (action.Context == "ui" ? " · UI" : "")); actions.SetItemTooltip(index, action.Label); }
        SelectAction(actionId); RefreshStatus(); WireFocus();
    }
    private void RefreshBindings()
    {
        refreshing = true; bindings.Clear(); ControlActionSetting row = draft.Get(actionId);
        details.Text = ControlActionCatalog.Get(actionId).Label;
        foreach (ControlBinding binding in row.Bindings) { bindings.AddItem((binding.Kind == "key" ? "Keyboard: " : "") + ControlInputMapAdapter.Describe(binding)); }
        if (row.Bindings.Count > 0) { bindings.Select(0); }
        deadzone.Value = row.Deadzone; deadzone.Editable = ControlActionCatalog.Get(actionId).Direction;
        zoneValue.Text = "Deadzone " + row.Deadzone.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        refreshing = false; EnableEdits();
    }
    private void RefreshStatus() { if (status is not null && !capture.Active) { status.Text = controller.Status; } }
    private int SelectedBinding => bindings.GetSelectedItems().FirstOrDefault(-1);
    private void EnableEdits()
    {
        if (add is null) { return; }
        add.Disabled = capture.Active; replace.Disabled = capture.Active || SelectedBinding < 0;
        remove.Disabled = capture.Active || draft.Get(actionId).Bindings.Count <= 1 || SelectedBinding < 0;
        rowDefault.Disabled = capture.Active;
    }
    private void Build()
    {
        ColorRect shade = new() { Name = "Shade", Color = new Color(0, 0, 0, 0.7f), MouseFilter = Control.MouseFilterEnum.Stop };
        AddChild(shade); shade.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        PanelContainer panel = new() { Name = "Panel" }; shade.AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        panel.OffsetLeft = -570; panel.OffsetRight = 570; panel.OffsetTop = -320; panel.OffsetBottom = 320;
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.055f, 0.067f, 0.082f),
            ContentMarginLeft = 24,
            ContentMarginRight = 24,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        });
        VBoxContainer content = new() { Name = "Content" }; panel.AddChild(content); content.AddThemeConstantOverride("separation", 12);
        Label title = new() { Text = "Controls settings" }; title.AddThemeFontSizeOverride("font_size", 26); content.AddChild(title);
        content.AddChild(new Label { Text = "Edit a draft · Apply now · Save for restart · Escape cancels capture or returns" });
        HBoxContainer columns = new() { Name = "Columns", SizeFlagsVertical = Control.SizeFlags.ExpandFill }; columns.AddThemeConstantOverride("separation", 18); content.AddChild(columns);
        actions = new() { Name = "Actions", CustomMinimumSize = new Vector2(370, 330), SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        columns.AddChild(actions); focusOrder.Add(actions);
        actions.ItemSelected += index => { actionId = ControlActionCatalog.Actions[(int)index].Id; RefreshBindings(); };
        VBoxContainer editor = new() { Name = "Editor", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; editor.AddThemeConstantOverride("separation", 12); columns.AddChild(editor);
        details = new() { Name = "ActionLabel", AutowrapMode = TextServer.AutowrapMode.WordSmart }; editor.AddChild(details);
        bindings = new() { Name = "Bindings", CustomMinimumSize = new Vector2(620, 150), SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        editor.AddChild(bindings); focusOrder.Add(bindings); bindings.ItemSelected += _ => EnableEdits();
        HBoxContainer devices = new(); editor.AddChild(devices); devices.AddChild(new Label { Text = "Capture device" });
        device = new() { Name = "Device" }; devices.AddChild(device); focusOrder.Add(device);
        device.AddItem("Keyboard"); device.AddItem("Mouse"); device.AddItem("Controller");
        HBoxContainer edits = new(); edits.AddThemeConstantOverride("separation", 8); editor.AddChild(edits);
        add = MakeButton(edits, "Add", "Add"); add.Pressed += () => BeginCapture(device.GetItemText(device.Selected));
        replace = MakeButton(edits, "Replace", "Replace"); replace.Pressed += () => { if (SelectedBinding >= 0) { BeginCapture(device.GetItemText(device.Selected), SelectedBinding); } };
        remove = MakeButton(edits, "Remove", "Remove"); remove.Pressed += () => { if (SelectedBinding >= 0) { Edit(() => ControlBindingEdit.Remove(draft, actionId, SelectedBinding)); } };
        rowDefault = MakeButton(edits, "RowDefaults", "Row defaults"); rowDefault.Pressed += () => Edit(() => ControlBindingEdit.Reset(draft, actionId));
        HBoxContainer zones = new(); editor.AddChild(zones); zoneValue = new() { CustomMinimumSize = new Vector2(135, 0) }; zones.AddChild(zoneValue);
        deadzone = new() { Name = "Deadzone", MinValue = 0.1, MaxValue = 0.9, Step = 0.05, CustomMinimumSize = new Vector2(240, 32), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        zones.AddChild(deadzone); focusOrder.Add(deadzone); deadzone.ValueChanged += value => { if (!refreshing) { Edit(() => ControlBindingEdit.Deadzone(draft, actionId, value)); } };
        status = new() { Name = "Status", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(0, 48) }; content.AddChild(status);
        HBoxContainer footer = new(); footer.AddThemeConstantOverride("separation", 12); content.AddChild(footer);
        Button apply = MakeButton(footer, "Apply", "Apply"); apply.Pressed += () => ApplyDraft(false);
        Button save = MakeButton(footer, "Save", "Save"); save.Pressed += () => ApplyDraft(true);
        restoreDefaults = MakeButton(footer, "Defaults", "Restore defaults"); restoreDefaults.Pressed += RestoreDefaults;
        close = MakeButton(footer, "Close", "Close"); close.Pressed += () => Close();
        cancelCapture = MakeButton(footer, "CancelCapture", "Cancel capture"); cancelCapture.Pressed += () => CancelCapture(); cancelCapture.Visible = false;
        confirmation = new() { Name = "Confirmation", Visible = false }; content.AddChild(confirmation);
        discard = MakeButton(confirmation, "Discard", "Discard and close"); discard.Pressed += () => FinishClose(true);
        keep = MakeButton(confirmation, "KeepEditing", "Keep editing"); keep.Pressed += () => { confirmation.Visible = false; WireFocus(); actions.GrabFocus(); };
    }
    private Button MakeButton(Node parent, string name, string text)
    { Button button = new() { Name = name, Text = text, CustomMinimumSize = new Vector2(0, 36) }; parent.AddChild(button); focusOrder.Add(button); return button; }
    private void WireFocus()
    {
        Control[] current = focusOrder.Where(c => c.IsVisibleInTree() && (c is not BaseButton button || !button.Disabled)
            && (c is not HSlider slider || slider.Editable)).ToArray();
        for (int i = 0; i < current.Length; i++)
        {
            NodePath previous = current[i].GetPathTo(current[(i + current.Length - 1) % current.Length]);
            NodePath next = current[i].GetPathTo(current[(i + 1) % current.Length]);
            current[i].FocusPrevious = previous; current[i].FocusNext = next;
            current[i].FocusNeighborTop = previous; current[i].FocusNeighborBottom = next;
            current[i].FocusNeighborLeft = previous; current[i].FocusNeighborRight = next;
        }
    }
    /// <inheritdoc />
    public override void _ExitTree()
    { capture.Cancel("View exited"); Input.JoyConnectionChanged -= ConnectionChanged; controller.Changed -= RefreshStatus; }
}
