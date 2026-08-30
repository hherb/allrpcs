using Allrpcs.Application.Movement;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Maps named player input actions to continuous Seeker movement.
/// </summary>
public partial class Seeker : CharacterBody2D
{
    private const float DefaultMovementSpeedPixelsPerSecond = 240.0f;
    private static readonly StringName MoveLeftAction = new("move_left");
    private static readonly StringName MoveRightAction = new("move_right");
    private static readonly StringName MoveUpAction = new("move_up");
    private static readonly StringName MoveDownAction = new("move_down");

    /// <summary>
    /// Gets or sets the Seeker's maximum movement speed in world pixels per second.
    /// </summary>
    [Export(PropertyHint.Range, "1.0,600.0,1.0,or_greater")]
    public float MovementSpeedPixelsPerSecond { get; set; } = DefaultMovementSpeedPixelsPerSecond;

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;

        Vector2 input = Input.GetVector(
            MoveLeftAction,
            MoveRightAction,
            MoveUpAction,
            MoveDownAction);
        NumericsVector2 velocity = MovementCalculator.CalculateVelocity(
            new NumericsVector2(input.X, input.Y),
            MovementSpeedPixelsPerSecond);

        Velocity = new Vector2(velocity.X, velocity.Y);
        MoveAndSlide();
    }
}
