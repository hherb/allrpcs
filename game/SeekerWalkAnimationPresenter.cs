using Godot;

namespace Allrpcs.Game;

/// <summary>Projects Seeker movement into the bounded four-direction walk presentation.</summary>
public partial class SeekerWalkAnimationPresenter : AnimatedSprite2D
{
    private static readonly StringName WalkSouth = new("walk_south");
    private static readonly StringName WalkWest = new("walk_west");
    private static readonly StringName WalkEast = new("walk_east");
    private static readonly StringName WalkNorth = new("walk_north");

    /// <summary>Shows the movement direction, or retains its first facing frame while stationary.</summary>
    public void PresentMovement(Vector2 movement)
    {
        if (movement.IsZeroApprox())
        {
            Stop();
            SetFrameAndProgress(0, 0.0f);
            return;
        }

        StringName nextAnimation = SelectAnimation(movement);
        if (Animation != nextAnimation || !IsPlaying())
        {
            Play(nextAnimation);
        }
    }

    private static StringName SelectAnimation(Vector2 movement)
    {
        if (Mathf.Abs(movement.X) > Mathf.Abs(movement.Y))
        {
            return movement.X < 0.0f ? WalkWest : WalkEast;
        }

        return movement.Y < 0.0f ? WalkNorth : WalkSouth;
    }
}
