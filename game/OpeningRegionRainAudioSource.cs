using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Binds the exact unchanged woodland-clearing anchor and retained exterior listener support without gameplay authority.</summary>
public partial class OpeningRegionRainAudioSource : Node
{
    private OpeningRegionProjectionSource projection = null!;
    private GreyboxNavigation navigation = null!;
    private OpeningRegionFirstPatientSceneSource sickhouse = null!;
    private OpeningRegionStreamLaundrySceneSource laundry = null!;
    private OpeningRegionProjection retainedProjection = null!;
    private OpeningRegionSickhouseLayout? retainedSickhouse;
    private OpeningRegionStreamLaundryLayout? retainedLaundry;
    private NavigationPolygon polygon = null!;
    private Vector2[] vertices = [];
    private int[][] indices = [];
    private Rid map;
    private Node2D[] sources = [];
    private Transform2D[] transforms = [];
    private bool topology;
    private OpeningRegionWoodlandClearingSceneSource clearing = null!;
    private OpeningRegionWoodlandClearingSceneBinding binding = null!;
    private OpeningRegionWoodlandClearingLayout layout = null!;
    private readonly List<Func<bool>> physical = [];

    /// <inheritdoc />
    public override void _Ready()
    {
        projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        sickhouse = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource");
        laundry = GetNode<OpeningRegionStreamLaundrySceneSource>("../OpeningRegionStreamLaundrySceneSource");
        retainedProjection = projection.Projection; polygon = navigation.NavigationPolygon;
        map = navigation.GetWorld2D().NavigationMap; vertices = polygon.GetVertices();
        indices = Enumerable.Range(0, polygon.GetPolygonCount()).Select(polygon.GetPolygon).ToArray();
        sources = [projection, navigation, sickhouse, laundry];
        transforms = sources.Select(n => n.GlobalTransform).ToArray();
        clearing = GetNode<OpeningRegionWoodlandClearingSceneSource>("../OpeningRegionWoodlandClearingSceneSource");
        binding = clearing.Binding; layout = clearing.Layout;
        CapturePhysical(clearing);
        topology = retainedProjection.Anchors.Count == OpeningRegionLayoutGeometry.Anchors.Count
            && retainedProjection.Anchors.All(a => OpeningRegionLayoutGeometry.Anchors.TryGetValue(
                a.Location.Identity.Id.Value, out Vector2 p) && p == new Vector2(a.Position.X, a.Position.Y))
            && retainedProjection.Connections.Count == OpeningRegionLayoutGeometry.Connections.Count
            && retainedProjection.Connections.All(c => OpeningRegionLayoutGeometry.Connections.Contains(
                (c.FirstAnchor.Location.Identity.Id.Value, c.SecondAnchor.Location.Identity.Id.Value)));
    }

    /// <summary>Requires exact active owners and unchanged navigation/physical contents, without mutation.</summary>
    internal bool Valid()
    {
        if (!ClearingValid() || !topology || sources.Any(n => !GodotObject.IsInstanceValid(n) || !n.IsInsideTree()
            || !n.CanProcess() || !n.IsVisibleInTree())
            || sources.Where((n, i) => !ReferenceEquals(GetNodeOrNull<Node>("../" + n.Name), n)
                || n.GlobalTransform != transforms[i]).Any()
            || !ReferenceEquals(projection.Projection, retainedProjection) || !navigation.Enabled
            || navigation.NavigationPolygon != polygon || !polygon.GetVertices().SequenceEqual(vertices)
            || polygon.GetPolygonCount() != indices.Length
            || indices.Where((p, i) => !p.SequenceEqual(polygon.GetPolygon(i))).Any()
            || NavigationServer2D.RegionGetMap(navigation.GetRid()) != map
            || !map.IsValid || !NavigationServer2D.MapIsActive(map)
            || NavigationServer2D.MapGetIterationId(map) == 0
            || NavigationServer2D.RegionGetIterationId(navigation.GetRid()) == 0
            || sickhouse.Layout is null || laundry.Layout is null) { return false; }
        retainedSickhouse ??= sickhouse.Layout; retainedLaundry ??= laundry.Layout;
        return ReferenceEquals(sickhouse.Layout, retainedSickhouse) && ReferenceEquals(laundry.Layout, retainedLaundry)
            && ReferenceEquals(retainedSickhouse.FirstPatientBinding.Projection, retainedProjection)
            && ReferenceEquals(retainedLaundry.SceneBinding.Projection, retainedProjection)
            && Bounds(retainedSickhouse.RoomBounds, 2480, 1310, 2800, 1502)
            && retainedSickhouse.EntranceCenter == new NVector(2528, 1310)
            && Bounds(retainedLaundry.SceneBinding.AbandonedLaundryBounds, 1420, 120, 1580, 360)
            && retainedSickhouse.WallThicknessPixels == 8 && retainedLaundry.WallThicknessPixels == 8;
    }

