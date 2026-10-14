using Allrpcs.Application.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>Tap Space preserves native input; a held Space owns voice independently of GUI focus.</summary>
public partial class SpaceHoldSpeechController : Node
{
    private const int MaximumQueuedEvents = 64;
    private static readonly StringName ReplayTag = "allrpcs_space_hold_replay";
    private sealed record QueuedInput(InputEvent Event, Viewport Target);
    private readonly TapHoldGesture gesture = new();
    private readonly List<QueuedInput> pending = [];
    private readonly List<QueuedInput> committed = [];
    private readonly HashSet<Window> routed = [];
    private PlayerSpeechController mic = null!;
    private CanvasLayer dialogue = null!;
    private TemporaryDialogueInputPanel panel = null!;
    private Button talk = null!, entry = null!;
    private Control? pressFocus;
    private Viewport? pressViewport;
    private ulong stopVersion;
    private int originalLayer;
    private bool raised, replaying, replayScheduled, ownsHold, focused = true;
    internal int Holds { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        mic = GetNode<PlayerSpeechController>("../PlayerSpeechController");
        dialogue = GetNode<CanvasLayer>("../DialogueOverlay"); originalLayer = dialogue.Layer;
        panel = GetNode<TemporaryDialogueInputPanel>("../DialogueOverlay/Panel");
        talk = (Button)panel.FindChild("HoldToTalk", true, false);
        entry = GetNode<Button>("../SettingsOverlay/DialogueEntry");
        talk.Text = "Hold to talk · hold Space";
        talk.AccessibilityDescription = "Hold Space for 300 milliseconds from any game control to speak. Tap Space for normal typing or activation. Release to finalize; Submit sends the draft.";
        panel.Closed += RestoreLayer;
        stopVersion = mic.StopVersion;
        RegisterDescendants(GetTree().Root);
        GetTree().NodeAdded += OnNodeAdded;
    }

    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    { Handle(@event, GetViewport()); }
    internal void Handle(InputEvent value, Viewport viewport)
    {
        if (replaying || !focused || value.HasMeta(ReplayTag)) { return; }
        if (value is InputEventKey key && (key.PhysicalKeycode == Key.Space || key.Keycode == Key.Space))
        {
            if (!gesture.Active && (key.CtrlPressed || key.AltPressed || key.MetaPressed || key.ShiftPressed)) { return; }
            ControlInputContext.Current?.Observe(value);
            if (key.Pressed)
            {
                if (!key.Echo && !gesture.Active)
                {
                    gesture.Press(Time.GetTicksMsec()); pressViewport = viewport; pressFocus = viewport.GuiGetFocusOwner();
                    stopVersion = mic.StopVersion; Queue(pending, value, viewport);
                }
            }
            else if (gesture.Active)
            {
                bool wasPending = gesture.Pending;
                TapHoldEffect effect = gesture.Release(Time.GetTicksMsec());
                if (effect.HasFlag(TapHoldEffect.Tap))
                {
                    Queue(pending, value, viewport);
                    bool sameFocus = pressViewport == viewport && pressFocus == viewport.GuiGetFocusOwner();
                    Commit(sameFocus && pressFocus != talk);
                }
                else
                {
                    if (wasPending) { BeginHold(); }
                    if (effect.HasFlag(TapHoldEffect.EndVoice) && ownsHold && stopVersion == mic.StopVersion) { mic.FinishSpaceHold(); }
                    ownsHold = false;
                    Commit(false);
                }
            }
            else { return; }
            viewport.SetInputAsHandled(); return;
        }
        // Keep overlapping keystrokes/pointer events in their original order until Space's intent is known.
        if (gesture.Pending)
        {
            ControlInputContext.Current?.Observe(value); Queue(pending, value, viewport);
            viewport.SetInputAsHandled();
        }
        else if (committed.Count > 0)
        {
            ControlInputContext.Current?.Observe(value); Queue(committed, value, viewport);
            viewport.SetInputAsHandled();
        }
    }

