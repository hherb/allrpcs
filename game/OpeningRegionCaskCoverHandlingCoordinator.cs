using Allrpcs.Application.Actions;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>The sole ordinary producer of the explicitly authorized environmental cover adjustment.</summary>
public partial class OpeningRegionCaskCoverHandlingCoordinator : Node
{
    private WorldObjectController? objects;
    private TemporaryWorldObjectSource? worldSource;
    private InteractionFocusController? focus;
    private OpeningRegionProjectionSource? projection;
    private OpeningRegionProjection? retainedProjection;
    private GreyboxNavigation? navigation;
    private Seeker? seeker;
    private TemporarySeekerCharacterSource? actor;
    private CharacterTurnActivityController? turns;
    private AudioSettingsPanel? audioPanel;
    private SaveSnapshotRestorationController? restoration;
    private CharacterObjectSabotageMoveController? moveState;
    private CharacterObjectSabotageMovePresentationController? movePresentation;
    private OpeningRegionCaskCoverMaterialBinding? binding;
    private NavigationPolygon? retainedPolygon;
    private Rid retainedMap;
    private Vector2[] navigationVertices = [];
    private int[][] navigationPolygons = [];
    private Transform2D projectionTransform, navigationTransform;
    private CaskCoverHandlingController? controller;
    private Vector2? lastPosition, lastTargetPosition;
    private bool focused = true;

