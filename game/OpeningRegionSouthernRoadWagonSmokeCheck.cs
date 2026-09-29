using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Discovery;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves synchronized shrine/road/wagon travel and the inert collision shell.</summary>
public partial class OpeningRegionSouthernRoadWagonSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_SOUTHERN_ROAD_WAGON_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 240;
    private const int ExpectedRoadBoundaryCount = 2;
    private const int ExpectedWagonBoundaryCount = 5;
    private const float SurfaceTolerancePixels = 1.0f;

    private int physicsFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        string configuredValue = OS.GetEnvironment(CheckEnvironmentVariable);
        bool enabled = configuredValue == EnabledEnvironmentValue;
        if (!enabled && !string.IsNullOrEmpty(configuredValue))
        {
            GD.PushWarning(
                $"Ignored {CheckEnvironmentVariable} value '{configuredValue}'.");
        }

        SetPhysicsProcess(enabled);
        if (!enabled)
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
        OpeningRegionSouthernRoadWagonSceneSource source =
            GetNode<OpeningRegionSouthernRoadWagonSceneSource>(
                "../OpeningRegionSouthernRoadWagonSceneSource");
        OpeningRegionSouthernRoadWagonLayout? layout = source.Layout;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && layout.RouteCenterline.All(point => IsOnSurface(navigationMap, point))
            && IsOnSurface(
                navigationMap,
                layout.SceneBinding.OldShrineLayout.SceneBinding.OldShrineAnchor.Position);
        if (!synchronized && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The southern-road layout or synchronized navigation surface was unavailable.");
            return;
        }

        RunChecks(source, layout, navigationMap);
    }

    private void RunChecks(
        OpeningRegionSouthernRoadWagonSceneSource source,
        OpeningRegionSouthernRoadWagonLayout layout,
        Rid navigationMap)
    {
        OpeningRegionOldShrineSceneSource shrineSource =
            GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        OpeningRegionOldShrineLayout shrineLayout = shrineSource.Layout;
        OpeningRegionWoodlandClearingLayout clearingLayout =
            GetNode<OpeningRegionWoodlandClearingSceneSource>(
                "../OpeningRegionWoodlandClearingSceneSource").Layout;
        OpeningRegionStreamLaundryLayout streamLayout =
            GetNode<OpeningRegionStreamLaundrySceneSource>(
                "../OpeningRegionStreamLaundrySceneSource").Layout;
        OpeningRegionVillageTransitionLayout villageLayout =
            GetNode<OpeningRegionVillageTransitionSceneSource>(
                "../OpeningRegionVillageTransitionSceneSource").Layout;
        OpeningRegionLayeredInscriptionDiscoveryState shrineState = shrineSource.DiscoveryState;
        DiscoveryScenario shrineScenario = shrineSource.DiscoveryScenario;
        Node2D roadBoundaries = source.GetNode<Node2D>("RoadBoundaries");
        Node2D wagonBoundaries = source.GetNode<Node2D>("WagonBoundaries");
        if (!HasExactBoundaries(
                roadBoundaries,
                layout.RoadBoundarySegments,
                ExpectedRoadBoundaryCount)
            || !HasExactBoundaries(
                wagonBoundaries,
                layout.WagonBoundarySegments,
                ExpectedWagonBoundaryCount)
            || source.AbandonedWagonMarker.GetChildCount() != 0
            || ToNumerics(source.AbandonedWagonMarker.Position)
                != layout.SceneBinding.AbandonedWagonAnchor.Position
            || source.FindChild("WagonClue", recursive: true, owned: false) is not null
            || source.FindChild("WagonResidue", recursive: true, owned: false) is not null
            || !ReferenceEquals(source.Binding, layout.SceneBinding)
            || !ReferenceEquals(layout.SceneBinding.OldShrineLayout, shrineLayout)
            || !ReferenceEquals(
                layout.SceneBinding.Projection,
                GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection))
        {
            Fail("The road/wagon scene lost exact geometry, inertness, or provenance.");
            return;
        }

        List<NumericsVector2> roundTrip = BuildSynchronizedRoundTrip(layout, navigationMap);
        if (roundTrip.Count == 0)
        {
            return;
        }

        bool shrineRouteAccepted =
            OpeningRegionOldShrineRoutePolicy.IsAcceptable(shrineLayout, roundTrip);
        bool clearingRouteAccepted =
            OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(clearingLayout, roundTrip);
        bool streamRouteAccepted =
            OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(streamLayout, roundTrip);
        bool villageRouteAccepted =
            OpeningRegionVillageTransitionRoutePolicy.IsAcceptable(villageLayout, roundTrip);
        bool roadRouteAccepted =
            OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(layout, roundTrip);
        if (!shrineRouteAccepted
            || !clearingRouteAccepted
            || !streamRouteAccepted
            || !villageRouteAccepted
            || !roadRouteAccepted)
        {
            Fail(
                "The synchronized shrine/road/wagon route crossed an authored boundary: "
                + $"shrine={shrineRouteAccepted}, clearing={clearingRouteAccepted}, "
                + $"stream={streamRouteAccepted}, village={villageRouteAccepted}, "
                + $"road={roadRouteAccepted}.");
            return;
        }

        if (!AssertPhysicalRoundTrip(roundTrip)
            || !AssertBoundaryCollision(
                ToGodot(layout.SceneBinding.SouthernRoadAnchor.Position),
                new Vector2(-120.0f, 140.0f),
                "RoadBoundary")
            || !AssertBoundaryCollision(
                ToGodot(layout.SceneBinding.AbandonedWagonAnchor.Position),
                Vector2.Right * 120.0f,
                "WagonBoundary"))
        {
            return;
        }

        if (!ReferenceEquals(shrineSource.Layout, shrineLayout)
            || !ReferenceEquals(shrineSource.DiscoveryState, shrineState)
            || !ReferenceEquals(shrineSource.DiscoveryScenario, shrineScenario)
            || shrineSource.Presentation is not null
            || shrineSource.EvidenceExposure is not null)
        {
            Fail("The road/wagon smoke changed retained shrine snapshots.");
            return;
        }

        GD.Print(
            "Opening-region southern-road/wagon smoke passed: exact retained shrine/road/wagon "
            + "provenance, two road and five wagon collision boundaries, minimum-clearance "
            + "synchronized round trip, representative collision rejection, one inert wagon "
            + "marker, no clue/residue state, and preserved shrine snapshots.");
        GetTree().Quit();
    }

    private List<NumericsVector2> BuildSynchronizedRoundTrip(
        OpeningRegionSouthernRoadWagonLayout layout,
        Rid navigationMap)
    {
        NumericsVector2 shrine = layout.SceneBinding.OldShrineLayout.SceneBinding
            .OldShrineAnchor.Position;
        List<NumericsVector2> outward =
        [
            shrine,
            layout.SceneBinding.OldShrineLayout.SouthernRoadOpening,
            .. layout.RouteCenterline,
        ];
        if (outward.Any(point => !IsOnSurface(navigationMap, point)))
        {
            Fail("An authored shrine/road/wagon waypoint was outside the synchronized surface.");
            return [];
        }

        return [.. outward, .. outward.AsEnumerable().Reverse().Skip(1)];
    }

    private bool AssertPhysicalRoundTrip(IReadOnlyList<NumericsVector2> waypoints)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        for (int index = 0; index < waypoints.Count - 1; index++)
        {
            seeker.GlobalPosition = ToGodot(waypoints[index]);
            KinematicCollision2D? collision = seeker.MoveAndCollide(
                ToGodot(waypoints[index + 1] - waypoints[index]),
                testOnly: true);
            if (collision is not null)
            {
                string collider = collision.GetCollider() is Node node
                    ? node.Name.ToString()
                    : collision.GetCollider().ToString() ?? "unknown";
                Fail(
                    $"The physical route segment {index} hit {collider}: "
                    + $"{waypoints[index]} -> {waypoints[index + 1]}.");
                return false;
            }
        }

        return true;
    }

    private bool AssertBoundaryCollision(Vector2 start, Vector2 motion, string expectedPrefix)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(motion, testOnly: true);
        if (collision?.GetCollider() is Node collider
            && (collider.Name.ToString().StartsWith(expectedPrefix, StringComparison.Ordinal)
                || collider.GetParent().Name == "RouteBoundaries"))
        {
            return true;
        }

        Fail($"Expected {expectedPrefix} collision, received {collision?.GetCollider()}.");
        return false;
    }

    private static bool HasExactBoundaries(
        Node2D owner,
        IReadOnlyList<OpeningRegionBoundarySegment> expected,
        int expectedCount)
    {
        if (owner.GetChildCount() != expectedCount || expected.Count != expectedCount)
        {
            return false;
        }

        for (int index = 0; index < expectedCount; index++)
        {
            if (owner.GetChild(index) is not StaticBody2D body
                || body.GetChildCount() != 2
                || body.GetChild(0) is not CollisionShape2D collision
                || collision.Shape is not SegmentShape2D segment
                || body.GetChild(1) is not Line2D line
                || line.Points.Length != 2
                || ToNumerics(segment.A) != expected[index].Start
                || ToNumerics(segment.B) != expected[index].End
                || ToNumerics(line.Points[0]) != expected[index].Start
                || ToNumerics(line.Points[1]) != expected[index].End)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsOnSurface(Rid navigationMap, NumericsVector2 position) =>
        NavigationServer2D.MapGetClosestPoint(navigationMap, ToGodot(position))
            .DistanceTo(ToGodot(position)) <= SurfaceTolerancePixels;

    private static Vector2 ToGodot(NumericsVector2 position) => new(position.X, position.Y);

    private static NumericsVector2 ToNumerics(Vector2 position) => new(position.X, position.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
