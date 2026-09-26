using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Defines an inert shrine shell with clearing and road openings.</summary>
public sealed class OpeningRegionOldShrineLayout
{
    private const string WoodlandClearingLocationId = "woodland-clearing";
    private const string SouthernRoadLocationId = "southern-road";

    /// <summary>Creates collision-aware shrine boundaries without discovery behavior.</summary>
    public OpeningRegionOldShrineLayout(
        OpeningRegionOldShrineSceneBinding sceneBinding,
        float openingWidthPixels,
        float wallThicknessPixels,
        Vector2 southernRoadOpening)
    {
        ArgumentNullException.ThrowIfNull(sceneBinding);
        ValidatePositive(openingWidthPixels, nameof(openingWidthPixels));
        ValidatePositive(wallThicknessPixels, nameof(wallThicknessPixels));
        float clearance = sceneBinding.Projection.MinimumPathClearancePixels;
        if (openingWidthPixels < clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(openingWidthPixels),
                openingWidthPixels,
                "Shrine openings must retain path clearance on both sides.");
        }

        if (wallThicknessPixels >= clearance * 2.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wallThicknessPixels),
                wallThicknessPixels,
                "Shrine walls must not consume endpoint clearance.");
        }

        OpeningRegionAnchor clearing = ConnectedAnchor(sceneBinding, WoodlandClearingLocationId);
        OpeningRegionAnchor road = ConnectedAnchor(sceneBinding, SouthernRoadLocationId);
        NavigationWorldBounds bounds = sceneBinding.OldShrineBounds;
        WoodlandClearingOpening = new Vector2(
            bounds.Right,
            sceneBinding.OldShrineAnchor.Position.Y);
        float halfWidth = openingWidthPixels / 2.0f;
        if (WoodlandClearingOpening.Y - halfWidth < bounds.Top + wallThicknessPixels
            || WoodlandClearingOpening.Y + halfWidth > bounds.Bottom - wallThicknessPixels)
        {
            throw new ArgumentException("The clearing opening must fit on the shrine boundary.");
        }

        if (southernRoadOpening.Y != bounds.Bottom
            || southernRoadOpening.X - halfWidth < bounds.Left + wallThicknessPixels
            || southernRoadOpening.X + halfWidth > bounds.Right)
        {
            throw new ArgumentException(
                "The road opening must fit on the shrine's southern boundary.",
                nameof(southernRoadOpening));
        }

        Vector2 shrine = sceneBinding.OldShrineAnchor.Position;
        float progress = (bounds.Bottom - road.Position.Y) / (shrine.Y - road.Position.Y);
        float crossingX = road.Position.X + ((shrine.X - road.Position.X) * progress);
        if (!float.IsFinite(crossingX)
            || MathF.Abs(crossingX - southernRoadOpening.X) > halfWidth
            || clearing.Position.X <= shrine.X)
        {
            throw new ArgumentException(
                "Shrine openings must retain their exact projected topology directions.",
                nameof(southernRoadOpening));
        }

        SceneBinding = sceneBinding;
        OpeningWidthPixels = openingWidthPixels;
        WallThicknessPixels = wallThicknessPixels;
        SouthernRoadOpening = southernRoadOpening;
    }

    /// <summary>Gets the exact old-shrine binding.</summary>
    public OpeningRegionOldShrineSceneBinding SceneBinding { get; }

    /// <summary>Gets the shared width of the shrine openings.</summary>
    public float OpeningWidthPixels { get; }

    /// <summary>Gets the collision-backed shrine wall thickness.</summary>
    public float WallThicknessPixels { get; }

    /// <summary>Gets the east opening toward the woodland clearing.</summary>
    public Vector2 WoodlandClearingOpening { get; }

    /// <summary>Gets the southeast opening toward the southern road.</summary>
    public Vector2 SouthernRoadOpening { get; }

    private static OpeningRegionAnchor ConnectedAnchor(
        OpeningRegionOldShrineSceneBinding binding,
        string locationId)
    {
        OpeningRegionProjectedConnection? connection = binding.Projection.Connections
            .SingleOrDefault(candidate =>
                (ReferenceEquals(candidate.FirstAnchor, binding.OldShrineAnchor)
                    && candidate.SecondAnchor.Location.Identity.Id.Value == locationId)
                || (ReferenceEquals(candidate.SecondAnchor, binding.OldShrineAnchor)
                    && candidate.FirstAnchor.Location.Identity.Id.Value == locationId));
        if (connection is null)
        {
            throw new ArgumentException("The shrine requires both exact topology connections.");
        }

        return ReferenceEquals(connection.FirstAnchor, binding.OldShrineAnchor)
            ? connection.SecondAnchor
            : connection.FirstAnchor;
    }

    private static void ValidatePositive(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Must be positive.");
        }
    }
}
