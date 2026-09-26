using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves the collision-backed clearing, concealed marker, and synchronized routes.</summary>
public partial class OpeningRegionWoodlandClearingSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_WOODLAND_CLEARING_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const int ExpectedBoundarySectionCount = 8;
    private const float SurfaceTolerancePixels = 1.0f;

    private int physicsFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetPhysicsProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        GetNode<CharacterBody2D>("../Seeker").SetPhysicsProcess(false);
        Healer healer = GetNode<Healer>("../Healer");
        healer.CommandWait();
        healer.GlobalPosition = new Vector2(1500.0f, 850.0f);
        healer.SetPhysicsProcess(false);
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        physicsFrames++;
        OpeningRegionWoodlandClearingSceneSource source =
            GetNode<OpeningRegionWoodlandClearingSceneSource>(
                "../OpeningRegionWoodlandClearingSceneSource");
        OpeningRegionWoodlandClearingLayout? layout = source.Layout;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && IsOnSurface(navigationMap, layout.SceneBinding.WoodlandClearingAnchor.Position);
        if (!synchronized && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The woodland-clearing layout or navigation surface was unavailable.");
            return;
        }

        RunChecks(layout, navigationMap);
    }

    private void RunChecks(OpeningRegionWoodlandClearingLayout layout, Rid navigationMap)
    {
        Node boundaries = GetNode(
            "../OpeningRegionWoodlandClearingSceneSource/TreeBoundaries");
        Marker2D specimen = GetNode<Marker2D>(
            "../OpeningRegionWoodlandClearingSceneSource/ConcealedRareSpecimen");
        if (!HasCollisionShapes(boundaries)
            || specimen.Visible
            || specimen.GetChildCount() != 0
            || ToNumerics(specimen.Position)
                != OpeningRegionWoodlandClearingGeometry.RareSpecimenPosition
            || !IsInside(layout.SceneBinding.WoodlandClearingBounds, specimen.Position)
            || !ReferenceEquals(
                layout.SceneBinding.Projection,
                GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection))
        {
            Fail("The clearing lost collision geometry, concealed placement, or provenance.");
            return;
        }

        OpeningRegionStreamLaundryLayout streamLayout =
            GetNode<OpeningRegionStreamLaundrySceneSource>(
                "../OpeningRegionStreamLaundrySceneSource").Layout;
        NumericsVector2 villageEdge = layout.SceneBinding.Projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "village-edge").Position;
        NumericsVector2 laundry = layout.SceneBinding.Projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "abandoned-laundry").Position;
        NumericsVector2 clearing = layout.SceneBinding.WoodlandClearingAnchor.Position;
        List<NumericsVector2> villageRoundTrip = Route(
            navigationMap,
            villageEdge,
            clearing,
            "village-edge/clearing");
        List<NumericsVector2> laundryRoundTrip = Route(
            navigationMap,
            laundry,
            clearing,
            "laundry/clearing");
        if (villageRoundTrip.Count == 0
            || laundryRoundTrip.Count == 0
            || !OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(layout, villageRoundTrip)
            || !OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(layout, laundryRoundTrip)
            || !OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(streamLayout, villageRoundTrip)
            || !OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(streamLayout, laundryRoundTrip)
            || !AssertPhysicalRoundTrips(layout, streamLayout)
            || !AssertBoundaryCollision(
                ToGodot(clearing),
                Vector2.Up * 80.0f,
                "ClearingTop")
            || !AssertBoundaryCollision(
                ToGodot(clearing),
                new Vector2(90.0f, 40.0f),
                "ClearingRightBottom"))
        {
            return;
        }

        GD.Print(
            "Opening-region woodland-clearing smoke passed: eight collision-backed tree "
            + "boundaries, four route openings, one concealed inert specimen marker, and "
            + "synchronized village/laundry round trips.");
        GetTree().Quit();
    }

    private List<NumericsVector2> Route(
        Rid navigationMap,
        NumericsVector2 origin,
        NumericsVector2 destination,
        string description)
    {
        List<NumericsVector2> roundTrip = [];
        foreach ((NumericsVector2 start, NumericsVector2 end) in new[]
        {
            (origin, destination),
            (destination, origin),
        })
        {
            Vector2[] path = NavigationServer2D.MapGetPath(
                navigationMap,
                ToGodot(start),
                ToGodot(end),
                optimize: true);
            if (path.Length < 2
                || path[^1].DistanceTo(ToGodot(end)) > SurfaceTolerancePixels)
            {
                Fail($"The synchronized {description} round trip did not reach its endpoint.");
                return [];
            }

            AppendPath(roundTrip, path);
        }

        return roundTrip;
    }

    private bool AssertPhysicalRoundTrips(
        OpeningRegionWoodlandClearingLayout layout,
        OpeningRegionStreamLaundryLayout streamLayout)
    {
        Vector2 villageEdge = ToGodot(streamLayout.VillageEdgeApproach);
        Vector2 stream = ToGodot(streamLayout.SceneBinding.StreamAnchor.Position);
        Vector2 bridgeNorth = ToGodot(streamLayout.BridgeNorthApproach);
        Vector2 laundry = ToGodot(streamLayout.SceneBinding.AbandonedLaundryAnchor.Position);
        Vector2 clearing = ToGodot(layout.SceneBinding.WoodlandClearingAnchor.Position);
        OpeningRegionVillageTransitionLayout villageLayout =
            GetNode<OpeningRegionVillageTransitionSceneSource>(
                "../OpeningRegionVillageTransitionSceneSource").Layout;
        Vector2 villageEdgeOpening = ToGodot(villageLayout.EdgeStreamOpening);
        Vector2 villageEdgeNorthApproach = new(
            villageEdgeOpening.X,
            OpeningRegionStreamLaundryGeometry.StreamBottom);
        Vector2 bridgeSouth = new(stream.X, OpeningRegionStreamLaundryGeometry.StreamBottom);
        Vector2[] waypoints =
        [
            villageEdge,
            villageEdgeOpening,
            villageEdgeNorthApproach,
            bridgeSouth,
            stream,
            bridgeNorth,
            laundry,
            clearing,
            laundry,
            bridgeNorth,
            stream,
            bridgeSouth,
            villageEdgeNorthApproach,
            villageEdgeOpening,
            villageEdge,
        ];
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        for (int index = 0; index < waypoints.Length - 1; index++)
        {
            seeker.GlobalPosition = waypoints[index];
            KinematicCollision2D? collision = seeker.MoveAndCollide(
                waypoints[index + 1] - waypoints[index],
                testOnly: true);
            if (collision is not null)
            {
                Fail($"The physical clearing round trip hit {collision.GetCollider()}.");
                return false;
            }
        }

        return true;
    }

    private bool AssertBoundaryCollision(Vector2 start, Vector2 motion, string expectedName)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(motion, testOnly: true);
        if (collision?.GetCollider() is Node collider && collider.Name == expectedName)
        {
            return true;
        }

        string actual = collision?.GetCollider() is Node actualCollider
            ? actualCollider.Name.ToString()
            : "none";
        Fail($"Expected clearing collision from {expectedName}, received {actual}.");
        return false;
    }

    private static bool HasCollisionShapes(Node boundaries) =>
        boundaries.GetChildCount() == ExpectedBoundarySectionCount
        && boundaries.GetChildren().All(child => child is StaticBody2D body
            && body.GetChildCount() == 1
            && body.GetChild(0) is CollisionShape2D shape
            && shape.Shape is not null);

    private static bool IsInside(NavigationWorldBounds bounds, Vector2 point) =>
        point.X > bounds.Left
        && point.X < bounds.Right
        && point.Y > bounds.Top
        && point.Y < bounds.Bottom;

    private static bool IsOnSurface(Rid navigationMap, NumericsVector2 position) =>
        NavigationServer2D.MapGetClosestPoint(navigationMap, ToGodot(position))
            .DistanceTo(ToGodot(position)) <= SurfaceTolerancePixels;

    private static void AppendPath(List<NumericsVector2> destination, Vector2[] path)
    {
        foreach (Vector2 point in path)
        {
            NumericsVector2 converted = ToNumerics(point);
            if (destination.Count == 0 || destination[^1] != converted)
            {
                destination.Add(converted);
            }
        }
    }

    private static Vector2 ToGodot(NumericsVector2 position) => new(position.X, position.Y);

    private static NumericsVector2 ToNumerics(Vector2 position) => new(position.X, position.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
