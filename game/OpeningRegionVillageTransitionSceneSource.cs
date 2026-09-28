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
        ConfigurePresentation();
        ConfigureBoundaries();
    }

    private void ConfigurePresentation()
    {
        GetNode<Polygon2D>("VillageSquareArea").Polygon = Rectangle(
            Binding.VillageSquareBounds);
        GetNode<Polygon2D>("VillageEdgeArea").Polygon = Rectangle(
            Binding.VillageEdgeBounds);
        GetNode<Polygon2D>("SquareGardenOpening").Polygon = HorizontalOpening(
            Layout.SquareGardenOpening);
        GetNode<Polygon2D>("SquareEdgeOpening").Polygon = VerticalOpening(
            Layout.SquareEdgeOpening);
        GetNode<Polygon2D>("EdgeSquareOpening").Polygon = VerticalOpening(
            Layout.EdgeSquareOpening);
        GetNode<Polygon2D>("EdgeStreamOpening").Polygon = HorizontalOpening(
            Layout.EdgeStreamOpening);
        GetNode<Polygon2D>("EdgeWoodlandOpening").Polygon = VerticalOpening(
            Layout.EdgeWoodlandOpening);
        GetNode<Label>("VillageSquareLabel").Position = new Vector2(
            Binding.VillageSquareBounds.Left + 12.0f,
            Binding.VillageSquareBounds.Top + 12.0f);
        GetNode<Label>("VillageEdgeLabel").Position = new Vector2(
            Binding.VillageEdgeBounds.Left + 12.0f,
            Binding.VillageEdgeBounds.Top + 12.0f);
    }

    private void ConfigureBoundaries()
    {
        float half = Layout.OpeningWidthPixels / 2.0f;
        NavigationWorldBounds square = Binding.VillageSquareBounds;
        NavigationWorldBounds edge = Binding.VillageEdgeBounds;
        List<OpeningRegionBoundarySegment> segments =
        [
            new(new(square.Left, square.Top), new(square.Right, square.Top)),
            new(new(square.Right, square.Top), new(square.Right, square.Bottom)),
            new(
                new(square.Left, square.Bottom),
                new(MathF.Min(
                    Layout.SquareGardenOpening.X,
                    Layout.SquareSickhouseOpening.X) - half, square.Bottom)),
            new(
                new(square.Left, square.Top),
                new(square.Left, MathF.Min(
                    Layout.SquareEdgeOpening.Y,
                    Layout.SquareSouthernRoadOpening.Y) - half)),
            new(new(edge.Left, edge.Top), new(Layout.EdgeStreamOpening.X - half, edge.Top)),
            new(new(Layout.EdgeStreamOpening.X + half, edge.Top), new(edge.Right, edge.Top)),
            new(new(edge.Left, edge.Bottom), new(edge.Right, edge.Bottom)),
            new(new(edge.Left, edge.Top), new(edge.Left, Layout.EdgeWoodlandOpening.Y - half)),
            new(new(edge.Left, Layout.EdgeWoodlandOpening.Y + half), new(edge.Left, edge.Bottom)),
            new(new(edge.Right, edge.Top), new(edge.Right, Layout.EdgeSquareOpening.Y - half)),
            new(new(edge.Right, Layout.EdgeSquareOpening.Y + half), new(edge.Right, edge.Bottom)),
        ];
        Node2D owner = GetNode<Node2D>("Boundaries");
        if (owner.GetChildCount() != segments.Count)
        {
            throw new InvalidOperationException(
                "The authored village-boundary count must match the derived layout.");
        }

        for (int index = 0; index < segments.Count; index++)
        {
            StaticBody2D body = owner.GetChild<StaticBody2D>(index);
            if (body.GetChildCount() != 1
                || body.GetChild(0) is not CollisionShape2D collision)
            {
                throw new InvalidOperationException(
                    "Each village boundary must contain one collision shape.");
            }

            body.Position = Vector2.Zero;
            collision.Shape = new SegmentShape2D
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

    private static Vector2[] HorizontalOpening(System.Numerics.Vector2 center)
    {
        float half = OpeningWidthPixels / 2.0f;
        return
        [
            new(center.X - half, center.Y - WallThicknessPixels),
            new(center.X + half, center.Y - WallThicknessPixels),
            new(center.X + half, center.Y + WallThicknessPixels),
            new(center.X - half, center.Y + WallThicknessPixels),
        ];
    }

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
