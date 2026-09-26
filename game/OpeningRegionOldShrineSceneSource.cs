using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the inert old-shrine shell and concealed layered inscription.</summary>
public partial class OpeningRegionOldShrineSceneSource : Node2D
{
    /// <summary>Gets the shared width of both authored shrine openings.</summary>
    public const float OpeningWidthPixels = OpeningRegionOldShrineGeometry.OpeningWidthPixels;

    /// <summary>Gets the thickness of every collision-backed shrine wall.</summary>
    public const float WallThicknessPixels = OpeningRegionOldShrineGeometry.WallThicknessPixels;

    /// <summary>Gets the exact old-shrine binding after scene sources become ready.</summary>
    public OpeningRegionOldShrineSceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware old-shrine layout after scene sources become ready.</summary>
    public OpeningRegionOldShrineLayout Layout { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionAnchor shrine = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "old-shrine");
        Binding = new OpeningRegionOldShrineSceneBinding(
            projection,
            shrine,
            OpeningRegionOldShrineGeometry.ShrineBounds);
        Layout = new OpeningRegionOldShrineLayout(
            Binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            OpeningRegionOldShrineGeometry.SouthernRoadOpening);
    }
}
