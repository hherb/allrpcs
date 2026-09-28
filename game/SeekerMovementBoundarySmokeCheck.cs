using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free Seeker collision assertions when explicitly enabled by the smoke script.
/// </summary>
public partial class SeekerMovementBoundarySmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_BOUNDARY_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private static readonly BlockedMotion[] BlockedMotions =
    [
        new(
            new Vector2(1600.0f, 50.0f),
            new Vector2(0.0f, -100.0f),
            "../Boundaries/Top",
            "top outer border"),
        new(
            new Vector2(1600.0f, 1750.0f),
            new Vector2(0.0f, 100.0f),
            "../Boundaries/Bottom",
            "bottom outer border"),
        new(
            new Vector2(50.0f, 900.0f),
            new Vector2(-100.0f, 0.0f),
            "../Boundaries/Left",
            "left outer border"),
        new(
            new Vector2(3150.0f, 900.0f),
            new Vector2(100.0f, 0.0f),
            "../Boundaries/Right",
            "right outer border"),
        new(
            new Vector2(1650.0f, 650.0f),
            new Vector2(0.0f, -250.0f),
            "../StreamCollision",
            "stream outside the bridge"),
        new(
            new Vector2(2250.0f, 850.0f),
            new Vector2(0.0f, -100.0f),
            "../OpeningRegionVillageTransitionSceneSource/Boundaries/SquareTop",
            "village-square wall"),
        new(
            new Vector2(1180.0f, 222.0f),
            new Vector2(0.0f, -40.0f),
            "../OpeningRegionWoodlandClearingSceneSource/TreeBoundaries/ClearingTopRight",
            "clearing tree boundary"),
    ];

    private static readonly ClearMotion[] ClearMotions =
    [
        new(
            new Vector2(1800.0f, 500.0f),
            new Vector2(-300.0f, -280.0f),
            "stream-to-laundry route"),
        new(
            new Vector2(1500.0f, 220.0f),
            new Vector2(300.0f, 280.0f),
            "laundry-to-stream route"),
        new(
            new Vector2(1700.0f, 900.0f),
            new Vector2(500.0f, 0.0f),
            "village-edge-to-square route"),
    ];

    private CharacterBody2D seeker = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetPhysicsProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.SetPhysicsProcess(false);

        Healer healer = GetNode<Healer>("../Healer");
        healer.CommandWait();
        healer.SetPhysicsProcess(false);
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        SetPhysicsProcess(false);

        foreach (BlockedMotion scenario in BlockedMotions)
        {
            if (!AssertBlocked(scenario))
            {
                return;
            }
        }

        foreach (ClearMotion scenario in ClearMotions)
        {
            if (!AssertClear(scenario))
            {
                return;
            }
        }

        GD.Print(
            "Seeker movement-boundary smoke passed: all outer borders and representative stream, "
            + "village, and clearing collisions blocked movement while exact stream/laundry and "
            + "village routes remained traversable.");
        GetTree().Quit();
    }

    private bool AssertBlocked(BlockedMotion scenario)
    {
        seeker.GlobalPosition = scenario.Start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(
            scenario.Motion,
            testOnly: true);
        if (collision is null)
        {
            Fail($"Expected the {scenario.Description} to block the Seeker.");
            return false;
        }

        Node2D expectedCollider = GetNode<Node2D>(scenario.ExpectedColliderPath);
        if (collision.GetCollider() != expectedCollider)
        {
            string actual = collision.GetCollider() is Node node
                ? $"{node.Name} under {node.GetParent().Name}"
                : collision.GetCollider().ToString() ?? "unknown";
            Fail(
                $"Expected the {scenario.Description} collision from "
                + $"{expectedCollider.GetPath()}, received {actual}.");
            return false;
        }

        return true;
    }

    private bool AssertClear(ClearMotion scenario)
    {
        seeker.GlobalPosition = scenario.Start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(
            scenario.Motion,
            testOnly: true);
        if (collision is null)
        {
            return true;
        }

        string collider = collision.GetCollider() is Node node
            ? node.Name.ToString()
            : collision.GetCollider().ToString() ?? "unknown";
        Fail($"Expected clear {scenario.Description}, but the Seeker hit {collider}.");
        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private readonly record struct BlockedMotion(
        Vector2 Start,
        Vector2 Motion,
        string ExpectedColliderPath,
        string Description);

    private readonly record struct ClearMotion(
        Vector2 Start,
        Vector2 Motion,
        string Description);
}
