using System.Numerics;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Navigation;

/// <summary>
/// Projects one exact opening topology into bounded gameplay-scale scene anchors.
/// </summary>
public sealed class OpeningRegionProjection
{
    /// <summary>
    /// Creates a projection only when every exact topology location has one separated anchor.
    /// </summary>
    public OpeningRegionProjection(
        OpeningRegionSnapshot topology,
        NavigationWorldBounds worldBounds,
        float minimumPathClearancePixels,
        float minimumLandmarkClearancePixels,
        IEnumerable<OpeningRegionAnchor> anchors)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(anchors);
        ValidateClearance(minimumPathClearancePixels, nameof(minimumPathClearancePixels));
        ValidateClearance(minimumLandmarkClearancePixels, nameof(minimumLandmarkClearancePixels));

        List<OpeningRegionAnchor> suppliedAnchors = [.. anchors];
        if (suppliedAnchors.Count != topology.Locations.Count)
        {
            throw new ArgumentException(
                "An opening-region projection requires one anchor per exact topology location.",
                nameof(anchors));
        }

        Dictionary<Location, OpeningRegionAnchor> anchorsByLocation =
            new(ReferenceEqualityComparer.Instance);
        foreach (OpeningRegionAnchor anchor in suppliedAnchors)
        {
            if (anchor is null
                || !topology.Locations.Any(location => ReferenceEquals(location, anchor.Location))
                || !anchorsByLocation.TryAdd(anchor.Location, anchor))
            {
                throw new ArgumentException(
                    "Projection anchors must be unique and use exact retained topology locations.",
                    nameof(anchors));
            }

            ValidateAnchorBounds(
                worldBounds,
                minimumPathClearancePixels,
                anchor.Position);
        }

        List<OpeningRegionAnchor> orderedAnchors = topology.Locations
            .Select(location => anchorsByLocation[location])
            .ToList();
        ValidateLandmarkClearance(orderedAnchors, minimumLandmarkClearancePixels);

        List<OpeningRegionProjectedConnection> projectedConnections = topology.Connections
            .Select(connection => new OpeningRegionProjectedConnection(
                connection,
                anchorsByLocation[connection.First],
                anchorsByLocation[connection.Second]))
            .ToList();

        Topology = topology;
        WorldBounds = worldBounds;
        MinimumPathClearancePixels = minimumPathClearancePixels;
        MinimumLandmarkClearancePixels = minimumLandmarkClearancePixels;
        Anchors = orderedAnchors.AsReadOnly();
        Connections = projectedConnections.AsReadOnly();
    }

    /// <summary>Gets the exact authored topology projected into the scene.</summary>
    public OpeningRegionSnapshot Topology { get; }

    /// <summary>Gets the explicit gameplay-scale scene bounds in pixels.</summary>
    public NavigationWorldBounds WorldBounds { get; }

    /// <summary>Gets the required free-space radius around every route endpoint.</summary>
    public float MinimumPathClearancePixels { get; }

    /// <summary>Gets the minimum direct separation between landmark anchors.</summary>
    public float MinimumLandmarkClearancePixels { get; }

    /// <summary>Gets exact anchors in stable topology-location order.</summary>
    public IReadOnlyList<OpeningRegionAnchor> Anchors { get; }

    /// <summary>Gets projected connections in stable exact topology order.</summary>
    public IReadOnlyList<OpeningRegionProjectedConnection> Connections { get; }

    private static void ValidateClearance(float clearance, string parameterName)
    {
        if (!float.IsFinite(clearance) || clearance <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                clearance,
                "Opening-region clearances must be finite and positive.");
        }
    }

    private static void ValidateAnchorBounds(
        NavigationWorldBounds worldBounds,
        float minimumPathClearancePixels,
        Vector2 position)
    {
        _ = NavigationRequestValidator.Create(worldBounds, position, position);
        if (position.X < worldBounds.Left + minimumPathClearancePixels
            || position.X > worldBounds.Right - minimumPathClearancePixels
            || position.Y < worldBounds.Top + minimumPathClearancePixels
            || position.Y > worldBounds.Bottom - minimumPathClearancePixels)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "An opening-region anchor must retain minimum path clearance inside the bounds.");
        }
    }

    private static void ValidateLandmarkClearance(
        IReadOnlyList<OpeningRegionAnchor> anchors,
        float minimumLandmarkClearancePixels)
    {
        for (int firstIndex = 0; firstIndex < anchors.Count; firstIndex++)
        {
            for (int secondIndex = firstIndex + 1; secondIndex < anchors.Count; secondIndex++)
            {
                if (Vector2.Distance(
                    anchors[firstIndex].Position,
                    anchors[secondIndex].Position) < minimumLandmarkClearancePixels)
                {
                    throw new ArgumentException(
                        "Opening-region anchors must retain minimum landmark clearance.",
                        nameof(anchors));
                }
            }
        }
    }
}
