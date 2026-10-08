using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Accessible presentation settings with live changes and explicit persistence.</summary>
public partial class AudioSettingsPanel : CanvasLayer
{
    private readonly Dictionary<AudioCategory, HSlider> sliders = [];
    private readonly Dictionary<AudioCategory, Label> values = [];
    private readonly Dictionary<AudioCategory, CheckButton> mutes = [];
    private readonly List<Control> focusOrder = [];
    private AudioSettingsController controller = null!;
    private OptionButton preset = null!;
    private CheckButton mono = null!;
    private CheckButton captions = null!;
    private Label status = null!;
    private Control? returnFocus;
    private bool refreshing;
    private MaterialAudioCreditsPanel creditsPanel = null!;

    /// <summary>Raised when the panel closes so its entry can be restored.</summary>
    public event Action<bool>? Closed;

    /// <inheritdoc />
    public override void _Ready()
    {
        controller = GetNode<AudioSettingsController>("../AudioSettingsController");
        BuildPanel();
        controller.Changed += Refresh;
        Refresh();
    }

    /// <summary>Opens with predictable first focus, retaining the originating button.</summary>
    public void Open(Control? origin = null)
    {
        returnFocus = origin;
        Visible = true;
        Refresh();
        sliders[AudioCategory.Master].GrabFocus();
    }

    /// <summary>Closes and optionally restores focus without changing world time.</summary>
    public void Close(bool restoreFocus = true)
    {
        if (creditsPanel.Visible) { creditsPanel.Close(false); }
        GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
        Visible = false;
        Closed?.Invoke(restoreFocus);
        if (restoreFocus && returnFocus is not null && GodotObject.IsInstanceValid(returnFocus)
            && returnFocus.IsVisibleInTree())
        {
            returnFocus.GrabFocus();
        }
    }

    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        if (creditsPanel.Visible && @event.IsActionPressed("ui_cancel"))
        {
            creditsPanel.Close();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (creditsPanel.Visible && @event.IsActionPressed("optional_overlays_toggle"))
        {
            Close(false);
            GetViewport().SetInputAsHandled();
            return;
        }
        if (Visible && (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("optional_overlays_toggle")))
        {
            Close(!@event.IsActionPressed("optional_overlays_toggle"));
            GetViewport().SetInputAsHandled();
        }
    }

    /// <inheritdoc />
    public override void _ExitTree() => controller.Changed -= Refresh;

