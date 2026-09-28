using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves collision-backed village transitions and their synchronized round trip.</summary>
public partial class OpeningRegionVillageTransitionSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPENING_REGION_VILLAGE_TRANSITION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MinimumSynchronizationPhysicsFrames = 3;
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const float SurfaceTolerancePixels = 1.0f;
    private const int ExpectedBoundarySectionCount = 11;

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
        OpeningRegionVillageTransitionSceneSource source =
            GetNode<OpeningRegionVillageTransitionSceneSource>(
                "../OpeningRegionVillageTransitionSceneSource");
        OpeningRegionVillageTransitionLayout? layout = source.Layout;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && NavigationServer2D.MapGetClosestPoint(
                    navigationMap,
                    ToGodot(layout.SceneBinding.VillageSquareAnchor.Position))
                .DistanceTo(ToGodot(layout.SceneBinding.VillageSquareAnchor.Position))
                <= SurfaceTolerancePixels;
        if (!synchronized && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The village transition layout or synchronized navigation surface was unavailable.");
            return;
        }

        RunChecks(layout, navigationMap);
    }

    private void RunChecks(OpeningRegionVillageTransitionLayout layout, Rid navigationMap)
    {
        Node boundaries = GetNode("../OpeningRegionVillageTransitionSceneSource/Boundaries");
        if (boundaries.GetChildCount() != ExpectedBoundarySectionCount
            || boundaries.GetChildren().Any(child => child is not StaticBody2D body
                || body.GetChildCount() != 1
                || body.GetChild(0) is not CollisionShape2D shape
                || shape.Shape is null)
            || !ReferenceEquals(
                layout.SceneBinding.Projection,
                GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection))
        {
            Fail("The village transitions lost collision sections or exact projection provenance.");
            return;
        }

        Vector2 garden = ToGodot(layout.GardenApproach);
        Vector2 square = ToGodot(layout.SceneBinding.VillageSquareAnchor.Position);
        Vector2 edge = ToGodot(layout.SceneBinding.VillageEdgeAnchor.Position);
        List<NumericsVector2> roundTrip = [];
        foreach ((Vector2 origin, Vector2 destination) in new[]
        {
            (garden, square),
            (square, edge),
            (edge, square),
            (square, garden),
        })
        {
            Vector2[] path = NavigationServer2D.MapGetPath(
                navigationMap,
                origin,
                destination,
                optimize: true);
            if (path.Length < 2 || path[^1].DistanceTo(destination) > SurfaceTolerancePixels)
            {
                Fail("The synchronized village transition round trip did not reach every endpoint.");
                return;
            }

            AppendPath(roundTrip, path);
        }

        if (!OpeningRegionVillageTransitionRoutePolicy.IsAcceptable(layout, roundTrip)
            || !AssertPhysicalOpenings(garden, square, edge)
            || !AssertBoundaryCollision(square, Vector2.Up * 120.0f, "SquareTop")
            || !AssertBoundaryCollision(edge, Vector2.Down * 120.0f, "EdgeBottom"))
        {
            return;
        }

        GD.Print(
            "Opening-region village transition smoke passed: 11 collision-backed boundary "
            + "sections, all retained topology openings, and synchronized round trip.");
        GetTree().Quit();
    }

    private bool AssertPhysicalOpenings(Vector2 garden, Vector2 square, Vector2 edge)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        return AssertClear(seeker, garden, square - garden, "garden-to-square opening")
            && AssertClear(seeker, square, edge - square, "square-to-edge openings")
            && AssertClear(seeker, edge, square - edge, "edge-to-square openings")
            && AssertClear(seeker, square, garden - square, "square-to-garden opening");
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
            && (collider.Name == expectedName
                || collider.GetParent().Name == "RouteBoundaries"))
        {
            return true;
        }

        string actual = collision?.GetCollider() is Node actualCollider
            ? actualCollider.Name.ToString()
            : "none";
        Fail($"Expected village boundary collision from {expectedName}, received {actual}.");
        return false;
    }

    private static void AppendPath(List<NumericsVector2> destination, Vector2[] path)
    {
        foreach (Vector2 point in path)
        {
            NumericsVector2 converted = new(point.X, point.Y);
            if (destination.Count == 0 || destination[^1] != converted)
            {
                destination.Add(converted);
            }
        }
    }

    private static Vector2 ToGodot(NumericsVector2 position) =>
        new(position.X, position.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
