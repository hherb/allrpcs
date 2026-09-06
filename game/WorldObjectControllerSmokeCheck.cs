using Allrpcs.Domain.Discovery;
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
            || !HasExpectedPropertySurface())
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
        string herbPatchDisplayName = herbPatch.DisplayName;
        object stoneMarkerObjectId = stoneMarker.ObjectId;
        string stoneMarkerDisplayName = stoneMarker.DisplayName;

        if (controller.TryInitializeWorldObjects(sourceSnapshot))
        {
            Fail("World-object controller smoke accepted repeated initialization.");
            return;
        }

        if (!RejectsNullInitialization())
        {
            return;
        }

        if (!ReferenceEquals(controller.WorldObjects, initializedSnapshot)
            || !ReferenceEquals(source.WorldObjects, sourceSnapshot)
            || !ReferenceEquals(herbPatch.ObjectId, herbPatchObjectId)
            || !ReferenceEquals(herbPatch.DisplayName, herbPatchDisplayName)
            || !ReferenceEquals(stoneMarker.ObjectId, stoneMarkerObjectId)
            || !ReferenceEquals(stoneMarker.DisplayName, stoneMarkerDisplayName)
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
            "World-object controller smoke passed: optional bounded surface, exact scene-wired "
            + "snapshot retention, repeated and null rejection, and unchanged source, targets, "
            + "focused-search state, and presentation were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedPropertySurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(WorldObjectController)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);

        return properties.Length == 1
            && string.Equals(properties[0].Name, "WorldObjects", StringComparison.Ordinal)
            && properties[0].PropertyType == typeof(WorldObjectSnapshot)
            && properties[0].CanRead
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate;
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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
