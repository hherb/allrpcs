using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines collision-aware village areas with explicit topology-facing openings.</summary>
public sealed class OpeningRegionVillageTransitionLayout
{
    /// <summary>Creates village transitions while retaining the exact scene binding.</summary>
    public OpeningRegionVillageTransitionLayout(
        OpeningRegionVillageTransitionSceneBinding sceneBinding,
        float openingWidthPixels,
        float wallThicknessPixels,
        Vector2 gardenApproach)
    {
        ArgumentNullException.ThrowIfNull(sceneBinding);
        ValidatePositiveFinite(openingWidthPixels, nameof(openingWidthPixels));
        ValidatePositiveFinite(wallThicknessPixels, nameof(wallThicknessPixels));
        float clearance = sceneBinding.Projection.MinimumPathClearancePixels;
        if (openingWidthPixels < clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(openingWidthPixels),
                openingWidthPixels,
                "Village transition openings must retain path clearance on both sides.");
        }

        if (wallThicknessPixels >= clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wallThicknessPixels),
                wallThicknessPixels,
                "Village transition walls must not consume endpoint clearance.");
        }

        NavigationWorldBounds square = sceneBinding.VillageSquareBounds;
        NavigationWorldBounds edge = sceneBinding.VillageEdgeBounds;
        Vector2 squareAnchor = sceneBinding.VillageSquareAnchor.Position;
        Vector2 edgeAnchor = sceneBinding.VillageEdgeAnchor.Position;
        _ = NavigationRequestValidator.Create(
            sceneBinding.Projection.WorldBounds,
            gardenApproach,
            squareAnchor);
        if (gardenApproach.X != squareAnchor.X
            || gardenApproach.Y < square.Bottom + clearance)
        {
            throw new ArgumentException(
                "The garden approach must align below the village-square opening with clearance.",
                nameof(gardenApproach));
        }

        ValidateOpeningFits(
            squareAnchor.X,
            square.Left,
            square.Right,
            openingWidthPixels,
            wallThicknessPixels);
        ValidateOpeningFits(
            squareAnchor.Y,
            square.Top,
            square.Bottom,
            openingWidthPixels,
            wallThicknessPixels);
        ValidateOpeningFits(
            edgeAnchor.X,
            edge.Left,
            edge.Right,
            openingWidthPixels,
            wallThicknessPixels);
        ValidateOpeningFits(
            edgeAnchor.Y,
            edge.Top,
            edge.Bottom,
            openingWidthPixels,
            wallThicknessPixels);

        SceneBinding = sceneBinding;
        OpeningWidthPixels = openingWidthPixels;
        WallThicknessPixels = wallThicknessPixels;
        GardenApproach = gardenApproach;
        SquareGardenOpening = new Vector2(squareAnchor.X, square.Bottom);
        SquareEdgeOpening = new Vector2(square.Left, squareAnchor.Y);
        EdgeSquareOpening = new Vector2(edge.Right, edgeAnchor.Y);
        EdgeStreamOpening = new Vector2(edgeAnchor.X, edge.Top);
        EdgeWoodlandOpening = new Vector2(edge.Left, edgeAnchor.Y);
    }

    /// <summary>Gets the exact village scene binding.</summary>
    public OpeningRegionVillageTransitionSceneBinding SceneBinding { get; }

    /// <summary>Gets the shared width of every explicit opening.</summary>
    public float OpeningWidthPixels { get; }

    /// <summary>Gets the physical thickness of each boundary section.</summary>
    public float WallThicknessPixels { get; }

    /// <summary>Gets the navigation point outside the square toward the healer's garden.</summary>
    public Vector2 GardenApproach { get; }

    /// <summary>Gets the square's south opening toward the healer's garden.</summary>
    public Vector2 SquareGardenOpening { get; }

    /// <summary>Gets the square's west opening toward the village edge.</summary>
    public Vector2 SquareEdgeOpening { get; }

    /// <summary>Gets the edge area's east opening toward the village square.</summary>
    public Vector2 EdgeSquareOpening { get; }

    /// <summary>Gets the edge area's north opening toward the stream branch.</summary>
    public Vector2 EdgeStreamOpening { get; }

    /// <summary>Gets the edge area's west opening toward the woodland branch.</summary>
    public Vector2 EdgeWoodlandOpening { get; }

    private static void ValidatePositiveFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Village transition dimensions must be finite and positive.");
        }
    }

    private static void ValidateOpeningFits(
        float center,
        float boundaryStart,
        float boundaryEnd,
        float openingWidthPixels,
        float wallThicknessPixels)
    {
        if (center - (openingWidthPixels / 2.0f) < boundaryStart + wallThicknessPixels
            || center + (openingWidthPixels / 2.0f) > boundaryEnd - wallThicknessPixels)
        {
            throw new ArgumentException(
                "Village transition openings must fit completely within their boundary.",
                nameof(openingWidthPixels));
        }
    }
}
