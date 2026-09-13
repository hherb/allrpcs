using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks one exact authored scene route for the active independent task.</summary>
public partial class TemporaryCharacterIndependentTaskTravelSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_INDEPENDENT_TASK_TRAVEL_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryCharacterIndependentTaskTravelSource source =
            GetNode<TemporaryCharacterIndependentTaskTravelSource>(
                "../TemporaryCharacterIndependentTaskTravelSource");
        CharacterIndependentTaskDepartureController departureController =
            GetNode<CharacterIndependentTaskDepartureController>(
                "../CharacterIndependentTaskDepartureController");
        CharacterIndependentTaskTravelController travelController =
            GetNode<CharacterIndependentTaskTravelController>(
                "../CharacterIndependentTaskTravelController");
        Marker2D healerStart = GetNode<Marker2D>("../HealerStart");
        GreyboxInteractionTarget herbPatch =
            GetNode<GreyboxInteractionTarget>("../InteractionCandidates/HerbPatch");
        CharacterIndependentTaskActivity active = departureController.ActiveActivity!;
        NumericsVector2 origin = ToNumerics(healerStart.GlobalPosition);

        if (travelController.Plan is not { } plan
            || travelController.Progress is not { } progress
            || !ReferenceEquals(plan.Activity, active)
            || plan.Origin != origin
            || plan.Waypoints.Count != 1
            || plan.Waypoints[0] != ToNumerics(herbPatch.GlobalPosition)
            || !ReferenceEquals(progress.Plan, plan)
            || progress.CompletedWaypointCount != 0
            || progress.IsComplete)
        {
            Fail("The authored scene route was not initialized from the exact active task.");
            return;
        }

        CharacterIndependentTaskTravelPlan second = source.CreatePlan(active, origin);
        CharacterIndependentTaskActivity inactive = new(
            active.Assignment,
            CharacterIndependentTaskActivityState.Completed);
        if (ReferenceEquals(second, plan)
            || !ReferenceEquals(second.Activity, active)
            || second.Origin != origin
            || second.Waypoints.Count != 1
            || second.Waypoints[0] != plan.Waypoints[0]
            || !ThrowsArgumentNull(() => source.CreatePlan(null!, origin))
            || !ThrowsInvalidOperation(() => source.CreatePlan(inactive, origin))
            || !HasClosedSourceSurface())
        {
            Fail("The temporary travel source accepted invalid input or exposed extra policy.");
            return;
        }

        GD.Print(
            "Temporary independent-task travel source smoke passed: the exact active healer "
            + "task retains its current origin and exact herb-patch destination in one authored "
            + "scene plan without advancing progress or moving the healer.");
        GetTree().Quit();
    }

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterIndependentTaskTravelSource);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        return methods.Length == 2
            && methods[0].Name == nameof(TemporaryCharacterIndependentTaskTravelSource.CreatePlan)
            && methods[1].Name == nameof(_Ready)
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private static bool ThrowsInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
