using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact stream and abandoned-laundry anchors to distinct scene areas.</summary>
public sealed class OpeningRegionStreamLaundrySceneBinding
{
    private const string StreamLocationId = "stream";
    private const string AbandonedLaundryLocationId = "abandoned-laundry";

    /// <summary>Creates one bounded pair without adding investigation content.</summary>
    public OpeningRegionStreamLaundrySceneBinding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor streamAnchor,
        NavigationWorldBounds streamBounds,
        OpeningRegionAnchor abandonedLaundryAnchor,
        NavigationWorldBounds abandonedLaundryBounds)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(streamAnchor);
        ArgumentNullException.ThrowIfNull(abandonedLaundryAnchor);
        ValidateExactAnchor(projection, streamAnchor, StreamLocationId, nameof(streamAnchor));
        ValidateExactAnchor(
            projection,
            abandonedLaundryAnchor,
            AbandonedLaundryLocationId,
            nameof(abandonedLaundryAnchor));
        ValidateBounds(projection, streamAnchor, streamBounds, nameof(streamBounds));
        ValidateBounds(
            projection,
            abandonedLaundryAnchor,
            abandonedLaundryBounds,
            nameof(abandonedLaundryBounds));
        if (Overlaps(streamBounds, abandonedLaundryBounds))
        {
            throw new ArgumentException(
                "Stream and abandoned-laundry scene areas must be distinct and non-overlapping.",
                nameof(abandonedLaundryBounds));
        }

        Projection = projection;
        StreamAnchor = streamAnchor;
        StreamBounds = streamBounds;
        AbandonedLaundryAnchor = abandonedLaundryAnchor;
        AbandonedLaundryBounds = abandonedLaundryBounds;
    }

    /// <summary>Gets the exact projection containing both retained anchors.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the projection's exact stream anchor.</summary>
    public OpeningRegionAnchor StreamAnchor { get; }

    /// <summary>Gets the bounded bridge and stream-crossing scene area.</summary>
    public NavigationWorldBounds StreamBounds { get; }

    /// <summary>Gets the projection's exact abandoned-laundry anchor.</summary>
    public OpeningRegionAnchor AbandonedLaundryAnchor { get; }

    /// <summary>Gets the bounded abandoned-laundry scene area.</summary>
    public NavigationWorldBounds AbandonedLaundryBounds { get; }

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
                "Stream/laundry anchors must be exact retained anchors for their authored locations.",
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
                "Stream/laundry scene areas must be finite, positive, and inside the region.");
        }

        float clearance = projection.MinimumPathClearancePixels;
        if (anchor.Position.X < bounds.Left + clearance
            || anchor.Position.X > bounds.Right - clearance
            || anchor.Position.Y < bounds.Top + clearance
            || anchor.Position.Y > bounds.Bottom - clearance)
        {
            throw new ArgumentException(
                "Each stream/laundry anchor must retain endpoint clearance inside its area.",
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
