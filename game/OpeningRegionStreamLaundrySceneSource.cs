using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the bounded stream crossing and inert abandoned-laundry layout.</summary>
public partial class OpeningRegionStreamLaundrySceneSource : Node2D
{
    /// <summary>Gets the shared width of both authored laundry openings.</summary>
    public const float OpeningWidthPixels = OpeningRegionStreamLaundryGeometry.OpeningWidthPixels;

    /// <summary>Gets the thickness of every collision-backed laundry wall section.</summary>
    public const float WallThicknessPixels = OpeningRegionStreamLaundryGeometry.WallThicknessPixels;

    /// <summary>Gets the exact stream/laundry scene binding after scene sources become ready.</summary>
    public OpeningRegionStreamLaundrySceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware stream/laundry layout after scene sources become ready.</summary>
    public OpeningRegionStreamLaundryLayout Layout { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionAnchor stream = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "stream");
        OpeningRegionAnchor laundry = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "abandoned-laundry");
        Binding = new OpeningRegionStreamLaundrySceneBinding(
            projection,
            stream,
            OpeningRegionStreamLaundryGeometry.StreamBounds,
            laundry,
            OpeningRegionStreamLaundryGeometry.LaundryBounds);
        Layout = new OpeningRegionStreamLaundryLayout(
            Binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            OpeningRegionStreamLaundryGeometry.VillageEdgeApproach,
            OpeningRegionStreamLaundryGeometry.BridgeNorthApproach,
            OpeningRegionStreamLaundryGeometry.WestStreamBank,
            OpeningRegionStreamLaundryGeometry.EastStreamBank);
    }
}
