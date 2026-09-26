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
            new Vector2(800.0f, 50.0f),
            new Vector2(0.0f, -100.0f),
            "../Boundaries/Top",
            "top outer border"),
        new(
            new Vector2(800.0f, 850.0f),
            new Vector2(0.0f, 100.0f),
            "../Boundaries/Bottom",
            "bottom outer border"),
        new(
            new Vector2(50.0f, 500.0f),
            new Vector2(-100.0f, 0.0f),
            "../Boundaries/Left",
            "left outer border"),
        new(
            new Vector2(1550.0f, 500.0f),
            new Vector2(100.0f, 0.0f),
            "../Boundaries/Right",
            "right outer border"),
        new(
            new Vector2(500.0f, 300.0f),
            new Vector2(0.0f, -250.0f),
            "../StreamCollision",
            "stream outside the bridge"),
        new(
            new Vector2(900.0f, 640.0f),
            new Vector2(300.0f, 0.0f),
            "../VillageCollision",
            "village block"),
        new(
            new Vector2(150.0f, 400.0f),
            new Vector2(200.0f, 0.0f),
            "../TreeCluster/TreeWest",
            "tree cluster"),
    ];

    private static readonly ClearMotion[] ClearMotions =
    [
        new(
            new Vector2(808.0f, 320.0f),
            new Vector2(0.0f, -280.0f),
            "south-to-north bridge traversal"),
        new(
            new Vector2(808.0f, 40.0f),
            new Vector2(0.0f, 280.0f),
            "north-to-south bridge traversal"),
        new(
            new Vector2(620.0f, 620.0f),
            new Vector2(70.0f, 124.0f),
            "representative open lower terrain"),
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
            "Seeker movement-boundary smoke passed: all outer borders, representative stream, "
            + "village, and tree collisions blocked movement while both bridge directions and "
            + "open lower terrain remained traversable.");
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
            Fail(
                $"Expected the {scenario.Description} collision from "
                + $"{expectedCollider.GetPath()}, received {collision.GetCollider()}.");
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
