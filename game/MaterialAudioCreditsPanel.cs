using System.Text.RegularExpressions;
using Godot;

namespace Allrpcs.Game;

/// <summary>Readable packaged source attribution, opened from the existing Audio panel.</summary>
public partial class MaterialAudioCreditsPanel : CanvasLayer
{
    private Button close = null!;
    private Control? origin;
    /// <summary>Returns ownership of the settings surface after credits close.</summary>
    public event Action<bool>? Closed;

    /// <inheritdoc />
    public override void _Ready()
    {
        ColorRect shade = new() { Name = "Shade", Color = new Color(0, 0, 0, 0.55f) };
        AddChild(shade);
        shade.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        PanelContainer panel = new() { Name = "Panel" };
        shade.AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        panel.OffsetLeft = -440; panel.OffsetRight = 440; panel.OffsetTop = -280; panel.OffsetBottom = 280;
        StyleBoxFlat background = new()
        {
            BgColor = new Color(0.055f, 0.067f, 0.082f),
            ContentMarginLeft = 24,
            ContentMarginRight = 24,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        };
        panel.AddThemeStyleboxOverride("panel", background);
        VBoxContainer content = new();
        panel.AddChild(content);
        content.AddChild(new Label { Text = "Audio source credits", ThemeTypeVariation = "HeaderLarge" });
        string text = Godot.FileAccess.GetFileAsString("res://assets/audio/material/CREDITS.txt");
        text += "\n\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/footsteps/CREDITS.txt");
        text += "\n\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/positional/CREDITS.txt");
        text += "\n\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/music/CREDITS.txt");
        RichTextLabel credits = new()
        {
            Name = "Credits",
            BbcodeEnabled = true,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(800, 430),
            SelectionEnabled = true,
            FocusMode = Control.FocusModeEnum.All
        };
        credits.Text = Regex.Replace(text.Replace("[", "[lb]"), @"https://[^\s]+",
            match => $"[url={match.Value}]{match.Value}[/url]");
        credits.MetaClicked += meta =>
        {
            string url = meta.AsString();
            if (url.StartsWith("https://", StringComparison.Ordinal)) { OS.ShellOpen(url); }
        };
        content.AddChild(credits);
        close = new() { Name = "CloseCredits", Text = "Back to Audio" };
        content.AddChild(close);
        close.Pressed += () => Close();
        close.FocusNext = close.GetPathTo(credits); close.FocusPrevious = close.GetPathTo(credits);
        credits.FocusNext = credits.GetPathTo(close); credits.FocusPrevious = credits.GetPathTo(close);
    }

    /// <summary>Shows source notices and focuses the return control.</summary>
    public void Open(Control entry) { origin = entry; Visible = true; close.GrabFocus(); }

    /// <summary>Returns focus to its originating entry, or closes silently with all overlays.</summary>
    public void Close(bool restore = true)
    {
        GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
        Visible = false;
        Closed?.Invoke(restore);
        if (restore && origin is not null && origin.IsVisibleInTree()) { origin.GrabFocus(); }
    }
}
