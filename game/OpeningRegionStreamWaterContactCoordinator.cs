using Allrpcs.Application.Actions;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>The sole ordinary producer of explicitly requested, supported Seeker water contacts.</summary>
public partial class OpeningRegionStreamWaterContactCoordinator : Node
{
    private OpeningRegionProjectionSource? projection;
    private OpeningRegionStreamLaundrySceneSource? stream;
    private GreyboxNavigation? navigation;
    private Seeker? seeker;
    private TemporarySeekerCharacterSource? actor;
    private CharacterTurnActivityController? turns;
    private AudioSettingsPanel? audioPanel;
    private SaveSnapshotRestorationController? restoration;
    private OpeningRegionProjection? retainedProjection;
    private OpeningRegionStreamLaundryLayout? retainedLayout;
    private NavigationPolygon? retainedPolygon;
    private Rid retainedMap;
    private Vector2[] navigationVertices = [];
    private int[][] navigationPolygons = [];
    private Transform2D projectionTransform, streamTransform, navigationTransform;
    private StreamWaterContactController? controller;
    private StreamWaterContactSite? site;
    private Vector2? lastPosition;
    private bool focused = true;

    /// <summary>Only ordinary Main observes live owners; detached proofs start stopped.</summary>
    [Export] public bool Automatic { get; set; }
    /// <summary>The latest exact accepted commit; successful load invalidates it.</summary>
    internal StreamWaterContactCommit? LastCommit => controller?.LastCommit;
    /// <summary>The accepted session sequence, independent of audio availability.</summary>
    internal long CommitCount => controller?.State.Sequence ?? 0;
    /// <summary>The remaining transient cooldown; no save field is created.</summary>
    internal double Cooldown => controller?.State.Cooldown ?? 0;
    /// <summary>Available means reachable and unblocked; cooldown disables the button separately.</summary>
    internal bool CanOffer => Query(out _);
    /// <summary>Isolated presentation notification after the engine commits its new state.</summary>
    internal event Action<StreamWaterContactCommit>? Committed;
    /// <summary>Clears only this action's presentation on invalid access or lifecycle transitions.</summary>
    internal event Action? PresentationCancelled;

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; ProcessPriority = -45;
        if (!Automatic) { return; }
        projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        stream = GetNode<OpeningRegionStreamLaundrySceneSource>("../OpeningRegionStreamLaundrySceneSource");
        navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        seeker = GetNode<Seeker>("../Seeker"); actor = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        audioPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        retainedProjection = projection.Projection; retainedLayout = stream.Layout; retainedPolygon = navigation.NavigationPolygon;
        retainedMap = navigation.GetWorld2D().NavigationMap;
        navigationVertices = retainedPolygon.GetVertices();
        navigationPolygons = Enumerable.Range(0, retainedPolygon.GetPolygonCount()).Select(retainedPolygon.GetPolygon).ToArray();
        projectionTransform = projection.GlobalTransform; streamTransform = stream.GlobalTransform;
        navigationTransform = navigation.GlobalTransform;
        NavigationWorldBounds bounds = retainedLayout.SceneBinding.StreamBounds;
        site = new(StreamWaterContactAccessPolicy.SiteKey, new(bounds.Left - 2, bounds.Top + 32));
        restoration.PresentationReset += ResetPresentation;
    }

    /// <summary>Re-observes actual position and current owners, then admits one freshly issued request.</summary>
    internal bool TryContact()
    {
        if (!Query(out Character? current) || current is null || site is null) { return false; }
        EnsureController(current);
        StreamWaterContactRequest? request = controller!.CreateRequest();
        return controller.TryCommit(request, current, Query(out Character? observed) && ReferenceEquals(observed, current));
    }

    /// <summary>Only the latest exact commit from this live owner may reach its audio consumer.</summary>
    internal bool Owns(StreamWaterContactCommit commit) => Automatic && ReferenceEquals(LastCommit, commit)
        && Query(out Character? current) && ReferenceEquals(current, commit.Request.Actor);

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Automatic) { return; }
        bool active = Active();
        bool validTime = double.IsFinite(delta) && delta >= 0 && delta <= StreamWaterContactController.MaximumDeltaSeconds;
        controller?.Advance(delta, active);
        Vector2? position = seeker is not null && GodotObject.IsInstanceValid(seeker) ? seeker.GlobalPosition : null;
        bool teleported = position is { } p && lastPosition is { } previous && p.DistanceTo(previous) > 96;
        lastPosition = position;
        if (!validTime || teleported || !Query(out _)) { Cancel(); }
    }

    /// <summary>Successful restore invalidates prior attempts and clears cooldown without replaying.</summary>
    internal void ResetPresentation() { controller?.Reset(); lastPosition = null; Cancel(); }

    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) { focused = false; Cancel(); }
        if (what == NotificationApplicationFocusIn) { focused = true; }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (restoration is not null && GodotObject.IsInstanceValid(restoration))
        { restoration.PresentationReset -= ResetPresentation; }
        ResetPresentation();
    }

    private void EnsureController(Character current)
    {
        if (controller is not null && ReferenceEquals(controller.State.Actor, current)) { return; }
        if (controller is not null) { controller.Committed -= Publish; Cancel(); }
        controller = new(current, site!); controller.Committed += Publish;
    }
    private void Publish(StreamWaterContactCommit commit)
    {
        foreach (Delegate observer in Committed?.GetInvocationList() ?? [])
        { try { ((Action<StreamWaterContactCommit>)observer)(commit); } catch (Exception error) { GD.PrintErr("Water observer: " + error.GetType().Name); } }
    }
    private void Cancel()
    {
        foreach (Delegate observer in PresentationCancelled?.GetInvocationList() ?? [])
        { try { ((Action)observer)(); } catch (Exception error) { GD.PrintErr("Water cancellation: " + error.GetType().Name); } }
    }
    private bool Active() => Automatic && focused && !GetTree().Paused && seeker is not null
        && GodotObject.IsInstanceValid(seeker) && seeker.IsInsideTree() && seeker.CanProcess() && seeker.IsPhysicsProcessing()
        && (DisplayServer.GetName() == "headless" || GetWindow().HasFocus());

    private bool Query(out Character? current)
    {
        current = null;
        if (!Active() || !Bindings() || actor is null || site is null || seeker is null || projection is null || navigation is null)
        { return false; }
        current = actor.Character;
        if (current is null || current.ControllerAssignment.ControllerType != ControllerType.Human || Blocked()) { return false; }
        Vector2 world = seeker.GlobalPosition;
        if (!Finite(world)) { return false; }
        Rid map = NavigationServer2D.RegionGetMap(navigation.GetRid());
        if (!map.IsValid || map != retainedMap || !NavigationServer2D.MapIsActive(map) || NavigationServer2D.MapGetIterationId(map) == 0
            || NavigationServer2D.RegionGetIterationId(navigation.GetRid()) == 0) { return false; }
        Vector2 local = projection.ToLocal(world);
        Vector2 closest = projection.ToLocal(NavigationServer2D.MapGetClosestPoint(map, world));
        NavigationWorldBounds bounds = retainedLayout!.SceneBinding.StreamBounds;
        return Finite(local) && Finite(closest) && StreamWaterContactAccessPolicy.CanReach(new(local.X, local.Y),
            local.DistanceSquaredTo(closest) <= 1, site, new(bounds.Left, bounds.Top), new(bounds.Right, bounds.Bottom));
    }
    private bool Blocked()
    {
        if (GetViewport().GuiGetFocusOwner() is LineEdit or TextEdit || audioPanel!.IsReading
            || GetNodeOrNull<Control>("../SeekerCustomizationOverlay/Panel") is not { Visible: false }) { return true; }
        foreach (string name in new[] { "SeekerMapPanel", "JournalSectionPanel", "DialogueOverlay", "SettingsOverlay" })
        { if (GetNodeOrNull<CanvasLayer>("../" + name) is not { Visible: false }) { return true; } }
        CharacterTurnActivityState? state = turns!.Activity?.State;
        return state == CharacterTurnActivityState.AwaitingResponse || state == CharacterTurnActivityState.Speaking
            || turns.SabotageReservation is not null || turns.ReservedSabotageTurn is not null;
    }
    private bool Bindings()
    {
        Node?[] owners = [projection, stream, navigation, seeker, actor, turns, audioPanel, restoration];
        if (owners.Any(node => node is null || !GodotObject.IsInstanceValid(node) || !node.IsInsideTree())) { return false; }
        if (!ReferenceEquals(GetNodeOrNull<Node>("../OpeningRegionProjectionSource"), projection)
            || !ReferenceEquals(GetNodeOrNull<Node>("../OpeningRegionStreamLaundrySceneSource"), stream)
            || !ReferenceEquals(GetNodeOrNull<Node>("../GreyboxNavigation"), navigation)
            || !ReferenceEquals(GetNodeOrNull<Node>("../Seeker"), seeker)
            || !ReferenceEquals(GetNodeOrNull<Node>("../TemporarySeekerCharacterSource"), actor)
            || !ReferenceEquals(GetNodeOrNull<Node>("../CharacterTurnActivityController"), turns)
            || !ReferenceEquals(GetNodeOrNull<Node>("../AudioSettingsPanel"), audioPanel)
            || !ReferenceEquals(GetNodeOrNull<Node>("../SaveSnapshotRestorationController"), restoration)
            || !ReferenceEquals(projection!.Projection, retainedProjection) || !ReferenceEquals(stream!.Layout, retainedLayout)
            || !navigation!.Enabled || navigation.NavigationPolygon != retainedPolygon
            || retainedPolygon is null || !retainedPolygon.GetVertices().SequenceEqual(navigationVertices)
            || retainedPolygon.GetPolygonCount() != navigationPolygons.Length
            || navigationPolygons.Where((polygon, index) => !polygon.SequenceEqual(retainedPolygon.GetPolygon(index))).Any()
            || projection.GlobalTransform != projectionTransform
            || stream.GlobalTransform != streamTransform || navigation.GlobalTransform != navigationTransform) { return false; }
        NavigationWorldBounds b = retainedLayout!.SceneBinding.StreamBounds;
        if (!ReferenceEquals(retainedLayout.SceneBinding.Projection, retainedProjection)
            || b.Left != 1736 || b.Top != 452 || b.Right != 1864 || b.Bottom != 548
            || retainedLayout.SceneBinding.StreamAnchor.Position != new NumericsVector2(1800, 500)
            || retainedProjection!.Anchors.Count != 11 || retainedProjection.Connections.Count != 12
            || retainedProjection.Anchors.Any(a => !OpeningRegionLayoutGeometry.Anchors.TryGetValue(a.Location.Identity.Id.Value,
                out Vector2 expected) || expected != new Vector2(a.Position.X, a.Position.Y))
            || retainedProjection.Connections.Any(c => !OpeningRegionLayoutGeometry.Connections.Contains(
                (c.FirstAnchor.Location.Identity.Id.Value, c.SecondAnchor.Location.Identity.Id.Value)))) { return false; }
        NumericsVector2[] water = [new(1600, 452), new(2000, 452), new(2000, 548), new(1600, 548)];
        NumericsVector2[] west = [new(1600, 452), new(1700, 452), new(1700, 548), new(1600, 548)];
        NumericsVector2[] east = [new(1900, 452), new(2000, 452), new(2000, 548), new(1900, 548)];
        Polygon2D? waterNode = stream.GetNodeOrNull<Polygon2D>("StreamWater");
        Polygon2D? crossingNode = stream.GetNodeOrNull<Polygon2D>("StreamCrossingArea");
        CollisionPolygon2D? westCollision = GetNodeOrNull<CollisionPolygon2D>("../StreamCollision/WestCollision");
        CollisionPolygon2D? eastCollision = GetNodeOrNull<CollisionPolygon2D>("../StreamCollision/EastCollision");
        if (waterNode is null || crossingNode is null || westCollision is null || eastCollision is null
            || waterNode.Transform != Transform2D.Identity || crossingNode.Transform != Transform2D.Identity
            || westCollision.GlobalTransform != streamTransform || eastCollision.GlobalTransform != streamTransform) { return false; }
        return OpeningRegionStreamLaundryGeometry.StreamWater.SequenceEqual(water)
            && retainedLayout.WestStreamBank.SequenceEqual(west) && retainedLayout.EastStreamBank.SequenceEqual(east)
            && waterNode.Polygon.SequenceEqual(water.Select(v => new Vector2(v.X, v.Y)))
            && westCollision.Polygon.SequenceEqual(west.Select(v => new Vector2(v.X, v.Y)))
            && eastCollision.Polygon.SequenceEqual(east.Select(v => new Vector2(v.X, v.Y)))
            && crossingNode.Polygon.SequenceEqual(new Vector2[]
                { new(1736,452), new(1864,452), new(1864,548), new(1736,548) });
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
}
