using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves collision-backed stream/laundry geometry and its synchronized round trip.</summary>
public partial class OpeningRegionStreamLaundrySmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPENING_REGION_STREAM_LAUNDRY_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const float SurfaceTolerancePixels = 1.0f;
    private const int ExpectedLaundryBoundarySectionCount = 6;
    private const int ExpectedStreamBankCount = 2;

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
        OpeningRegionStreamLaundrySceneSource source =
            GetNode<OpeningRegionStreamLaundrySceneSource>(
                "../OpeningRegionStreamLaundrySceneSource");
        OpeningRegionStreamLaundryLayout? layout = source.Layout;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && IsOnSurface(navigationMap, layout.SceneBinding.StreamAnchor.Position)
            && IsOnSurface(navigationMap, layout.SceneBinding.AbandonedLaundryAnchor.Position);
        if (!synchronized && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The stream/laundry layout or synchronized navigation surface was unavailable.");
            return;
        }

        RunChecks(layout, navigationMap);
    }

    private void RunChecks(OpeningRegionStreamLaundryLayout layout, Rid navigationMap)
    {
        Node laundryBoundaries = GetNode(
            "../OpeningRegionStreamLaundrySceneSource/LaundryBoundaries");
        StaticBody2D streamBanks = GetNode<StaticBody2D>("../StreamCollision");
        if (!HasCollisionShapes(laundryBoundaries, ExpectedLaundryBoundarySectionCount)
            || streamBanks.GetChildCount() != ExpectedStreamBankCount
            || streamBanks.GetChildren().Any(child =>
                child is not CollisionPolygon2D polygon || polygon.Polygon.Length < 3)
            || !MatchesBankGeometry(streamBanks, layout)
            || !ReferenceEquals(
                layout.SceneBinding.Projection,
                GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection))
        {
            Fail("The stream/laundry scene lost collision geometry or exact projection provenance.");
            return;
        }

        Vector2 villageEdge = ToGodot(layout.VillageEdgeApproach);
        Vector2 stream = ToGodot(layout.SceneBinding.StreamAnchor.Position);
        Vector2 bridgeNorth = ToGodot(layout.BridgeNorthApproach);
        Vector2 laundry = ToGodot(layout.SceneBinding.AbandonedLaundryAnchor.Position);
        OpeningRegionVillageTransitionLayout villageLayout =
            GetNode<OpeningRegionVillageTransitionSceneSource>(
                "../OpeningRegionVillageTransitionSceneSource").Layout;
        Vector2 villageEdgeOpening = ToGodot(villageLayout.EdgeStreamOpening);
        List<NumericsVector2> roundTrip = [];
        foreach ((Vector2 origin, Vector2 destination) in new[]
        {
            (villageEdge, stream),
            (stream, bridgeNorth),
            (bridgeNorth, laundry),
            (laundry, bridgeNorth),
            (bridgeNorth, stream),
            (stream, villageEdge),
        })
        {
            Vector2[] path = NavigationServer2D.MapGetPath(
                navigationMap,
                origin,
                destination,
                optimize: true);
            if (path.Length < 2 || path[^1].DistanceTo(destination) > SurfaceTolerancePixels)
            {
                Fail("The synchronized stream/laundry round trip did not reach every endpoint.");
                return;
            }

            AppendPath(roundTrip, path);
        }

        if (!OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(layout, roundTrip)
            || !AssertPhysicalRoute(
                villageEdge,
                villageEdgeOpening,
                stream,
                bridgeNorth,
                ToGodot(layout.LaundryStreamOpening),
                laundry)
            || !AssertBoundaryCollision(
                new Vector2(1668.0f, 600.0f),
                Vector2.Up * 200.0f,
                "StreamCollision")
            || !AssertBoundaryCollision(laundry, Vector2.Up * 160.0f, "LaundryTop"))
        {
            return;
        }

        GD.Print(
            "Opening-region stream/laundry smoke passed: two collision-backed banks, six inert "
            + "laundry boundary sections, explicit route openings, and synchronized round trip.");
        GetTree().Quit();
    }

    private bool AssertPhysicalRoute(
        Vector2 villageEdge,
        Vector2 villageEdgeOpening,
        Vector2 stream,
        Vector2 bridgeNorth,
        Vector2 laundryStreamOpening,
        Vector2 laundry)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        return AssertClear(
                seeker,
                villageEdge,
                villageEdgeOpening - villageEdge,
                "village-edge north opening")
            && AssertClear(
                seeker,
                villageEdgeOpening,
                stream - villageEdgeOpening,
                "village-edge-to-stream route")
            && AssertClear(seeker, stream, bridgeNorth - stream, "northbound bridge")
            && AssertClear(
                seeker,
                bridgeNorth,
                laundryStreamOpening - bridgeNorth,
                "stream-to-laundry approach")
            && AssertClear(
                seeker,
                laundryStreamOpening,
                laundry - laundryStreamOpening,
                "stream-to-laundry opening")
            && AssertClear(seeker, laundry, laundryStreamOpening - laundry, "laundry return opening")
            && AssertClear(
                seeker,
                laundryStreamOpening,
                bridgeNorth - laundryStreamOpening,
                "laundry-to-stream approach")
            && AssertClear(seeker, bridgeNorth, stream - bridgeNorth, "southbound bridge")
            && AssertClear(seeker, stream, villageEdgeOpening - stream, "stream-to-village-edge route")
            && AssertClear(
                seeker,
                villageEdgeOpening,
                villageEdge - villageEdgeOpening,
                "village-edge north return opening");
    }

    private bool AssertClear(
        CharacterBody2D seeker,
        Vector2 start,
        Vector2 motion,
        string description)
    {
        seeker.GlobalPosition = start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(motion, testOnly: true);
        if (collision is null)
        {
            return true;
        }

        Fail($"Expected a clear {description}, but the Seeker hit {collision.GetCollider()}.");
        return false;
    }

    private bool AssertBoundaryCollision(Vector2 start, Vector2 motion, string expectedName)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(motion, testOnly: true);
        if (collision?.GetCollider() is Node collider
            && (collider.Name == expectedName || collider.GetParent().Name == "RouteBoundaries"))
        {
            return true;
        }

        string actual = collision?.GetCollider() is Node actualCollider
            ? actualCollider.Name.ToString()
            : "none";
        Fail($"Expected stream/laundry collision from {expectedName}, received {actual}.");
        return false;
    }

    private static bool HasCollisionShapes(Node boundaries, int expectedCount) =>
        boundaries.GetChildCount() == expectedCount
        && boundaries.GetChildren().All(child => child is StaticBody2D body
            && body.GetChildCount() == 1
            && body.GetChild(0) is CollisionShape2D shape
            && shape.Shape is not null);

    private static bool MatchesBankGeometry(
        StaticBody2D streamBanks,
        OpeningRegionStreamLaundryLayout layout)
    {
        CollisionPolygon2D west = streamBanks.GetNode<CollisionPolygon2D>("WestCollision");
        CollisionPolygon2D east = streamBanks.GetNode<CollisionPolygon2D>("EastCollision");
        return MatchesPolygon(west.Polygon, layout.WestStreamBank)
            && MatchesPolygon(east.Polygon, layout.EastStreamBank);
    }

    private static bool MatchesPolygon(
        Vector2[] actual,
        IReadOnlyList<NumericsVector2> expected) =>
        actual.Length == expected.Count
        && actual.Select(ToNumerics).SequenceEqual(expected);

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
