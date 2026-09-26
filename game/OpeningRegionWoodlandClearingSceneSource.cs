using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the collision-aware clearing and its inert concealed specimen marker.</summary>
public partial class OpeningRegionWoodlandClearingSceneSource : Node2D
{
    /// <summary>Gets the shared width of all authored clearing openings.</summary>
    public const float OpeningWidthPixels = OpeningRegionWoodlandClearingGeometry.OpeningWidthPixels;

    /// <summary>Gets the thickness of each collision-backed tree boundary.</summary>
    public const float WallThicknessPixels =
        OpeningRegionWoodlandClearingGeometry.WallThicknessPixels;

    /// <summary>Gets the exact clearing binding after scene sources become ready.</summary>
    public OpeningRegionWoodlandClearingSceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware clearing layout after scene sources become ready.</summary>
    public OpeningRegionWoodlandClearingLayout Layout { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionAnchor clearing = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "woodland-clearing");
        Binding = new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            clearing,
            OpeningRegionWoodlandClearingGeometry.ClearingBounds);
        Layout = new OpeningRegionWoodlandClearingLayout(
            Binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            OpeningRegionWoodlandClearingGeometry.VillageEdgeOpening,
            OpeningRegionWoodlandClearingGeometry.ClosedForestPathOpening);
    }
}
