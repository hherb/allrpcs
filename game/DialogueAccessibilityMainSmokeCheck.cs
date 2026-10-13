using System.Text.Json;
using Allrpcs.Application.Input;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in Main keyboard fixture; property and focus evidence never claims reader output.</summary>
public partial class DialogueAccessibilityMainSmokeCheck : Node
{
    private static readonly string[] MetadataNodes = ["Input", "Whispering", "Submit", "Close", "Status", "Response"];
    private static readonly string[] InputLabels = ["../Prompt"];
    private static readonly string[] InputDescriptions = ["../Prompt", "../Status"];
    private static readonly string[] NamedControls = ["Whispering", "Submit", "Close"];
    private readonly List<string> checks = [];
    private readonly List<string> focusTrace = [];
    private ControlSettingsController controls = null!;
    private CanvasLayer settings = null!;
    private CanvasLayer dialogue = null!;
    private LineEdit input = null!;
    private CheckButton whisper = null!;
    private Label status = null!;
    private DialogueInputController submission = null!;
    private string mode = "headless";
    private Key next = Key.Tab;
    private Key previous = Key.Tab;
    private Key cancel = Key.Escape;
    private bool reverseShift = true;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_DIALOGUE_ACCESSIBILITY_CHECK") != "1") { return; }
        await Frames(4);
        try
        {
            mode = OS.GetEnvironment("ALLRPCS_DIALOGUE_ACCESSIBILITY_MODE");
            if (string.IsNullOrEmpty(mode)) { mode = "headless"; }
            bool native = mode.StartsWith("native", StringComparison.Ordinal);
            if (native)
            {
                Engine.MaxFps = 60;
                GetWindow().Size = mode == "native-1920" ? new(1920, 1080) : new(1280, 720);
                GetWindow().GrabFocus(); await Frames(10);
            }
            settings = GetNode<CanvasLayer>("../SettingsOverlay");
            dialogue = GetNode<CanvasLayer>("../DialogueOverlay");
            input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
            whisper = GetNode<CheckButton>("../DialogueOverlay/Panel/Whispering");
            status = GetNode<Label>("../DialogueOverlay/Panel/Status");
            submission = GetNode<DialogueInputController>("../DialogueInputController");
            controls = GetNode<ControlSettingsController>("../ControlSettingsController");
            Require(!settings.Visible && !dialogue.Visible, "Initial visibility");
            await Configuration(false);
            await Configuration(true);
            await PointerSubmission();
            Require(controls.Apply(ControlPreferences.Defaults()), "Restore defaults");
            Require(GetViewport().GuiGetFocusOwner() is null && !settings.Visible && !dialogue.Visible,
                "Final hidden views retained focus");
            CheckMetadata(); checks.Add("assistive-metadata");
            string output = OS.GetEnvironment("ALLRPCS_DIALOGUE_ACCESSIBILITY_REPORT");
            Require(!string.IsNullOrEmpty(output), "Report path missing");
            File.WriteAllText(output, JsonSerializer.Serialize(new
            {
                status = "passed",
                mode,
                checks,
                focusTrace,
                provenance = "synthetic keyboard events through actual Main; metadata inspection only; reader output not evaluated",
                engine = Engine.GetVersionInfo()["string"].AsString(),
                size = new[] { GetWindow().Size.X, GetWindow().Size.Y },
                metadata = Metadata(),
                providersDisabled = OS.GetEnvironment("ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED") == "1",
            }));
            GD.Print("PASS: dialogue accessibility Main: " + mode); GetTree().Quit();
        }
        catch (Exception error) { GD.PrintErr("FAIL: dialogue accessibility Main: " + error); GetTree().Quit(1); }
    }

    private async Task Configuration(bool remapped)
    {
        string prefix = remapped ? "remapped" : "default";
        next = remapped ? Key.F9 : Key.Tab;
        previous = remapped ? Key.F10 : Key.Tab;
        cancel = remapped ? Key.F11 : Key.Escape;
        reverseShift = !remapped;
        ControlPreferences preferences = ControlPreferences.Defaults();
        if (remapped)
        {
            preferences = ControlBindingEdit.Set(preferences, "ui_focus_next", new("key", (int)Key.F9, "logical"), KeyboardSlot(preferences, "ui_focus_next"));
            preferences = ControlBindingEdit.Set(preferences, "ui_focus_prev", new("key", (int)Key.F10, "logical"), KeyboardSlot(preferences, "ui_focus_prev"));
            preferences = ControlBindingEdit.Set(preferences, "ui_cancel", new("key", (int)Key.F11, "logical"), KeyboardSlot(preferences, "ui_cancel"));
        }
        Require(controls.Apply(preferences), "Apply UI configuration");
        await Press(Key.Tab); Focus("ControlsEntry");
        Require(settings.Visible && !dialogue.Visible, "Settings entry view");
        if (!remapped) { await Capture("settings"); }
        string[] entries = ["AudioEntry", "DialogueEntry", "SettingsClose", "ControlsEntry"];
        foreach (string name in entries) { await Press(next); Focus(name); }
        foreach (string name in new[] { "SettingsClose", "DialogueEntry", "AudioEntry", "ControlsEntry" })
        { await Press(previous, reverseShift); Focus(name); }
        checks.Add(prefix + ":settings-cycle");
        // Audio and Controls retain their own exact return entry, without exposing dialogue.
        await Press(Key.Enter);
        Require(GetNode<ControlSettingsPanel>("../ControlSettingsPanel").Visible, "Controls keyboard entry");
        await Press(cancel); Focus("ControlsEntry");
        Require(settings.Visible && !dialogue.Visible, "Controls return sibling visibility");
        await Press(next); Focus("AudioEntry"); await Press(Key.Enter);
        Require(GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Visible, "Audio keyboard entry");
        await Press(cancel); Focus("AudioEntry");
        Require(settings.Visible && !dialogue.Visible, "Audio return sibling visibility");
        checks.Add(prefix + ":sibling-return");
        await Press(next); Focus("DialogueEntry"); await Press(Key.Enter); Focus("Input");
        Require(dialogue.Visible && !settings.Visible && !GetNode<ControlSettingsPanel>("../ControlSettingsPanel").Visible
            && !GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Visible, "Dialogue hidden siblings");
        if (!remapped) { await Capture("dialogue-input"); }
        foreach (string name in new[] { "Whispering", "Submit", "Close", "Input" })
        { await Press(next); Focus(name); }
        foreach (string name in new[] { "Close", "Submit", "Whispering", "Input" })
        { await Press(previous, reverseShift); Focus(name); }
        checks.Add(prefix + ":dialogue-cycle");
        await WorldGate(); checks.Add(prefix + ":world-gate");
        // Use actual keyboard text/Enter routing, then a keyboard-activated Submit button.
        whisper.ButtonPressed = false;
        await TypeText("Fixture ordinary"); await Press(Key.Enter);
        DialogueDelivery? ordinary = submission.LastSubmittedDelivery;
        CheckDelivery(ordinary, "Fixture ordinary", DialogueDeliveryMode.OrdinarySpeech);
        Require(input.Text.Length == 0 && status.Text == TemporaryDialogueInputPanel.AcceptedStatusText,
            "Accepted ordinary did not clear");
        await TypeText("  "); await Press(Key.Enter);
        Require(input.Text == "  " && status.Text == "Dialogue submission: REJECTED"
            && ReferenceEquals(submission.LastSubmittedDelivery, ordinary), "Rejected input retained/owner: length=" + input.Text.Length + "; status=" + status.Text
            + "; delivery retained=" + ReferenceEquals(submission.LastSubmittedDelivery, ordinary));
        input.Clear(); await TypeText("Fixture whisper");
        await Press(next); Focus("Whispering"); await Press(Key.Enter);
        Require(whisper.ButtonPressed, "Keyboard whisper mode");
        await Press(next); Focus("Submit"); await Press(Key.Enter);
        CheckDelivery(submission.LastSubmittedDelivery, "Fixture whisper", DialogueDeliveryMode.Whispering);
        Require(!ReferenceEquals(ordinary, submission.LastSubmittedDelivery) && input.Text.Length == 0
            && status.Text == TemporaryDialogueInputPanel.AcceptedStatusText, "Accepted whisper did not clear");
        checks.Add(prefix + ":submission-semantics");
        await Press(next); Focus("Close");
        if (!remapped) { await Capture("dialogue-close"); }
        await Press(Key.Enter); Focus("DialogueEntry");
        Require(settings.Visible && !dialogue.Visible && !input.IsVisibleInTree(), "Visible close return");
        await Press(Key.Enter); Focus("Input"); await Press(cancel); Focus("DialogueEntry");
        // A non-navigation toggle still closes settings even while an entry owns focus.
        if (remapped)
        {
            Require(ControlSettingsController.Same(controls.Preferences, preferences), "Preferences changed during UI");
            Require(!ControlInputMatcher.Pressed(KeyEvent(Key.Tab, true), "ui_focus_next"), "Tab still matches remapped next: events=" + string.Join("/", InputMap.ActionGetEvents("ui_focus_next").Select(e => e.AsText())));
            Require(ControlInputMatcher.Pressed(KeyEvent(Key.Tab, true), "optional_overlays_toggle"), "Tab lost toggle");
        }
        await Press(Key.Tab);
        if (!remapped) { Focus("SettingsClose"); await Press(Key.Enter); }
        Require(!settings.Visible && !dialogue.Visible && GetViewport().GuiGetFocusOwner() is null, "Settings close release: config=" + prefix + "; settings=" + settings.Visible
            + "; dialogue=" + dialogue.Visible + "; focus=" + GetViewport().GuiGetFocusOwner()?.Name);
        await Press(Key.Tab); Focus("ControlsEntry"); await Press(cancel);
        Require(!settings.Visible && GetViewport().GuiGetFocusOwner() is null, "Settings cancel release");
        checks.Add(prefix + ":close-reopen");
    }

    private async Task PointerSubmission()
    {
        ControlPreferences preferences = ControlBindingEdit.Set(controls.Preferences, "ui_cancel",
            new("mouseButton", 1), KeyboardSlot(controls.Preferences, "ui_cancel"));
        Require(controls.Apply(preferences), "Pointer cancel configuration");
        await Press(Key.Tab);
        await Click(GetNode<Button>("../SettingsOverlay/DialogueEntry"));
        Require(dialogue.Visible && !settings.Visible && input.HasFocus(), "Pointer Dialogue entry stolen by UI cancel");
        whisper.ButtonPressed = false;
        await TypeText("Fixture pointer");
        await Click(GetNode<Button>("../DialogueOverlay/Panel/Submit"));
        CheckDelivery(submission.LastSubmittedDelivery, "Fixture pointer", DialogueDeliveryMode.OrdinarySpeech);
        Require(input.Text.Length == 0 && status.Text == TemporaryDialogueInputPanel.AcceptedStatusText && dialogue.Visible,
            "Pointer Submit stolen by UI cancel");
        await Click(GetNode<Button>("../DialogueOverlay/Panel/Close"));
        Require(settings.Visible && !dialogue.Visible && GetNode<Button>("../SettingsOverlay/DialogueEntry").HasFocus(),
            "Pointer Close return");
        await Click(GetNode<Button>("../SettingsOverlay/SettingsClose"));
        Require(!settings.Visible && GetViewport().GuiGetFocusOwner() is null, "Pointer settings close");
        checks.Add("pointer-submit-and-close");
    }

    private async Task Click(Control target)
    {
        Vector2 point = GetViewport().GetFinalTransform() * target.GetGlobalRect().GetCenter();
        Input.ParseInputEvent(new InputEventMouseMotion { Position = point, GlobalPosition = point }); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton
        {
            Position = point,
            GlobalPosition = point,
            ButtonIndex = MouseButton.Left,
            Pressed = true
        }); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton
        {
            Position = point,
            GlobalPosition = point,
            ButtonIndex = MouseButton.Left,
            Pressed = false
        }); await Frames(2);
    }

    private static int KeyboardSlot(ControlPreferences preferences, string action)
        => Array.FindIndex(preferences.Get(action).Bindings.ToArray(), binding => binding.Kind == "key");

    private async Task WorldGate()
    {
        Seeker seeker = GetNode<Seeker>("../Seeker");
        Healer healer = GetNode<Healer>("../Healer");
        Vector2 position = seeker.Position;
        var command = healer.CommandMode;
        Require(!ControlInputContext.WorldAllowed(this), "GUI did not block world");
        await Press(Key.X);
        Input.ParseInputEvent(KeyEvent(Key.D, true)); await PhysicsFrames(5);
        Input.ParseInputEvent(KeyEvent(Key.D, false)); await PhysicsFrames(2);
        Require(seeker.Position.DistanceTo(position) < 0.01f && healer.CommandMode == command, "World command escaped dialogue");
        Require(!GetTree().Paused && GetNode<Node>("../WorldEventController").CanProcess(), "World processing paused");
        // Key D is normal text editing; discard this canned gate input before submission.
        input.Clear();
    }

    private void CheckDelivery(DialogueDelivery? delivery, string text, DialogueDeliveryMode deliveryMode)
    {
        Require(delivery is not null && ReferenceEquals(delivery.Mode, deliveryMode)
            && delivery.Utterance.Text == text && ReferenceEquals(delivery.Utterance.SpeakerCharacterId,
                GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id),
            "Exact speaker/delivery/text submission");
    }

    private void CheckMetadata()
    {
        Require(input.AccessibilityName == "Dialogue as Seeker" && input.AccessibilityDescription.Length > 0
            && input.AccessibilityLabeledByNodes.Select(p => p.ToString()).SequenceEqual(InputLabels)
            && input.AccessibilityDescribedByNodes.Select(p => p.ToString()).SequenceEqual(InputDescriptions),
            "Input semantics and relationships");
        foreach (string name in NamedControls)
        {
            Control node = GetNode<Control>("../DialogueOverlay/Panel/" + name);
            Require(node.AccessibilityName.Length > 0 && node.AccessibilityDescription.Length > 0, "Missing name/description");
        }
        Require(status.AccessibilityLive == DisplayServer.AccessibilityLiveMode.Polite
            && GetNode<Label>("../DialogueOverlay/Panel/Response").AccessibilityName == "Healer final response",
            "Status/response semantics");
        foreach (string name in new[] { "ModelStatus", "ModelStreamText" })
        { Require(GetNode<Control>("../DialogueOverlay/Panel/" + name).AccessibilityName.Length == 0, "Partial content copied to metadata"); }
    }

    private object[] Metadata() => MetadataNodes
        .Select(name =>
        {
            Control node = GetNode<Control>("../DialogueOverlay/Panel/" + name);
            return (object)new
            {
                node = name,
                name = node.AccessibilityName,
                description = node.AccessibilityDescription,
                live = node.AccessibilityLive.ToString(),
                labeledBy = node.AccessibilityLabeledByNodes.Select(p => p.ToString()).ToArray(),
                describedBy = node.AccessibilityDescribedByNodes.Select(p => p.ToString()).ToArray(),
                visible = node.IsVisibleInTree()
            };
        }).ToArray();

    private void Focus(string name)
    {
        Require(GetViewport().GuiGetFocusOwner()?.Name.ToString() == name, "Expected focus " + name + "; actual "
            + GetViewport().GuiGetFocusOwner()?.Name);
        focusTrace.Add(name);
    }
    private async Task TypeText(string text)
    {
        foreach (char letter in text)
        {
            Input.ParseInputEvent(new InputEventKey { Pressed = true, Unicode = letter }); await Frames(1);
            Input.ParseInputEvent(new InputEventKey { Pressed = false, Unicode = letter });
        }
        await Frames(1);
    }
    private async Task Press(Key key, bool shift = false)
    {
        Input.ParseInputEvent(KeyEvent(key, true, shift)); await Frames(2);
        Input.ParseInputEvent(KeyEvent(key, false, shift)); await Frames(2);
    }
    private static InputEventKey KeyEvent(Key key, bool pressed, bool shift = false)
        => new() { Keycode = key, PhysicalKeycode = key, Pressed = pressed, ShiftPressed = shift };
    private async Task Capture(string state)
    {
        if (!mode.StartsWith("native", StringComparison.Ordinal)) { return; }
        await Frames(3); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        string directory = Path.GetDirectoryName(OS.GetEnvironment("ALLRPCS_DIALOGUE_ACCESSIBILITY_REPORT"))!;
        Require(GetViewport().GetTexture().GetImage().SavePng(Path.Combine(directory, mode + "-" + state + ".png")) == Error.Ok,
            "Native capture write");
    }
    private async Task Frames(int count)
    { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task PhysicsFrames(int count)
    { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); } }
    private static void Require(bool condition, string message)
    { if (!condition) { throw new InvalidOperationException(message); } }
}