    /// <summary>Only ordinary Main produces commits; detached diagnostics remain stopped.</summary>
    [Export] public bool Automatic { get; set; }
    /// <summary>The exact most recent accepted token, invalidated on successful load.</summary>
    internal CaskCoverHandlingCommit? LastCommit => controller?.LastCommit;
    /// <summary>Accepted session sequence, independent of feedback availability.</summary>
    internal long CommitCount => controller?.State.Sequence ?? 0;
    /// <summary>Remaining active cooldown.</summary>
    internal double Cooldown => controller?.State.Cooldown ?? 0;
    /// <summary>Actual supported visible access; cooldown disables the native button separately.</summary>
    internal bool CanOffer => Query(out _);
    /// <summary>Exception-isolated notification after accepted state replacement.</summary>
    internal event Action<CaskCoverHandlingCommit>? Committed;
    /// <summary>Cancels only this owner's feedback.</summary>
    internal event Action? PresentationCancelled;

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; ProcessPriority = -44;
        if (!Automatic) { return; }
        objects = GetNode<WorldObjectController>("../WorldObjectController");
        worldSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        focus = GetNode<InteractionFocusController>("../InteractionFocusController");
        projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        seeker = GetNode<Seeker>("../Seeker"); actor = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        audioPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        moveState = GetNode<CharacterObjectSabotageMoveController>("../CharacterObjectSabotageMoveController");
        movePresentation = GetNode<CharacterObjectSabotageMovePresentationController>("../CharacterObjectSabotageMovePresentationController");
        binding = new(worldSource.WorldObjects, GetNode<GreyboxInteractionTarget>("../InteractionCandidates/WaterCask"));
        retainedProjection = projection.Projection; retainedPolygon = navigation.NavigationPolygon;
        retainedMap = navigation.GetWorld2D().NavigationMap;
        navigationVertices = retainedPolygon.GetVertices();
        navigationPolygons = Enumerable.Range(0, retainedPolygon.GetPolygonCount()).Select(retainedPolygon.GetPolygon).ToArray();
        projectionTransform = projection.GlobalTransform; navigationTransform = navigation.GlobalTransform;
        restoration.PresentationReset += ResetPresentation;
    }
    /// <summary>Reobserves actual live ownership/access twice before accepting one newly issued attempt.</summary>
    internal bool TryHandle()
    {
        if (!Query(out Character? current) || current is null || binding is null) { return false; }
        EnsureController(current);
        var request = controller!.CreateRequest();
        return controller.TryCommit(request, current, binding.Material.Target, binding.Material,
            Query(out Character? observed) && ReferenceEquals(observed, current));
    }
    /// <summary>Only the latest exact accepted token with still-current bindings reaches feedback.</summary>
    internal bool Owns(CaskCoverHandlingCommit commit) => Automatic && ReferenceEquals(LastCommit, commit)
        && Query(out Character? current) && ReferenceEquals(current, commit.Request.Actor);
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Automatic) { return; }
        bool validTime = double.IsFinite(delta) && delta >= 0 && delta <= CaskCoverHandlingController.MaximumDeltaSeconds;
        controller?.Advance(delta, Active());
        Vector2? position = seeker is not null && GodotObject.IsInstanceValid(seeker) ? seeker.GlobalPosition : null;
        Vector2? target = binding is not null && GodotObject.IsInstanceValid(binding.Marker) ? binding.Marker.GlobalPosition : null;
        bool discontinuity = position is { } p && lastPosition is { } previous && p.DistanceTo(previous) > 96;
        bool moved = target is { } t && lastTargetPosition is { } old && t != old;
        lastPosition = position; lastTargetPosition = target;
        if (actor is not null && GodotObject.IsInstanceValid(actor) && controller is not null
            && !ReferenceEquals(controller.State.Actor, actor.Character))
        { controller.Committed -= Publish; controller = null; Cancel(); }
        if (!validTime || discontinuity || moved || !Query(out _)) { Cancel(); }
    }
    /// <summary>Successful restoration invalidates ownership and clears cooldown without replay.</summary>
    internal void ResetPresentation()
    { controller?.Reset(); lastPosition = lastTargetPosition = null; Cancel(); }
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
        controller = new(current, binding!.Material); controller.Committed += Publish;
    }
    private void Publish(CaskCoverHandlingCommit commit)
    {
        foreach (Delegate observer in Committed?.GetInvocationList() ?? [])
        { try { ((Action<CaskCoverHandlingCommit>)observer)(commit); } catch (Exception error) { GD.PrintErr("Cover observer: " + error.GetType().Name); } }
    }
    private void Cancel()
    {
        foreach (Delegate observer in PresentationCancelled?.GetInvocationList() ?? [])
        { try { ((Action)observer)(); } catch (Exception error) { GD.PrintErr("Cover cancellation: " + error.GetType().Name); } }
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
        if (!binding.Marker.IsVisibleInTree() || !binding.Original.IsVisibleInTree() || binding.Substitute.IsVisibleInTree()
            || !binding.Marker.IsInteractionVisible || !binding.Marker.IsInteractionEnabled) { return false; }
        Rid map = NavigationServer2D.RegionGetMap(navigation.GetRid());
        if (!map.IsValid || map != retainedMap || !NavigationServer2D.MapIsActive(map)
            || NavigationServer2D.MapGetIterationId(map) == 0 || NavigationServer2D.RegionGetIterationId(navigation.GetRid()) == 0)
        { return false; }
        Vector2 p = seeker.GlobalPosition, t = binding.Marker.GlobalPosition;
        bool reachable = CaskCoverHandlingAccessPolicy.CanReach(new(p.X, p.Y), new(t.X, t.Y), sample =>
        {
            Vector2 world = new(sample.X, sample.Y);
            Vector2 closest = NavigationServer2D.MapGetClosestPoint(map, world);
            return Finite(closest) && world.DistanceSquaredTo(closest) <= 1;
        });
        if (!reachable) { return false; }
        focus!.RefreshFocus();
        return focus.FocusedTargetObjectId == binding.Material.Target.Identity.Id;
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
        Node?[] nodes = [objects, worldSource, focus, projection, navigation, seeker, actor, turns, audioPanel, restoration, moveState, movePresentation];
        if (nodes.Any(n => n is null || !GodotObject.IsInstanceValid(n) || !n.IsInsideTree()) || binding is null) { return false; }
        string[] names = ["WorldObjectController",
            "TemporaryWorldObjectSource",
            "InteractionFocusController",
            "OpeningRegionProjectionSource",
            "GreyboxNavigation",
            "Seeker",
            "TemporarySeekerCharacterSource",
            "CharacterTurnActivityController",
            "AudioSettingsPanel",
            "SaveSnapshotRestorationController",
            "CharacterObjectSabotageMoveController",
            "CharacterObjectSabotageMovePresentationController"];
        if (nodes.Where((n, i) => !ReferenceEquals(GetNodeOrNull<Node>("../" + names[i]), n)).Any()) { return false; }
        if (!GodotObject.IsInstanceValid(binding.Marker) || !GodotObject.IsInstanceValid(binding.Original)
            || !GodotObject.IsInstanceValid(binding.Substitute)
            || !ReferenceEquals(GetNodeOrNull<Node>("../InteractionCandidates/WaterCask"), binding.Marker)
            || !ReferenceEquals(binding.Marker.GetNodeOrNull<Node>("Body"), binding.Original)
            || !ReferenceEquals(binding.Marker.GetNodeOrNull<Node>("SubstituteBody"), binding.Substitute)
            || !ReferenceEquals(objects!.WorldObjects, binding.Snapshot) || !ReferenceEquals(worldSource!.WorldObjects, binding.Snapshot)
            || !ReferenceEquals(objects.FindWorldObject(binding.Material.Target.Identity.Id), binding.Material.Target)
            || binding.Marker.ObjectId != binding.Material.Target.Identity.Id
            || binding.Marker.ObjectIdText != CaskCoverHandlingMaterial.ObjectIdText
            || binding.Marker.Scale != binding.Scale || binding.Marker.Rotation != binding.Rotation
            || binding.Marker.GetParent<Node2D>().GlobalTransform != binding.ParentTransform
            || !ReferenceEquals(projection!.Projection, retainedProjection) || projection.GlobalTransform != projectionTransform
            || navigation!.GlobalTransform != navigationTransform || !navigation.Enabled || navigation.NavigationPolygon != retainedPolygon
            || retainedPolygon is null || !retainedPolygon.GetVertices().SequenceEqual(navigationVertices)
            || retainedPolygon.GetPolygonCount() != navigationPolygons.Length
            || navigationPolygons.Where((p, i) => !p.SequenceEqual(retainedPolygon.GetPolygon(i))).Any()
            || !ReferenceEquals(movePresentation!.Target, binding.Material.Target)
            || !ReferenceEquals(movePresentation.LastPresentedPlacements, moveState!.Placements)) { return false; }
        var placement = moveState.Placements?.Entries.SingleOrDefault(p => ReferenceEquals(p.WorldObject, binding.Material.Target));
        Vector2 expected = placement?.Location.Identity.Id.Value switch
        { "village-square" => new(2200, 900), "abandoned-wagon" => new(1900, 1640), _ => new(float.NaN, float.NaN) };
        return binding.Marker.Position == expected;
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
}
