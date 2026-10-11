using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Binds the exact unchanged garden planting and retained exterior listener support without gameplay authority.</summary>
public partial class OpeningRegionFoliageAudioSource : Node
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
    private Node2D garden = null!;
    private Sprite2D sprite = null!;
    private Texture2D texture = null!;
    private int spriteIndex;
    private Transform2D gardenTransform;
    private Transform2D spriteTransform;
    private Node[] gardenChildren = [];


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
        garden = GetNode<Node2D>("../OpeningRegionRemedyPreparationSceneSource");
        sprite = garden.GetNode<Sprite2D>("BroadleafFoliageTopLeft");
        texture = sprite.Texture; spriteIndex = sprite.GetIndex();
        gardenTransform = garden.GlobalTransform; spriteTransform = sprite.Transform;
        gardenChildren = garden.GetChildren().ToArray();
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
        if (!GardenValid() || !topology || sources.Any(n => !GodotObject.IsInstanceValid(n) || !n.IsInsideTree()
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
        return FoliageSpatialProjection.Exterior(new(local.X, local.Y),
            Finite(local) && Finite(localClosest) && local.DistanceSquaredTo(localClosest) <= 1);
    }

    private bool GardenValid() => GodotObject.IsInstanceValid(garden) && garden.IsInsideTree()
        && garden.CanProcess() && garden.IsVisibleInTree() && garden.GlobalTransform == gardenTransform
        && garden.Position == new Vector2(2300, 1300) && garden.Rotation == 0 && garden.Scale == Vector2.One
        && ReferenceEquals(GetNodeOrNull<Node>("../OpeningRegionRemedyPreparationSceneSource"), garden)
        && GodotObject.IsInstanceValid(sprite) && sprite.IsInsideTree() && sprite.CanProcess() && sprite.IsVisibleInTree()
        && ReferenceEquals(garden.GetNodeOrNull<Sprite2D>("BroadleafFoliageTopLeft"), sprite)
        && sprite.GetParent() == garden && sprite.GetIndex() == spriteIndex
        && garden.GetChildren().SequenceEqual(gardenChildren)
        && sprite.GetChildCount() == 0 && sprite.Transform == spriteTransform
        && sprite.Position == new Vector2(-68, 28) && sprite.Rotation == 0 && sprite.Scale == Vector2.One
        && sprite.GlobalPosition == new Vector2(2232, 1328) && sprite.Offset == new Vector2(-36, -53)
        && !sprite.Centered && !sprite.RegionEnabled && sprite.ZIndex == 0 && !sprite.ShowBehindParent
        && ReferenceEquals(sprite.Texture, texture) && texture.GetSize() == new Vector2(72, 56)
        && texture.ResourcePath == "res://art/runtime/plants/foliage-broadleaf.png";

    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    private static bool Bounds(NavigationWorldBounds b, float left, float top, float right, float bottom) =>
        b.Left == left && b.Top == top && b.Right == right && b.Bottom == bottom;
}
