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
        ConfigurePresentation();
        ConfigureCollisionGeometry();
    }

    private void ConfigurePresentation()
    {
        GetNode<Polygon2D>("StreamCrossingArea").Polygon = Rectangle(Binding.StreamBounds);
        GetNode<Polygon2D>("LaundryArea").Polygon = Rectangle(
            Binding.AbandonedLaundryBounds);
        GetNode<Polygon2D>("LaundryWoodlandOpening").Polygon = VerticalOpening(
            Layout.LaundryWoodlandOpening);
        GetNode<Polygon2D>("LaundryStreamOpening").Polygon = VerticalOpening(
            Layout.LaundryStreamOpening);
        GetNode<Label>("LaundryLabel").Position = new Vector2(
            Binding.AbandonedLaundryBounds.Left + 12.0f,
            Binding.AbandonedLaundryBounds.Top + 12.0f);
    }

    private void ConfigureCollisionGeometry()
    {
        GetNode<CollisionPolygon2D>("../StreamCollision/WestCollision").Polygon =
            [.. Layout.WestStreamBank.Select(ToGodot)];
        GetNode<CollisionPolygon2D>("../StreamCollision/EastCollision").Polygon =
            [.. Layout.EastStreamBank.Select(ToGodot)];
        float half = Layout.OpeningWidthPixels / 2.0f;
        NavigationWorldBounds bounds = Binding.AbandonedLaundryBounds;
        float westOpeningY = Layout.LaundryWoodlandOpening.Y;
        float eastOpeningY = Layout.LaundryStreamOpening.Y;
        List<OpeningRegionBoundarySegment> segments =
        [
            new(new(bounds.Left, bounds.Top), new(bounds.Right, bounds.Top)),
            new(new(bounds.Left, bounds.Bottom), new(bounds.Right, bounds.Bottom)),
            new(new(bounds.Left, bounds.Top), new(bounds.Left, westOpeningY - half)),
            new(new(bounds.Left, westOpeningY + half), new(bounds.Left, bounds.Bottom)),
            new(new(bounds.Right, bounds.Top), new(bounds.Right, eastOpeningY - half)),
            new(new(bounds.Right, eastOpeningY + half), new(bounds.Right, bounds.Bottom)),
        ];
        ConfigureSegments(GetNode<Node2D>("LaundryBoundaries"), segments);
    }

    private static void ConfigureSegments(
        Node2D owner,
        IReadOnlyList<OpeningRegionBoundarySegment> segments)
    {
        if (owner.GetChildCount() != segments.Count)
        {
            throw new InvalidOperationException(
                "The authored laundry-boundary count must match the derived layout.");
        }

        for (int index = 0; index < segments.Count; index++)
        {
            StaticBody2D body = owner.GetChild<StaticBody2D>(index);
            body.Position = Vector2.Zero;
            body.GetChild<CollisionShape2D>(0).Shape = new SegmentShape2D
            {
                A = ToGodot(segments[index].Start),
                B = ToGodot(segments[index].End),
            };
        }
    }

    private static Vector2 ToGodot(System.Numerics.Vector2 position) =>
        new(position.X, position.Y);

    private static Vector2[] Rectangle(NavigationWorldBounds bounds) =>
    [
        new(bounds.Left, bounds.Top),
        new(bounds.Right, bounds.Top),
        new(bounds.Right, bounds.Bottom),
        new(bounds.Left, bounds.Bottom),
    ];

    private static Vector2[] VerticalOpening(System.Numerics.Vector2 center)
    {
        float half = OpeningWidthPixels / 2.0f;
        return
        [
            new(center.X - WallThicknessPixels, center.Y - half),
            new(center.X + WallThicknessPixels, center.Y - half),
            new(center.X + WallThicknessPixels, center.Y + half),
            new(center.X - WallThicknessPixels, center.Y + half),
        ];
    }
}
