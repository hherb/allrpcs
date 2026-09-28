using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines collision-aware village areas with explicit topology-facing openings.</summary>
public sealed class OpeningRegionVillageTransitionLayout
{
    private const string GardenLocationId = "healer-garden";
    private const string SickhouseLocationId = "sickhouse";
    private const string SouthernRoadLocationId = "southern-road";
    private const string StreamLocationId = "stream";
    private const string WoodlandLocationId = "woodland-clearing";

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
        OpeningRegionAnchor garden = ConnectedAnchor(
            sceneBinding,
            sceneBinding.VillageSquareAnchor,
            GardenLocationId);
        OpeningRegionAnchor sickhouse = ConnectedAnchor(
            sceneBinding,
            sceneBinding.VillageSquareAnchor,
            SickhouseLocationId);
        OpeningRegionAnchor southernRoad = ConnectedAnchor(
            sceneBinding,
            sceneBinding.VillageSquareAnchor,
            SouthernRoadLocationId);
        OpeningRegionAnchor stream = ConnectedAnchor(
            sceneBinding,
            sceneBinding.VillageEdgeAnchor,
            StreamLocationId);
        OpeningRegionAnchor woodland = ConnectedAnchor(
            sceneBinding,
            sceneBinding.VillageEdgeAnchor,
            WoodlandLocationId);
        _ = NavigationRequestValidator.Create(
            sceneBinding.Projection.WorldBounds,
            gardenApproach,
            squareAnchor);
        if (gardenApproach.Y < square.Bottom + clearance
            || DistanceFromLine(gardenApproach, squareAnchor, garden.Position) > 0.01f)
        {
            throw new ArgumentException(
                "The garden approach must align with the projected garden route and retain clearance.",
                nameof(gardenApproach));
        }

        bool usesCardinalLayout = gardenApproach.X == squareAnchor.X;
        Vector2 squareGardenOpening = usesCardinalLayout
            ? new Vector2(squareAnchor.X, square.Bottom)
            : HorizontalIntersection(squareAnchor, garden.Position, square.Bottom);
        Vector2 squareSickhouseOpening = HorizontalIntersection(
            squareAnchor,
            sickhouse.Position,
            square.Bottom);
        Vector2 squareSouthernRoadOpening = VerticalIntersection(
            squareAnchor,
            southernRoad.Position,
            square.Left);
        Vector2 edgeStreamOpening = usesCardinalLayout
            ? new Vector2(edgeAnchor.X, edge.Top)
            : HorizontalIntersection(edgeAnchor, stream.Position, edge.Top);
        Vector2 edgeWoodlandOpening = usesCardinalLayout
            ? new Vector2(edge.Left, edgeAnchor.Y)
            : VerticalIntersection(edgeAnchor, woodland.Position, edge.Left);
        ValidateOpeningFits(
            squareGardenOpening.X,
            square.Left,
            square.Right,
            openingWidthPixels,
            wallThicknessPixels);
        ValidateCornerOpeningFits(
            squareSickhouseOpening.X,
            square.Left,
            square.Right,
            clearance);
        ValidateOpeningFits(
            squareAnchor.Y,
            square.Top,
            square.Bottom,
            openingWidthPixels,
            wallThicknessPixels);
        ValidateCornerOpeningFits(
            squareSouthernRoadOpening.Y,
            square.Top,
            square.Bottom,
            clearance);
        ValidateOverlappingOpenings(
            squareGardenOpening.X,
            squareSickhouseOpening.X,
            openingWidthPixels);
        ValidateOverlappingOpenings(
            squareAnchor.Y,
            squareSouthernRoadOpening.Y,
            openingWidthPixels);
        ValidateOpeningFits(
            edgeStreamOpening.X,
            edge.Left,
            edge.Right,
            openingWidthPixels,
            wallThicknessPixels);
        ValidateOpeningFits(
            edgeWoodlandOpening.Y,
            edge.Top,
            edge.Bottom,
            openingWidthPixels,
            wallThicknessPixels);

        SceneBinding = sceneBinding;
        OpeningWidthPixels = openingWidthPixels;
        WallThicknessPixels = wallThicknessPixels;
        GardenApproach = gardenApproach;
        SquareGardenOpening = squareGardenOpening;
        SquareSickhouseOpening = squareSickhouseOpening;
        SquareEdgeOpening = new Vector2(square.Left, squareAnchor.Y);
        SquareSouthernRoadOpening = squareSouthernRoadOpening;
        EdgeSquareOpening = new Vector2(edge.Right, edgeAnchor.Y);
        EdgeStreamOpening = edgeStreamOpening;
        EdgeWoodlandOpening = edgeWoodlandOpening;
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

    /// <summary>Gets the square's south opening toward the sickhouse.</summary>
    public Vector2 SquareSickhouseOpening { get; }

    /// <summary>Gets the square's west opening toward the village edge.</summary>
    public Vector2 SquareEdgeOpening { get; }

    /// <summary>Gets the square's west opening toward the southern road.</summary>
    public Vector2 SquareSouthernRoadOpening { get; }

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

    private static void ValidateCornerOpeningFits(
        float center,
        float boundaryStart,
        float boundaryEnd,
        float clearance)
    {
        if (center < boundaryStart + clearance || center > boundaryEnd - clearance)
        {
            throw new ArgumentException(
                "A corner-adjacent village opening must retain route clearance.",
                nameof(center));
        }
    }

    private static void ValidateOverlappingOpenings(
        float firstCenter,
        float secondCenter,
        float openingWidthPixels)
    {
        if (MathF.Abs(firstCenter - secondCenter) > openingWidthPixels)
        {
            throw new ArgumentException(
                "Adjacent village routes must form one continuous boundary opening.",
                nameof(openingWidthPixels));
        }
    }

    private static OpeningRegionAnchor ConnectedAnchor(
        OpeningRegionVillageTransitionSceneBinding binding,
        OpeningRegionAnchor origin,
        string otherLocationId)
    {
        OpeningRegionProjectedConnection connection = binding.Projection.Connections.Single(
            candidate => (ReferenceEquals(candidate.FirstAnchor, origin)
                    && candidate.SecondAnchor.Location.Identity.Id.Value == otherLocationId)
                || (ReferenceEquals(candidate.SecondAnchor, origin)
                    && candidate.FirstAnchor.Location.Identity.Id.Value == otherLocationId));
        return ReferenceEquals(connection.FirstAnchor, origin)
            ? connection.SecondAnchor
            : connection.FirstAnchor;
    }

    private static Vector2 HorizontalIntersection(Vector2 start, Vector2 end, float y)
    {
        float progress = (y - start.Y) / (end.Y - start.Y);
        return new Vector2(start.X + ((end.X - start.X) * progress), y);
    }

    private static Vector2 VerticalIntersection(Vector2 start, Vector2 end, float x)
    {
        float progress = (x - start.X) / (end.X - start.X);
        return new Vector2(x, start.Y + ((end.Y - start.Y) * progress));
    }

    private static float DistanceFromLine(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float progress = Vector2.Dot(point - start, segment) / segment.LengthSquared();
        return Vector2.Distance(point, start + (segment * progress));
    }
}
