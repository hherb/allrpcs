using Godot;

namespace Allrpcs.Game;

/// <summary>Projects Seeker movement into bounded four-direction walk and idle presentation.</summary>
public partial class SeekerWalkAnimationPresenter : AnimatedSprite2D
{
    private static readonly StringName IdleSouth = new("idle_south");
    private static readonly StringName IdleWest = new("idle_west");
    private static readonly StringName IdleEast = new("idle_east");
    private static readonly StringName IdleNorth = new("idle_north");
    private static readonly StringName WalkSouth = new("walk_south");
    private static readonly StringName WalkWest = new("walk_west");
    private static readonly StringName WalkEast = new("walk_east");
    private static readonly StringName WalkNorth = new("walk_north");
    private StringName idleAnimation = IdleSouth;

    /// <summary>Shows the movement direction, or breathes while retaining that facing at rest.</summary>
    public void PresentMovement(Vector2 movement)
    {
        if (movement.IsZeroApprox())
        {
            if (Animation != idleAnimation || !IsPlaying())
            {
                Play(idleAnimation);
            }

            return;
        }

        (StringName walk, StringName idle) = SelectAnimations(movement);
        idleAnimation = idle;
        if (Animation != walk || !IsPlaying())
        {
            Play(walk);
        }
    }

    private static (StringName Walk, StringName Idle) SelectAnimations(Vector2 movement)
    {
        if (Mathf.Abs(movement.X) > Mathf.Abs(movement.Y))
        {
            return movement.X < 0.0f ? (WalkWest, IdleWest) : (WalkEast, IdleEast);
        }

        return movement.Y < 0.0f ? (WalkNorth, IdleNorth) : (WalkSouth, IdleSouth);
    }
}
