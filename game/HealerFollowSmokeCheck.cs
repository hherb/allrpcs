using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free healer-follow assertions when explicitly enabled by the smoke script.
/// </summary>
public partial class HealerFollowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_HEALER_FOLLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const int MaximumFollowPhysicsFrames = 300;
    private const int StableStopPhysicsFrames = 30;
    private const int MaximumBridgePhysicsFrames = 600;
    private const float NavigationSynchronizationTolerancePixels = 1.0f;
    private const float FollowArrivalTolerancePixels = 2.0f;
    private const float StablePositionTolerancePixels = 1.0f;
    private const float MinimumBodySeparationPixels = 35.0f;
    private const float RequiredInitialSeparationPixels = 200.0f;
    private const float RequiredFollowDisplacementPixels = 100.0f;
    private const float LowerBankThresholdY = 286.0f;

    private static readonly Vector2 FollowStart = new(560.0f, 450.0f);
    private static readonly Vector2 LowerFieldTarget = new(800.0f, 450.0f);
    private static readonly Vector2 UpperFieldStart = new(808.0f, 64.0f);

    private SmokeStage stage;
    private int stagePhysicsFrames;
    private Healer healer = null!;
    private CharacterBody2D seeker = null!;
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
                CheckFollowAndArrival();
                break;
            case SmokeStage.HoldingStopDistance:
                CheckStableStop();
                break;
            case SmokeStage.CrossingBridge:
                CheckBridgeRegroup();
                break;
            default:
                Fail("Healer follow smoke entered an unknown stage.");
                break;
        }
    }

    private void CheckSynchronization()
    {
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool isSynchronized = NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && NavigationServer2D.MapGetClosestPoint(navigationMap, LowerFieldTarget)
                .DistanceTo(LowerFieldTarget) <= NavigationSynchronizationTolerancePixels;
        if (isSynchronized)
        {
            BeginFollowCheck();
            return;
        }

        if (stagePhysicsFrames >= MaximumSynchronizationPhysicsFrames)
        {
            Fail("Healer follow smoke could not synchronize the navigation map.");
        }
    }

    private void BeginFollowCheck()
    {
        PlaceCharacters(FollowStart, LowerFieldTarget);
        float initialSeparation = healer.GlobalPosition.DistanceTo(seeker.GlobalPosition);
        if (initialSeparation < RequiredInitialSeparationPixels)
        {
            Fail("Healer follow smoke did not create the required representative separation.");
            return;
        }

        minimumObservedSeparation = initialSeparation;
        SetStage(SmokeStage.Following);
    }

    private void CheckFollowAndArrival()
    {
        if (!TryObserveSeparation(out float separation))
        {
            return;
        }

        float displacement = healer.GlobalPosition.DistanceTo(FollowStart);
        if (separation <= healer.StopDistancePixels + FollowArrivalTolerancePixels
            && displacement >= RequiredFollowDisplacementPixels)
        {
            stationaryPosition = healer.GlobalPosition;
            SetStage(SmokeStage.HoldingStopDistance);
            return;
        }

        if (stagePhysicsFrames >= MaximumFollowPhysicsFrames)
        {
            Fail(
                "Healer did not follow the separated Seeker into the configured stop distance. "
                + $"Separation: {separation}; displacement: {displacement}.");
        }
    }

    private void CheckStableStop()
    {
        if (!TryObserveSeparation(out float separation))
        {
            return;
        }

        float drift = healer.GlobalPosition.DistanceTo(stationaryPosition);
        if (drift > StablePositionTolerancePixels)
        {
            Fail($"Healer drifted {drift} pixels after reaching the stop distance.");
            return;
        }

        if (stagePhysicsFrames < StableStopPhysicsFrames)
        {
            return;
        }

        GD.Print(
            $"Healer follow diagnostics: lower-field stop separation={separation}, "
            + $"minimum separation={minimumObservedSeparation}.");
        PlaceCharacters(UpperFieldStart, LowerFieldTarget);
        minimumObservedSeparation = healer.GlobalPosition.DistanceTo(seeker.GlobalPosition);
        SetStage(SmokeStage.CrossingBridge);
    }

    private void CheckBridgeRegroup()
    {
        if (!TryObserveSeparation(out float separation))
        {
            return;
        }

        bool reachedLowerBank = healer.GlobalPosition.Y >= LowerBankThresholdY;
        if (reachedLowerBank
            && separation <= healer.StopDistancePixels + FollowArrivalTolerancePixels)
        {
            GD.Print(
                "Healer follow smoke passed: separated follower moved, held stop distance, "
                + "preserved body separation, and regrouped across the bridge. "
                + $"Final separation={separation}; minimum separation={minimumObservedSeparation}.");
            SetPhysicsProcess(false);
            GetTree().Quit();
            return;
        }

        if (stagePhysicsFrames >= MaximumBridgePhysicsFrames)
        {
            Fail(
                "Healer did not regroup with the Seeker through the bridge. "
                + $"Position: {healer.GlobalPosition}; separation: {separation}.");
        }
    }

    private bool TryObserveSeparation(out float separation)
    {
        separation = healer.GlobalPosition.DistanceTo(seeker.GlobalPosition);
        minimumObservedSeparation = MathF.Min(minimumObservedSeparation, separation);
        if (separation < MinimumBodySeparationPixels)
        {
            Fail(
                $"Healer overlapped the Seeker: observed separation was {separation} pixels.");
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
        HoldingStopDistance,
        CrossingBridge,
    }
}
