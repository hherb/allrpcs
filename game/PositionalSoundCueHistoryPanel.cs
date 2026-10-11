using Godot;

namespace Allrpcs.Game;

/// <summary>Accessible transient generic reply cues, with independent focus and return ownership.</summary>
public partial class PositionalSoundCueHistoryPanel : CanvasLayer
{
    private RichTextLabel entries = null!;
    private Button close = null!;
    private Control? origin;
    private OpeningRegionFoliageAudioController? foliage;
    private OpeningRegionHealerFootstepAudioController? footsteps;
    private OpeningRegionPositionalAudioController? audio;
    /// <summary>Restores the Audio surface when its child view closes.</summary>
    public event Action<bool>? Closed;

    /// <inheritdoc />
    public override void _Ready()
    {
        foliage = GetNodeOrNull<OpeningRegionFoliageAudioController>("../../OpeningRegionFoliageAudioController");
        footsteps = GetNodeOrNull<OpeningRegionHealerFootstepAudioController>("../../OpeningRegionHealerFootstepAudioController");
        audio = GetNodeOrNull<OpeningRegionPositionalAudioController>("../../OpeningRegionPositionalAudioController");
        ColorRect shade = new() { Name = "Shade", Color = new(0, 0, 0, 0.55f) };
        AddChild(shade); shade.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        PanelContainer panel = new() { Name = "Panel" };
        shade.AddChild(panel); panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        panel.OffsetLeft = -360; panel.OffsetRight = 360; panel.OffsetTop = -240; panel.OffsetBottom = 240;
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new(0.055f, 0.067f, 0.082f),
            ContentMarginLeft = 24,
            ContentMarginRight = 24,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        });
        VBoxContainer content = new(); panel.AddChild(content);
        content.AddChild(new Label { Text = "Recent sound cues", ThemeTypeVariation = "HeaderLarge" });
        content.AddChild(new Label { Text = "Reply, footstep and foliage cues · cleared when loading a save" });
        entries = new()
        {
            Name = "Entries",
            CustomMinimumSize = new(640, 320),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SelectionEnabled = true,
            FocusMode = Control.FocusModeEnum.All
        };
        content.AddChild(entries);
        close = new() { Name = "CloseHistory", Text = "Back to Audio" };
        content.AddChild(close); close.Pressed += () => Close();
        entries.FocusNext = entries.GetPathTo(close); entries.FocusPrevious = entries.GetPathTo(close);
        close.FocusNext = close.GetPathTo(entries); close.FocusPrevious = close.GetPathTo(entries);
        Refresh();
    }

    /// <summary>Shows bounded plain history even with muted output or disabled captions.</summary>
    public void Open(Control entry) { origin = entry; Visible = true; Refresh(); close.GrabFocus(); }
    /// <summary>Returns focus to the visible entry, or silently closes all overlays.</summary>
    public void Close(bool restore = true)
    {
        GetViewport().GuiGetFocusOwner()?.ReleaseFocus(); Visible = false; Closed?.Invoke(restore);
        if (restore && origin is not null && GodotObject.IsInstanceValid(origin) && origin.IsVisibleInTree()) { origin.GrabFocus(); }
    }
    /// <inheritdoc />
    public override void _Process(double delta) { _ = delta; if (Visible) { Refresh(); } }
    private void Refresh()
    {
        IReadOnlyList<string> cues = audio?.RecentCues ?? [];
        string replies = cues.Count == 0 ? "No recent heard reply cues." : string.Join('\n', cues);
        IReadOnlyList<string> nearby = footsteps?.RecentCues ?? [];
        string steps = nearby.Count == 0 ? "No recent nearby footstep cues." : string.Join('\n', nearby);
        IReadOnlyList<string> leaves = foliage?.RecentCues ?? [];
        string rustling = leaves.Count == 0 ? "No recent nearby foliage cues." : string.Join('\n', leaves);
        entries.Text = "Heard reply cues\n" + replies + "\n\nNearby footstep cues\n" + steps + "\n\nNearby foliage cues\n" + rustling;
    }
}
