using Allrpcs.Application.Movement;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Moves the placeholder healer toward the Seeker over the authored navigation surface.
/// </summary>
public partial class Healer : CharacterBody2D
{
    private const float DefaultMovementSpeedPixelsPerSecond = 180.0f;
    private const float DefaultStopDistancePixels = 72.0f;
    private const float PathPointTolerancePixels = 1.0f;

    private NavigationAgent2D navigationAgent = null!;
    private Node2D followedTarget = null!;

    /// <summary>
    /// Gets or sets the healer's maximum movement speed in world pixels per second.
    /// </summary>
    [Export(PropertyHint.Range, "1.0,600.0,1.0,or_greater")]
    public float MovementSpeedPixelsPerSecond { get; set; } =
        DefaultMovementSpeedPixelsPerSecond;

    /// <summary>
    /// Gets or sets the inclusive distance from the Seeker at which the healer stops moving.
    /// </summary>
    [Export(PropertyHint.Range, "0.0,300.0,1.0,or_greater")]
    public float StopDistancePixels { get; set; } = DefaultStopDistancePixels;

    /// <inheritdoc />
    public override void _Ready()
    {
        navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        followedTarget = GetNode<Node2D>("../Seeker");
        navigationAgent.TargetDesiredDistance = StopDistancePixels;
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;

        FollowTargetSelection selection = FollowTargetSelector.Select(
            ToNumerics(GlobalPosition),
            ToNumerics(followedTarget.GlobalPosition),
            MovementSpeedPixelsPerSecond,
            StopDistancePixels);
        if (!selection.ShouldMove)
        {
            StopMoving();
            return;
        }

        navigationAgent.TargetPosition = selection.NavigationDestination is { } destination
            ? ToGodot(destination)
            : GlobalPosition;
        Vector2 nextPathPosition = navigationAgent.GetNextPathPosition();
        Vector2 pathOffset = nextPathPosition - GlobalPosition;
        if (pathOffset.LengthSquared() <= PathPointTolerancePixels * PathPointTolerancePixels)
        {
            StopMoving();
            return;
        }

        NumericsVector2 velocity = MovementCalculator.CalculateVelocity(
            ToNumerics(pathOffset.Normalized()),
            selection.SpeedPixelsPerSecond);
        Velocity = ToGodot(velocity);
        MoveAndSlide();
    }

    private void StopMoving()
    {
        Velocity = Vector2.Zero;
        MoveAndSlide();
    }

    private static NumericsVector2 ToNumerics(Vector2 vector)
    {
        return new NumericsVector2(vector.X, vector.Y);
    }

    private static Vector2 ToGodot(NumericsVector2 vector)
    {
        return new Vector2(vector.X, vector.Y);
    }
}
