using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Observes two retained structures for shared presentation gains and local floor contacts.</summary>
public partial class OpeningRegionInteriorAudioSource : Node
{
    private readonly InteriorAudioRamp ramp = new();
    private OpeningRegionProjectionSource? projection;
    private GreyboxNavigation? navigation;
    private OpeningRegionFirstPatientSceneSource? sickhouse;
    private OpeningRegionStreamLaundrySceneSource? laundry;
    private Seeker? seeker;
    private MaterialSoundCaptionPanel captions = null!;
    private SaveSnapshotRestorationController? restoration;
    private OpeningRegionProjection? retainedProjection;
    private NavigationPolygon? retainedPolygon;
    private OpeningRegionSickhouseLayout? retainedSickhouse;
    private OpeningRegionStreamLaundryLayout? retainedLaundry;
    private Transform2D projectionTransform;
    private Transform2D navigationTransform;
    private Transform2D sickhouseTransform;
    private Transform2D laundryTransform;
    private InteriorAudioLayout? layout;
    private Vector2? lastPosition;
    private bool reported;

    /// <summary>Only ordinary Main enables live observations; standalone proofs start neutral.</summary>
    [Export] public bool Automatic { get; set; }
    /// <summary>The current safe target, independent of output gain/mute.</summary>
    internal InteriorAudioSnapshot Current { get; private set; } = InteriorAudioSnapshot.Neutral;
    /// <summary>One bed multiplier; consumers must apply it exactly once.</summary>
    internal double Gain(int bed) => ramp.Gain(bed);

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        ProcessPriority = -50;
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        if (!Automatic) { return; }
        projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        sickhouse = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource");
        laundry = GetNode<OpeningRegionStreamLaundrySceneSource>("../OpeningRegionStreamLaundrySceneSource");
        seeker = GetNode<Seeker>("../Seeker");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        retainedProjection = projection.Projection;
        retainedPolygon = navigation.NavigationPolygon;
        projectionTransform = projection.GlobalTransform;
        navigationTransform = navigation.GlobalTransform;
        sickhouseTransform = sickhouse.GlobalTransform;
        laundryTransform = laundry.GlobalTransform;
        restoration.PresentationReset += ResetPresentation;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Automatic) { return; }
        if (!Active() || !TryLayout() || seeker is null
            || !double.IsFinite(delta) || delta < 0 || delta > InteriorAudioRamp.MaximumDeltaSeconds)
        { ResetPresentation(); return; }
        Vector2 position = seeker.GlobalPosition;
        if (!Finite(position) || (lastPosition is { } previous && previous.DistanceTo(position) > 96))
        { ResetPresentation(); lastPosition = Finite(position) ? position : null; return; }
        lastPosition = position;
        if (!TryPosition(new(position.X, position.Y), out NumericsVector2 local, out bool supported))
        { ResetPresentation(); return; }
        Present(OpeningRegionInteriorAudioProjection.Select(layout, local, supported), delta);
    }

    /// <summary>Returns an override only with valid current live owners and navigation.</summary>
    internal bool TrySelectMaterial(NumericsVector2 world, out FootstepMaterial? material)
    {
        material = null;
        if (!Automatic || !Active() || !TryLayout() || !TryPosition(world, out NumericsVector2 local, out bool supported))
        { return false; }
        material = OpeningRegionInteriorAudioProjection.MaterialAt(layout, local, supported);
        return true;
    }

    /// <summary>Immediately clears owned gains and caption, without changing other presentation.</summary>
    internal void ResetPresentation()
    {
        Current = InteriorAudioSnapshot.Neutral; ramp.Reset(); lastPosition = null;
        if (GodotObject.IsInstanceValid(captions) && captions.IsInsideTree()) { captions.ClearInterior(); }
    }

    /// <summary>Only stopped diagnostic scenes may supply a target; ordinary owners cannot be overridden.</summary>
    internal void DiagnosticPresent(InteriorAudioSnapshot target, double delta)
    {
        if (Automatic) { throw new InvalidOperationException("Live interior ownership cannot be overridden."); }
        Present(target, delta);
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (restoration is not null && GodotObject.IsInstanceValid(restoration))
        { restoration.PresentationReset -= ResetPresentation; }
        ResetPresentation(); layout = null;
    }

    private void Present(InteriorAudioSnapshot target, double delta)
    {
        if (!ramp.Advance(target, delta)) { ResetPresentation(); return; }
        Current = target;
        captions.SetInterior(target.Room != InteriorAudioRoom.None && target.Blend >= 0.5 ? "Outdoor sounds softened" : string.Empty);
    }
    private bool Active() => !GetTree().Paused && seeker is not null && GodotObject.IsInstanceValid(seeker)
        && seeker.IsInsideTree() && seeker.CanProcess() && seeker.IsPhysicsProcessing()
        && (DisplayServer.GetName() == "headless" || GetWindow().HasFocus());

    private bool TryLayout()
    {
        if (projection is null || navigation is null || sickhouse is null || laundry is null
            || !GodotObject.IsInstanceValid(projection) || !GodotObject.IsInstanceValid(navigation)
            || !GodotObject.IsInstanceValid(sickhouse) || !GodotObject.IsInstanceValid(laundry)
            || !ReferenceEquals(GetNodeOrNull<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource"), projection)
            || !ReferenceEquals(GetNodeOrNull<GreyboxNavigation>("../GreyboxNavigation"), navigation)
            || !ReferenceEquals(GetNodeOrNull<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource"), sickhouse)
            || !ReferenceEquals(GetNodeOrNull<OpeningRegionStreamLaundrySceneSource>("../OpeningRegionStreamLaundrySceneSource"), laundry)
            || !ReferenceEquals(GetNodeOrNull<Seeker>("../Seeker"), seeker)
            || !ReferenceEquals(projection.Projection, retainedProjection) || navigation.NavigationPolygon != retainedPolygon
            || projection.GlobalTransform != projectionTransform || navigation.GlobalTransform != navigationTransform
            || sickhouse.GlobalTransform != sickhouseTransform || laundry.GlobalTransform != laundryTransform)
        { return false; }
        if (sickhouse.Layout is null || laundry.Layout is null) { return false; }
        retainedSickhouse ??= sickhouse.Layout; retainedLaundry ??= laundry.Layout;
        if (!ReferenceEquals(sickhouse.Layout, retainedSickhouse) || !ReferenceEquals(laundry.Layout, retainedLaundry)
            || !ReferenceEquals(retainedSickhouse.FirstPatientBinding.Projection, retainedProjection)
            || !ReferenceEquals(retainedLaundry.SceneBinding.Projection, retainedProjection)
            || !Bounds(retainedSickhouse.RoomBounds, 2480, 1310, 2800, 1502)
            || !Bounds(retainedLaundry.SceneBinding.AbandonedLaundryBounds, 1420, 120, 1580, 360)
            || retainedSickhouse.WallThicknessPixels != 8 || retainedLaundry.WallThicknessPixels != 8)
        { return false; }
        if (layout is not null) { return true; }
        try
        {
            if (retainedProjection is null || retainedProjection.Anchors.Count != OpeningRegionLayoutGeometry.Anchors.Count
                || retainedProjection.Connections.Count != OpeningRegionLayoutGeometry.Connections.Count
                || retainedProjection.Anchors.Any(a => !OpeningRegionLayoutGeometry.Anchors.TryGetValue(a.Location.Identity.Id.Value,
                    out Vector2 expected) || expected != new Vector2(a.Position.X, a.Position.Y))
                || retainedProjection.Connections.Any(c => !OpeningRegionLayoutGeometry.Connections.Contains(
                    (c.FirstAnchor.Location.Identity.Id.Value, c.SecondAnchor.Location.Identity.Id.Value)))) { return false; }
            layout = new(retainedSickhouse.EntranceCenter, retainedSickhouse.EntranceWidthPixels,
                retainedLaundry.LaundryWoodlandOpening, retainedLaundry.LaundryStreamOpening, retainedLaundry.OpeningWidthPixels);
            return true;
        }
        catch (ArgumentException)
        {
            if (!reported) { reported = true; GD.PrintErr("Interior audio owners differ from approved scope; neutral ambience and no interior steps."); }
            return false;
        }
    }
    private bool TryPosition(NumericsVector2 world, out NumericsVector2 local, out bool supported)
    {
        local = default; supported = false;
        if (projection is null || navigation is null || !float.IsFinite(world.X) || !float.IsFinite(world.Y)) { return false; }
        Rid map = NavigationServer2D.RegionGetMap(navigation.GetRid());
        if (!map.IsValid || NavigationServer2D.MapGetIterationId(map) == 0
            || NavigationServer2D.RegionGetIterationId(navigation.GetRid()) == 0) { return false; }
        Vector2 point = projection.ToLocal(new(world.X, world.Y));
        Vector2 closest = projection.ToLocal(NavigationServer2D.MapGetClosestPoint(map, new(world.X, world.Y)));
        local = new(point.X, point.Y); supported = point.DistanceSquaredTo(closest) <= 1;
        return Finite(point) && Finite(closest);
    }
    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    private static bool Bounds(NavigationWorldBounds b, float left, float top, float right, float bottom) =>
        b.Left == left && b.Top == top && b.Right == right && b.Bottom == bottom;
}
