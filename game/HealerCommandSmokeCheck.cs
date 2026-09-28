using Allrpcs.Application.Movement;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free healer-command assertions when explicitly enabled by the smoke script.
/// </summary>
public partial class HealerCommandSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_HEALER_COMMAND_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const int MaximumFollowPhysicsFrames = 300;
    private const int StableWaitPhysicsFrames = 30;
    private const int MaximumMovePhysicsFrames = 600;
    private const int MaximumReturnPhysicsFrames = 600;
    private const float NavigationSynchronizationTolerancePixels = 1.0f;
    private const float ArrivalTolerancePixels = 2.0f;
    private const float StablePositionTolerancePixels = 1.0f;
    private const float MinimumBodySeparationPixels = 35.0f;
    private const float RequiredInitialSeparationPixels = 200.0f;
    private const float RequiredFollowDisplacementPixels = 100.0f;
    private const float RequiredMoveDisplacementPixels = 300.0f;
    private static readonly Vector2 FollowStart = new(1700.0f, 900.0f);
    private static readonly Vector2 FollowTarget = new(2200.0f, 900.0f);
    private static readonly Vector2 SeparatedWaitTarget = new(2640.0f, 1450.0f);
    private static readonly Vector2 ValidMoveDestination = new(1800.0f, 500.0f);
    private static readonly Vector2 InvalidDestination = new(-1.0f, 450.0f);
    private static readonly Vector2 ExcludedVillageDestination = new(2800.0f, 300.0f);

    private SmokeStage stage;
    private int stagePhysicsFrames;
    private Healer healer = null!;
    private CharacterBody2D seeker = null!;
    private bool collisionPassThroughChecked;
    private Vector2 moveStart;
    private Vector2 stationaryPosition;
    private float minimumObservedSeparation = float.PositiveInfinity;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetPhysicsProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        healer = GetNode<Healer>("../Healer");
        seeker = GetNode<CharacterBody2D>("../Seeker");
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        stagePhysicsFrames++;

        switch (stage)
        {
            case SmokeStage.Synchronizing:
                CheckSynchronization();
                break;
            case SmokeStage.Following:
                CheckFollowAndBeginWait();
                break;
            case SmokeStage.Waiting:
                CheckStableWaitAndBeginMove();
                break;
            case SmokeStage.MovingToDestination:
                CheckMoveCompletionAndResumeFollow();
                break;
            case SmokeStage.ReturningToFollow:
                CheckReturnToFollow();
                break;
            default:
                Fail("Healer command smoke entered an unknown stage.");
                break;
        }
    }

    private void CheckSynchronization()
    {
        if (!collisionPassThroughChecked)
        {
            collisionPassThroughChecked = true;
            if (!healer.GetCollisionExceptions().Contains(seeker)
                || !seeker.GetCollisionExceptions().Contains(healer))
            {
                Fail("Healer and Seeker did not mutually ignore body collision.");
                return;
            }
        }

        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool isSynchronized = NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && NavigationServer2D.MapGetClosestPoint(navigationMap, FollowTarget)
                .DistanceTo(FollowTarget) <= NavigationSynchronizationTolerancePixels;
        if (isSynchronized)
        {
            BeginFollowCheck();
            return;
        }

        if (stagePhysicsFrames >= MaximumSynchronizationPhysicsFrames)
        {
            Fail("Healer command smoke could not synchronize the navigation map.");
        }
    }

    private void BeginFollowCheck()
    {
        PlaceCharacters(FollowStart, FollowTarget);
        healer.CommandFollow();
        float initialSeparation = healer.GlobalPosition.DistanceTo(seeker.GlobalPosition);
        if (initialSeparation < RequiredInitialSeparationPixels)
        {
            Fail("Healer command smoke did not create the required representative separation.");
            return;
        }

        minimumObservedSeparation = initialSeparation;
        SetStage(SmokeStage.Following);
    }

    private void CheckFollowAndBeginWait()
    {
        if (!TryObserveSeparation(out float separation))
        {
            return;
        }

        float displacement = healer.GlobalPosition.DistanceTo(FollowStart);
        if (separation <= healer.StopDistancePixels + ArrivalTolerancePixels
            && displacement >= RequiredFollowDisplacementPixels)
        {
            healer.CommandWait();
            stationaryPosition = healer.GlobalPosition;
            seeker.GlobalPosition = SeparatedWaitTarget;
            SetStage(SmokeStage.Waiting);
            return;
        }

        if (stagePhysicsFrames >= MaximumFollowPhysicsFrames)
        {
            Fail(
                "Healer did not follow before the wait command. "
                + $"Separation: {separation}; displacement: {displacement}.");
        }
    }

    private void CheckStableWaitAndBeginMove()
    {
        float drift = healer.GlobalPosition.DistanceTo(stationaryPosition);
        if (healer.CommandMode != HealerCommandMode.Wait)
        {
            Fail($"Healer left wait mode unexpectedly for {healer.CommandMode}.");
            return;
        }

        if (drift > StablePositionTolerancePixels)
        {
            Fail($"Healer drifted {drift} pixels during the wait command.");
            return;
        }

        if (stagePhysicsFrames < StableWaitPhysicsFrames)
        {
            return;
        }

        if (healer.TryCommandMoveToDestination(InvalidDestination))
        {
            Fail("Healer accepted an out-of-bounds move destination.");
            return;
        }

        if (healer.TryCommandMoveToDestination(ExcludedVillageDestination))
        {
            Fail("Healer accepted a destination excluded from the navigation surface.");
            return;
        }

        if (healer.CommandMode != HealerCommandMode.Wait)
        {
            Fail("A rejected move destination changed the healer command.");
            return;
        }

        moveStart = healer.GlobalPosition;
        if (!healer.TryCommandMoveToDestination(ValidMoveDestination))
        {
            Fail("Healer rejected the representative valid move destination.");
            return;
        }

        SetStage(SmokeStage.MovingToDestination);
    }

    private void CheckMoveCompletionAndResumeFollow()
    {
        float destinationDistance = healer.GlobalPosition.DistanceTo(ValidMoveDestination);
        float displacement = healer.GlobalPosition.DistanceTo(moveStart);
        if (healer.CommandMode == HealerCommandMode.Wait
            && destinationDistance <= healer.DestinationStopDistancePixels + ArrivalTolerancePixels
            && displacement >= RequiredMoveDisplacementPixels)
        {
            PlaceCharacters(healer.GlobalPosition, FollowTarget);
            healer.CommandFollow();
            minimumObservedSeparation = healer.GlobalPosition.DistanceTo(seeker.GlobalPosition);
            SetStage(SmokeStage.ReturningToFollow);
            return;
        }

        if (stagePhysicsFrames >= MaximumMovePhysicsFrames)
        {
            Fail(
                "Healer did not complete the valid move command and transition to wait. "
                + $"Mode: {healer.CommandMode}; position: {healer.GlobalPosition}; "
                + $"destination distance: {destinationDistance}; "
                + $"displacement: {displacement}.");
        }
    }

    private void CheckReturnToFollow()
    {
        if (!TryObserveSeparation(out float separation))
        {
            return;
        }

        if (separation <= healer.StopDistancePixels + ArrivalTolerancePixels)
        {
            GD.Print(
                "Healer command smoke passed: mutual body pass-through was configured, follow "
                + "transitioned to stable wait, invalid and "
                + "excluded destinations were rejected, a valid move completed, and follow "
                + $"resumed across the approved route network. Final separation={separation}; "
                + $"minimum separation={minimumObservedSeparation}.");
            SetPhysicsProcess(false);
            GetTree().Quit();
            return;
        }

        if (stagePhysicsFrames >= MaximumReturnPhysicsFrames)
        {
            Fail(
                "Healer did not return to follow across the approved route network. "
                + $"Position: {healer.GlobalPosition}; separation: {separation}.");
        }
    }

    private bool TryObserveSeparation(out float separation)
    {
        separation = healer.GlobalPosition.DistanceTo(seeker.GlobalPosition);
        minimumObservedSeparation = MathF.Min(minimumObservedSeparation, separation);
        if (separation < MinimumBodySeparationPixels)
        {
            Fail($"Healer overlapped the Seeker at {separation} pixels of separation.");
            return false;
        }

        return true;
    }

    private void PlaceCharacters(Vector2 healerPosition, Vector2 seekerPosition)
    {
        healer.GlobalPosition = healerPosition;
        healer.Velocity = Vector2.Zero;
        seeker.GlobalPosition = seekerPosition;
        seeker.Velocity = Vector2.Zero;
    }

    private void SetStage(SmokeStage nextStage)
    {
        stage = nextStage;
        stagePhysicsFrames = 0;
    }

    private void Fail(string message)
    {
        SetPhysicsProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private enum SmokeStage
    {
        Synchronizing,
        Following,
        Waiting,
        MovingToDestination,
        ReturningToFollow,
    }
}
