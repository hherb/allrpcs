namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact old-shrine anchor to one bounded scene area.</summary>
public sealed class OpeningRegionOldShrineSceneBinding
{
    private const string OldShrineLocationId = "old-shrine";

    /// <summary>Creates one bounded shrine without adding inscription discovery.</summary>
    public OpeningRegionOldShrineSceneBinding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor oldShrineAnchor,
        NavigationWorldBounds oldShrineBounds)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(oldShrineAnchor);
        if (!projection.Anchors.Any(candidate => ReferenceEquals(candidate, oldShrineAnchor))
            || oldShrineAnchor.Location.Identity.Id.Value != OldShrineLocationId)
        {
            throw new ArgumentException(
                "The shrine anchor must be the projection's exact retained old-shrine anchor.",
                nameof(oldShrineAnchor));
        }

        NavigationWorldBounds world = projection.WorldBounds;
        if (!float.IsFinite(oldShrineBounds.Left)
            || !float.IsFinite(oldShrineBounds.Top)
            || !float.IsFinite(oldShrineBounds.Right)
            || !float.IsFinite(oldShrineBounds.Bottom)
            || oldShrineBounds.Right <= oldShrineBounds.Left
            || oldShrineBounds.Bottom <= oldShrineBounds.Top
            || oldShrineBounds.Left < world.Left
            || oldShrineBounds.Top < world.Top
            || oldShrineBounds.Right > world.Right
            || oldShrineBounds.Bottom > world.Bottom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(oldShrineBounds),
                oldShrineBounds,
                "The shrine area must be finite, positive, and inside the region.");
        }

        float clearance = projection.MinimumPathClearancePixels;
        if (oldShrineAnchor.Position.X < oldShrineBounds.Left + clearance
            || oldShrineAnchor.Position.X > oldShrineBounds.Right - clearance
            || oldShrineAnchor.Position.Y < oldShrineBounds.Top + clearance
            || oldShrineAnchor.Position.Y > oldShrineBounds.Bottom - clearance)
        {
            throw new ArgumentException(
                "The shrine anchor must retain endpoint clearance inside its area.",
                nameof(oldShrineBounds));
        }

        Projection = projection;
        OldShrineAnchor = oldShrineAnchor;
        OldShrineBounds = oldShrineBounds;
    }

    /// <summary>Gets the exact opening-region projection.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the projection's exact old-shrine anchor.</summary>
    public OpeningRegionAnchor OldShrineAnchor { get; }

    /// <summary>Gets the bounded old-shrine scene area.</summary>
    public NavigationWorldBounds OldShrineBounds { get; }
}
