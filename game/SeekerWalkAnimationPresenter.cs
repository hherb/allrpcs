using Godot;

namespace Allrpcs.Game;

/// <summary>Projects Seeker or Healer movement into bounded four-direction walk and idle presentation.</summary>
public partial class SeekerWalkAnimationPresenter : AnimatedSprite2D
{
    private static readonly StringName IdleSouth = new("idle_south");
    private static readonly StringName IdleWest = new("idle_west");
    private static readonly StringName IdleEast = new("idle_east");
    private static readonly StringName IdleNorth = new("idle_north");
    private static readonly StringName InteractionSouth = new("interact_south");
    private static readonly StringName InteractionWest = new("interact_west");
    private static readonly StringName InteractionEast = new("interact_east");
    private static readonly StringName InteractionNorth = new("interact_north");
    private static readonly StringName WalkSouth = new("walk_south");
    private static readonly StringName WalkWest = new("walk_west");
    private static readonly StringName WalkEast = new("walk_east");
    private static readonly StringName WalkNorth = new("walk_north");
    private StringName idleAnimation = IdleSouth;
    private StringName interactionAnimation = InteractionSouth;
    private bool interactionPlaying;

    /// <inheritdoc />
    public override void _Ready()
    {
        AnimationFinished += OnAnimationFinished;
    }

    /// <summary>Shows the non-authoritative interaction gesture for the retained facing.</summary>
    public void PresentSuccessfulInteraction()
    {
        interactionPlaying = true;
        Play(interactionAnimation);
    }

    /// <summary>Shows the movement direction, or breathes while retaining that facing at rest.</summary>
    public void PresentMovement(Vector2 movement)
    {
        if (movement.IsZeroApprox())
        {
            if (interactionPlaying && IsPlaying())
            {
                return;
            }

            interactionPlaying = false;
            if (Animation != idleAnimation || !IsPlaying())
            {
                Play(idleAnimation);
            }

            return;
        }

        interactionPlaying = false;
        (StringName walk, StringName idle, StringName interaction) = SelectAnimations(movement);
        idleAnimation = idle;
        interactionAnimation = interaction;
        if (Animation != walk || !IsPlaying())
        {
            Play(walk);
        }
    }

    private static (StringName Walk, StringName Idle, StringName Interaction) SelectAnimations(
        Vector2 movement)
    {
        if (Mathf.Abs(movement.X) > Mathf.Abs(movement.Y))
        {
            return movement.X < 0.0f
                ? (WalkWest, IdleWest, InteractionWest)
                : (WalkEast, IdleEast, InteractionEast);
        }

        return movement.Y < 0.0f
            ? (WalkNorth, IdleNorth, InteractionNorth)
            : (WalkSouth, IdleSouth, InteractionSouth);
    }

    private void OnAnimationFinished()
    {
        if (!interactionPlaying || Animation != interactionAnimation)
        {
            return;
        }

        interactionPlaying = false;
        Play(idleAnimation);
    }
}
