using Godot;

namespace Allrpcs.Game;

/// <summary>Two noninteractive caption lines, independent of user output attenuation.</summary>
public partial class MaterialSoundCaptionPanel : CanvasLayer
{
    private AudioSettingsController settings = null!;
    private Label regionLabel = null!;
    private Label actionLabel = null!;
    private Label footstepLabel = null!;
    private string region = string.Empty;
    private string action = string.Empty;
    private double remaining;
    private string footstep = string.Empty;
    private double footstepRemaining;

    /// <inheritdoc />
    public override void _Ready()
    {
        settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        PanelContainer panel = new() { Name = "Panel", MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomLeft);
        panel.OffsetLeft = 16; panel.OffsetRight = 420; panel.OffsetTop = -108; panel.OffsetBottom = -16;
        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.035f, 0.045f, 0.055f, 0.92f),
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };
        panel.AddThemeStyleboxOverride("panel", style);
        VBoxContainer lines = new() { Name = "Lines", MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddChild(lines);
        regionLabel = new() { Name = "Region", MouseFilter = Control.MouseFilterEnum.Ignore };
        actionLabel = new() { Name = "Action", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(regionLabel); lines.AddChild(actionLabel);
        footstepLabel = new() { Name = "Footstep", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(footstepLabel);
        settings.Changed += Refresh;
        Refresh();
    }

    /// <summary>Changes only the current safe regional wording, without a repeated loop caption.</summary>
    internal void SetRegion(string text) { region = text; Refresh(); }
    /// <summary>Shows the latest committed action during its accepted passage interval.</summary>
    internal void SetAction(string text, double seconds) { action = text; remaining = seconds; Refresh(); }
    /// <summary>Clears transient captions at load, stop or reset.</summary>
    internal void Clear() { region = action = string.Empty; remaining = 0; Refresh(); }

    /// <summary>Shows only the local Seeker's latest eligible material contact, independently of output gain.</summary>
    internal void SetFootstep(string text) { footstep = text; footstepRemaining = 0.75; Refresh(); }
    /// <summary>Clears footstep ownership without removing regional or committed-action captions.</summary>
    internal void ClearFootstep() { footstep = string.Empty; footstepRemaining = 0; Refresh(); }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (footstepRemaining > 0)
        {
            footstepRemaining = Math.Max(0, footstepRemaining - delta);
            if (footstepRemaining == 0) { footstep = string.Empty; Refresh(); }
        }
        if (remaining <= 0) { return; }
        remaining = Math.Max(0, remaining - delta);
        if (remaining == 0) { action = string.Empty; Refresh(); }
    }

    /// <inheritdoc />
    public override void _ExitTree() => settings.Changed -= Refresh;

    private void Refresh()
    {
        bool enabled = settings.Preferences.Captions;
        regionLabel.Text = enabled ? region : string.Empty;
        actionLabel.Text = enabled ? action : string.Empty;
        footstepLabel.Text = enabled ? footstep : string.Empty;
        Visible = enabled && (region.Length > 0 || action.Length > 0 || footstep.Length > 0);
    }
}
