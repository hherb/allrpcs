using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines a minimum-clearance southern road ending at one inert wagon shell.</summary>
public sealed class OpeningRegionSouthernRoadWagonLayout
{
    /// <summary>Creates collision boundaries around the exact shrine/road/wagon route.</summary>
    public OpeningRegionSouthernRoadWagonLayout(
        OpeningRegionSouthernRoadWagonSceneBinding sceneBinding,
        float roadHalfWidthPixels,
        float boundaryThicknessPixels,
        float wagonOpeningWidthPixels,
        Vector2 wagonOpening)
    {
        ArgumentNullException.ThrowIfNull(sceneBinding);
        ValidatePositive(roadHalfWidthPixels, nameof(roadHalfWidthPixels));
        ValidatePositive(boundaryThicknessPixels, nameof(boundaryThicknessPixels));
        ValidatePositive(wagonOpeningWidthPixels, nameof(wagonOpeningWidthPixels));
        float clearance = sceneBinding.Projection.MinimumPathClearancePixels;
        if (roadHalfWidthPixels < clearance)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roadHalfWidthPixels),
                roadHalfWidthPixels,
                "The road must retain minimum clearance on both sides.");
        }

        if (boundaryThicknessPixels >= roadHalfWidthPixels)
        {
            throw new ArgumentOutOfRangeException(
                nameof(boundaryThicknessPixels),
                boundaryThicknessPixels,
                "Road boundaries must not consume the route clearance.");
        }

        if (wagonOpeningWidthPixels < clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wagonOpeningWidthPixels),
                wagonOpeningWidthPixels,
                "The wagon opening must retain minimum clearance on both sides.");
        }

        ValidateWagonOpening(sceneBinding, wagonOpeningWidthPixels, wagonOpening);
        Vector2[] centerline =
        [
            sceneBinding.SouthernRoadAnchor.Position,
            wagonOpening,
            sceneBinding.AbandonedWagonAnchor.Position,
        ];
        List<OpeningRegionBoundarySegment> roadBoundaries = CreateRoadBoundaries(
            centerline,
            roadHalfWidthPixels,
            wagonOpeningWidthPixels);
        List<OpeningRegionBoundarySegment> wagonBoundaries = CreateWagonBoundaries(
            sceneBinding.AbandonedWagonBounds,
            wagonOpening,
            wagonOpeningWidthPixels);
        if (roadBoundaries.Concat(wagonBoundaries).Any(segment =>
            !Inside(sceneBinding.Projection.WorldBounds, segment.Start)
            || !Inside(sceneBinding.Projection.WorldBounds, segment.End)))
        {
            throw new ArgumentException("Road and wagon boundaries must remain inside the region.");
        }

        SceneBinding = sceneBinding;
        RoadHalfWidthPixels = roadHalfWidthPixels;
        BoundaryThicknessPixels = boundaryThicknessPixels;
        WagonOpeningWidthPixels = wagonOpeningWidthPixels;
        WagonOpening = wagonOpening;
        RouteCenterline = Array.AsReadOnly(centerline);
        RoadBoundarySegments = roadBoundaries.AsReadOnly();
        WagonBoundarySegments = wagonBoundaries.AsReadOnly();
    }

    /// <summary>Gets the exact road/wagon scene binding.</summary>
    public OpeningRegionSouthernRoadWagonSceneBinding SceneBinding { get; }

    /// <summary>Gets the free-space road half-width.</summary>
    public float RoadHalfWidthPixels { get; }

    /// <summary>Gets the authored collision-boundary thickness.</summary>
    public float BoundaryThicknessPixels { get; }

    /// <summary>Gets the width of the sole wagon-shell opening.</summary>
    public float WagonOpeningWidthPixels { get; }

    /// <summary>Gets the topology-aligned wagon-shell opening.</summary>
    public Vector2 WagonOpening { get; }

    /// <summary>Gets the stable southern-road-to-wagon route centerline.</summary>
    public IReadOnlyList<Vector2> RouteCenterline { get; }

    /// <summary>Gets the two collision segments bounding the road.</summary>
    public IReadOnlyList<OpeningRegionBoundarySegment> RoadBoundarySegments { get; }

    /// <summary>Gets the five collision segments forming the inert wagon shell.</summary>
    public IReadOnlyList<OpeningRegionBoundarySegment> WagonBoundarySegments { get; }

    private static void ValidateWagonOpening(
        OpeningRegionSouthernRoadWagonSceneBinding binding,
        float openingWidth,
        Vector2 opening)
    {
        NavigationWorldBounds bounds = binding.AbandonedWagonBounds;
        float half = openingWidth / 2.0f;
        if (!float.IsFinite(opening.X)
            || opening.Y != bounds.Top
            || opening.X - half < bounds.Left
            || opening.X + half > bounds.Right)
        {
            throw new ArgumentException(
                "The wagon opening must fit on the shell's northern boundary.",
                nameof(opening));
        }

        Vector2 road = binding.SouthernRoadAnchor.Position;
        Vector2 wagon = binding.AbandonedWagonAnchor.Position;
        float progress = (bounds.Top - road.Y) / (wagon.Y - road.Y);
        float crossingX = road.X + ((wagon.X - road.X) * progress);
        if (!float.IsFinite(crossingX)
            || progress <= 0.0f
            || progress >= 1.0f
            || MathF.Abs(crossingX - opening.X) > half)
        {
            throw new ArgumentException(
                "The wagon opening must retain the projected road connection.",
                nameof(opening));
        }
    }

    private static List<OpeningRegionBoundarySegment> CreateRoadBoundaries(
        IReadOnlyList<Vector2> centerline,
        float roadHalfWidth,
        float wagonOpeningWidth)
    {
        Vector2 firstOffset = Perpendicular(centerline[0], centerline[1]) * roadHalfWidth;
        Vector2 firstCenter = centerline[0]
            + (Vector2.Normalize(centerline[1] - centerline[0]) * roadHalfWidth);
        Vector2 horizontalOpeningOffset = Vector2.UnitX * (wagonOpeningWidth / 2.0f);
        Vector2 firstLeft = firstCenter + firstOffset;
        Vector2 firstRight = firstCenter - firstOffset;
        Vector2 lastLeft = centerline[1] - horizontalOpeningOffset;
        Vector2 lastRight = centerline[1] + horizontalOpeningOffset;
        return
        [
            new(firstLeft, lastLeft),
            new(firstRight, lastRight),
        ];
    }

    private static List<OpeningRegionBoundarySegment> CreateWagonBoundaries(
        NavigationWorldBounds bounds,
        Vector2 opening,
        float openingWidth)
    {
        float half = openingWidth / 2.0f;
        Vector2 topLeft = new(bounds.Left, bounds.Top);
        Vector2 topRight = new(bounds.Right, bounds.Top);
        Vector2 bottomLeft = new(bounds.Left, bounds.Bottom);
        Vector2 bottomRight = new(bounds.Right, bounds.Bottom);
        return
        [
            new(topLeft, new Vector2(opening.X - half, bounds.Top)),
            new(new Vector2(opening.X + half, bounds.Top), topRight),
            new(topLeft, bottomLeft),
            new(topRight, bottomRight),
            new(bottomLeft, bottomRight),
        ];
    }

    private static Vector2 Perpendicular(Vector2 start, Vector2 end)
    {
        Vector2 direction = Vector2.Normalize(end - start);
        return new Vector2(-direction.Y, direction.X);
    }

    private static bool Inside(NavigationWorldBounds bounds, Vector2 point) =>
        float.IsFinite(point.X)
        && float.IsFinite(point.Y)
        && point.X >= bounds.Left
        && point.X <= bounds.Right
        && point.Y >= bounds.Top
        && point.Y <= bounds.Bottom;

    private static void ValidatePositive(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Must be positive.");
        }
    }
}
