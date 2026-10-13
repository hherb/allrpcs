namespace Allrpcs.Application.Input;

/// <summary>A finite action definition and its immutable authored defaults.</summary>
public sealed class ControlActionDefinition
{
    /// <summary>Stable action ID.</summary>
    public string Id { get; }
    /// <summary>Visible action label.</summary>
    public string Label { get; }
    /// <summary>Gameplay/UI conflict context.</summary>
    public string Context { get; }
    /// <summary>Whether signed axes and adjustable deadzones are permitted.</summary>
    public bool Direction { get; }
    /// <summary>Immutable default events.</summary>
    public IReadOnlyList<ControlBinding> Defaults { get; }
    internal ControlActionDefinition(string id, string label, string context, bool direction, ControlBinding[] defaults)
    { Id = id; Label = label; Context = context; Direction = direction; Defaults = Array.AsReadOnly(defaults); }
}

/// <summary>The approved 21 project/eight UI action catalog, with no new gameplay actions.</summary>
public static class ControlActionCatalog
{
    /// <summary>Maximum events on one action.</summary>
    public const int MaximumBindings = 8;
    /// <summary>Maximum events per device class on one action.</summary>
    public const int MaximumDeviceBindings = 4;
    /// <summary>Immutable stable order used by storage and UI.</summary>
    public static IReadOnlyList<ControlActionDefinition> Actions { get; } = Array.AsReadOnly<ControlActionDefinition>([
        new("move_left", "Move left", "gameplay", true, [new("key", 65, "physical"), new("joyButton", 13), new("joyAxis", 0, sign: -1)]),
        new("move_right", "Move right", "gameplay", true, [new("key", 68, "physical"), new("joyButton", 14), new("joyAxis", 0, sign: 1)]),
        new("move_up", "Move up", "gameplay", true, [new("key", 87, "physical"), new("joyButton", 11), new("joyAxis", 1, sign: -1)]),
        new("move_down", "Move down", "gameplay", true, [new("key", 83, "physical"), new("joyButton", 12), new("joyAxis", 1, sign: 1)]),
        new("open_seeker_customization", "Customize Seeker", "gameplay", false, [new("key", 85, "physical")]),
        new("journal_toggle", "Open/close journal", "gameplay", false, [new("key", 74, "physical")]),
        new("map_toggle", "Open/close map", "gameplay", false, [new("key", 77, "physical")]),
        new("optional_overlays_toggle", "Show/hide settings and dialogue", "gameplay", false, [new("key", 4194306, "physical"), new("joyButton", 6)]),
        new("save_current", "Save current game", "gameplay", false, [new("key", 81, "physical")]),
        new("load_current", "Load current game", "gameplay", false, [new("key", 76, "physical")]),
        new("healer_toggle_follow_wait", "Healer follow/wait", "gameplay", false, [new("key", 88, "physical")]),
        new("healer_move", "Move Healer to marker", "gameplay", false, [new("key", 72, "physical")]),
        new("focused_search_start", "Start focused search", "gameplay", false, [new("key", 70, "physical")]),
        new("focused_search_cancel", "Cancel focused search", "gameplay", false, [new("key", 67, "physical")]),
        new("focused_search_complete", "Complete focused search", "gameplay", false, [new("key", 69, "physical")]),
        new("focused_search_resolve", "Resolve focused search", "gameplay", false, [new("key", 82, "physical")]),
        new("layered_inscription_discover", "Discover shrine inscription", "gameplay", false, [new("key", 73, "physical")]),
        new("remedy_prepare", "Advance remedy preparation", "gameplay", false, [new("key", 80, "physical")]),
        new("remedy_administer", "Administer remedy", "gameplay", false, [new("key", 84, "physical")]),
        new("sage_consult", "Consult Sage", "gameplay", false, [new("key", 75, "physical")]),
        new("character_turn_cancel", "Cancel character turn", "gameplay", false, [new("key", 86, "physical")]),
        new("ui_accept", "Accept", "ui", false, [new("key", 4194309, "logical", shift: false), new("key", 4194310, "logical", shift: false), new("key", 32, "logical", shift: false), new("joyButton", 0)]),
        new("ui_cancel", "Cancel", "ui", false, [new("key", 4194305, "logical", shift: false), new("joyButton", 1)]),
        new("ui_left", "Left", "ui", true, [new("key", 4194319, "logical", shift: false), new("joyButton", 13), new("joyAxis", 0, sign: -1)]),
        new("ui_right", "Right", "ui", true, [new("key", 4194321, "logical", shift: false), new("joyButton", 14), new("joyAxis", 0, sign: 1)]),
        new("ui_up", "Up", "ui", true, [new("key", 4194320, "logical", shift: false), new("joyButton", 11), new("joyAxis", 1, sign: -1)]),
        new("ui_down", "Down", "ui", true, [new("key", 4194322, "logical", shift: false), new("joyButton", 12), new("joyAxis", 1, sign: 1)]),
        new("ui_focus_next", "Focus Next", "ui", false, [new("key", 4194306, "logical", shift: false), new("joyButton", 10)]),
        new("ui_focus_prev", "Focus Prev", "ui", false, [new("key", 4194306, "logical", shift: true), new("joyButton", 9)]),
    ]);
    /// <summary>Finds one exact known action, rejecting aliases and unknown actions.</summary>
    public static ControlActionDefinition Get(string id) => Actions.FirstOrDefault(a => a.Id == id)
        ?? throw new ArgumentException("Unknown control action.", nameof(id));
}
