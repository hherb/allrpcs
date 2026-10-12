using Godot;

namespace Allrpcs.Game;

/// <summary>Independently owned noninteractive caption lines, regardless of user output attenuation.</summary>
public partial class MaterialSoundCaptionPanel : CanvasLayer
{
    private AudioSettingsController settings = null!;
    private Label regionLabel = null!;
    private Label actionLabel = null!;
    private Label footstepLabel = null!;
    private Label conversationLabel = null!;
    private Label interiorLabel = null!;
    private Label waterLabel = null!;
    private Label coverLabel = null!;
    private Label heatingLabel = null!;
    private string heating = string.Empty;
    private Label rainLabel = null!;
    private string rain = string.Empty;
    private Label windLabel = null!;
    private string wind = string.Empty;
    private Label foliageLabel = null!;
    private string foliage = string.Empty;
    private Label healerFootstepLabel = null!;
    private string healerFootstep = string.Empty;
    private Label handToolLabel = null!;
    private string handTool = string.Empty;
    private string caskCover = string.Empty;
    private string waterContact = string.Empty;
    private string interior = string.Empty;
    private string conversation = string.Empty;
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
        panel.OffsetLeft = 16; panel.OffsetRight = 420; panel.OffsetTop = -464; panel.OffsetBottom = -16;
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
        conversationLabel = new() { Name = "Conversation", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(conversationLabel);
        interiorLabel = new() { Name = "Interior", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(interiorLabel);
        waterLabel = new() { Name = "WaterContact", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(waterLabel);
        coverLabel = new() { Name = "CaskCover", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(coverLabel);
        handToolLabel = new() { Name = "HandToolTap", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(handToolLabel);
        healerFootstepLabel = new() { Name = "HealerFootstep", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(healerFootstepLabel);
        foliageLabel = new() { Name = "Foliage", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(foliageLabel);
        windLabel = new() { Name = "Wind", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(windLabel);
        rainLabel = new() { Name = "Rain", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(rainLabel);
        heatingLabel = new() { Name = "RemedyHeating", MouseFilter = Control.MouseFilterEnum.Ignore };
        lines.AddChild(heatingLabel);
        foreach (Label label in new[] { regionLabel, actionLabel, footstepLabel, conversationLabel, interiorLabel, waterLabel, coverLabel, handToolLabel, healerFootstepLabel, foliageLabel, windLabel, rainLabel, heatingLabel })
        { label.AutowrapMode = TextServer.AutowrapMode.WordSmart; }
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

    /// <summary>Sets only the independently owned heard reply line.</summary>
    internal void SetConversation(string text) { conversation = text; Refresh(); }

    /// <summary>Changes only the generic local enclosure line, without adding cue history.</summary>
    internal void SetInterior(string text) { interior = text; Refresh(); }
    /// <summary>Clears only enclosure ownership on invalid sources, load or stop.</summary>
    internal void ClearInterior() { interior = string.Empty; Refresh(); }

    /// <summary>Sets only the new local committed-contact caption, independent of output availability.</summary>
    internal void SetWaterContact(string text) { waterContact = text; Refresh(); }
    /// <summary>Clears only water-contact ownership without changing prior caption owners.</summary>
    internal void ClearWaterContact() { waterContact = string.Empty; Refresh(); }

    /// <summary>Sets only the independently owned local cask-cover handling caption.</summary>
    internal void SetCaskCover(string text) { caskCover = text; Refresh(); }
    /// <summary>Clears only cask-cover ownership, preserving every retained caption timer.</summary>
    internal void ClearCaskCover() { caskCover = string.Empty; Refresh(); }

    /// <summary>Sets only the new wooden mallet caption.</summary>
    internal void SetHandToolTap(string text) { handTool = text; Refresh(); }
    /// <summary>Clears only tool ownership, retaining all prior captions and timers.</summary>
    internal void ClearHandToolTap() { handTool = string.Empty; Refresh(); }

    /// <summary>Shows only the anonymous spatial contact, without changing prior caption owners.</summary>
    internal void SetHealerFootstep(string text) { healerFootstep = text; Refresh(); }
    /// <summary>Cancels only the new spatial contact line.</summary>
    internal void ClearHealerFootstep() { healerFootstep = string.Empty; Refresh(); }

    /// <summary>Shows only the independently owned anonymous garden passage.</summary>
    internal void SetFoliage(string text) { foliage = text; Refresh(); }
    /// <summary>Cancels only foliage without changing prior owners or timers.</summary>
    internal void ClearFoliage() { foliage = string.Empty; Refresh(); }

    /// <summary>Shows only the independent generic road wind passage.</summary>
    internal void SetWind(string text) { wind = text; Refresh(); }
    /// <summary>Clears wind without changing any retained caption timer.</summary>
    internal void ClearWind() { wind = string.Empty; Refresh(); }

    /// <summary>Shows only the independent generic woodland rain passage.</summary>
    internal void SetRain(string text) { rain = text; Refresh(); }
    /// <summary>Clears rain without changing any retained caption timer.</summary>
    internal void ClearRain() { rain = string.Empty; Refresh(); }

    /// <summary>Shows only the independent anonymous heating operation.</summary>
    internal void SetRemedyHeating(string text) { heating = text; Refresh(); }
    /// <summary>Cancels heating without changing prior caption owners.</summary>
    internal void ClearRemedyHeating() { heating = string.Empty; Refresh(); }

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
        conversationLabel.Text = enabled ? conversation : string.Empty;
        interiorLabel.Text = enabled ? interior : string.Empty;
        waterLabel.Text = enabled ? waterContact : string.Empty;
        coverLabel.Text = enabled ? caskCover : string.Empty;
        handToolLabel.Text = enabled ? handTool : string.Empty;
        healerFootstepLabel.Text = enabled ? healerFootstep : string.Empty;
        foliageLabel.Text = enabled ? foliage : string.Empty;
        windLabel.Text = enabled ? wind : string.Empty;
        heatingLabel.Text = enabled ? heating : string.Empty;
        rainLabel.Text = enabled ? rain : string.Empty;
        Visible = enabled && (region.Length > 0 || action.Length > 0 || footstep.Length > 0 || conversation.Length > 0 || interior.Length > 0 || waterContact.Length > 0 || caskCover.Length > 0 || handTool.Length > 0 || healerFootstep.Length > 0 || foliage.Length > 0 || wind.Length > 0 || rain.Length > 0 || heating.Length > 0);
    }
}
