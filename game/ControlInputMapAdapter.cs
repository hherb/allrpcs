using Allrpcs.Application.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>Transactional per-action input replacement; unrelated engine actions remain exact.</summary>
public partial class ControlInputMapAdapter : Node
{
    /// <summary>Test-only injected installation failure, never enabled by a user document.</summary>
    internal int? FailAfterAction { get; set; }
    /// <summary>Fixture-only layout resolution; no user document or UI can choose it.</summary>
    internal Func<Key, Key>? LayoutResolverForCheck { get; set; }
    /// <summary>Builds every event and verifies aliases before replacing any live event set.</summary>
    public bool Apply(ControlPreferences preferences, out string status)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        Dictionary<string, InputEvent[]> events;
        try
        {
            ValidateLayoutAliases(preferences);
            events = preferences.Actions.ToDictionary(a => a.Id, a => a.Bindings.Select(ToEvent).ToArray(), StringComparer.Ordinal);
        }
        catch (ArgumentException error) { status = error.Message; return false; }
        var previous = preferences.Actions.ToDictionary(a => a.Id, a => (
            Exists: InputMap.HasAction(a.Id), Events: InputMap.ActionGetEvents(a.Id).ToArray(),
            Deadzone: InputMap.ActionGetDeadzone(a.Id)), StringComparer.Ordinal);
        try
        {
            ControlInputContext.ReleaseOwnedActions(); int index = 0;
            foreach (ControlActionSetting row in preferences.Actions)
            {
                if (!InputMap.HasAction(row.Id)) { InputMap.AddAction(row.Id); }
                InputMap.ActionEraseEvents(row.Id);
                foreach (InputEvent value in events[row.Id]) { InputMap.ActionAddEvent(row.Id, value); }
                InputMap.ActionSetDeadzone(row.Id, (float)row.Deadzone);
                if (FailAfterAction == ++index) { throw new InvalidOperationException("Injected controls installation failure"); }
            }
            status = "Controls applied · Save to keep after restart"; return true;
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException)
        {
            foreach (var row in previous)
            {
                if (!row.Value.Exists) { InputMap.EraseAction(row.Key); continue; }
                InputMap.ActionEraseEvents(row.Key);
                foreach (InputEvent value in row.Value.Events) { InputMap.ActionAddEvent(row.Key, value); }
                InputMap.ActionSetDeadzone(row.Key, row.Value.Deadzone);
            }
            status = "Controls could not be applied · Previous controls retained"; return false;
        }
    }
    /// <summary>Constructs a canonical single-code event, never a resource from a user document.</summary>
    internal static InputEvent ToEvent(ControlBinding binding)
    {
        return binding.Kind switch
        {
            "key" => new InputEventKey
            {
                Device = -1,
                Keycode = binding.CodeMode == "logical" ? (Key)binding.Code : Key.None,
                PhysicalKeycode = binding.CodeMode == "physical" ? (Key)binding.Code : Key.None,
                ShiftPressed = binding.Shift,
                CtrlPressed = binding.Ctrl,
                AltPressed = binding.Alt,
                MetaPressed = binding.Meta
            },
            "mouseButton" => new InputEventMouseButton { Device = -1, ButtonIndex = (MouseButton)binding.Code },
            "joyButton" => new InputEventJoypadButton { Device = -1, ButtonIndex = (JoyButton)binding.Code },
            "joyAxis" => new InputEventJoypadMotion { Device = -1, Axis = (JoyAxis)binding.Code, AxisValue = binding.Sign },
            _ => throw new ArgumentException("Unknown controls event."),
        };
    }
    /// <summary>Reads only supported primitive candidates; modifiers and pointer motion never become bindings.</summary>
    internal static ControlBinding? FromEvent(InputEvent value)
    {
        try
        {
            return value switch
            {
                InputEventKey key => new("key", (int)(key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode),
                    key.PhysicalKeycode != Key.None ? "physical" : "logical", shift: key.ShiftPressed, ctrl: key.CtrlPressed, alt: key.AltPressed, meta: key.MetaPressed),
                InputEventMouseButton mouse when !mouse.ShiftPressed && !mouse.CtrlPressed && !mouse.AltPressed && !mouse.MetaPressed => new("mouseButton", (int)mouse.ButtonIndex),
                InputEventJoypadButton button => new("joyButton", (int)button.ButtonIndex),
                InputEventJoypadMotion axis when float.IsFinite(axis.AxisValue) && axis.AxisValue != 0 => new("joyAxis", (int)axis.Axis, sign: Math.Sign(axis.AxisValue)),
                _ => null,
            };
        }
        catch (ArgumentException) { return null; }
    }
    /// <summary>Displays all bindings with truthful location/index labels.</summary>
    internal static string Describe(ControlBinding value)
    {
        if (value.Kind == "key")
        {
            InputEventKey key = (InputEventKey)ToEvent(value);
            return (value.CodeMode == "physical" ? key.AsTextPhysicalKeycode() + " (physical)" : key.AsTextKeycode());
        }
        return value.Kind == "mouseButton" ? "Mouse " + (value.Code switch { 1 => "left", 2 => "right", 3 => "middle", 8 => "side button 1", 9 => "side button 2", _ => "unsupported" }) : value.Kind == "joyButton"
            ? $"Controller button {value.Code} ({(JoyButton)value.Code})" : $"Controller axis {value.Code} {(value.Sign < 0 ? "−" : "+")}";
    }
    private void ValidateLayoutAliases(ControlPreferences preferences)
    {
        var keys = preferences.Actions.SelectMany(a => a.Bindings.Where(b => b.Kind == "key").Select(b => (Action: a, Binding: b))).ToArray();
        for (int i = 0; i < keys.Length; i++)
            for (int j = i + 1; j < keys.Length; j++)
            {
                var left = keys[i]; var right = keys[j];
                ControlActionDefinition ld = ControlActionCatalog.Get(left.Action.Id), rd = ControlActionCatalog.Get(right.Action.Id);
                if (ld.Context != rd.Context || left.Binding.CodeMode == right.Binding.CodeMode) { continue; }
                ControlBinding physical = left.Binding.CodeMode == "physical" ? left.Binding : right.Binding;
                ControlBinding logical = left.Binding.CodeMode == "logical" ? left.Binding : right.Binding;
                int resolved = (int)(LayoutResolverForCheck?.Invoke((Key)physical.Code) ?? DisplayServer.KeyboardGetKeycodeFromPhysical((Key)physical.Code));
                if (resolved > 0 && new ControlBinding("key", resolved, "logical", shift: physical.Shift, ctrl: physical.Ctrl,
                    alt: physical.Alt, meta: physical.Meta).ConflictsWith(logical, ld.Direction || rd.Direction))
                { throw new ArgumentException("Binding conflicts on this keyboard layout: " + ld.Label + " / " + rd.Label + "."); }
            }
    }
}
