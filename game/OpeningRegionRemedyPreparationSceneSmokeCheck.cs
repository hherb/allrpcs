using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the exact garden and inert remedy-preparation station when enabled.</summary>
public partial class OpeningRegionRemedyPreparationSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_REMEDY_PREPARATION_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MinimumSynchronizationPhysicsFrames = 3;
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const float SurfaceTolerancePixels = 1.0f;

    private int physicsFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        SetPhysicsProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        physicsFrames++;
        OpeningRegionRemedyPreparationSceneSource source =
            GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "../OpeningRegionRemedyPreparationSceneSource");
        OpeningRegionRemedyPreparationSceneBinding? binding = source.Binding;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = binding is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && NavigationServer2D.MapGetClosestPoint(
                    navigationMap,
                    ToGodot(binding.StationPosition))
                .DistanceTo(ToGodot(binding.StationPosition)) <= SurfaceTolerancePixels;
        if ((!synchronized || physicsFrames < MinimumSynchronizationPhysicsFrames)
            && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (binding is null || !synchronized)
        {
            Fail("The remedy-preparation scene or synchronized station surface was unavailable.");
            return;
        }

        RunChecks(binding, source, navigationMap);
    }

    private void RunChecks(
        OpeningRegionRemedyPreparationSceneBinding binding,
        OpeningRegionRemedyPreparationSceneSource source,
        Rid navigationMap)
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        FirstPatientPreparationController preparation =
            GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        Vector2 stationPosition = ToGodot(binding.StationPosition);
        Vector2[] path = NavigationServer2D.MapGetPath(
            navigationMap,
            GetNode<CharacterBody2D>("../Seeker").GlobalPosition,
            stationPosition,
            optimize: true);
        if (!ReferenceEquals(binding.Projection, projection)
            || !projection.Anchors.Any(anchor => ReferenceEquals(anchor, binding.GardenAnchor))
            || binding.GardenAnchor.Location.Identity.Id.Value != "healer-garden"
            || !ReferenceEquals(binding.Recipe, binding.TreatmentContext.Scenario.Remedy.Recipe)
            || !ReferenceEquals(preparation.Recipe, binding.Recipe)
            || preparation.Activity is not null
            || preparation.Outcome is not null
            || binding.StationPosition.X != source.Position.X
            || binding.StationPosition.Y != source.Position.Y
            || path.Length < 2
            || path[^1].DistanceTo(stationPosition) > SurfaceTolerancePixels)
        {
            Fail("The garden station lost exact anchor, recipe, inert state, or navigation access.");
            return;
        }

        GD.Print(
            "Opening-region remedy-preparation scene smoke passed: exact healer-garden anchor, "
            + "exact first-remedy recipe, bounded reachable station, and inert preparation state.");
        GetTree().Quit();
    }

    private static Vector2 ToGodot(System.Numerics.Vector2 position) =>
        new(position.X, position.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
