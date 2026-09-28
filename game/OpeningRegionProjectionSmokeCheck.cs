using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves every exact opening-region connection over the synchronized navigation map.</summary>
public partial class OpeningRegionProjectionSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPENING_REGION_PROJECTION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MinimumSynchronizationPhysicsFrames = 3;
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const float SurfaceTolerancePixels = 1.0f;
    private const string ExpectedApprovedProposalSha256 =
        "a5ebd5e23b5f41a8958477c953ba2763afffcb636f3bb6f69a969c0c1d57f8f1";

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

        OpeningRegionProjectionSource source =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        Vector2 entryPosition = ToGodot(source.Projection.Anchors[0].Position);
        bool hasSynchronizedSurface = NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && NavigationServer2D.MapGetClosestPoint(navigationMap, entryPosition)
                .DistanceTo(entryPosition) <= SurfaceTolerancePixels;
        if (!hasSynchronizedSurface && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        RunChecks(source.Projection, navigationMap);
    }

    private void RunChecks(OpeningRegionProjection projection, Rid navigationMap)
    {
        try
        {
            if (OpeningRegionProjectionSource.ApprovedProposalSha256
                != ExpectedApprovedProposalSha256)
            {
                throw new InvalidOperationException(
                    "The opening-region projection lost its approved proposal provenance.");
            }

            foreach (OpeningRegionAnchor anchor in projection.Anchors)
            {
                AssertEndpointClearance(projection, navigationMap, anchor);
            }

            for (int index = 0; index < projection.Connections.Count; index++)
            {
                OpeningRegionProjectedConnection connection = projection.Connections[index];
                if (!ReferenceEquals(
                        connection.TopologyConnection,
                        projection.Topology.Connections[index])
                    || !ReferenceEquals(
                        connection.TopologyConnection.First,
                        connection.FirstAnchor.Location)
                    || !ReferenceEquals(
                        connection.TopologyConnection.Second,
                        connection.SecondAnchor.Location))
                {
                    throw new InvalidOperationException(
                        "A projected connection lost its exact topology provenance.");
                }

                AssertTraversable(navigationMap, connection);
                if (connection.SecondAnchor.Location.Identity.Id.Value
                    != "closed-deeper-forest-path")
                {
                    AssertPhysicallyTraversable(connection);
                }
            }
        }
        catch (InvalidOperationException exception)
        {
            Fail(exception.Message);
            return;
        }

        GD.Print(
            "Opening-region projection smoke passed: "
            + $"{projection.Anchors.Count} exact anchors, "
            + $"{projection.Connections.Count} navigation-backed connections, "
            + $"{projection.MinimumPathClearancePixels}px path clearance, "
            + $"{projection.MinimumLandmarkClearancePixels}px landmark clearance.");
        GetTree().Quit();
    }

    private static void AssertEndpointClearance(
        OpeningRegionProjection projection,
        Rid navigationMap,
        OpeningRegionAnchor anchor)
    {
        Vector2 position = ToGodot(anchor.Position);
        float clearance = projection.MinimumPathClearancePixels;
        Vector2[] probes =
        [
            position,
            position + Vector2.Left * clearance,
            position + Vector2.Right * clearance,
            position + Vector2.Up * clearance,
            position + Vector2.Down * clearance,
        ];
        foreach (Vector2 probe in probes)
        {
            if (NavigationServer2D.MapGetClosestPoint(navigationMap, probe)
                    .DistanceTo(probe) > SurfaceTolerancePixels)
            {
                throw new InvalidOperationException(
                    $"Opening-region anchor lacks path clearance: {anchor.Location.Identity.Id.Value}.");
            }
        }
    }

    private static void AssertTraversable(
        Rid navigationMap,
        OpeningRegionProjectedConnection connection)
    {
        Vector2 origin = ToGodot(connection.FirstAnchor.Position);
        Vector2 destination = ToGodot(connection.SecondAnchor.Position);
        Vector2[] path = NavigationServer2D.MapGetPath(
            navigationMap,
            origin,
            destination,
            optimize: true);
        if (path.Length < 2 || path[^1].DistanceTo(destination) > SurfaceTolerancePixels)
        {
            throw new InvalidOperationException(
                "Expected a traversable navigation route for exact opening-region connection: "
                + $"{connection.TopologyConnection.First.Identity.Id.Value} -> "
                + $"{connection.TopologyConnection.Second.Identity.Id.Value}.");
        }
    }

    private void AssertPhysicallyTraversable(OpeningRegionProjectedConnection connection)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        Vector2 origin = ToGodot(connection.FirstAnchor.Position);
        Vector2 destination = ToGodot(connection.SecondAnchor.Position);
        seeker.GlobalPosition = origin;
        KinematicCollision2D? collision = seeker.MoveAndCollide(
            destination - origin,
            testOnly: true);
        if (collision is null)
        {
            return;
        }

        string collider = collision.GetCollider() is Node node
            ? node.Name.ToString()
            : "unknown";
        throw new InvalidOperationException(
            "Expected a physically clear opening-region connection, but "
            + $"{connection.TopologyConnection.First.Identity.Id.Value} -> "
            + $"{connection.TopologyConnection.Second.Identity.Id.Value} hit {collider}.");
    }

    private static Vector2 ToGodot(NumericsVector2 position) => new(position.X, position.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