    private void Queue(List<QueuedInput> target, InputEvent value, Viewport viewport)
    {
        if (value is InputEventMouseMotion && target.LastOrDefault() is { Event: InputEventMouseMotion } last && last.Target == viewport)
        { target[^1] = new((InputEvent)value.Duplicate(), viewport); return; }
        if (target.Count >= MaximumQueuedEvents)
        { gesture.Cancel(); pending.Clear(); committed.Clear(); mic.StopCapture("space-event-limit"); return; }
        target.Add(new((InputEvent)value.Duplicate(), viewport));
    }
    private static bool Space(InputEvent value) => value is InputEventKey key
        && (key.PhysicalKeycode == Key.Space || key.Keycode == Key.Space);
    private void Commit(bool includeSpace)
    {
        QueuedInput[] events = pending.ToArray(); pending.Clear();
        foreach (QueuedInput value in events) { if (includeSpace || !Space(value.Event)) { Queue(committed, value.Event, value.Target); } }
        if (committed.Count == 0 || replayScheduled) { return; }
        replayScheduled = true; Callable.From(Replay).CallDeferred();
    }
    private void Replay()
    {
        replayScheduled = false;
        QueuedInput[] events = committed.ToArray(); committed.Clear();
        if (!focused || stopVersion != mic.StopVersion) { gesture.Cancel(); return; }
        replaying = true;
        try
        {
            foreach (QueuedInput value in events)
            {
                if (!GodotObject.IsInstanceValid(value.Target) || !value.Target.IsInsideTree()) { continue; }
                value.Event.SetMeta(ReplayTag, true); value.Target.PushInput(value.Event, true);
            }
        }
        finally { replaying = false; }
    }
    private void BeginHold()
    {
        Commit(false); Replay();
        if (stopVersion != mic.StopVersion) { gesture.Cancel(); stopVersion = mic.StopVersion; return; }
        foreach (Window window in routed.Where(window => GodotObject.IsInstanceValid(window) && window.Visible && window.HasFocus()).ToArray()) { window.Hide(); }
        GetWindow().GrabFocus();
        if (!dialogue.Visible) { entry.EmitSignal(BaseButton.SignalName.Pressed); }
        // Existing reading views keep their draft/state; only the speech review is brought forward.
        dialogue.Layer = Math.Max(originalLayer, GetParent().GetChildren().OfType<CanvasLayer>()
            .Where(layer => layer != dialogue && layer.Visible).Select(layer => layer.Layer + 1).DefaultIfEmpty(originalLayer).Max());
        raised = true; mic.BeginSpaceHold(); stopVersion = mic.StopVersion; ownsHold = true; Holds++;
    }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stopVersion != mic.StopVersion)
        { gesture.Cancel(); pending.Clear(); committed.Clear(); ownsHold = false; stopVersion = mic.StopVersion; }
        if (focused && gesture.Tick(Time.GetTicksMsec()).HasFlag(TapHoldEffect.BeginVoice)) { BeginHold(); }
        if (raised && !dialogue.Visible) { RestoreLayer(); }
    }
    private void RestoreLayer()
    { if (GodotObject.IsInstanceValid(dialogue)) { dialogue.Layer = originalLayer; } raised = false; }
    private void RegisterDescendants(Node node)
    {
        if (node is Window window && window != GetWindow()) { Register(window); }
        foreach (Node child in node.GetChildren(true)) { RegisterDescendants(child); }
    }
    private void OnNodeAdded(Node node)
    { if (node is Window window) { Callable.From(() => Register(window)).CallDeferred(); } }
    private void Register(Window window)
    {
        if (!GodotObject.IsInstanceValid(window) || !window.IsInsideTree() || window == GetWindow() || !routed.Add(window)) { return; }
        window.AddChild(new SpaceHoldInputRouter { OwnerController = this });
    }
    internal bool AnyGameWindowFocused() => GetWindow().HasFocus()
        || routed.Any(window => GodotObject.IsInstanceValid(window) && window.Visible && window.HasFocus());
    private void CheckWindowFocus()
    {
        if (!IsInsideTree() || AnyGameWindowFocused()) { return; }
        focused = false; gesture.Cancel(); pending.Clear(); committed.Clear(); ownsHold = false;
    }
    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (what == (int)NotificationApplicationFocusOut)
        { focused = false; gesture.Cancel(); pending.Clear(); committed.Clear(); ownsHold = false; }
        if (what == (int)NotificationWMWindowFocusOut) { Callable.From(CheckWindowFocus).CallDeferred(); }
        if (what is (int)NotificationApplicationFocusIn or (int)NotificationWMWindowFocusIn)
        { focused = true; if (!Input.IsPhysicalKeyPressed(Key.Space)) { gesture.Release(Time.GetTicksMsec()); } }
    }
    /// <inheritdoc />
    public override void _ExitTree()
    { GetTree().NodeAdded -= OnNodeAdded; panel.Closed -= RestoreLayer; gesture.Cancel(); pending.Clear(); committed.Clear(); routed.Clear(); RestoreLayer(); }
}
