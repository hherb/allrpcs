using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free temporary world-object source checks when explicitly enabled.
/// </summary>
public partial class TemporaryWorldObjectSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WORLD_OBJECT_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string HerbPatchObjectId = "herb-patch";
    private const string HerbPatchDisplayName = "HERB PATCH";
    private const string StoneMarkerObjectId = "stone-marker";
    private const string StoneMarkerDisplayName = "STONE MARKER";

    private TemporaryWorldObjectSource source = null!;
    private FocusedSearchActivityController controller = null!;
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

        source = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        controller = GetNode<FocusedSearchActivityController>(
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

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        bool expectedSearchIndicatorVisibility = searchIndicator.Visible;
        bool expectedHerbPatchIndicatorVisibility = herbPatchResolvedIndicator.Visible;
        bool expectedStoneMarkerIndicatorVisibility = stoneMarkerResolvedIndicator.Visible;
        WorldObjectSnapshot first = source.WorldObjects;
        WorldObjectSnapshot second = source.WorldObjects;

        if (!HasExpectedPropertySurface())
        {
            Fail("Temporary world-object source smoke failed its bounded snapshot surface.");
            return;
        }

        if (!ReferenceEquals(first, second) || first.Entries.Count != 2)
        {
            Fail("Temporary world-object source smoke failed stable ordered snapshot retention.");
            return;
        }

        WorldObject herbPatchObject = first.Entries[0];
        WorldObject stoneMarkerObject = first.Entries[1];
        if (ReferenceEquals(herbPatchObject, stoneMarkerObject)
            || !HasExpectedWorldObject(
                herbPatchObject,
                herbPatch,
                HerbPatchObjectId,
                HerbPatchDisplayName)
            || !HasExpectedWorldObject(
                stoneMarkerObject,
                stoneMarker,
                StoneMarkerObjectId,
                StoneMarkerDisplayName))
        {
            Fail("Temporary world-object source smoke failed exact authored target retention.");
            return;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || searchIndicator.Visible != expectedSearchIndicatorVisibility
            || herbPatchResolvedIndicator.Visible != expectedHerbPatchIndicatorVisibility
            || stoneMarkerResolvedIndicator.Visible != expectedStoneMarkerIndicatorVisibility)
        {
            Fail(
                "Temporary world-object source smoke changed controller state or presentation.");
            return;
        }

        GD.Print(
            "Temporary world-object source smoke passed: bounded surface, stable ordered snapshot, "
            + "distinct objects, exact authored identifiers and names, empty descriptions, "
            + "unchanged targets, controller snapshots, and presentation were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedWorldObject(
        WorldObject worldObject,
        GreyboxInteractionTarget target,
        string expectedObjectId,
        string expectedDisplayName)
    {
        return string.Equals(target.ObjectId.Value, expectedObjectId, StringComparison.Ordinal)
            && string.Equals(target.DisplayName, expectedDisplayName, StringComparison.Ordinal)
            && ReferenceEquals(worldObject.Identity.Id, target.ObjectId)
            && ReferenceEquals(worldObject.Identity.DisplayName, target.DisplayName)
            && string.Equals(worldObject.Description.Text, string.Empty, StringComparison.Ordinal);
    }

    private static bool HasExpectedPropertySurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(TemporaryWorldObjectSource)
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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
