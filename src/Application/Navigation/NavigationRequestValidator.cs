using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>
/// Describes the finite rectangular region in which navigation may be requested.
/// </summary>
/// <param name="Left">The left world edge in pixels.</param>
/// <param name="Top">The top world edge in pixels.</param>
/// <param name="Right">The right world edge in pixels.</param>
/// <param name="Bottom">The bottom world edge in pixels.</param>
public readonly record struct NavigationWorldBounds(float Left, float Top, float Right, float Bottom);

/// <summary>
/// Holds a navigation request whose coordinates and world bounds have been validated.
/// </summary>
/// <param name="WorldBounds">The world region that contains both requested points.</param>
/// <param name="Origin">The requested starting position.</param>
/// <param name="Destination">The requested target position.</param>
public readonly record struct NavigationRequest(
    NavigationWorldBounds WorldBounds,
    Vector2 Origin,
    Vector2 Destination);

/// <summary>
/// Validates navigation requests without depending on the presentation engine.
/// </summary>
public static class NavigationRequestValidator
{
    /// <summary>
    /// Creates a navigation request with finite coordinates contained by finite ordered bounds.
    /// </summary>
    /// <param name="worldBounds">The finite world rectangle containing valid requests.</param>
    /// <param name="origin">The requested starting position.</param>
    /// <param name="destination">The requested target position.</param>
    /// <returns>A validated immutable navigation request.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the world bounds are invalid or either requested point is non-finite or outside
    /// the world bounds.
    /// </exception>
    public static NavigationRequest Create(
        NavigationWorldBounds worldBounds,
        Vector2 origin,
        Vector2 destination)
    {
        ValidateWorldBounds(worldBounds);
        ValidatePoint(origin, worldBounds, nameof(origin));
        ValidatePoint(destination, worldBounds, nameof(destination));

        return new NavigationRequest(worldBounds, origin, destination);
    }

    private static void ValidateWorldBounds(NavigationWorldBounds worldBounds)
    {
        if (!float.IsFinite(worldBounds.Left)
            || !float.IsFinite(worldBounds.Top)
            || !float.IsFinite(worldBounds.Right)
            || !float.IsFinite(worldBounds.Bottom)
            || worldBounds.Right <= worldBounds.Left
            || worldBounds.Bottom <= worldBounds.Top)
        {
            throw new ArgumentOutOfRangeException(
                nameof(worldBounds),
                worldBounds,
                "Navigation world bounds must be finite with positive width and height.");
        }
    }

    private static void ValidatePoint(
        Vector2 point,
        NavigationWorldBounds worldBounds,
        string parameterName)
    {
        if (!float.IsFinite(point.X)
            || !float.IsFinite(point.Y)
            || point.X < worldBounds.Left
            || point.X > worldBounds.Right
            || point.Y < worldBounds.Top
            || point.Y > worldBounds.Bottom)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                point,
                "Navigation coordinates must be finite and inside the world bounds.");
        }
    }
}
