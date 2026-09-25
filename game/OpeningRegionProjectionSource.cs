using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Projects the exact opening topology through scene-owned gameplay-scale anchors.</summary>
public partial class OpeningRegionProjectionSource : Node2D
{
    /// <summary>Gets the authored gameplay-scale world width in pixels.</summary>
    public const float WorldWidthPixels = 1600.0f;

    /// <summary>Gets the authored gameplay-scale world height in pixels.</summary>
    public const float WorldHeightPixels = 900.0f;

    /// <summary>Gets the free-space radius required around each route endpoint.</summary>
    public const float MinimumPathClearancePixels = 24.0f;

    /// <summary>Gets the minimum direct separation between landmark anchors.</summary>
    public const float MinimumLandmarkClearancePixels = 120.0f;

    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, WorldWidthPixels, WorldHeightPixels);

    /// <summary>Gets the validated projection created from the exact topology and scene anchors.</summary>
    public OpeningRegionProjection Projection { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        Dictionary<string, Location> locationsById = topology.Locations.ToDictionary(
            location => location.Identity.Id.Value,
            StringComparer.Ordinal);
        List<OpeningRegionAnchor> anchors = [];
        foreach (Node child in GetChildren())
        {
            if (child is not Marker2D marker
                || !locationsById.TryGetValue(marker.Name.ToString(), out Location? location))
            {
                throw new InvalidOperationException(
                    "Opening-region scene anchors must be markers named for exact topology locations.");
            }

            anchors.Add(new OpeningRegionAnchor(location, ToNumerics(marker.Position)));
        }

        Projection = new OpeningRegionProjection(
            topology,
            WorldBounds,
            MinimumPathClearancePixels,
            MinimumLandmarkClearancePixels,
            anchors);
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
