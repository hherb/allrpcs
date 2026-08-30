using System.Numerics;

namespace Allrpcs.Application.Camera;

/// <summary>
/// Describes the finite rectangular region that a camera may reveal.
/// </summary>
/// <param name="Left">The left world edge in pixels.</param>
/// <param name="Top">The top world edge in pixels.</param>
/// <param name="Right">The right world edge in pixels.</param>
/// <param name="Bottom">The bottom world edge in pixels.</param>
public readonly record struct CameraWorldBounds(float Left, float Top, float Right, float Bottom)
{
    /// <summary>
    /// Gets the region width in pixels.
    /// </summary>
    public float Width => Right - Left;

    /// <summary>
    /// Gets the region height in pixels.
    /// </summary>
    public float Height => Bottom - Top;
}

/// <summary>
/// Holds validated camera bounds and fixed zoom tuning for a viewport.
/// </summary>
/// <param name="WorldBounds">The world region the viewport must remain inside.</param>
/// <param name="MinimumZoom">The furthest supported zoom-out value.</param>
/// <param name="DefaultZoom">The stable zoom used when the scene starts.</param>
/// <param name="MaximumZoom">The nearest supported zoom-in value.</param>
public readonly record struct CameraFramingPlan(
    CameraWorldBounds WorldBounds,
    float MinimumZoom,
    float DefaultZoom,
    float MaximumZoom);

/// <summary>
/// Validates camera bounds and zoom tuning without depending on the presentation engine.
/// </summary>
public static class CameraFramingCalculator
{
    /// <summary>
    /// Creates a framing plan whose minimum zoom cannot reveal beyond the supplied world bounds.
    /// </summary>
    /// <param name="worldBounds">The finite world rectangle to keep on screen.</param>
    /// <param name="viewportSizePixels">The finite positive viewport size in pixels.</param>
    /// <param name="minimumZoom">The furthest supported zoom-out value.</param>
    /// <param name="defaultZoom">The stable zoom used when the scene starts.</param>
    /// <param name="maximumZoom">The nearest supported zoom-in value.</param>
    /// <returns>A validated immutable camera framing plan.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when bounds, viewport size, or zoom values are invalid, or when minimum zoom would
    /// reveal space beyond the world region.
    /// </exception>
    public static CameraFramingPlan Create(
        CameraWorldBounds worldBounds,
        Vector2 viewportSizePixels,
        float minimumZoom,
        float defaultZoom,
        float maximumZoom)
    {
        ValidateWorldBounds(worldBounds);
        ValidateViewportSize(viewportSizePixels);
        ValidateZoomTuning(minimumZoom, defaultZoom, maximumZoom);

        float minimumCoveringZoom = MathF.Max(
            viewportSizePixels.X / worldBounds.Width,
            viewportSizePixels.Y / worldBounds.Height);
        if (minimumZoom < minimumCoveringZoom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumZoom),
                minimumZoom,
                $"Minimum zoom must be at least {minimumCoveringZoom} for this viewport and world.");
        }

        return new CameraFramingPlan(worldBounds, minimumZoom, defaultZoom, maximumZoom);
    }

    private static void ValidateWorldBounds(CameraWorldBounds worldBounds)
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
                "Camera world bounds must be finite with positive width and height.");
        }
    }

    private static void ValidateViewportSize(Vector2 viewportSizePixels)
    {
        if (!float.IsFinite(viewportSizePixels.X)
            || !float.IsFinite(viewportSizePixels.Y)
            || viewportSizePixels.X <= 0.0f
            || viewportSizePixels.Y <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(viewportSizePixels),
                viewportSizePixels,
                "Viewport size must be finite and positive.");
        }
    }

    private static void ValidateZoomTuning(float minimumZoom, float defaultZoom, float maximumZoom)
    {
        ValidatePositiveZoom(minimumZoom, nameof(minimumZoom));
        ValidatePositiveZoom(defaultZoom, nameof(defaultZoom));
        ValidatePositiveZoom(maximumZoom, nameof(maximumZoom));

        if (defaultZoom < minimumZoom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(defaultZoom),
                defaultZoom,
                "Default zoom must be greater than or equal to minimum zoom.");
        }

        if (maximumZoom < defaultZoom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumZoom),
                maximumZoom,
                "Maximum zoom must be greater than or equal to default zoom.");
        }
    }

    private static void ValidatePositiveZoom(float zoom, string parameterName)
    {
        if (!float.IsFinite(zoom) || zoom <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                zoom,
                "Camera zoom must be finite and positive.");
        }
    }
}