    /// <summary>Admits only finite supported exterior listener positions on the unchanged active map.</summary>
    internal bool ListenerSupported(NVector world)
    {
        if (!OpeningRegionPositionalAudioProjection.Finite(world) || !Valid()) { return false; }
        Vector2 p = new(world.X, world.Y);
        Vector2 closest = NavigationServer2D.MapGetClosestPoint(map, p);
        if (!Finite(closest)) { return false; }
        Vector2 local = projection.ToLocal(p), localClosest = projection.ToLocal(closest);
        return RainSpatialProjection.Exterior(new(local.X, local.Y),
            Finite(local) && Finite(localClosest) && local.DistanceSquaredTo(localClosest) <= 1);
    }

    private bool ClearingValid() => GodotObject.IsInstanceValid(clearing) && clearing.IsInsideTree()
        && clearing.CanProcess() && clearing.IsVisibleInTree() && clearing.GlobalTransform == Transform2D.Identity
        && ReferenceEquals(GetNodeOrNull<Node>("../OpeningRegionWoodlandClearingSceneSource"), clearing)
        && ReferenceEquals(clearing.Binding, binding) && ReferenceEquals(clearing.Layout, layout)
        && ReferenceEquals(layout.SceneBinding, binding) && ReferenceEquals(binding.Projection, retainedProjection)
        && binding.WoodlandClearingAnchor.Location.Identity.Id.Value == "woodland-clearing"
        && binding.WoodlandClearingAnchor.Position == RainSpatialProjection.EmitterPosition
        && retainedProjection.Anchors.Any(a => ReferenceEquals(a, binding.WoodlandClearingAnchor))
        && Bounds(binding.WoodlandClearingBounds, 600, 200, 1248, 450)
        && layout.OpeningWidthPixels == 96 && layout.WallThicknessPixels == 8
        && layout.VillageEdgeOpening == new NVector(1128, 450)
        && layout.ClosedForestPathOpening == new NVector(744, 200)
        && clearing.GetNodeOrNull<Node2D>("TreeBoundaries") is { } boundaries && boundaries.GetChildCount() == 8
        && physical.All(check => check());

    // Capture geometry without consulting the inert concealed marker or any discovery state.
    private void CapturePhysical(Node2D node)
    {
        Node[] children = node.GetChildren().ToArray(); Transform2D transform = node.Transform;
        bool visible = node.Visible; Node? parent = node.GetParent(); int slot = node.GetIndex();
        physical.Add(() => GodotObject.IsInstanceValid(node) && node.IsInsideTree() && node.CanProcess()
            && node.GetParent() == parent && node.GetIndex() == slot && node.Transform == transform
            && node.Visible == visible && node.GetChildren().SequenceEqual(children));
        if (node is Line2D line)
        {
            Vector2[] points = line.Points; float width = line.Width; bool closed = line.Closed;
            physical.Add(() => line.Points.SequenceEqual(points) && line.Width == width && line.Closed == closed);
        }
        if (node is Polygon2D shell)
        {
            Vector2[] points = shell.Polygon; Vector2[] uv = shell.UV; Color color = shell.Color;
            Texture2D? texture = shell.Texture;
            physical.Add(() => shell.Polygon.SequenceEqual(points) && shell.UV.SequenceEqual(uv)
                && shell.Color == color && ReferenceEquals(shell.Texture, texture));
        }
        if (node is StaticBody2D body)
        {
            uint layer = body.CollisionLayer, mask = body.CollisionMask;
            physical.Add(() => body.CollisionLayer == layer && body.CollisionMask == mask);
        }
        if (node is CollisionShape2D collision)
        {
            Shape2D shape = collision.Shape; bool disabled = collision.Disabled;
            physical.Add(() => ReferenceEquals(collision.Shape, shape) && collision.Disabled == disabled);
            if (shape is SegmentShape2D segment)
            {
                Vector2 start = segment.A, end = segment.B;
                physical.Add(() => segment.A == start && segment.B == end);
            }
        }
        if (node is Sprite2D sprite)
        {
            Texture2D texture = sprite.Texture; Rect2 region = sprite.RegionRect;
            bool enabled = sprite.RegionEnabled, centered = sprite.Centered; Vector2 offset = sprite.Offset;
            physical.Add(() => ReferenceEquals(sprite.Texture, texture) && sprite.RegionRect == region
                && sprite.RegionEnabled == enabled && sprite.Centered == centered && sprite.Offset == offset);
        }
        foreach (Node2D child in children.OfType<Node2D>().Where(child => child.Name != "ConcealedRareSpecimen"))
        { CapturePhysical(child); }
    }

    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    private static bool Bounds(NavigationWorldBounds b, float left, float top, float right, float bottom) =>
        b.Left == left && b.Top == top && b.Right == right && b.Bottom == bottom;
}
