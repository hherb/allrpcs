using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free world-event log controller checks when explicitly enabled.</summary>
public partial class WorldEventLogControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WORLD_EVENT_LOG_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private WorldEventLogController controller = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private WorldObjectController worldObjectController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<WorldEventLogController>("../WorldEventLogController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldEventLogSnapshot emptyLog = controller.EventLog;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        WorldEventId first = new("greybox-ready");

        if (emptyLog.Entries.Count != 0
            || !controller.TryAppend(first)
            || !HasExpectedControllerSurface())
        {
            Fail("World-event log controller failed empty ownership or initial append.");
            return;
        }

        WorldEventLogSnapshot appendedLog = controller.EventLog;
        WorldEventId duplicate = new("greybox-ready");
        if (ReferenceEquals(emptyLog, appendedLog)
            || appendedLog.Entries.Count != 1
            || !ReferenceEquals(appendedLog.Entries[0], first)
            || controller.TryAppend(duplicate)
            || !ReferenceEquals(controller.EventLog, appendedLog))
        {
            Fail("World-event log controller failed exact append or duplicate rejection.");
            return;
        }

        if (!RejectsNullAppend()
            || !ReferenceEquals(controller.EventLog, appendedLog)
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || !ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects))
        {
            Fail("World-event log controller changed state after rejection or changed unrelated state.");
            return;
        }

        GD.Print(
            "World-event log controller smoke passed: empty ownership, exact append, value-equal "
            + "duplicate and null rejection, minimal surface, and unchanged unrelated state were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedControllerSurface()
    {
        System.Reflection.PropertyInfo property = AssertSingle(
            typeof(WorldEventLogController).GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly));
        System.Reflection.MethodInfo method = AssertSingle(
            typeof(WorldEventLogController)
                .GetMethods(
                    System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.DeclaredOnly)
                .Where(candidate => !candidate.IsSpecialName)
                .ToArray());

        return property.Name == nameof(WorldEventLogController.EventLog)
            && property.PropertyType == typeof(WorldEventLogSnapshot)
            && property.SetMethod is not null
            && property.SetMethod.IsPrivate
            && method.Name == nameof(WorldEventLogController.TryAppend)
            && method.ReturnType == typeof(bool)
            && method.GetParameters().Length == 1
            && method.GetParameters()[0].ParameterType == typeof(WorldEventId);
    }

    private bool RejectsNullAppend()
    {
        try
        {
            controller.TryAppend(null!);
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private static T AssertSingle<T>(IReadOnlyCollection<T> values) =>
        values.Count == 1
            ? values.Single()
            : throw new InvalidOperationException("Expected one reflected member.");

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
