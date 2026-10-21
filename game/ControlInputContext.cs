using Allrpcs.Application.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presentation input gates; never pauses world time, AI or task progress.</summary>
public partial class ControlInputContext : Node
{
    internal static ControlInputContext? Current { get; private set; }
    private readonly Dictionary<string, InputEvent> held = [];
    private bool focused = true;
    private bool fenced;
    private bool wasBlocked;
    /// <summary>World input is suppressed by capture/settings, GUI focus and existing reading modals.</summary>
    internal bool Blocked => !focused || fenced || Reading || GetViewport().GuiGetFocusOwner() is not null;
    private bool Reading => GetNodeOrNull<ControlSettingsPanel>("../ControlSettingsPanel")?.Visible == true
        || GetNodeOrNull<AudioSettingsPanel>("../AudioSettingsPanel")?.IsReading == true
        || GetNodeOrNull<CanvasLayer>("../SeekerMapPanel")?.Visible == true
        || GetNodeOrNull<CanvasLayer>("../JournalSectionPanel")?.Visible == true
        || GetNodeOrNull<Control>("../SeekerCustomizationOverlay/Panel")?.Visible == true
        || GetNodeOrNull<Control>("../SeekerCustomizationOverlay/StartPanel")?.Visible == true;
    /// <inheritdoc />
    public override void _Ready() { Current = this; }
    /// <summary>Standalone predecessor scenes without this new owner retain their exact routing.</summary>
    public static bool WorldAllowed(Node owner) => Current is null || !GodotObject.IsInstanceValid(Current)
        || !Current.IsInsideTree() || Current.GetTree() != owner.GetTree() || !Current.Blocked;
    /// <summary>Other views can toggle themselves unless the new Controls view owns navigation.</summary>
    public static bool TogglesAllowed(Node owner) => Current is null || Current.GetTree() != owner.GetTree()
        || Current.GetNodeOrNull<ControlSettingsPanel>("../ControlSettingsPanel")?.Visible != true;
    /// <summary>Releases only the approved gameplay action states, leaving text/GUI actions untouched.</summary>
    public static void ReleaseOwnedActions()
    { foreach (ControlActionDefinition action in ControlActionCatalog.Actions.Where(a => a.Context == "gameplay")) { Input.ActionRelease(action.Id); } }
    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        InputEvent value = @event;
        Observe(value);
        if (fenced && !Reading && GetViewport().GuiGetFocusOwner() is null) { GetViewport().SetInputAsHandled(); }
    }
    /// <summary>Also receives consumed capture events to retain a reliable release fence.</summary>
    internal void Observe(InputEvent value)
    {
        string? identity = value switch
        {
            InputEventKey key => "key:" + (int)(key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode),
            InputEventMouseButton mouse => "mouse:" + (int)mouse.ButtonIndex,
            InputEventJoypadButton button => $"joy:{button.Device}:{(int)button.ButtonIndex}",
            InputEventJoypadMotion axis => $"axis:{axis.Device}:{(int)axis.Axis}",
            InputEventAction action => "action:" + action.Action,
            _ => null,
        };
        if (identity is null) { return; }
        bool pressed = value is InputEventJoypadMotion motion ? Math.Abs(motion.AxisValue) > ControlBindingCapture.NeutralThreshold : value.IsPressed();
        if (pressed) { held[identity] = value; } else { held.Remove(identity); }
        if (fenced && held.Count == 0) { fenced = false; ReleaseOwnedActions(); }
    }
    /// <summary>After modal entry/exit or disconnection, held presses cannot resume the world.</summary>
    internal void Fence() { ReleaseOwnedActions(); fenced = held.Count > 0; }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (fenced && focused)
        {
            foreach (var pair in held.ToArray()) { if (!StillHeld(pair.Value)) { held.Remove(pair.Key); } }
            if (held.Count == 0) { fenced = false; ReleaseOwnedActions(); }
        }
        bool blocked = !focused || Reading || GetViewport().GuiGetFocusOwner() is not null;
        if (blocked != wasBlocked) { Fence(); wasBlocked = blocked; }
    }
    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (what is (int)NotificationApplicationFocusOut or (int)NotificationWMWindowFocusOut)
        { focused = false; Fence(); }
        if (what is (int)NotificationApplicationFocusIn or (int)NotificationWMWindowFocusIn) { focused = true; Fence(); }
    }
    /// <summary>Clears only pressed state for the lost controller, then releases gameplay actions.</summary>
    internal void Disconnect(int device)
    { foreach (string id in held.Keys.Where(id => id.StartsWith($"joy:{device}:", StringComparison.Ordinal) || id.StartsWith($"axis:{device}:", StringComparison.Ordinal)).ToArray()) { held.Remove(id); } Fence(); }
    private static bool StillHeld(InputEvent value) => value switch
    {
        InputEventKey key => (key.PhysicalKeycode != Key.None && Input.IsPhysicalKeyPressed(key.PhysicalKeycode))
            || (key.Keycode != Key.None && Input.IsKeyPressed(key.Keycode)),
        InputEventMouseButton mouse => Input.IsMouseButtonPressed(mouse.ButtonIndex),
        InputEventJoypadButton button => Input.IsJoyButtonPressed(button.Device, button.ButtonIndex),
        InputEventJoypadMotion axis => Math.Abs(Input.GetJoyAxis(axis.Device, axis.Axis)) > ControlBindingCapture.NeutralThreshold,
        InputEventAction action => Input.IsActionPressed(action.Action),
        _ => false,
    };
    /// <inheritdoc />
    public override void _ExitTree() { ReleaseOwnedActions(); if (ReferenceEquals(Current, this)) { Current = null; } }
}
