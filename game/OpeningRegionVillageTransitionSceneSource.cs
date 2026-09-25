using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the bounded village-square and village-edge transition layout.</summary>
public partial class OpeningRegionVillageTransitionSceneSource : Node2D
{
    /// <summary>Gets the shared width of every authored transition opening.</summary>
    public const float OpeningWidthPixels =
        OpeningRegionVillageTransitionGeometry.OpeningWidthPixels;

    /// <summary>Gets the thickness of every collision-backed boundary section.</summary>
    public const float WallThicknessPixels =
        OpeningRegionVillageTransitionGeometry.WallThicknessPixels;

    /// <summary>Gets the exact village scene binding after scene sources become ready.</summary>
    public OpeningRegionVillageTransitionSceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware transition layout after scene sources become ready.</summary>
    public OpeningRegionVillageTransitionLayout Layout { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionAnchor villageSquare = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "village-square");
        OpeningRegionAnchor villageEdge = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "village-edge");
        Binding = new OpeningRegionVillageTransitionSceneBinding(
            projection,
            villageSquare,
            OpeningRegionVillageTransitionGeometry.VillageSquareBounds,
            villageEdge,
            OpeningRegionVillageTransitionGeometry.VillageEdgeBounds);
        Layout = new OpeningRegionVillageTransitionLayout(
            Binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            OpeningRegionVillageTransitionGeometry.GardenApproach);
    }
}
