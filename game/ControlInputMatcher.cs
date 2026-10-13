using Godot;

namespace Allrpcs.Game;

/// <summary>Exact discrete press matching with modifier-independent release clearing.</summary>
public static class ControlInputMatcher
{
    /// <summary>Accepts a new exact named-action press, never keyboard echo or modifier supersets.</summary>
    public static bool Pressed(InputEvent value, StringName action) => value.IsActionPressed(action, false, true);
    /// <summary>Matches releases even if keyboard modifiers were released before the key.</summary>
    public static bool Released(InputEvent value, StringName action) => value.IsActionReleased(action, false);
    /// <summary>Recognizes action press/release, preserving standalone synthetic named-action fixtures.</summary>
    public static bool Matches(InputEvent value, StringName action) => value.IsAction(action, value.IsPressed());
}
