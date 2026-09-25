using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact village-square and village-edge anchors to distinct scene areas.</summary>
public sealed class OpeningRegionVillageTransitionSceneBinding
{
    private const string VillageSquareLocationId = "village-square";
    private const string VillageEdgeLocationId = "village-edge";

    /// <summary>Creates one bounded pair without starting investigation or scene progression.</summary>
    public OpeningRegionVillageTransitionSceneBinding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor villageSquareAnchor,
        NavigationWorldBounds villageSquareBounds,
        OpeningRegionAnchor villageEdgeAnchor,
        NavigationWorldBounds villageEdgeBounds)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(villageSquareAnchor);
        ArgumentNullException.ThrowIfNull(villageEdgeAnchor);
        ValidateExactAnchor(
            projection,
            villageSquareAnchor,
            VillageSquareLocationId,
            nameof(villageSquareAnchor));
        ValidateExactAnchor(
            projection,
            villageEdgeAnchor,
            VillageEdgeLocationId,
            nameof(villageEdgeAnchor));
        ValidateBounds(
            projection,
            villageSquareAnchor,
            villageSquareBounds,
            nameof(villageSquareBounds));
        ValidateBounds(
            projection,
            villageEdgeAnchor,
            villageEdgeBounds,
            nameof(villageEdgeBounds));
        if (Overlaps(villageSquareBounds, villageEdgeBounds))
        {
            throw new ArgumentException(
                "Village transition scene areas must be distinct and non-overlapping.",
                nameof(villageEdgeBounds));
        }

        Projection = projection;
        VillageSquareAnchor = villageSquareAnchor;
        VillageSquareBounds = villageSquareBounds;
        VillageEdgeAnchor = villageEdgeAnchor;
        VillageEdgeBounds = villageEdgeBounds;
    }

    /// <summary>Gets the exact projection containing both transition anchors.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the projection's exact village-square anchor.</summary>
    public OpeningRegionAnchor VillageSquareAnchor { get; }

    /// <summary>Gets the bounded village-square scene area.</summary>
    public NavigationWorldBounds VillageSquareBounds { get; }

    /// <summary>Gets the projection's exact village-edge anchor.</summary>
    public OpeningRegionAnchor VillageEdgeAnchor { get; }

    /// <summary>Gets the bounded village-edge scene area.</summary>
    public NavigationWorldBounds VillageEdgeBounds { get; }

    private static void ValidateExactAnchor(
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        string expectedLocationId,
        string parameterName)
    {
        if (!projection.Anchors.Any(candidate => ReferenceEquals(candidate, anchor))
            || anchor.Location.Identity.Id.Value != expectedLocationId)
        {
            throw new ArgumentException(
                "Village transition anchors must be exact retained anchors for their authored locations.",
                parameterName);
        }
    }

    private static void ValidateBounds(
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        NavigationWorldBounds bounds,
        string parameterName)
    {
        NavigationWorldBounds world = projection.WorldBounds;
        if (!float.IsFinite(bounds.Left)
            || !float.IsFinite(bounds.Top)
            || !float.IsFinite(bounds.Right)
            || !float.IsFinite(bounds.Bottom)
            || bounds.Right <= bounds.Left
            || bounds.Bottom <= bounds.Top
            || bounds.Left < world.Left
            || bounds.Top < world.Top
            || bounds.Right > world.Right
            || bounds.Bottom > world.Bottom)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                bounds,
                "Village transition scene areas must be finite, positive, and inside the region.");
        }

        float clearance = projection.MinimumPathClearancePixels;
        if (anchor.Position.X < bounds.Left + clearance
            || anchor.Position.X > bounds.Right - clearance
            || anchor.Position.Y < bounds.Top + clearance
            || anchor.Position.Y > bounds.Bottom - clearance)
        {
            throw new ArgumentException(
                "Each village transition anchor must retain endpoint clearance inside its area.",
                parameterName);
        }
    }

    private static bool Overlaps(
        NavigationWorldBounds first,
        NavigationWorldBounds second) =>
        first.Left < second.Right
        && first.Right > second.Left
        && first.Top < second.Bottom
        && first.Bottom > second.Top;
}
