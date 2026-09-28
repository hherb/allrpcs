using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves clear travel to an inert collision-backed closed-forest gate.</summary>
public partial class OpeningRegionClosedForestPathSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_CLOSED_FOREST_PATH_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 60;
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
        OpeningRegionClosedForestPathSceneSource source =
            GetNode<OpeningRegionClosedForestPathSceneSource>(
                "../OpeningRegionClosedForestPathSceneSource");
        OpeningRegionClosedForestPathLayout? layout = source.Layout;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && IsOnSurface(navigationMap, layout.GateApproachPosition);
        if (!synchronized && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The closed-forest layout or navigation surface was unavailable.");
            return;
        }

        RunChecks(source, layout, navigationMap);
    }

    private void RunChecks(
        OpeningRegionClosedForestPathSceneSource source,
        OpeningRegionClosedForestPathLayout layout,
        Rid navigationMap)
    {
        OpeningRegionWoodlandClearingSceneSource clearingSource =
            GetNode<OpeningRegionWoodlandClearingSceneSource>(
                "../OpeningRegionWoodlandClearingSceneSource");
        OpeningRegionOldShrineSceneSource shrineSource =
            GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        OpeningRegionSouthernRoadWagonSceneSource roadSource =
            GetNode<OpeningRegionSouthernRoadWagonSceneSource>(
                "../OpeningRegionSouthernRoadWagonSceneSource");
        Marker2D specimen = clearingSource.GetNode<Marker2D>("ConcealedRareSpecimen");
        object discoveryState = shrineSource.DiscoveryState;
        object? presentation = shrineSource.Presentation;
        object? evidenceExposure = shrineSource.EvidenceExposure;
        bool inscriptionVisible = shrineSource.LayeredInscription.Visible;
        OpeningRegionSouthernRoadWagonSceneBinding roadBinding = roadSource.Binding;
        OpeningRegionSouthernRoadWagonLayout roadLayout = roadSource.Layout;
        Marker2D wagonMarker = roadSource.AbandonedWagonMarker;

        if (!VerifyClosedScene(source, layout)
            || !VerifyTravel(layout, navigationMap)
            || !AssertPhysicalTravel(layout)
            || !AssertSideCollision(layout)
            || !AssertGateCollision(layout))
        {
            return;
        }

        if (specimen.Visible
            || ToNumerics(specimen.Position)
                != OpeningRegionWoodlandClearingGeometry.RareSpecimenPosition
            || !ReferenceEquals(shrineSource.DiscoveryState, discoveryState)
            || !ReferenceEquals(shrineSource.Presentation, presentation)
            || !ReferenceEquals(shrineSource.EvidenceExposure, evidenceExposure)
            || shrineSource.LayeredInscription.Visible != inscriptionVisible
            || !ReferenceEquals(roadSource.Binding, roadBinding)
            || !ReferenceEquals(roadSource.Layout, roadLayout)
            || !ReferenceEquals(roadSource.AbandonedWagonMarker, wagonMarker))
        {
            Fail("The closed-path proof changed retained discovery or landmark snapshots.");
            return;
        }

        GD.Print(
            "Opening-region closed-forest-path smoke passed: exact clearing/path provenance, "
            + "two approach boundaries, one visible collision-backed closed gate, synchronized "
            + "and physically clear gate travel, representative side/gate collision rejection, "
            + "and retained clearing, shrine, road, and wagon snapshots.");
        GetTree().Quit();
    }

    private bool VerifyClosedScene(
        OpeningRegionClosedForestPathSceneSource source,
        OpeningRegionClosedForestPathLayout layout)
    {
        Node2D approachBoundaries = source.GetNode<Node2D>("ApproachBoundaries");
        StaticBody2D gate = source.GetNode<StaticBody2D>("ClosedGate");
        bool isValid = layout.IsGateClosed
            && ReferenceEquals(source.Binding.WoodlandClearingLayout,
                GetNode<OpeningRegionWoodlandClearingSceneSource>(
                    "../OpeningRegionWoodlandClearingSceneSource").Layout)
            && ReferenceEquals(source.Binding.Projection,
                GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection)
            && HasBoundaries(approachBoundaries, expectedCount: 2)
            && HasBoundaries(gate, expectedCount: 1)
            && gate.GetChild<Line2D>(1).Visible
            && source.ClosedGateMarker.Visible
            && source.ClosedGateMarker.GetChildCount() == 0
            && ToNumerics(source.ClosedGateMarker.Position) == layout.GateCenter;
        if (isValid)
        {
            return true;
        }

        return FailClosedScene();
    }

    private bool FailClosedScene()
    {
        Fail("The closed path lost exact provenance, collision geometry, or inert gate state.");
        return false;
    }

    private bool VerifyTravel(
        OpeningRegionClosedForestPathLayout layout,
        Rid navigationMap)
    {
        NumericsVector2 clearing = layout.SceneBinding.WoodlandClearingLayout
            .SceneBinding.WoodlandClearingAnchor.Position;
        Vector2[] path = NavigationServer2D.MapGetPath(
            navigationMap,
            ToGodot(clearing),
            ToGodot(layout.GateApproachPosition),
            optimize: true);
        List<NumericsVector2> route = path.Select(ToNumerics).ToList();
        bool reachesApproach = path.Length >= 2
            && path[^1].DistanceTo(ToGodot(layout.GateApproachPosition))
                <= SurfaceTolerancePixels;
        bool clearingAccepts = OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(
            layout.SceneBinding.WoodlandClearingLayout,
            route);
        bool closedPathAccepts = OpeningRegionClosedForestPathRoutePolicy.IsAcceptable(
            layout,
            [clearing, layout.GateApproachPosition]);
        if (reachesApproach && clearingAccepts && closedPathAccepts)
        {
            return true;
        }

        Fail(
            "The synchronized clearing-to-gate route was unavailable or crossed a boundary: "
            + $"points={path.Length}, reaches={reachesApproach}, clearing={clearingAccepts}, "
            + $"closed={closedPathAccepts}, approach={layout.GateApproachPosition}.");
        return false;
    }

    private bool AssertPhysicalTravel(OpeningRegionClosedForestPathLayout layout)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = ToGodot(layout.SceneBinding.WoodlandClearingLayout
            .SceneBinding.WoodlandClearingAnchor.Position);
        KinematicCollision2D? collision = seeker.MoveAndCollide(
            ToGodot(layout.GateApproachPosition) - seeker.GlobalPosition,
            testOnly: true);
        if (collision is null)
        {
            return true;
        }

        string collider = collision.GetCollider() is Node node
            ? $"{node.Name} under {node.GetParent().Name}"
            : collision.GetCollider().ToString() ?? "unknown";
        Fail($"The physical clearing-to-gate route hit {collider}.");
        return false;
    }

    private bool AssertSideCollision(OpeningRegionClosedForestPathLayout layout)
    {
        OpeningRegionBoundarySegment side = layout.ApproachBoundarySegments[0];
        NumericsVector2 probe = NumericsVector2.Lerp(side.Start, side.End, 0.5f);
        NumericsVector2 direction = NumericsVector2.Normalize(side.End - side.Start);
        NumericsVector2 normal = new(-direction.Y, direction.X);
        Vector2 from = ToGodot(probe + (normal * 24.0f));
        Vector2 to = ToGodot(probe - (normal * 24.0f));
        PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(from, to);
        Godot.Collections.Dictionary result =
            GetViewport().World2D.DirectSpaceState.IntersectRay(query);
        if (result.Count > 0
            && result["collider"].AsGodotObject() is Node collider
            && (collider.Name == "ApproachBoundary1"
                || collider.GetParent().Name == "RouteBoundaries"))
        {
            return true;
        }

        string actual = result.Count > 0
            && result["collider"].AsGodotObject() is Node actualCollider
                ? actualCollider.Name.ToString()
                : "none";
        Fail($"Expected closed-path ray collision from ApproachBoundary1, received {actual}.");
        return false;
    }

    private bool AssertGateCollision(OpeningRegionClosedForestPathLayout layout)
    {
        NumericsVector2 direction = NumericsVector2.Normalize(
            layout.GateCenter - layout.GateApproachPosition);
        return AssertCollision(
            layout.GateApproachPosition,
            direction * 48.0f,
            "ClosedGate");
    }

    private bool AssertCollision(
        NumericsVector2 start,
        NumericsVector2 motion,
        string expectedName)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = ToGodot(start);
        KinematicCollision2D? collision = seeker.MoveAndCollide(ToGodot(motion), testOnly: true);
        if (collision?.GetCollider() is Node collider && collider.Name == expectedName)
        {
            return true;
        }

        string actual = collision?.GetCollider() is Node actualCollider
            ? actualCollider.Name.ToString()
            : "none";
        Fail($"Expected closed-path collision from {expectedName}, received {actual}.");
        return false;
    }

    private static bool HasBoundaries(Node owner, int expectedCount)
    {
        IEnumerable<StaticBody2D> bodies = owner is StaticBody2D body
            ? [body]
            : owner.GetChildren().OfType<StaticBody2D>();
        StaticBody2D[] retained = bodies.ToArray();
        return retained.Length == expectedCount
            && retained.All(candidate => candidate.GetChildCount() == 2
                && candidate.GetChild(0) is CollisionShape2D collision
                && collision.Shape is SegmentShape2D
                && !collision.Disabled
                && candidate.GetChild(1) is Line2D line
                && line.Visible
                && line.Points.Length == 2);
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
