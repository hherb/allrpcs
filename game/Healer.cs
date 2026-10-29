using Allrpcs.Application.Audio;
using Allrpcs.Application.Movement;
using Allrpcs.Application.Navigation;
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
    private const float DefaultDestinationStopDistancePixels = 12.0f;
    private const float PathPointTolerancePixels = 1.0f;
    private const float NavigationSurfaceTolerancePixels = 1.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 3200.0f, 1800.0f);

    /// <summary>Changes only when an external command replaces the current navigation ownership.</summary>
    internal ulong CommandRevision { get; private set; }

    /// <summary>Checks that the authored examination destination has a complete navigation route.</summary>
    internal bool TryCommandExaminationMove(Vector2 destination)
    {
        if (!destination.IsFinite()) { return false; }
        Rid map = NavigationServer2D.RegionGetMap(navigationRegion.GetRid());
        if (NavigationServer2D.MapGetIterationId(map) == 0) { return false; }
        Vector2[] path = NavigationServer2D.MapGetPath(map, GlobalPosition, destination, true);
        return path.Length > 0 && path[^1].DistanceTo(destination) <= NavigationSurfaceTolerancePixels
            && TryCommandMoveToDestination(destination);
    }

    private long motionTick;
    /// <summary>Exact most recent producer-issued motion, without world-state authority.</summary>
    internal FootstepMotionSample? LastMotion { get; private set; }
    /// <summary>Exception-isolated observation after collision-resolved motion, including stationary stops.</summary>
    internal event Action<FootstepMotionSample>? MotionCompleted;

    private HealerCommandSelection activeCommand = HealerCommandSelector.SelectFollow();
    private NavigationAgent2D navigationAgent = null!;
    private NavigationRegion2D navigationRegion = null!;
    private CharacterBody2D followedTarget = null!;
    private SeekerWalkAnimationPresenter walkAnimationPresenter = null!;

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

    /// <summary>
    /// Gets or sets the inclusive distance at which a move-to-destination command completes.
    /// </summary>
    [Export(PropertyHint.Range, "0.0,100.0,1.0,or_greater")]
    public float DestinationStopDistancePixels { get; set; } =
        DefaultDestinationStopDistancePixels;

    /// <summary>Gets the currently selected deterministic healer mode.</summary>
    public HealerCommandMode CommandMode => activeCommand.Mode;

    /// <inheritdoc />
    public override void _Ready()
    {
        navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        walkAnimationPresenter = GetNode<SeekerWalkAnimationPresenter>("WalkAnimation");
        navigationRegion = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        followedTarget = GetNode<CharacterBody2D>("../Seeker");
        AddCollisionExceptionWith(followedTarget);
        followedTarget.AddCollisionExceptionWith(this);
        navigationAgent.PathPostprocessing =
            NavigationPathQueryParameters2D.PathPostProcessing.Edgecentered;
        navigationAgent.TargetDesiredDistance = StopDistancePixels;
    }

    /// <summary>Selects deterministic follow mode.</summary>
    public void CommandFollow()
    {
        CommandRevision++;
        activeCommand = HealerCommandSelector.SelectFollow();
    }

    /// <summary>Selects deterministic wait mode.</summary>
    public void CommandWait()
    {
        CommandRevision++;
        activeCommand = HealerCommandSelector.SelectWait();
    }

    /// <summary>
    /// Selects a move command only when its finite in-bounds destination is on the navigation map.
    /// </summary>
    /// <param name="destination">The requested world destination.</param>
    /// <returns><see langword="true"/> when the command was accepted.</returns>
    public bool TryCommandMoveToDestination(Vector2 destination)
    {
        HealerCommandSelection selection;
        try
        {
            selection = HealerCommandSelector.SelectMoveToDestination(
                WorldBounds,
                ToNumerics(destination));
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        Rid navigationMap = NavigationServer2D.RegionGetMap(navigationRegion.GetRid());
        Vector2 closestPoint = NavigationServer2D.MapGetClosestPoint(navigationMap, destination);
        if (NavigationServer2D.MapGetIterationId(navigationMap) == 0
            || closestPoint.DistanceTo(destination) > NavigationSurfaceTolerancePixels)
        {
            return false;
        }

        CommandRevision++;
        activeCommand = selection;
        return true;
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        if (activeCommand.Mode == HealerCommandMode.Wait)
        {
            StopMoving(delta);
            return;
        }

        NumericsVector2 destination;
        float stopDistancePixels;
        bool waitOnArrival;
        if (activeCommand.Mode == HealerCommandMode.MoveToDestination
            && activeCommand.NavigationDestination is { } commandedDestination)
        {
            destination = commandedDestination;
            stopDistancePixels = DestinationStopDistancePixels;
            waitOnArrival = true;
        }
        else
        {
            destination = ToNumerics(followedTarget.GlobalPosition);
            stopDistancePixels = StopDistancePixels;
            waitOnArrival = false;
        }

        FollowTargetSelection selection = FollowTargetSelector.Select(
            ToNumerics(GlobalPosition),
            destination,
            MovementSpeedPixelsPerSecond,
            stopDistancePixels);
        if (!selection.ShouldMove)
        {
            if (waitOnArrival)
            {
                activeCommand = HealerCommandSelector.SelectWait();
            }

            StopMoving(delta);
            return;
        }

        navigationAgent.TargetDesiredDistance = stopDistancePixels;
        navigationAgent.TargetPosition = selection.NavigationDestination is { } targetDestination
            ? ToGodot(targetDestination)
            : GlobalPosition;
        Vector2 nextPathPosition = navigationAgent.GetNextPathPosition();
        Vector2 pathOffset = nextPathPosition - GlobalPosition;
        if (pathOffset.LengthSquared() <= PathPointTolerancePixels * PathPointTolerancePixels)
        {
            StopMoving(delta);
            return;
        }

        NumericsVector2 velocity = MovementCalculator.CalculateVelocity(
            ToNumerics(pathOffset.Normalized()),
            selection.SpeedPixelsPerSecond);
        Velocity = ToGodot(velocity);
        CompleteMovement(delta);
    }

    private void StopMoving(double delta)
    {
        Velocity = Vector2.Zero;
        CompleteMovement(delta);
    }

    private void CompleteMovement(double delta)
    {
        // Presentation only, as for the Seeker: the chosen velocity picks the facing and walk/idle loop.
        walkAnimationPresenter.PresentMovement(Velocity);
        Vector2 start = GlobalPosition;
        MoveAndSlide();
        if (motionTick == long.MaxValue) { LastMotion = null; return; }
        FootstepMotionSample sample = new(++motionTick, ToNumerics(start), ToNumerics(GlobalPosition),
            delta, MovementSpeedPixelsPerSecond);
        LastMotion = sample;
        foreach (Delegate observer in MotionCompleted?.GetInvocationList() ?? [])
        {
            try { ((Action<FootstepMotionSample>)observer)(sample); }
            catch (Exception error) { GD.PrintErr("Healer motion observer: " + error.GetType().Name); }
        }
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
