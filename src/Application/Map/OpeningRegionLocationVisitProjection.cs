using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Projects a bounded exact anchor visit without retaining scene coordinates.</summary>
public static class OpeningRegionLocationVisitProjection
{
    /// <summary>Returns a safe visit only when the Seeker is within the supplied positive bound.</summary>
    public static OpeningRegionLocationVisit? Project(
        OpeningRegionProjection projection,
        CharacterId seekerId,
        OpeningRegionAnchor anchor,
        Vector2 seekerPosition,
        float maximumDistancePixels)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(anchor);

        if (!float.IsFinite(seekerPosition.X) || !float.IsFinite(seekerPosition.Y))
        {
            throw new ArgumentOutOfRangeException(
                nameof(seekerPosition),
                seekerPosition,
                "A map visit requires a finite Seeker position.");
        }

        float unambiguousDistanceLimit = projection.MinimumLandmarkClearancePixels / 2.0f;
        if (!float.IsFinite(maximumDistancePixels)
            || maximumDistancePixels <= 0.0f
            || maximumDistancePixels >= unambiguousDistanceLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDistancePixels),
                maximumDistancePixels,
                "A map-visit bound must be positive and less than half the landmark clearance.");
        }

        if (!projection.Anchors.Any(candidate => ReferenceEquals(candidate, anchor)))
        {
            throw new ArgumentException(
                "A map visit requires an exact retained opening-region anchor.",
                nameof(anchor));
        }

        if (Vector2.Distance(seekerPosition, anchor.Position) > maximumDistancePixels)
        {
            return null;
        }

        return new OpeningRegionLocationVisit(
            projection,
            seekerId,
            anchor.Location.Identity.Id);
    }
}
