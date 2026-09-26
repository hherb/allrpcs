using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves the inert shrine shell, layered marker, and clearing round trip.</summary>
public partial class OpeningRegionOldShrineSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPENING_REGION_OLD_SHRINE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const int ExpectedBoundarySectionCount = 5;
    private const int ExpectedInscriptionLayerCount = 2;
    private const float SurfaceTolerancePixels = 1.0f;

    private int physicsFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool enabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
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
        OpeningRegionOldShrineLayout? layout =
            GetNode<OpeningRegionOldShrineSceneSource>(
                "../OpeningRegionOldShrineSceneSource").Layout;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && IsOnSurface(navigationMap, layout.SceneBinding.OldShrineAnchor.Position);
        if (!synchronized && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The old-shrine layout or synchronized navigation surface was unavailable.");
            return;
        }

        RunChecks(layout, navigationMap);
    }

    private void RunChecks(OpeningRegionOldShrineLayout layout, Rid navigationMap)
    {
        Node boundaries = GetNode("../OpeningRegionOldShrineSceneSource/ShrineBoundaries");
        Node2D inscription = GetNode<Node2D>(
            "../OpeningRegionOldShrineSceneSource/LayeredInscription");
        if (!HasCollisionShapes(boundaries)
            || inscription.Visible
            || inscription.GetChildCount() != ExpectedInscriptionLayerCount
            || inscription.GetChildren().Any(child => child is not Marker2D marker
                || marker.GetChildCount() != 0)
            || ToNumerics(inscription.Position)
                != OpeningRegionOldShrineGeometry.LayeredInscriptionPosition
            || !ReferenceEquals(
                layout.SceneBinding.Projection,
                GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection))
        {
            Fail("The shrine lost collision geometry, layered placement, or exact provenance.");
            return;
        }

        OpeningRegionWoodlandClearingLayout clearingLayout =
            GetNode<OpeningRegionWoodlandClearingSceneSource>(
                "../OpeningRegionWoodlandClearingSceneSource").Layout;
        NumericsVector2 clearing = clearingLayout.SceneBinding.WoodlandClearingAnchor.Position;
        NumericsVector2 shrine = layout.SceneBinding.OldShrineAnchor.Position;
        List<NumericsVector2> roundTrip = Route(navigationMap, clearing, shrine);
        if (roundTrip.Count == 0
            || !OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(clearingLayout, roundTrip)
            || !OpeningRegionOldShrineRoutePolicy.IsAcceptable(layout, roundTrip)
            || !AssertClear(ToGodot(clearing), ToGodot(shrine))
            || !AssertClear(ToGodot(shrine), ToGodot(clearing))
            || !AssertCollision(ToGodot(shrine), Vector2.Up * 80.0f, "ShrineTop")
            || !AssertCollision(ToGodot(shrine), Vector2.Left * 160.0f, "ShrineLeft"))
        {
            return;
        }

        GD.Print(
            "Opening-region old-shrine smoke passed: five collision-backed boundaries, two "
            + "topology openings, one concealed two-layer inscription marker, and synchronized "
            + "clearing round trip.");
        GetTree().Quit();
    }

    private List<NumericsVector2> Route(
        Rid navigationMap,
        NumericsVector2 clearing,
        NumericsVector2 shrine)
    {
        List<NumericsVector2> roundTrip = [];
        foreach ((NumericsVector2 start, NumericsVector2 end) in new[]
        {
            (clearing, shrine),
            (shrine, clearing),
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
                Fail("The synchronized clearing/shrine round trip did not reach its endpoint.");
                return [];
            }

            foreach (Vector2 point in path)
            {
                NumericsVector2 converted = ToNumerics(point);
                if (roundTrip.Count == 0 || roundTrip[^1] != converted)
                {
                    roundTrip.Add(converted);
                }
            }
        }

        return roundTrip;
    }

    private bool AssertClear(Vector2 start, Vector2 end)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(end - start, testOnly: true);
        if (collision is null)
        {
            return true;
        }

        Fail($"The physical clearing/shrine route hit {collision.GetCollider()}.");
        return false;
    }

    private bool AssertCollision(Vector2 start, Vector2 motion, string expectedName)
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
        Fail($"Expected shrine collision from {expectedName}, received {actual}.");
        return false;
    }

    private static bool HasCollisionShapes(Node boundaries) =>
        boundaries.GetChildCount() == ExpectedBoundarySectionCount
        && boundaries.GetChildren().All(child => child is StaticBody2D body
            && body.GetChildCount() == 1
            && body.GetChild(0) is CollisionShape2D shape
            && shape.Shape is not null);

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
