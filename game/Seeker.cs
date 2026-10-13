using Allrpcs.Application.Audio;
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
    private SeekerWalkAnimationPresenter? walkAnimationPresenter;
    private long motionTick;

    /// <summary>Observes completed displacement without allowing presentation to deny movement.</summary>
    public event Action<FootstepMotionSample>? MotionCompleted;

    /// <inheritdoc />
    public override void _Ready()
    {
        walkAnimationPresenter = GetNode<SeekerWalkAnimationPresenter>("WalkAnimation");
    }

    /// <summary>
    /// Gets or sets the Seeker's maximum movement speed in world pixels per second.
    /// </summary>
    [Export(PropertyHint.Range, "1.0,600.0,1.0,or_greater")]
    public float MovementSpeedPixelsPerSecond { get; set; } = DefaultMovementSpeedPixelsPerSecond;

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = ControlInputContext.WorldAllowed(this) ? Input.GetVector(
            MoveLeftAction,
            MoveRightAction,
            MoveUpAction,
            MoveDownAction) : Vector2.Zero;
        NumericsVector2 velocity = MovementCalculator.CalculateVelocity(
            new NumericsVector2(input.X, input.Y),
            MovementSpeedPixelsPerSecond);

        Velocity = new Vector2(velocity.X, velocity.Y);
        walkAnimationPresenter?.PresentMovement(Velocity);
        Vector2 start = GlobalPosition;
        MoveAndSlide();
        FootstepMotionSample sample = new(++motionTick,
            new NumericsVector2(start.X, start.Y), new NumericsVector2(GlobalPosition.X, GlobalPosition.Y),
            delta, MovementSpeedPixelsPerSecond);
        foreach (Delegate listener in MotionCompleted?.GetInvocationList() ?? [])
        {
            try { ((Action<FootstepMotionSample>)listener)(sample); }
            catch (Exception error) { GD.PrintErr($"Footstep observer failed: {error.GetType().Name}"); }
        }
    }
}
