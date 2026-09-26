namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact southern road and wagon to the retained old-shrine layout.</summary>
public sealed class OpeningRegionSouthernRoadWagonSceneBinding
{
    private const string SouthernRoadLocationId = "southern-road";
    private const string AbandonedWagonLocationId = "abandoned-wagon";

    /// <summary>Creates one exact road/wagon binding without activating wagon discovery.</summary>
    public OpeningRegionSouthernRoadWagonSceneBinding(
        OpeningRegionOldShrineLayout oldShrineLayout,
        OpeningRegionAnchor southernRoadAnchor,
        OpeningRegionAnchor abandonedWagonAnchor,
        NavigationWorldBounds abandonedWagonBounds)
    {
        ArgumentNullException.ThrowIfNull(oldShrineLayout);
        ArgumentNullException.ThrowIfNull(southernRoadAnchor);
        ArgumentNullException.ThrowIfNull(abandonedWagonAnchor);
        OpeningRegionProjection projection = oldShrineLayout.SceneBinding.Projection;
        ValidateAnchor(
            projection,
            southernRoadAnchor,
            SouthernRoadLocationId,
            nameof(southernRoadAnchor));
        ValidateAnchor(
            projection,
            abandonedWagonAnchor,
            AbandonedWagonLocationId,
            nameof(abandonedWagonAnchor));
        if (!HasExactConnection(
                projection,
                oldShrineLayout.SceneBinding.OldShrineAnchor,
                southernRoadAnchor)
            || !HasExactConnection(projection, southernRoadAnchor, abandonedWagonAnchor))
        {
            throw new ArgumentException(
                "The road and wagon must retain both exact projected topology connections.");
        }

        ValidateWagonBounds(
            projection,
            oldShrineLayout.SceneBinding.OldShrineBounds,
            abandonedWagonAnchor,
            abandonedWagonBounds);
        OldShrineLayout = oldShrineLayout;
        Projection = projection;
        SouthernRoadAnchor = southernRoadAnchor;
        AbandonedWagonAnchor = abandonedWagonAnchor;
        AbandonedWagonBounds = abandonedWagonBounds;
    }

    /// <summary>Gets the exact retained old-shrine layout.</summary>
    public OpeningRegionOldShrineLayout OldShrineLayout { get; }

    /// <summary>Gets the exact opening-region projection.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the projection's exact southern-road anchor.</summary>
    public OpeningRegionAnchor SouthernRoadAnchor { get; }

    /// <summary>Gets the projection's exact abandoned-wagon anchor.</summary>
    public OpeningRegionAnchor AbandonedWagonAnchor { get; }

    /// <summary>Gets the bounded inert wagon area.</summary>
    public NavigationWorldBounds AbandonedWagonBounds { get; }

    private static void ValidateAnchor(
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        string expectedLocationId,
        string parameterName)
    {
        if (!projection.Anchors.Any(candidate => ReferenceEquals(candidate, anchor))
            || anchor.Location.Identity.Id.Value != expectedLocationId)
        {
            throw new ArgumentException(
                "Road/wagon anchors must be exact retained projection anchors in their roles.",
                parameterName);
        }
    }

    private static bool HasExactConnection(
        OpeningRegionProjection projection,
        OpeningRegionAnchor first,
        OpeningRegionAnchor second) => projection.Connections.Any(connection =>
            (ReferenceEquals(connection.FirstAnchor, first)
                && ReferenceEquals(connection.SecondAnchor, second))
            || (ReferenceEquals(connection.FirstAnchor, second)
                && ReferenceEquals(connection.SecondAnchor, first)));

    private static void ValidateWagonBounds(
        OpeningRegionProjection projection,
        NavigationWorldBounds shrineBounds,
        OpeningRegionAnchor wagon,
        NavigationWorldBounds bounds)
    {
        NavigationWorldBounds world = projection.WorldBounds;
        if (!IsFinitePositive(bounds)
            || bounds.Left < world.Left
            || bounds.Top < world.Top
            || bounds.Right > world.Right
            || bounds.Bottom > world.Bottom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bounds),
                bounds,
                "The wagon area must be finite, positive, and inside the region.");
        }

        if (Overlaps(shrineBounds, bounds))
        {
            throw new ArgumentException(
                "The wagon shell must not overlap the retained old-shrine area.",
                nameof(bounds));
        }

        float clearance = projection.MinimumPathClearancePixels;
        if (wagon.Position.X < bounds.Left + clearance
            || wagon.Position.X > bounds.Right - clearance
            || wagon.Position.Y < bounds.Top + clearance
            || wagon.Position.Y > bounds.Bottom - clearance)
        {
            throw new ArgumentException(
                "The wagon anchor must retain endpoint clearance inside its shell.",
                nameof(bounds));
        }
    }

    private static bool IsFinitePositive(NavigationWorldBounds bounds) =>
        float.IsFinite(bounds.Left)
        && float.IsFinite(bounds.Top)
        && float.IsFinite(bounds.Right)
        && float.IsFinite(bounds.Bottom)
        && bounds.Right > bounds.Left
        && bounds.Bottom > bounds.Top;

    private static bool Overlaps(NavigationWorldBounds first, NavigationWorldBounds second) =>
        first.Left < second.Right
        && first.Right > second.Left
        && first.Top < second.Bottom
        && first.Bottom > second.Top;
}
