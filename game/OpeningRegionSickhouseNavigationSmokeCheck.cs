using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves the Seeker can enter, approach the patient, and leave the sickhouse.</summary>
public partial class OpeningRegionSickhouseNavigationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPENING_REGION_SICKHOUSE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MinimumSynchronizationPhysicsFrames = 3;
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const float SurfaceTolerancePixels = 1.0f;
    private const int ExpectedWallBodyCount = 5;

    private int physicsFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        SetPhysicsProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        physicsFrames++;
        OpeningRegionFirstPatientSceneSource source =
            GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource");
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        OpeningRegionSickhouseLayout? layout = source.Layout;
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && NavigationServer2D.MapGetClosestPoint(
                    navigationMap,
                    ToGodot(layout.PatientApproach))
                .DistanceTo(ToGodot(layout.PatientApproach)) <= SurfaceTolerancePixels;
        if ((!synchronized || physicsFrames < MinimumSynchronizationPhysicsFrames)
            && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The sickhouse layout or synchronized patient-approach surface was unavailable.");
            return;
        }

        RunChecks(layout, navigationMap);
    }

    private void RunChecks(OpeningRegionSickhouseLayout layout, Rid navigationMap)
    {
        Node walls = GetNode<Node>("../OpeningRegionFirstPatientSceneSource/Walls");
        if (walls.GetChildCount() != ExpectedWallBodyCount
            || walls.GetChildren().Any(child => child is not StaticBody2D body
                || body.GetChildCount() != 1
                || body.GetChild(0) is not CollisionShape2D shape
                || shape.Shape is null))
        {
            Fail("The sickhouse did not retain five collision-backed wall sections and one entrance.");
            return;
        }

        Vector2 seekerStart = GetNode<CharacterBody2D>("../Seeker").GlobalPosition;
        Vector2 patientApproach = ToGodot(layout.PatientApproach);
        Vector2 representedRoomInterior = patientApproach + new Vector2(100.0f, 20.0f);
        if (NavigationServer2D.MapGetClosestPoint(navigationMap, representedRoomInterior)
                .DistanceTo(representedRoomInterior) > SurfaceTolerancePixels)
        {
            Fail("The represented sickhouse interior was excluded from navigation.");
            return;
        }

        if (!AssertClear(
                GetNode<CharacterBody2D>("../Seeker"),
                patientApproach,
                representedRoomInterior - patientApproach,
                "represented sickhouse interior"))
        {
            return;
        }

        Vector2[] inbound = NavigationServer2D.MapGetPath(
            navigationMap,
            seekerStart,
            patientApproach,
            optimize: true);
        Vector2[] outbound = NavigationServer2D.MapGetPath(
            navigationMap,
            patientApproach,
            seekerStart,
            optimize: true);
        bool inboundAcceptable = IsCompleteRoute(inbound, patientApproach, layout);
        bool outboundAcceptable = IsCompleteRoute(outbound, seekerStart, layout);
        if (!inboundAcceptable || !outboundAcceptable)
        {
            Fail(
                "The synchronized navigation route did not enter through the sickhouse door, "
                + "reach the patient approach, and leave without crossing a wall or world bound. "
                + $"Inbound points={inbound.Length}, end={RouteEnd(inbound)}, "
                + $"acceptable={inboundAcceptable}; outbound points={outbound.Length}, "
                + $"end={RouteEnd(outbound)}, acceptable={outboundAcceptable}.");
            return;
        }

        GD.Print(
            "Opening-region sickhouse navigation smoke passed: five collision-backed wall "
            + "sections retained one entrance, and the Seeker can enter, reach the exact "
            + "patient approach, and leave over bounded synchronized navigation.");
        GetTree().Quit();
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

    private static bool IsCompleteRoute(
        IReadOnlyList<Vector2> route,
        Vector2 destination,
        OpeningRegionSickhouseLayout layout) =>
        route.Count >= 2
        && route[^1].DistanceTo(destination) <= SurfaceTolerancePixels
        && OpeningRegionSickhouseRoutePolicy.IsAcceptable(
            layout,
            route.Select(ToNumerics).ToArray());

    private static Vector2 ToGodot(NumericsVector2 position) => new(position.X, position.Y);

    private static NumericsVector2 ToNumerics(Vector2 position) => new(position.X, position.Y);

    private static string RouteEnd(IReadOnlyList<Vector2> route) =>
        route.Count == 0 ? "none" : route[^1].ToString();

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
