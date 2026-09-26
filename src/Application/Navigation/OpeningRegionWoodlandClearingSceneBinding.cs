using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact woodland-clearing anchor to one bounded scene area.</summary>
public sealed class OpeningRegionWoodlandClearingSceneBinding
{
    private const string WoodlandClearingLocationId = "woodland-clearing";

    /// <summary>Creates one bounded clearing without adding discovery behavior.</summary>
    public OpeningRegionWoodlandClearingSceneBinding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor woodlandClearingAnchor,
        NavigationWorldBounds woodlandClearingBounds)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(woodlandClearingAnchor);
        if (!projection.Anchors.Any(candidate => ReferenceEquals(
                candidate,
                woodlandClearingAnchor))
            || woodlandClearingAnchor.Location.Identity.Id.Value != WoodlandClearingLocationId)
        {
            throw new ArgumentException(
                "The clearing anchor must be the projection's exact retained woodland anchor.",
                nameof(woodlandClearingAnchor));
        }

        ValidateBounds(projection, woodlandClearingAnchor, woodlandClearingBounds);
        Projection = projection;
        WoodlandClearingAnchor = woodlandClearingAnchor;
        WoodlandClearingBounds = woodlandClearingBounds;
    }

    /// <summary>Gets the exact projection containing the retained clearing anchor.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the projection's exact woodland-clearing anchor.</summary>
    public OpeningRegionAnchor WoodlandClearingAnchor { get; }

    /// <summary>Gets the bounded woodland-clearing scene area.</summary>
    public NavigationWorldBounds WoodlandClearingBounds { get; }

    private static void ValidateBounds(
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        NavigationWorldBounds bounds)
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
                nameof(bounds),
                bounds,
                "The clearing area must be finite, positive, and inside the region.");
        }

        float clearance = projection.MinimumPathClearancePixels;
        if (anchor.Position.X < bounds.Left + clearance
            || anchor.Position.X > bounds.Right - clearance
            || anchor.Position.Y < bounds.Top + clearance
            || anchor.Position.Y > bounds.Bottom - clearance)
        {
            throw new ArgumentException(
                "The clearing anchor must retain endpoint clearance inside its area.",
                nameof(bounds));
        }
    }
}
