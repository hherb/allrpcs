namespace Allrpcs.Application.Movement;

/// <summary>Formats the healer command overlay's mode line.</summary>
public static class HealerModeStatusText
{
    /// <summary>
    /// Returns the mode line; a move issued for the herb-patch examination reads as that
    /// examination rather than as the pink-ring move.
    /// </summary>
    public static string Format(HealerCommandMode mode, bool examining) =>
        "Current healer mode: " + (examining && mode == HealerCommandMode.MoveToDestination
            ? "EXAMINE HERB PATCH"
            : mode switch
            {
                HealerCommandMode.Follow => "FOLLOW",
                HealerCommandMode.Wait => "WAIT",
                HealerCommandMode.MoveToDestination => "MOVE TO PINK RING",
                _ => "UNKNOWN",
            });
}
