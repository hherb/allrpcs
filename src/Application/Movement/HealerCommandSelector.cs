using System.Numerics;
using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Movement;

/// <summary>
/// Identifies the deterministic movement mode selected for the healer.
/// </summary>
public enum HealerCommandMode
{
    /// <summary>Follow the currently configured character.</summary>
    Follow,

    /// <summary>Remain stationary.</summary>
    Wait,

    /// <summary>Move to one validated world destination, then wait.</summary>
    MoveToDestination,
}

/// <summary>
/// Holds one validated deterministic healer command.
/// </summary>
public readonly record struct HealerCommandSelection
{
    internal HealerCommandSelection(
        HealerCommandMode mode,
        Vector2? navigationDestination)
    {
        Mode = mode;
        NavigationDestination = navigationDestination;
    }

    /// <summary>Gets the selected deterministic movement mode.</summary>
    public HealerCommandMode Mode { get; }

    /// <summary>
    /// Gets the validated world destination for move mode, or <see langword="null"/> otherwise.
    /// </summary>
    public Vector2? NavigationDestination { get; }
}

/// <summary>
/// Selects validated follow, wait, and move-to-destination commands without a Godot dependency.
/// </summary>
public static class HealerCommandSelector
{
    /// <summary>Selects deterministic follow mode.</summary>
    /// <returns>A destination-free follow command.</returns>
    public static HealerCommandSelection SelectFollow()
    {
        return new HealerCommandSelection(HealerCommandMode.Follow, null);
    }

    /// <summary>Selects deterministic wait mode.</summary>
    /// <returns>A destination-free wait command.</returns>
    public static HealerCommandSelection SelectWait()
    {
        return new HealerCommandSelection(HealerCommandMode.Wait, null);
    }

    /// <summary>Selects a finite move destination inside the inclusive world bounds.</summary>
    /// <param name="worldBounds">The finite rectangular world region.</param>
    /// <param name="destination">The requested finite destination inside that region.</param>
    /// <returns>A validated move-to-destination command.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the bounds are invalid or the destination is non-finite or outside them.
    /// </exception>
    public static HealerCommandSelection SelectMoveToDestination(
        NavigationWorldBounds worldBounds,
        Vector2 destination)
    {
        _ = NavigationRequestValidator.Create(worldBounds, destination, destination);
        return new HealerCommandSelection(HealerCommandMode.MoveToDestination, destination);
    }
}
