using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines collision-aware stream banks and an inert two-opening laundry shell.</summary>
public sealed class OpeningRegionStreamLaundryLayout
{
    private const string VillageEdgeLocationId = "village-edge";

    /// <summary>Creates the stream/laundry layout while retaining exact scene provenance.</summary>
    public OpeningRegionStreamLaundryLayout(
        OpeningRegionStreamLaundrySceneBinding sceneBinding,
        float openingWidthPixels,
        float wallThicknessPixels,
        Vector2 villageEdgeApproach,
        Vector2 bridgeNorthApproach,
        IEnumerable<Vector2> westStreamBank,
        IEnumerable<Vector2> eastStreamBank)
    {
        ArgumentNullException.ThrowIfNull(sceneBinding);
        ArgumentNullException.ThrowIfNull(westStreamBank);
        ArgumentNullException.ThrowIfNull(eastStreamBank);
        ValidatePositiveFinite(openingWidthPixels, nameof(openingWidthPixels));
        ValidatePositiveFinite(wallThicknessPixels, nameof(wallThicknessPixels));
        float clearance = sceneBinding.Projection.MinimumPathClearancePixels;
        if (openingWidthPixels < clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(openingWidthPixels),
                openingWidthPixels,
                "Laundry openings must retain path clearance on both sides.");
        }

        if (wallThicknessPixels >= clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wallThicknessPixels),
                wallThicknessPixels,
                "Laundry walls must not consume endpoint clearance.");
        }

        ValidateVillageEdgeApproach(sceneBinding, villageEdgeApproach);
        ValidateBridgeNorthApproach(sceneBinding, bridgeNorthApproach);
        ValidateOpeningFits(
            sceneBinding.AbandonedLaundryBounds,
            bridgeNorthApproach.Y,
            openingWidthPixels,
            wallThicknessPixels);
        List<Vector2> westBank = ValidateBank(
            sceneBinding.Projection.WorldBounds,
            westStreamBank,
            nameof(westStreamBank));
        List<Vector2> eastBank = ValidateBank(
            sceneBinding.Projection.WorldBounds,
            eastStreamBank,
            nameof(eastStreamBank));

        SceneBinding = sceneBinding;
        OpeningWidthPixels = openingWidthPixels;
        WallThicknessPixels = wallThicknessPixels;
        VillageEdgeApproach = villageEdgeApproach;
        BridgeNorthApproach = bridgeNorthApproach;
        LaundryWoodlandOpening = new Vector2(
            sceneBinding.AbandonedLaundryBounds.Left,
            bridgeNorthApproach.Y);
        LaundryStreamOpening = new Vector2(
            sceneBinding.AbandonedLaundryBounds.Right,
            bridgeNorthApproach.Y);
        WestStreamBank = westBank.AsReadOnly();
        EastStreamBank = eastBank.AsReadOnly();
    }

    /// <summary>Gets the exact stream/laundry scene binding.</summary>
    public OpeningRegionStreamLaundrySceneBinding SceneBinding { get; }

    /// <summary>Gets the shared width of both explicit laundry openings.</summary>
    public float OpeningWidthPixels { get; }

    /// <summary>Gets the physical thickness of every laundry wall section.</summary>
    public float WallThicknessPixels { get; }

    /// <summary>Gets the exact projected village-edge approach.</summary>
    public Vector2 VillageEdgeApproach { get; }

    /// <summary>Gets the clear bridge endpoint north of the stream banks.</summary>
    public Vector2 BridgeNorthApproach { get; }

    /// <summary>Gets the laundry's west opening toward the later woodland route.</summary>
    public Vector2 LaundryWoodlandOpening { get; }

    /// <summary>Gets the laundry's east opening toward the stream bridge.</summary>
    public Vector2 LaundryStreamOpening { get; }

    /// <summary>Gets a copied collision polygon for the west stream bank.</summary>
    public IReadOnlyList<Vector2> WestStreamBank { get; }

    /// <summary>Gets a copied collision polygon for the east stream bank.</summary>
    public IReadOnlyList<Vector2> EastStreamBank { get; }

    private static void ValidatePositiveFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Stream/laundry dimensions must be finite and positive.");
        }
    }

    private static void ValidateVillageEdgeApproach(
        OpeningRegionStreamLaundrySceneBinding binding,
        Vector2 approach)
    {
        OpeningRegionProjectedConnection? connection = binding.Projection.Connections.SingleOrDefault(
            candidate => ReferenceEquals(candidate.SecondAnchor, binding.StreamAnchor)
                && candidate.FirstAnchor.Location.Identity.Id.Value == VillageEdgeLocationId);
        if (connection is null || connection.FirstAnchor.Position != approach)
        {
            throw new ArgumentException(
                "The village-edge approach must be the exact projected stream connection endpoint.",
                nameof(approach));
        }
    }

    private static void ValidateBridgeNorthApproach(
        OpeningRegionStreamLaundrySceneBinding binding,
        Vector2 approach)
    {
        _ = NavigationRequestValidator.Create(
            binding.Projection.WorldBounds,
            binding.StreamAnchor.Position,
            approach);
        if (approach.X != binding.StreamAnchor.Position.X
            || approach.Y >= binding.StreamBounds.Top)
        {
            throw new ArgumentException(
                "The north bridge approach must align above the exact stream anchor.",
                nameof(approach));
        }
    }

    private static void ValidateOpeningFits(
        NavigationWorldBounds bounds,
        float center,
        float openingWidthPixels,
        float wallThicknessPixels)
    {
        if (center - (openingWidthPixels / 2.0f) < bounds.Top + wallThicknessPixels
            || center + (openingWidthPixels / 2.0f) > bounds.Bottom - wallThicknessPixels)
        {
            throw new ArgumentException(
                "Laundry openings must fit completely within both side boundaries.",
                nameof(openingWidthPixels));
        }
    }

    private static List<Vector2> ValidateBank(
        NavigationWorldBounds world,
        IEnumerable<Vector2> bank,
        string parameterName)
    {
        List<Vector2> points = [.. bank];
        if (points.Count < 3)
        {
            throw new ArgumentException(
                "A stream-bank collision polygon requires at least three points.",
                parameterName);
        }

        foreach (Vector2 point in points)
        {
            if (!float.IsFinite(point.X)
                || !float.IsFinite(point.Y)
                || point.X < world.Left
                || point.X > world.Right
                || point.Y < world.Top
                || point.Y > world.Bottom)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    point,
                    "Stream-bank points must be finite and inside the opening region.");
            }
        }

        float doubledArea = 0.0f;
        for (int index = 0; index < points.Count; index++)
        {
            Vector2 current = points[index];
            Vector2 next = points[(index + 1) % points.Count];
            doubledArea += (current.X * next.Y) - (next.X * current.Y);
        }

        if (!float.IsFinite(doubledArea) || doubledArea == 0.0f)
        {
            throw new ArgumentException(
                "A stream-bank collision polygon must enclose a finite non-zero area.",
                parameterName);
        }

        return points;
    }
}
