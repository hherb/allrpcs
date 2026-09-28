using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines an inert clearing shell with four topology-aligned openings.</summary>
public sealed class OpeningRegionWoodlandClearingLayout
{
    private const string VillageEdgeLocationId = "village-edge";
    private const string LaundryLocationId = "abandoned-laundry";
    private const string ShrineLocationId = "old-shrine";
    private const string ClosedPathLocationId = "closed-deeper-forest-path";

    /// <summary>Creates collision-aware tree boundaries around the exact clearing.</summary>
    public OpeningRegionWoodlandClearingLayout(
        OpeningRegionWoodlandClearingSceneBinding sceneBinding,
        float openingWidthPixels,
        float wallThicknessPixels,
        Vector2 villageEdgeOpening,
        Vector2 closedForestPathOpening)
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
                "Clearing openings must retain path clearance on both sides.");
        }

        if (wallThicknessPixels >= clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wallThicknessPixels),
                wallThicknessPixels,
                "Tree boundaries must not consume endpoint clearance.");
        }

        OpeningRegionAnchor laundry = ConnectedAnchor(sceneBinding, LaundryLocationId);
        OpeningRegionAnchor shrine = ConnectedAnchor(sceneBinding, ShrineLocationId);
        OpeningRegionAnchor villageEdge = ConnectedAnchor(sceneBinding, VillageEdgeLocationId);
        OpeningRegionAnchor closedPath = ConnectedAnchor(sceneBinding, ClosedPathLocationId);
        NavigationWorldBounds bounds = sceneBinding.WoodlandClearingBounds;
        Vector2 clearing = sceneBinding.WoodlandClearingAnchor.Position;
        Vector2 laundryOpening = VerticalIntersection(clearing, laundry.Position, bounds.Right);
        Vector2 shrineOpening = VerticalIntersection(clearing, shrine.Position, bounds.Left);
        ValidateSideOpening(bounds, laundryOpening, openingWidthPixels, wallThicknessPixels);
        ValidateSideOpening(bounds, shrineOpening, openingWidthPixels, wallThicknessPixels);
        ValidateBottomOpening(
            sceneBinding,
            villageEdge,
            villageEdgeOpening,
            openingWidthPixels,
            wallThicknessPixels,
            nameof(villageEdgeOpening));
        ValidateVerticalOpening(
            sceneBinding,
            closedPath,
            closedForestPathOpening,
            openingWidthPixels,
            wallThicknessPixels,
            nameof(closedForestPathOpening));
        if (villageEdgeOpening.Y == closedForestPathOpening.Y
            && MathF.Abs(villageEdgeOpening.X - closedForestPathOpening.X) < openingWidthPixels)
        {
            throw new ArgumentException(
            "The clearing route openings must remain distinct and non-overlapping.",
                nameof(closedForestPathOpening));
        }

        SceneBinding = sceneBinding;
        OpeningWidthPixels = openingWidthPixels;
        WallThicknessPixels = wallThicknessPixels;
        LaundryOpening = laundryOpening;
        ShrineOpening = shrineOpening;
        VillageEdgeOpening = villageEdgeOpening;
        ClosedForestPathOpening = closedForestPathOpening;
    }

    /// <summary>Gets the exact clearing scene binding.</summary>
    public OpeningRegionWoodlandClearingSceneBinding SceneBinding { get; }

    /// <summary>Gets the shared width of all four route openings.</summary>
    public float OpeningWidthPixels { get; }

    /// <summary>Gets the thickness of each collision-backed tree boundary.</summary>
    public float WallThicknessPixels { get; }

    /// <summary>Gets the east opening toward the abandoned laundry.</summary>
    public Vector2 LaundryOpening { get; }

    /// <summary>Gets the west opening toward the old shrine.</summary>
    public Vector2 ShrineOpening { get; }

    /// <summary>Gets the southeast opening toward the village edge.</summary>
    public Vector2 VillageEdgeOpening { get; }

    /// <summary>Gets the northwest opening toward the closed deeper-forest path.</summary>
    public Vector2 ClosedForestPathOpening { get; }

    private static OpeningRegionAnchor ConnectedAnchor(
        OpeningRegionWoodlandClearingSceneBinding binding,
        string locationId)
    {
        OpeningRegionProjectedConnection? connection = binding.Projection.Connections
            .SingleOrDefault(candidate =>
                IsPair(candidate, binding.WoodlandClearingAnchor, locationId));
        if (connection is null)
        {
            throw new ArgumentException(
                "The clearing layout requires each exact authored topology connection.",
                nameof(binding));
        }

        return ReferenceEquals(connection.FirstAnchor, binding.WoodlandClearingAnchor)
            ? connection.SecondAnchor
            : connection.FirstAnchor;
    }

    private static bool IsPair(
        OpeningRegionProjectedConnection connection,
        OpeningRegionAnchor clearing,
        string otherLocationId) =>
        (ReferenceEquals(connection.FirstAnchor, clearing)
            && connection.SecondAnchor.Location.Identity.Id.Value == otherLocationId)
        || (ReferenceEquals(connection.SecondAnchor, clearing)
            && connection.FirstAnchor.Location.Identity.Id.Value == otherLocationId);

    private static Vector2 VerticalIntersection(Vector2 start, Vector2 end, float x)
    {
        float progress = (x - start.X) / (end.X - start.X);
        return new Vector2(x, start.Y + ((end.Y - start.Y) * progress));
    }

    private static void ValidatePositiveFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Clearing dimensions must be finite and positive.");
        }
    }

    private static void ValidateSideOpening(
        NavigationWorldBounds bounds,
        Vector2 opening,
        float width,
        float wallThickness)
    {
        float halfWidth = width / 2.0f;
        if (opening.Y - halfWidth < bounds.Top + wallThickness
            || opening.Y + halfWidth > bounds.Bottom - wallThickness)
        {
            throw new ArgumentException(
                "Side openings must fit completely within the clearing boundary.",
                nameof(width));
        }
    }

    private static void ValidateBottomOpening(
        OpeningRegionWoodlandClearingSceneBinding binding,
        OpeningRegionAnchor connectedAnchor,
        Vector2 opening,
        float width,
        float wallThickness,
        string parameterName)
    {
        NavigationWorldBounds bounds = binding.WoodlandClearingBounds;
        float halfWidth = width / 2.0f;
        if (!float.IsFinite(opening.X)
            || opening.Y != bounds.Bottom
            || opening.X - halfWidth < bounds.Left + wallThickness
            || opening.X + halfWidth > bounds.Right - wallThickness)
        {
            throw new ArgumentException(
                "Southern openings must fit completely on the clearing boundary.",
                parameterName);
        }

        Vector2 clearing = binding.WoodlandClearingAnchor.Position;
        float progress = (bounds.Bottom - connectedAnchor.Position.Y)
            / (clearing.Y - connectedAnchor.Position.Y);
        float crossingX = connectedAnchor.Position.X
            + ((clearing.X - connectedAnchor.Position.X) * progress);
        if (!float.IsFinite(crossingX) || MathF.Abs(crossingX - opening.X) > halfWidth)
        {
            throw new ArgumentException(
                "Southern openings must retain the projected topology route.",
                parameterName);
        }
    }

    private static void ValidateVerticalOpening(
        OpeningRegionWoodlandClearingSceneBinding binding,
        OpeningRegionAnchor connectedAnchor,
        Vector2 opening,
        float width,
        float wallThickness,
        string parameterName)
    {
        NavigationWorldBounds bounds = binding.WoodlandClearingBounds;
        float halfWidth = width / 2.0f;
        if (!float.IsFinite(opening.X)
            || (opening.Y != bounds.Top && opening.Y != bounds.Bottom)
            || opening.X - halfWidth < bounds.Left + wallThickness
            || opening.X + halfWidth > bounds.Right - wallThickness)
        {
            throw new ArgumentException(
                "Vertical openings must fit completely on the clearing boundary.",
                parameterName);
        }

        Vector2 clearing = binding.WoodlandClearingAnchor.Position;
        float progress = (opening.Y - connectedAnchor.Position.Y)
            / (clearing.Y - connectedAnchor.Position.Y);
        float crossingX = connectedAnchor.Position.X
            + ((clearing.X - connectedAnchor.Position.X) * progress);
        if (!float.IsFinite(crossingX) || MathF.Abs(crossingX - opening.X) > halfWidth)
        {
            throw new ArgumentException(
                "Vertical openings must retain the projected topology route.",
                parameterName);
        }
    }
}
