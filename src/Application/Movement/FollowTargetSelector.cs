using System.Numerics;

namespace Allrpcs.Application.Movement;

/// <summary>
/// Describes whether a follower should move toward a validated navigation destination.
/// </summary>
/// <param name="NavigationDestination">
/// The followed position when movement is required, or <see langword="null"/> when stopped.
/// </param>
/// <param name="SpeedPixelsPerSecond">The validated positive follow speed.</param>
/// <param name="StopDistancePixels">The validated inclusive distance at which movement stops.</param>
public readonly record struct FollowTargetSelection(
    Vector2? NavigationDestination,
    float SpeedPixelsPerSecond,
    float StopDistancePixels)
{
    /// <summary>
    /// Gets a value indicating whether the follower is outside the stop distance and should move.
    /// </summary>
    public bool ShouldMove => NavigationDestination.HasValue;
}

/// <summary>
/// Selects a deterministic follow target without depending on the presentation engine.
/// </summary>
public static class FollowTargetSelector
{
    /// <summary>
    /// Selects the followed position only when the follower is outside the inclusive stop distance.
    /// </summary>
    /// <param name="followerPosition">The current follower position in world pixels.</param>
    /// <param name="followedPosition">The current followed-character position in world pixels.</param>
    /// <param name="speedPixelsPerSecond">The positive maximum follow speed.</param>
    /// <param name="stopDistancePixels">The finite non-negative inclusive stop distance.</param>
    /// <returns>An immutable validated target selection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when a position is non-finite, speed is not finite and positive, or the stop distance
    /// is not finite and non-negative.
    /// </exception>
    public static FollowTargetSelection Select(
        Vector2 followerPosition,
        Vector2 followedPosition,
        float speedPixelsPerSecond,
        float stopDistancePixels)
    {
        ValidatePosition(followerPosition, nameof(followerPosition));
        ValidatePosition(followedPosition, nameof(followedPosition));

        if (!float.IsFinite(speedPixelsPerSecond) || speedPixelsPerSecond <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(speedPixelsPerSecond),
                speedPixelsPerSecond,
                "Follow speed must be finite and positive.");
        }

        if (!float.IsFinite(stopDistancePixels) || stopDistancePixels < 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stopDistancePixels),
                stopDistancePixels,
                "Follow stop distance must be finite and non-negative.");
        }

        double horizontalDistance = (double)followedPosition.X - followerPosition.X;
        double verticalDistance = (double)followedPosition.Y - followerPosition.Y;
        double distanceSquared = (horizontalDistance * horizontalDistance)
            + (verticalDistance * verticalDistance);
        double stopDistanceSquared = (double)stopDistancePixels * stopDistancePixels;
        Vector2? destination = distanceSquared > stopDistanceSquared
            ? followedPosition
            : null;

        return new FollowTargetSelection(
            destination,
            speedPixelsPerSecond,
            stopDistancePixels);
    }

    private static void ValidatePosition(Vector2 position, string parameterName)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                position,
                "Follow positions must contain finite coordinates.");
        }
    }
}
