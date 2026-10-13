using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Observes only retained supported exterior materials for the locally qualified committed rustle.</summary>
public partial class OpeningRegionHerbCollectionAudioSource : Node
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
    private Healer healer = null!;
    private Seeker seeker = null!;
    private GreyboxInteractionTarget patch = null!;
    private Node[] owners = [];
    private string[] paths = [];
    private Node[] patchChildren = [];
    private Transform2D[] patchTransforms = [];
    private string[] patchNames = [];
    private CollisionShape2D[] colliders = [];
    private Shape2D[] shapes = [];
    private Node[] healerChildren = [];
    private Node[] seekerChildren = [];
    private Allrpcs.Domain.Characters.Character character = null!;
    private TemporaryHealerCharacterSource actor = null!;
    private TemporaryOrdinaryCharacterParticipantSource roster = null!;
    private Allrpcs.Domain.Identifiers.ObjectId target = null!;
    private InteriorAudioLayout? layout;
    /// <summary>Captured exact patch world position, never a caller's position.</summary>
    internal Vector2 PatchPosition { get; private set; }
    /// <summary>Retained exact acting character identifier.</summary>
    internal Allrpcs.Domain.Identifiers.CharacterId ActorId => character.Identity.Id;
    /// <summary>Retained canonical object identity distinct from the marker's equal value.</summary>
    internal Allrpcs.Domain.Identifiers.ObjectId TargetId => target;


    /// <inheritdoc />
    public override void _Ready()
    {
        paths = ["Healer",
            "Seeker",
            "InteractionCandidates",
            "InteractionCandidates/HerbPatch",
            "TemporaryHealerCharacterSource",
            "TemporaryOrdinaryCharacterParticipantSource",
            "TemporaryWorldObjectSource"];
        owners = paths.Select(p => GetNode<Node>("../" + p)).ToArray();
        healer = (Healer)owners[0]; seeker = (Seeker)owners[1]; patch = (GreyboxInteractionTarget)owners[3];
        actor = (TemporaryHealerCharacterSource)owners[4]; roster = (TemporaryOrdinaryCharacterParticipantSource)owners[5];
        character = actor.Character; target = ((TemporaryWorldObjectSource)owners[6]).WorldObjects.Entries[0].Identity.Id;
        patchChildren = patch.GetChildren().ToArray(); healerChildren = healer.GetChildren().ToArray();
        seekerChildren = seeker.GetChildren().ToArray(); PatchPosition = patch.GlobalPosition;
        patchTransforms = patchChildren.Select(n => n is Node2D item ? item.Transform : Transform2D.Identity).ToArray();
        patchNames = patchChildren.Select(n => n.Name.ToString()).ToArray();
        colliders = [healer.GetNode<CollisionShape2D>("CollisionShape2D"), seeker.GetNode<CollisionShape2D>("CollisionShape2D")];
        shapes = colliders.Select(c => c.Shape).ToArray();
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
        if (!topology || !BodiesValid() || sources.Any(n => !GodotObject.IsInstanceValid(n) || !n.IsInsideTree()
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
        bool valid = ReferenceEquals(sickhouse.Layout, retainedSickhouse) && ReferenceEquals(laundry.Layout, retainedLaundry)
            && ReferenceEquals(retainedSickhouse.FirstPatientBinding.Projection, retainedProjection)
            && ReferenceEquals(retainedLaundry.SceneBinding.Projection, retainedProjection)
            && Bounds(retainedSickhouse.RoomBounds, 2480, 1310, 2800, 1502)
            && retainedSickhouse.EntranceCenter == new NVector(2528, 1310)
            && Bounds(retainedLaundry.SceneBinding.AbandonedLaundryBounds, 1420, 120, 1580, 360)
            && retainedSickhouse.WallThicknessPixels == 8 && retainedLaundry.WallThicknessPixels == 8;
        if (!valid) { return false; }
        try
        {
            layout ??= new(retainedSickhouse.EntranceCenter, retainedSickhouse.EntranceWidthPixels,
                retainedLaundry.LaundryWoodlandOpening, retainedLaundry.LaundryStreamOpening, retainedLaundry.OpeningWidthPixels);
            return true;
        }
        catch (ArgumentException) { return false; }
    }

    /// <summary>Both real bodies must be on supported exterior navigation, with valid current layouts.</summary>
    internal bool Exterior(Vector2 world)
    {
        if (!Finite(world) || !Valid() || layout is null) { return false; }
        Vector2 closest = NavigationServer2D.MapGetClosestPoint(map, world);
        Vector2 local = projection.ToLocal(world), localClosest = projection.ToLocal(closest);
        bool supported = Finite(local) && Finite(localClosest) && local.DistanceSquaredTo(localClosest) <= 1;
        return supported && OpeningRegionInteriorAudioProjection.Select(layout, new(local.X, local.Y), supported).Room == InteriorAudioRoom.None;
    }
    private bool BodiesValid() => owners.Length > 0
        && owners.All(n => GodotObject.IsInstanceValid(n) && n.IsInsideTree() && n.CanProcess())
        && owners.Select((n, i) => ReferenceEquals(GetNodeOrNull<Node>("../" + paths[i]), n)).All(x => x)
        && ReferenceEquals(healer.GetParent(), GetParent()) && ReferenceEquals(seeker.GetParent(), GetParent())
        && healer.IsVisibleInTree() && seeker.IsVisibleInTree() && patch.IsVisibleInTree()
        && healer.IsPhysicsProcessing() && seeker.IsPhysicsProcessing()
        && healer.Rotation == 0 && healer.Scale == Vector2.One && seeker.Rotation == 0 && seeker.Scale == Vector2.One
        && patch.GlobalTransform == new Transform2D(0, new Vector2(920, 350)) && PatchPosition == new Vector2(920, 350)
        && patch.ObjectId == target && patch.ObjectIdText == "herb-patch"
        && ((Node2D)owners[2]).GlobalTransform == Transform2D.Identity
        && patch.GetChildren().SequenceEqual(patchChildren) && healer.GetChildren().SequenceEqual(healerChildren)
        && seeker.GetChildren().SequenceEqual(seekerChildren)
        && patchChildren.Select((n, i) => n.Name.ToString() == patchNames[i]
            && (n is not Node2D item || item.Transform == patchTransforms[i])).All(x => x)
        && colliders.Select((c, i) => !c.Disabled && ReferenceEquals(c.Shape, shapes[i])
            && c.Shape is CircleShape2D { Radius: 18 } && c.Transform == Transform2D.Identity).All(x => x)
        && ReferenceEquals(actor.Character, character) && roster.Participants.Count == 2
        && ReferenceEquals(roster.Participants[0], character)
        && ReferenceEquals(GetNode<TemporaryCharacterTurnSource>("../TemporaryCharacterTurnSource").Activity.Request.RespondingCharacterId, ActorId)
        && ReferenceEquals(((TemporaryWorldObjectSource)owners[6]).WorldObjects.Entries[0].Identity.Id, target);
    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    private static bool Bounds(NavigationWorldBounds b, float left, float top, float right, float bottom) =>
        b.Left == left && b.Top == top && b.Right == right && b.Bottom == bottom;
}
