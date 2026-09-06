using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free world-object controller initialization checks when explicitly enabled.
/// </summary>
public partial class WorldObjectControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WORLD_OBJECT_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private WorldObjectController controller = null!;
    private TemporaryWorldObjectSource source = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private GreyboxInteractionTarget herbPatch = null!;
    private GreyboxInteractionTarget stoneMarker = null!;
    private Line2D searchIndicator = null!;
    private Label herbPatchResolvedIndicator = null!;
    private Label stoneMarkerResolvedIndicator = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<WorldObjectController>("../WorldObjectController");
        source = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        herbPatch = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/HerbPatch");
        stoneMarker = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/StoneMarker");
        searchIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        herbPatchResolvedIndicator = GetNode<Label>(
            "../InteractionCandidates/HerbPatch/TemporaryResolvedPerceptionIndicator/Indicator");
        stoneMarkerResolvedIndicator = GetNode<Label>(
            "../InteractionCandidates/StoneMarker/TemporaryResolvedPerceptionIndicator/Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldObjectSnapshot sourceSnapshot = source.WorldObjects;
        WorldObjectSnapshot? initializedSnapshot = controller.WorldObjects;
        if (initializedSnapshot is null
            || !ReferenceEquals(initializedSnapshot, sourceSnapshot)
            || sourceSnapshot.Entries.Count != 2
            || !HasExpectedControllerSurface())
        {
            Fail(
                "World-object controller smoke failed its optional exact scene-wired snapshot "
                + "boundary.");
            return;
        }

        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        bool expectedSearchIndicatorVisibility = searchIndicator.Visible;
        bool expectedHerbPatchIndicatorVisibility = herbPatchResolvedIndicator.Visible;
        bool expectedStoneMarkerIndicatorVisibility = stoneMarkerResolvedIndicator.Visible;
        object herbPatchObjectId = herbPatch.ObjectId;
        object stoneMarkerObjectId = stoneMarker.ObjectId;

        WorldObject expectedHerbPatch = sourceSnapshot.Entries[0];
        WorldObject expectedStoneMarker = sourceSnapshot.Entries[1];
        ObjectId valueEqualStoneMarkerObjectId = new(stoneMarker.ObjectId.Value);
        WorldObject? herbPatchResult = controller.FindWorldObject(herbPatch.ObjectId);
        WorldObject? stoneMarkerResult = controller.FindWorldObject(valueEqualStoneMarkerObjectId);
        WorldObject? absentResult = controller.FindWorldObject(new ObjectId("absent-object"));
        WorldObjectController uninitializedController = new();
        AddChild(uninitializedController);
        WorldObject? uninitializedResult = uninitializedController.FindWorldObject(
            herbPatch.ObjectId);

        if (!ReferenceEquals(herbPatchResult, expectedHerbPatch)
            || !ReferenceEquals(stoneMarkerResult, expectedStoneMarker)
            || ReferenceEquals(valueEqualStoneMarkerObjectId, stoneMarker.ObjectId)
            || valueEqualStoneMarkerObjectId != stoneMarker.ObjectId
            || absentResult is not null
            || uninitializedResult is not null
            || uninitializedController.WorldObjects is not null)
        {
            Fail(
                "World-object controller smoke failed exact, value-equal, absent, or "
                + "uninitialized lookup behavior.");
            return;
        }

        if (controller.TryInitializeWorldObjects(sourceSnapshot))
        {
            Fail("World-object controller smoke accepted repeated initialization.");
            return;
        }

        if (!RejectsNullInitialization())
        {
            return;
        }

        if (!RejectsNullLookup(controller) || !RejectsNullLookup(uninitializedController))
        {
            return;
        }

        if (!ReferenceEquals(controller.WorldObjects, initializedSnapshot)
            || uninitializedController.WorldObjects is not null
            || !ReferenceEquals(source.WorldObjects, sourceSnapshot)
            || !ReferenceEquals(sourceSnapshot.Entries[0], expectedHerbPatch)
            || !ReferenceEquals(sourceSnapshot.Entries[1], expectedStoneMarker)
            || !ReferenceEquals(herbPatch.ObjectId, herbPatchObjectId)
            || !ReferenceEquals(stoneMarker.ObjectId, stoneMarkerObjectId)
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || searchIndicator.Visible != expectedSearchIndicatorVisibility
            || herbPatchResolvedIndicator.Visible != expectedHerbPatchIndicatorVisibility
            || stoneMarkerResolvedIndicator.Visible != expectedStoneMarkerIndicatorVisibility)
        {
            Fail(
                "World-object controller smoke changed its first snapshot, source, targets, "
                + "focused-search state, or presentation after rejected initialization.");
            return;
        }

        GD.Print(
            "World-object controller smoke passed: optional bounded surface, exact and "
            + "value-equal lookup, absent and uninitialized results, null and repeated "
            + "rejection, exact scene-wired snapshot retention, and unchanged source, targets, "
            + "focused-search state, and presentation were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedControllerSurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(WorldObjectController)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);

        System.Reflection.MethodInfo[] methods = typeof(WorldObjectController)
            .GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        return properties.Length == 1
            && string.Equals(properties[0].Name, "WorldObjects", StringComparison.Ordinal)
            && properties[0].PropertyType == typeof(WorldObjectSnapshot)
            && properties[0].CanRead
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate
            && methods.Length == 2
            && methods.Any(
                method =>
                    string.Equals(
                        method.Name,
                        nameof(WorldObjectController.TryInitializeWorldObjects),
                        StringComparison.Ordinal)
                    && method.ReturnType == typeof(bool)
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(WorldObjectSnapshot))
            && methods.Any(
                method =>
                    string.Equals(
                        method.Name,
                        "FindWorldObject",
                        StringComparison.Ordinal)
                    && method.ReturnType == typeof(WorldObject)
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(ObjectId));
    }

    private bool RejectsNullInitialization()
    {
        try
        {
            controller.TryInitializeWorldObjects(null!);
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        Fail("World-object controller smoke failed null rejection.");
        return false;
    }

    private bool RejectsNullLookup(WorldObjectController targetController)
    {
        try
        {
            targetController.FindWorldObject(null!);
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        Fail("World-object controller smoke failed null lookup rejection.");
        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
