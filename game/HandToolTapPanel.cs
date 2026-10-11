using Allrpcs.Domain.Actions;
using Godot;

namespace Allrpcs.Game;

/// <summary>Contextual native mouse/keyboard controls for one explicit wooden mallet contact.</summary>
public partial class HandToolTapPanel : CanvasLayer
{
    private OpeningRegionHandToolTapCoordinator coordinator = null!;
    private Button button = null!;
    private Label status = null!;
    private bool acceptHeld;
    private double remaining;

    /// <summary>The generic own-action status, independent of audio/caption preferences.</summary>
    internal const string StatusText = "You tap the practice block.";
    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        coordinator = GetNode<OpeningRegionHandToolTapCoordinator>("../OpeningRegionHandToolTapCoordinator");
        PanelContainer panel = new() { Name = "Panel" }; AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
        panel.OffsetLeft = -300; panel.OffsetRight = -16; panel.OffsetTop = 444; panel.OffsetBottom = 542;
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
            Name = "TapPracticeBlock",
            Text = "Tap practice block",
            TooltipText = "Use the communal wooden mallet to tap the wooden practice block once; leave both here.",
            FocusMode = Control.FocusModeEnum.All
        }; lines.AddChild(button);
        status = new() { Name = "Status", AutowrapMode = TextServer.AutowrapMode.WordSmart }; lines.AddChild(status);
        button.Pressed += () => coordinator.TryTap();
        coordinator.Committed += Receive; coordinator.PresentationCancelled += Clear;
        Visible = false;
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        bool offer = coordinator.CanOffer;
        if (!offer && button.HasFocus()) { button.ReleaseFocus(); }
        Visible = offer; button.Disabled = coordinator.Cooldown > 0;
        if (double.IsFinite(delta) && delta >= 0 && delta <= 0.25) { remaining = Math.Max(0, remaining - delta); }
        if (remaining == 0) { status.Text = string.Empty; }
    }
    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey key) { return; }
        if (Visible && key.Pressed && !key.Echo && key.IsAction("ui_focus_next") && !button.HasFocus())
        {
            button.GrabFocus(); GetViewport().SetInputAsHandled(); return;
        }
        if (!key.IsAction("ui_accept")) { return; }
        if (!key.Pressed) { acceptHeld = false; }
        if (!Visible || !button.HasFocus()) { return; }
        if (key.Pressed && !key.Echo && !acceptHeld) { acceptHeld = true; coordinator.TryTap(); }
        GetViewport().SetInputAsHandled();
    }
    private void Receive(HandToolTapCommit commit)
    { _ = commit; status.Text = StatusText; remaining = 0.75; }
    private void Clear() { status.Text = string.Empty; remaining = 0; }
    /// <inheritdoc />
    public override void _ExitTree()
    { coordinator.Committed -= Receive; coordinator.PresentationCancelled -= Clear; Clear(); }
}
