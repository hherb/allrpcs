using Allrpcs.Domain.Actions;
using Godot;

namespace Allrpcs.Game;

/// <summary>Contextual native mouse/keyboard controls for one explicit water contact.</summary>
public partial class StreamWaterContactPanel : CanvasLayer
{
    private OpeningRegionStreamWaterContactCoordinator coordinator = null!;
    private Button button = null!;
    private Label status = null!;
    private bool acceptHeld;
    private double remaining;

    /// <summary>The generic own-action status, independent of audio/caption preferences.</summary>
    internal const string StatusText = "You touch the flowing water.";
    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        coordinator = GetNode<OpeningRegionStreamWaterContactCoordinator>("../OpeningRegionStreamWaterContactCoordinator");
        PanelContainer panel = new() { Name = "Panel" }; AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
        panel.OffsetLeft = -300; panel.OffsetRight = -16; panel.OffsetTop = 250; panel.OffsetBottom = 334;
        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.035f, 0.045f, 0.055f, 0.94f),
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };
        panel.AddThemeStyleboxOverride("panel", style);
        VBoxContainer lines = new(); panel.AddChild(lines);
        button = new()
        {
            Name = "TouchWater",
            Text = "Touch water",
            TooltipText = "Touch the flowing water beside the bridge.",
            FocusMode = Control.FocusModeEnum.All
        }; lines.AddChild(button);
        status = new() { Name = "Status", AutowrapMode = TextServer.AutowrapMode.WordSmart }; lines.AddChild(status);
        button.Pressed += () => coordinator.TryContact();
        coordinator.Committed += Receive; coordinator.PresentationCancelled += Clear;
        Visible = false;
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        bool offer = coordinator.CanOffer;
        if (!offer) { acceptHeld = false; if (button.HasFocus()) { button.ReleaseFocus(); } }
        Visible = offer; button.Disabled = coordinator.Cooldown > 0;
        if (double.IsFinite(delta) && delta >= 0 && delta <= 0.25) { remaining = Math.Max(0, remaining - delta); }
        if (remaining == 0) { status.Text = string.Empty; }
    }
    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        if (ControlInputMatcher.Released(@event, "ui_accept")) { acceptHeld = false; }
        if (!ControlInputContext.TogglesAllowed(this)) { return; }
        // Primary pointer clicks retain native hit testing, even if mapped to UI accept.
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left }) { return; }
        if (Visible && ControlInputMatcher.Pressed(@event, "ui_focus_next") && !button.HasFocus())
        {
            button.GrabFocus(); GetViewport().SetInputAsHandled(); return;
        }
        if (!ControlInputMatcher.Matches(@event, "ui_accept")) { return; }
        if (!Visible || !button.HasFocus()) { return; }
        if (ControlInputMatcher.Pressed(@event, "ui_accept") && !acceptHeld)
        { acceptHeld = true; coordinator.TryContact(); }
        GetViewport().SetInputAsHandled();
    }
    /// <inheritdoc />
    public override void _Notification(int what)
    { if (what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut) { acceptHeld = false; } }

    private void Receive(StreamWaterContactCommit commit)
    { _ = commit; status.Text = StatusText; remaining = 1.5; }
    private void Clear() { status.Text = string.Empty; remaining = 0; }
    /// <inheritdoc />
    public override void _ExitTree()
    { coordinator.Committed -= Receive; coordinator.PresentationCancelled -= Clear; Clear(); }
}
