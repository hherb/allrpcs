using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks actual Main composition, control focus and unchanged information authority.</summary>
public partial class AudioSettingsMainSmokeCheck : Node
{
    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_AUDIO_MAIN_CHECK") != "1") { return; }
        try
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            AudioSettingsController audio = GetNode<AudioSettingsController>("../AudioSettingsController");
            AudioSettingsPanel panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
            OptionalOverlayVisibilityController visibility = GetNode<OptionalOverlayVisibilityController>("../OptionalOverlayVisibilityController");
            CanvasLayer settings = GetNode<CanvasLayer>("../SettingsOverlay");
            CanvasLayer dialogue = GetNode<CanvasLayer>("../DialogueOverlay");
            Button entry = GetNode<Button>("../SettingsOverlay/AudioEntry");
            Label response = GetNode<Label>("../DialogueOverlay/Panel/Response");
            WorldTimeController time = GetNode<WorldTimeController>("../WorldTimeController");
            object beforeTime = time.WorldTime;
            string beforeText = response.Text;
            Require(!panel.Visible && !settings.Visible && !dialogue.Visible, "Audio UI did not start hidden.");
            Require(AudioServer.BusCount == 6, "Main did not install all six buses.");
            Require(!entry.GetGlobalRect().Intersects(GetNode<Control>("../CommandOverlay/Panel").GetGlobalRect()),
                "Audio entry obscured the compact HUD.");
            visibility._UnhandledInput(new InputEventAction { Action = "optional_overlays_toggle", Pressed = true });
            Require(entry.IsVisibleInTree(), "Overlay toggle did not reveal Audio entry.");
            entry.EmitSignal(BaseButton.SignalName.Pressed);
            Require(panel.Visible && !settings.Visible && !dialogue.Visible, "Audio panel did not replace temporary overlays.");
            Require(GetViewport().GuiGetFocusOwner()?.Name == new StringName("MasterGain"), "Missing initial focus.");
            foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900) })
            {
                GetWindow().Size = size;
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Control surface = panel.GetNode<Control>("Shade/Panel");
                Require(GetViewport().GetVisibleRect().Encloses(surface.GetGlobalRect()), "Audio panel exceeded viewport.");
            }
            audio.SetPreferences(new AudioPreferences(audio.Preferences.WithChannel(AudioCategory.Master, 0, true).Channels,
                AudioMixPreset.ReducedRange, true, false));
            Require(ReferenceEquals(time.WorldTime, beforeTime) && response.Text == beforeText,
                "Audio preferences changed world time or dialogue text.");
            panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Require(!panel.Visible && settings.Visible && dialogue.Visible && entry.HasFocus(), "Close did not restore entry focus.");
            entry.EmitSignal(BaseButton.SignalName.Pressed);
            panel._Input(new InputEventAction { Action = "optional_overlays_toggle", Pressed = true });
            Require(!panel.Visible && !settings.Visible && !dialogue.Visible, "Tab did not hide all optional surfaces.");
            Require(GetViewport().GuiGetFocusOwner() is null, "Hidden controls retained focus.");
            audio.RestoreDefaults();
            string preview = OS.GetEnvironment("ALLRPCS_AUDIO_PREVIEW_PATH");
            if (!string.IsNullOrEmpty(preview))
            {
                visibility._UnhandledInput(new InputEventAction { Action = "optional_overlays_toggle", Pressed = true });
                entry.EmitSignal(BaseButton.SignalName.Pressed);
                GetWindow().Size = new Vector2I(1280, 720);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                Require(GetViewport().GetTexture().GetImage().SavePng(preview) == Error.Ok, "Cannot save audio panel preview.");
            }
            GD.Print("Audio Main smoke passed: six buses, hidden entry/panel, base/expanded fit, focus return, Tab/Escape, unchanged text/world time.");
            GetTree().Quit();
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            GetTree().Quit(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException(message); }
    }
}