    private void BuildPanel()
    {
        ColorRect shade = new() { Name = "Shade", Color = new Color(0, 0, 0, 0.4f) };
        AddChild(shade);
        shade.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        PanelContainer panel = new() { Name = "Panel" };
        shade.AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        panel.OffsetLeft = -330;
        panel.OffsetRight = 330;
        panel.OffsetTop = -290;
        panel.OffsetBottom = 290;
        StyleBoxFlat background = new()
        {
            BgColor = new Color(0.055f, 0.067f, 0.082f),
            ContentMarginLeft = 24,
            ContentMarginRight = 24,
            ContentMarginTop = 20,
            ContentMarginBottom = 20,
        };
        panel.AddThemeStyleboxOverride("panel", background);
        VBoxContainer content = new();
        content.AddThemeConstantOverride("separation", 12);
        panel.AddChild(content);
        content.AddChild(new Label { Text = "Audio", ThemeTypeVariation = "HeaderLarge" });
        content.AddChild(new Label { Text = "Changes apply now. Save to keep them after restart." });
        GridContainer grid = new() { Columns = 4 };
        grid.AddThemeConstantOverride("h_separation", 14);
        grid.AddThemeConstantOverride("v_separation", 10);
        content.AddChild(grid);
        foreach (AudioCategory category in Enum.GetValues<AudioCategory>())
        {
            grid.AddChild(new Label { Text = category == AudioCategory.Speech ? "Speech (future)" : category.ToString() });
            HSlider slider = new()
            {
                Name = category + "Gain",
                MinValue = 0,
                MaxValue = 100,
                Step = 1,
                CustomMinimumSize = new Vector2(280, 38),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                TooltipText = $"{category} volume · zero is silence",
            };
            grid.AddChild(slider);
            sliders.Add(category, slider);
            focusOrder.Add(slider);
            Label value = new() { CustomMinimumSize = new Vector2(48, 0), HorizontalAlignment = HorizontalAlignment.Right };
            grid.AddChild(value);
            values.Add(category, value);
            CheckButton mute = new() { Name = category + "Mute", Text = "Mute", TooltipText = $"Mute {category}" };
            grid.AddChild(mute);
            mutes.Add(category, mute);
            focusOrder.Add(mute);
            slider.ValueChanged += gain => ChangeChannel(category, gain / 100, mute.ButtonPressed);
            mute.Toggled += muted => ChangeChannel(category, slider.Value / 100, muted);
        }
        HBoxContainer mixRow = new();
        content.AddChild(mixRow);
        mixRow.AddChild(new Label { Text = "Dynamic range", CustomMinimumSize = new Vector2(150, 0) });
        preset = new OptionButton { Name = "Preset", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        preset.AddItem("Default");
        preset.AddItem("Reduced range");
        mixRow.AddChild(preset);
        focusOrder.Add(preset);
        preset.ItemSelected += _ => ChangeMix();
        mono = Toggle(content, "Mono", "Mono output");
        captions = Toggle(content, "Captions", "Sound captions · dialogue and action text always stays on");
        status = new Label { Name = "Status", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        content.AddChild(status);
        HBoxContainer buttons = new();
        buttons.AddThemeConstantOverride("separation", 12);
        content.AddChild(buttons);
        Button save = Button(buttons, "Save", "Save");
        save.Pressed += () => controller.Save();
        Button defaults = Button(buttons, "Defaults", "Restore defaults");
        defaults.Pressed += controller.RestoreDefaults;
        Button close = Button(buttons, "Close", "Close");
        close.Pressed += () => Close();
        Button credits = Button(buttons, "SourceCredits", "Audio source credits");
        creditsPanel = GD.Load<PackedScene>("res://MaterialAudioCreditsPanel.tscn")
            .Instantiate<MaterialAudioCreditsPanel>();
        AddChild(creditsPanel);
        creditsPanel.Closed += restore => { if (restore) { Visible = true; } };
        credits.Pressed += () => { Visible = false; creditsPanel.Open(credits); };
        for (int index = 0; index < focusOrder.Count; index++)
        {
            Control control = focusOrder[index];
            NodePath next = control.GetPathTo(focusOrder[(index + 1) % focusOrder.Count]);
            NodePath previous = control.GetPathTo(focusOrder[(index + focusOrder.Count - 1) % focusOrder.Count]);
            control.FocusNext = next;
            control.FocusPrevious = previous;
            control.FocusNeighborBottom = next;
            control.FocusNeighborTop = previous;
            if (control is not HSlider)
            {
                control.FocusNeighborRight = next;
                control.FocusNeighborLeft = previous;
            }
        }
    }

    private CheckButton Toggle(VBoxContainer parent, string name, string text)
    {
        CheckButton button = new() { Name = name, Text = text };
        parent.AddChild(button);
        focusOrder.Add(button);
        button.Toggled += _ => ChangeMix();
        return button;
    }

    private Button Button(HBoxContainer parent, string name, string text)
    {
        Button button = new() { Name = name, Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        parent.AddChild(button);
        focusOrder.Add(button);
        return button;
    }

    private void ChangeChannel(AudioCategory category, double gain, bool muted)
    {
        if (!refreshing)
        {
            controller.SetPreferences(controller.Preferences.WithChannel(category, gain, muted));
        }
    }

    private void ChangeMix()
    {
        if (!refreshing)
        {
            controller.SetPreferences(new AudioPreferences(controller.Preferences.Channels,
                (AudioMixPreset)preset.Selected, mono.ButtonPressed, captions.ButtonPressed));
        }
    }

    private void Refresh()
    {
        refreshing = true;
        foreach (AudioChannelSetting channel in controller.Preferences.Channels)
        {
            sliders[channel.Category].Value = channel.Gain * 100;
            values[channel.Category].Text = $"{channel.Gain * 100:0}%";
            mutes[channel.Category].ButtonPressed = channel.Muted;
        }
        preset.Select((int)controller.Preferences.Preset);
        mono.ButtonPressed = controller.Preferences.Mono;
        captions.ButtonPressed = controller.Preferences.Captions;
        status.Text = controller.Status;
        refreshing = false;
    }
}
