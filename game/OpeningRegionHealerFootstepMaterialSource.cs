using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Observes only retained supported exterior materials for the newly granted Healer emitter.</summary>
public partial class OpeningRegionHealerFootstepMaterialSource : Node
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
        if (!topology || sources.Any(n => !GodotObject.IsInstanceValid(n) || !n.IsInsideTree()
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

    /// <summary>Returns a mapped exterior material only at finite supported projection-local positions.</summary>
    internal FootstepMaterial? Select(NVector world)
    {
        if (!OpeningRegionPositionalAudioProjection.Finite(world) || !Valid()) { return null; }
        Vector2 p = new(world.X, world.Y);
        Vector2 closest = NavigationServer2D.MapGetClosestPoint(map, p);
        if (!Finite(closest)) { return null; }
        Vector2 local = projection.ToLocal(p), localClosest = projection.ToLocal(closest);
        return OpeningRegionFootstepMaterialProjection.Select(new(local.X, local.Y),
            Finite(local) && Finite(localClosest) && local.DistanceSquaredTo(localClosest) <= 1);
    }
    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    private static bool Bounds(NavigationWorldBounds b, float left, float top, float right, float bottom) =>
        b.Left == left && b.Top == top && b.Right == right && b.Bottom == bottom;
}
