using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Observes the retained navigation surface and exact geometry for audio-only material selection.</summary>
public partial class OpeningRegionFootstepMaterialSource : Node
{
    private OpeningRegionProjectionSource projection = null!;
    private GreyboxNavigation navigation = null!;
    private OpeningRegionProjection retainedProjection = null!;
    private NavigationPolygon retainedPolygon = null!;
    private bool valid;
    internal string Status { get; private set; } = "not selected";

    /// <inheritdoc />
    public override void _Ready()
    {
        projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        retainedProjection = projection.Projection;
        retainedPolygon = navigation.NavigationPolygon;
        valid = retainedProjection.Anchors.Count == OpeningRegionLayoutGeometry.Anchors.Count
            && retainedProjection.Anchors.All(a => OpeningRegionLayoutGeometry.Anchors.TryGetValue(
                a.Location.Identity.Id.Value, out Vector2 expected) && expected == new Vector2(a.Position.X, a.Position.Y))
            && retainedProjection.Connections.Count == OpeningRegionLayoutGeometry.Connections.Count
            && retainedProjection.Connections.All(c => OpeningRegionLayoutGeometry.Connections.Contains(
                (c.FirstAnchor.Location.Identity.Id.Value, c.SecondAnchor.Location.Identity.Id.Value)));
        if (!valid) { GD.PrintErr("Footstep material prerequisites do not match the authorized registry; silence."); }
    }

    /// <summary>Resolves world position without altering navigation, collision or world knowledge.</summary>
    internal FootstepMaterial? Select(NumericsVector2 worldPosition)
    {
        if (!valid || !ReferenceEquals(projection.Projection, retainedProjection)
            || navigation.NavigationPolygon != retainedPolygon
            || !float.IsFinite(worldPosition.X) || !float.IsFinite(worldPosition.Y)) { Status = "initial binding/position"; return null; }
        OpeningRegionSickhouseLayout? sickhouse = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource").Layout;
        if (sickhouse is null) { Status = "sickhouse not ready"; return null; }
        if (sickhouse.EntranceCenter != new NumericsVector2(2528, 1310)
            || sickhouse.RoomBounds.Left != 2480 || sickhouse.RoomBounds.Top != 1310
            || sickhouse.RoomBounds.Right != 2800 || sickhouse.RoomBounds.Bottom != 1502
            || !ReferenceEquals(sickhouse.FirstPatientBinding.Projection, retainedProjection)) { Status = $"sickhouse geometry {sickhouse.EntranceCenter} {sickhouse.RoomBounds.Left},{sickhouse.RoomBounds.Top},{sickhouse.RoomBounds.Right},{sickhouse.RoomBounds.Bottom}"; return null; }
        Vector2 world = new(worldPosition.X, worldPosition.Y);
        Rid map = NavigationServer2D.RegionGetMap(navigation.GetRid());
        if (!map.IsValid || NavigationServer2D.MapGetIterationId(map) == 0
            || NavigationServer2D.RegionGetIterationId(navigation.GetRid()) == 0) { Status = "navigation not ready"; return null; }
        Vector2 closest = NavigationServer2D.MapGetClosestPoint(map, world);
        Vector2 local = projection.ToLocal(world);
        Vector2 localClosest = projection.ToLocal(closest);
        Status = $"local {local}, closest {localClosest}";
        OpeningRegionInteriorAudioSource? interior = GetNodeOrNull<OpeningRegionInteriorAudioSource>("../OpeningRegionInteriorAudioSource");
        if (interior is not null && interior.TrySelectMaterial(worldPosition, out FootstepMaterial? material)) { return material; }
        return OpeningRegionFootstepMaterialProjection.Select(new(local.X, local.Y),
            local.DistanceSquaredTo(localClosest) <= 1);
    }
}
