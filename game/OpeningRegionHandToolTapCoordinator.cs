using Allrpcs.Application.Actions;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>The sole ordinary issuer of the approved fixed environmental wooden mallet contact.</summary>
public partial class OpeningRegionHandToolTapCoordinator : Node
{
    private OpeningRegionProjectionSource? projection;
    private OpeningRegionVillageTransitionSceneSource? village;
    private GreyboxNavigation? navigation;
    private Seeker? seeker;
    private TemporarySeekerCharacterSource? actor;
    private CharacterTurnActivityController? turns;
    private AudioSettingsPanel? audioPanel;
    private SaveSnapshotRestorationController? restoration;
    private OpeningRegionHandToolTapSiteBinding? binding;
    private NavigationPolygon? retainedPolygon;
    private Rid retainedMap;
    private Vector2[] vertices = [];
    private int[][] polygons = [];
    private Transform2D navigationTransform;
    private HandToolTapController? controller;
    private Vector2? lastPosition;
    private bool focused = true;

    /// <summary>Ordinary Main observes actual live owners; standalone diagnostics start stopped.</summary>
    [Export] public bool Automatic { get; set; }
    /// <summary>The latest exact accepted token; successful load invalidates it.</summary>
    internal HandToolTapCommit? LastCommit => controller?.LastCommit;
    /// <summary>The cumulative accepted session sequence, independent of sound availability.</summary>
    internal long CommitCount => controller?.State.Sequence ?? 0;
    /// <summary>Remaining bounded active-time cooldown.</summary>
    internal double Cooldown => controller?.State.Cooldown ?? 0;
    /// <summary>Actual supported unblocked access; cooldown disables the native button separately.</summary>
    internal bool CanOffer => Query(out _);
    /// <summary>Exception-isolated notification after installing accepted state and cooldown.</summary>
    internal event Action<HandToolTapCommit>? Committed;
    /// <summary>Cancels only this action's feedback.</summary>
    internal event Action? PresentationCancelled;

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; ProcessPriority = -43;
        if (!Automatic) { return; }
        projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        village = GetNode<OpeningRegionVillageTransitionSceneSource>("../OpeningRegionVillageTransitionSceneSource");
        navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        seeker = GetNode<Seeker>("../Seeker"); actor = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        audioPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        binding = new(projection, village); retainedPolygon = navigation.NavigationPolygon;
        retainedMap = navigation.GetWorld2D().NavigationMap; vertices = retainedPolygon.GetVertices();
        polygons = Enumerable.Range(0, retainedPolygon.GetPolygonCount()).Select(retainedPolygon.GetPolygon).ToArray();
        navigationTransform = navigation.GlobalTransform; restoration.PresentationReset += ResetPresentation;
    }
    /// <summary>Reobserves current ownership and reach immediately before accepting one issued attempt.</summary>
    internal bool TryTap()
    {
        if (!Query(out Character? current) || current is null || binding is null) { return false; }
        EnsureController(current);
        var request = controller!.CreateRequest();
        return controller.TryCommit(request, current, binding.Site, Query(out Character? observed) && ReferenceEquals(observed, current));
    }
    /// <summary>Only the exact current accepted token with still-valid bindings reaches feedback.</summary>
    internal bool Owns(HandToolTapCommit commit) => Automatic && ReferenceEquals(LastCommit, commit)
        && binding is not null && ReferenceEquals(commit.Request.Site, binding.Site)
        && Query(out Character? current) && ReferenceEquals(current, commit.Request.Actor);
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Automatic) { return; }
        bool validTime = double.IsFinite(delta) && delta >= 0 && delta <= HandToolTapController.MaximumDeltaSeconds;
        controller?.Advance(delta, Active());
        Vector2? position = seeker is not null && GodotObject.IsInstanceValid(seeker) ? seeker.GlobalPosition : null;
        bool teleport = position is { } p && lastPosition is { } previous && p.DistanceTo(previous) > 96;
        lastPosition = position;
        if (actor is not null && GodotObject.IsInstanceValid(actor) && controller is not null
            && !ReferenceEquals(controller.State.Actor, actor.Character))
        { controller.Committed -= Publish; controller = null; Cancel(); }
        if (!validTime || teleport || !Query(out _)) { Cancel(); }
    }
    /// <summary>Invalidates tokens and clears cooldown on successful load, with no replay.</summary>
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
        controller = new(current, binding!.Site); controller.Committed += Publish;
    }
    private void Publish(HandToolTapCommit commit)
    {
        foreach (Delegate observer in Committed?.GetInvocationList() ?? [])
        { try { ((Action<HandToolTapCommit>)observer)(commit); } catch (Exception error) { GD.PrintErr("Tool observer: " + error.GetType().Name); } }
    }
    private void Cancel()
    {
        foreach (Delegate observer in PresentationCancelled?.GetInvocationList() ?? [])
        { try { ((Action)observer)(); } catch (Exception error) { GD.PrintErr("Tool cancellation: " + error.GetType().Name); } }
    }
    private bool Active() => Automatic && focused && !GetTree().Paused && seeker is not null
        && GodotObject.IsInstanceValid(seeker) && seeker.IsInsideTree() && seeker.CanProcess() && seeker.IsPhysicsProcessing()
        && (DisplayServer.GetName() == "headless" || GetWindow().HasFocus());
    private bool Query(out Character? current)
    {
        current = null;
        if (!Active() || !Bindings() || actor is null || seeker is null || binding is null || navigation is null) { return false; }
        current = actor.Character;
        if (current is null || current.ControllerAssignment.ControllerType != ControllerType.Human || Blocked()) { return false; }
        Rid map = NavigationServer2D.RegionGetMap(navigation.GetRid());
        if (!map.IsValid || map != retainedMap || !NavigationServer2D.MapIsActive(map)
            || NavigationServer2D.MapGetIterationId(map) == 0 || NavigationServer2D.RegionGetIterationId(navigation.GetRid()) == 0)
        { return false; }
        Vector2 p = seeker.GlobalPosition, t = binding.Marker.GlobalPosition;
        return HandToolTapAccessPolicy.CanReach(new(p.X, p.Y), new(t.X, t.Y), sample =>
        {
            Vector2 world = new(sample.X, sample.Y), closest = NavigationServer2D.MapGetClosestPoint(map, world);
            return Finite(closest) && world.DistanceSquaredTo(closest) <= 1;
        });
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
        Node?[] nodes = [projection, village, navigation, seeker, actor, turns, audioPanel, restoration];
        string[] names = ["OpeningRegionProjectionSource",
            "OpeningRegionVillageTransitionSceneSource",
            "GreyboxNavigation",
            "Seeker",
            "TemporarySeekerCharacterSource",
            "CharacterTurnActivityController",
            "AudioSettingsPanel",
            "SaveSnapshotRestorationController"];
        if (nodes.Any(n => n is null || !GodotObject.IsInstanceValid(n) || !n.IsInsideTree() || !n.CanProcess()) || binding is null
            || nodes.Where((n, i) => !ReferenceEquals(GetNodeOrNull<Node>("../" + names[i]), n)).Any()) { return false; }
        return GodotObject.IsInstanceValid(binding.Marker) && binding.Marker.IsInsideTree() && binding.Marker.IsVisibleInTree()
            && binding.Marker.CanProcess() && village!.IsVisibleInTree() && projection!.IsVisibleInTree()
            && ReferenceEquals(projection.GetNodeOrNull<Marker2D>("village-edge"), binding.Marker)
            && ReferenceEquals(projection.Projection, binding.Projection) && ReferenceEquals(village.Layout, binding.Village)
            && binding.Projection.Anchors.Any(a => ReferenceEquals(a, binding.Anchor))
            && binding.Anchor.Position == binding.Site.Position && binding.Marker.Transform == binding.MarkerTransform
            && projection.GlobalTransform == binding.ProjectionTransform && village.GlobalTransform == binding.VillageTransform
            && navigation!.GlobalTransform == navigationTransform && navigation.Enabled && navigation.NavigationPolygon == retainedPolygon
            && retainedPolygon is not null && retainedPolygon.GetVertices().SequenceEqual(vertices)
            && retainedPolygon.GetPolygonCount() == polygons.Length
            && !polygons.Where((p, i) => !p.SequenceEqual(retainedPolygon.GetPolygon(i))).Any();
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
}
