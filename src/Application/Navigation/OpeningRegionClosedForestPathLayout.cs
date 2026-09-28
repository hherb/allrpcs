using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines a minimum-clearance clearing approach ending at one closed gate.</summary>
public sealed class OpeningRegionClosedForestPathLayout
{
    /// <summary>Creates collision boundaries for the near side of the closed forest path.</summary>
    public OpeningRegionClosedForestPathLayout(
        OpeningRegionClosedForestPathSceneBinding sceneBinding,
        float approachHalfWidthPixels,
        float boundaryThicknessPixels,
        float approachLengthPixels,
        Vector2 gateCenter)
    {
        ArgumentNullException.ThrowIfNull(sceneBinding);
        ValidatePositive(approachHalfWidthPixels, nameof(approachHalfWidthPixels));
        ValidatePositive(boundaryThicknessPixels, nameof(boundaryThicknessPixels));
        ValidatePositive(approachLengthPixels, nameof(approachLengthPixels));
        OpeningRegionWoodlandClearingLayout clearing = sceneBinding.WoodlandClearingLayout;
        float clearance = sceneBinding.Projection.MinimumPathClearancePixels;
        if (approachHalfWidthPixels < clearance
            || approachHalfWidthPixels * 2.0f != clearing.OpeningWidthPixels)
        {
            throw new ArgumentOutOfRangeException(
                nameof(approachHalfWidthPixels),
                approachHalfWidthPixels,
                "The approach must retain clearance and exactly span the authored opening.");
        }

        if (boundaryThicknessPixels >= approachHalfWidthPixels)
        {
            throw new ArgumentOutOfRangeException(
                nameof(boundaryThicknessPixels),
                boundaryThicknessPixels,
                "Approach boundaries must not consume the path clearance.");
        }

        if (!IsFinite(gateCenter) || gateCenter != clearing.ClosedForestPathOpening)
        {
            throw new ArgumentException(
                "The gate must remain at the clearing's exact closed-path opening.",
                nameof(gateCenter));
        }

        Vector2 clearingPosition = clearing.SceneBinding.WoodlandClearingAnchor.Position;
        float availableLength = Vector2.Distance(clearingPosition, gateCenter);
        if (approachLengthPixels >= availableLength)
        {
            throw new ArgumentException(
                "The bounded approach must not overlap or pass the clearing anchor.",
                nameof(approachLengthPixels));
        }

        Vector2 direction = Vector2.Normalize(gateCenter - clearingPosition);
        Vector2 perpendicular = new(-direction.Y, direction.X);
        Vector2 approachStart = gateCenter - (direction * approachLengthPixels);
        Vector2 firstStart = approachStart + (perpendicular * approachHalfWidthPixels);
        Vector2 secondStart = approachStart - (perpendicular * approachHalfWidthPixels);
        Vector2 gateLeft = gateCenter - (Vector2.UnitX * approachHalfWidthPixels);
        Vector2 gateRight = gateCenter + (Vector2.UnitX * approachHalfWidthPixels);
        bool firstSideEndsAtRight = Vector2.Dot(
            gateRight - gateCenter,
            perpendicular) >= 0.0f;
        Vector2 firstGateEnd = firstSideEndsAtRight ? gateRight : gateLeft;
        Vector2 secondGateEnd = firstSideEndsAtRight ? gateLeft : gateRight;
        NavigationWorldBounds bounds = clearing.SceneBinding.WoodlandClearingBounds;
        if (!Inside(bounds, approachStart)
            || !Inside(bounds, firstStart)
            || !Inside(bounds, secondStart)
            || !Inside(bounds, gateLeft)
            || !Inside(bounds, gateRight))
        {
            throw new ArgumentException(
                "The closed-path approach must remain inside the retained clearing area.");
        }

        SceneBinding = sceneBinding;
        ApproachHalfWidthPixels = approachHalfWidthPixels;
        BoundaryThicknessPixels = boundaryThicknessPixels;
        ApproachLengthPixels = approachLengthPixels;
        GateCenter = gateCenter;
        ApproachStart = approachStart;
        GateApproachPosition = gateCenter
            - (direction * (clearance / MathF.Abs(direction.Y)));
        ApproachBoundarySegments = Array.AsReadOnly(
        [
            new OpeningRegionBoundarySegment(firstStart, firstGateEnd),
            new OpeningRegionBoundarySegment(secondStart, secondGateEnd),
        ]);
        GateBoundary = new OpeningRegionBoundarySegment(gateLeft, gateRight);
    }

    /// <summary>Gets the exact retained clearing/path binding.</summary>
    public OpeningRegionClosedForestPathSceneBinding SceneBinding { get; }

    /// <summary>Gets the free-space half-width of the bounded approach.</summary>
    public float ApproachHalfWidthPixels { get; }

    /// <summary>Gets the authored collision-boundary thickness.</summary>
    public float BoundaryThicknessPixels { get; }

    /// <summary>Gets the length of the collision-backed approach sides.</summary>
    public float ApproachLengthPixels { get; }

    /// <summary>Gets the exact gate center at the existing clearing opening.</summary>
    public Vector2 GateCenter { get; }

    /// <summary>Gets the inner start of the bounded approach.</summary>
    public Vector2 ApproachStart { get; }

    /// <summary>Gets the closest reachable point retaining clearance from the gate.</summary>
    public Vector2 GateApproachPosition { get; }

    /// <summary>Gets the two collision segments bounding the approach.</summary>
    public IReadOnlyList<OpeningRegionBoundarySegment> ApproachBoundarySegments { get; }

    /// <summary>Gets the collision segment that keeps the deeper-forest path closed.</summary>
    public OpeningRegionBoundarySegment GateBoundary { get; }

    /// <summary>Gets whether this immutable layout keeps its gate closed.</summary>
    public bool IsGateClosed { get; } = true;

    private static bool Inside(NavigationWorldBounds bounds, Vector2 point) =>
        IsFinite(point)
        && point.X >= bounds.Left
        && point.X <= bounds.Right
        && point.Y >= bounds.Top
        && point.Y <= bounds.Bottom;

    private static bool IsFinite(Vector2 point) =>
        float.IsFinite(point.X) && float.IsFinite(point.Y);

    private static void ValidatePositive(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Must be positive.");
        }
    }
}
