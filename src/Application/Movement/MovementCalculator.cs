using System.Numerics;

namespace Allrpcs.Application.Movement;

/// <summary>
/// Converts a two-dimensional movement intent into a bounded world velocity.
/// </summary>
public static class MovementCalculator
{
    /// <summary>
    /// Calculates velocity without allowing diagonal input to exceed the configured speed.
    /// </summary>
    /// <param name="input">The desired direction and magnitude on each axis.</param>
    /// <param name="speedPixelsPerSecond">The maximum movement speed in pixels per second.</param>
    /// <returns>A velocity whose magnitude does not exceed <paramref name="speedPixelsPerSecond"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the input or speed contains a non-finite value, or when speed is negative.
    /// </exception>
    public static Vector2 CalculateVelocity(Vector2 input, float speedPixelsPerSecond)
    {
        if (!float.IsFinite(input.X) || !float.IsFinite(input.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(input), input, "Movement input must be finite.");
        }

        if (!float.IsFinite(speedPixelsPerSecond) || speedPixelsPerSecond < 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(speedPixelsPerSecond),
                speedPixelsPerSecond,
                "Movement speed must be finite and non-negative.");
        }

        Vector2 boundedInput = input.LengthSquared() > 1.0f
            ? Vector2.Normalize(input)
            : input;

        return boundedInput * speedPixelsPerSecond;
    }
}
