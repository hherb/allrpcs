using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free navigation assertions when explicitly enabled by the smoke script.
/// </summary>
public partial class NavigationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_NAVIGATION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MinimumSynchronizationPhysicsFrames = 3;
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const float DestinationTolerancePixels = 1.0f;
    private const float WorldRight = 3200.0f;
    private const float WorldBottom = 1800.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, WorldRight, WorldBottom);
    private static readonly Vector2 ReachableOrigin = new(2200.0f, 900.0f);
    private static readonly Vector2 ReachableDestination = new(1800.0f, 500.0f);
    private static readonly Vector2 StreamObstacleDestination = new(3000.0f, 300.0f);
    private static readonly Vector2 TreeObstacleDestination = new(300.0f, 1000.0f);
    private static readonly Vector2 VillageObstacleDestination = new(2700.0f, 600.0f);

    private int physicsFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        SetPhysicsProcess(
            OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        physicsFrames++;
        if (physicsFrames < MinimumSynchronizationPhysicsFrames)
        {
            return;
        }

        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool hasSynchronizedSurface = NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && NavigationServer2D.MapGetClosestPoint(navigationMap, ReachableOrigin)
                .DistanceTo(ReachableOrigin) <= DestinationTolerancePixels;
        if (!hasSynchronizedSurface && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        RunChecks(region, navigationMap);
    }

    private void RunChecks(NavigationRegion2D region, Rid navigationMap)
    {
        GD.Print(
            $"Navigation diagnostics: vertices={region.NavigationPolygon?.Vertices.Length ?? 0}, "
            + $"polygons={region.NavigationPolygon?.GetPolygonCount() ?? 0}, "
            + $"region_iteration={NavigationServer2D.RegionGetIterationId(region.GetRid())}, "
            + $"map_iteration={NavigationServer2D.MapGetIterationId(navigationMap)}.");
        if (NavigationServer2D.MapGetIterationId(navigationMap) == 0
            || NavigationServer2D.MapGetRegions(navigationMap).Count == 0)
        {
            Fail("Navigation map did not synchronize an authored region.");
            return;
        }

        try
        {
            AssertReachable(navigationMap, ReachableOrigin, ReachableDestination, "bridge crossing");
            AssertUnreachable(
                navigationMap,
                ReachableOrigin,
                StreamObstacleDestination,
                "stream outside crossing");
            AssertUnreachable(
                navigationMap,
                ReachableOrigin,
                TreeObstacleDestination,
                "tree cluster");
            AssertUnreachable(
                navigationMap,
                ReachableOrigin,
                VillageObstacleDestination,
                "village block");
        }
        catch (InvalidOperationException exception)
        {
            Fail(exception.Message);
            return;
        }
        catch (ArgumentOutOfRangeException exception)
        {
            Fail(exception.Message);
            return;
        }

        GD.Print(
            "Navigation smoke passed: map synchronized, retained route reachable, "
            + "empty-area budgets excluded.");
        GetTree().Quit();
    }

    private static void AssertReachable(
        Rid navigationMap,
        Vector2 origin,
        Vector2 destination,
        string description)
    {
        NavigationRequestValidator.Create(
            WorldBounds,
            ToNumerics(origin),
            ToNumerics(destination));
        Vector2[] path = NavigationServer2D.MapGetPath(
            navigationMap,
            origin,
            destination,
            optimize: true);

        if (path.Length < 2 || path[^1].DistanceTo(destination) > DestinationTolerancePixels)
        {
            Vector2 closestOrigin = NavigationServer2D.MapGetClosestPoint(navigationMap, origin);
            Vector2 closestDestination = NavigationServer2D.MapGetClosestPoint(
                navigationMap,
                destination);
            string endpoint = path.Length == 0 ? "none" : path[^1].ToString();
            throw new InvalidOperationException(
                $"Expected reachable navigation path: {description}. "
                + $"Path points: {path.Length}; endpoint: {endpoint}; "
                + $"closest origin: {closestOrigin}; closest destination: {closestDestination}.");
        }
    }

    private static void AssertUnreachable(
        Rid navigationMap,
        Vector2 origin,
        Vector2 destination,
        string description)
    {
        NavigationRequestValidator.Create(
            WorldBounds,
            ToNumerics(origin),
            ToNumerics(destination));
        Vector2[] path = NavigationServer2D.MapGetPath(
            navigationMap,
            origin,
            destination,
            optimize: true);

        if (path.Length > 0 && path[^1].DistanceTo(destination) <= DestinationTolerancePixels)
        {
            throw new InvalidOperationException($"Expected excluded navigation target: {description}.");
        }
    }

    private static NumericsVector2 ToNumerics(Vector2 vector)
    {
        return new NumericsVector2(vector.X, vector.Y);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
