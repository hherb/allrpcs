using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
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
    private InteractionFocusController focusController = null!;
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
        focusController = GetNode<InteractionFocusController>("../InteractionFocusController");
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
        CharacterPerceptionSnapshot? expectedPerception = controller.Perception;
        int? expectedFocusedTargetIndex = focusController.FocusedTargetIndex;
        ObjectId? expectedFocusedTargetObjectId = focusController.FocusedTargetObjectId;
        GreyboxInteractionTarget[] expectedTargets = Enumerable.Range(
                0,
                focusController.TargetCount)
            .Select(focusController.GetTarget)
            .ToArray();
        string[] expectedObjectIdTexts = expectedTargets
            .Select(target => target.ObjectIdText)
            .ToArray();
        ObjectId[] expectedTargetObjectIds = expectedTargets
            .Select(target => target.ObjectId)
            .ToArray();
        Vector2[] expectedTargetPositions = expectedTargets
            .Select(target => target.Position)
            .ToArray();
        bool[] expectedTargetVisibility = expectedTargets
            .Select(target => target.Visible)
            .ToArray();
        bool[] expectedInteractionVisibility = expectedTargets
            .Select(target => target.IsInteractionVisible)
            .ToArray();
        bool[] expectedInteractionEligibility = expectedTargets
            .Select(target => target.IsInteractionEnabled)
            .ToArray();
        bool[] expectedFocusPresentation = expectedTargets
            .Select(target => target.IsFocused)
            .ToArray();
        Label?[] expectedMarkerLabels = expectedTargets
            .Select(target => target.GetNodeOrNull<Label>("Label"))
            .ToArray();
        string?[] expectedMarkerLabelTexts = expectedMarkerLabels
            .Select(label => label?.Text)
            .ToArray();
        bool expectedSearchIndicatorVisibility = searchIndicator.Visible;
        bool expectedHerbPatchIndicatorVisibility = herbPatchResolvedIndicator.Visible;
        bool expectedStoneMarkerIndicatorVisibility = stoneMarkerResolvedIndicator.Visible;
        WorldObjectSnapshot first = source.WorldObjects;
        WorldObjectSnapshot second = source.WorldObjects;

        if (!HasExpectedPropertySurface() || !HasExpectedMarkerPropertySurface())
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
        WorldObjectIdentity herbPatchIdentity = herbPatchObject.Identity;
        WorldObjectIdentity stoneMarkerIdentity = stoneMarkerObject.Identity;
        string herbPatchName = herbPatchIdentity.DisplayName;
        string stoneMarkerName = stoneMarkerIdentity.DisplayName;
        if (ReferenceEquals(herbPatchObject, stoneMarkerObject)
            || ReferenceEquals(herbPatchIdentity, stoneMarkerIdentity)
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
            || !ReferenceEquals(controller.Perception, expectedPerception)
            || focusController.FocusedTargetIndex != expectedFocusedTargetIndex
            || !ReferenceEquals(
                focusController.FocusedTargetObjectId,
                expectedFocusedTargetObjectId)
            || !ReferenceEquals(first.Entries[0].Identity, herbPatchIdentity)
            || !ReferenceEquals(first.Entries[1].Identity, stoneMarkerIdentity)
            || !ReferenceEquals(first.Entries[0].Identity.DisplayName, herbPatchName)
            || !ReferenceEquals(first.Entries[1].Identity.DisplayName, stoneMarkerName)
            || !HasUnchangedTargets(
                expectedTargets,
                expectedObjectIdTexts,
                expectedTargetObjectIds,
                expectedTargetPositions,
                expectedTargetVisibility,
                expectedInteractionVisibility,
                expectedInteractionEligibility,
                expectedFocusPresentation,
                expectedMarkerLabels,
                expectedMarkerLabelTexts)
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
            + "distinct objects, independently authored value-equal identifiers, exact names, "
            + "empty descriptions, stable nested source values, and unchanged marker order, "
            + "geometry, visibility, focus, controller snapshots, and presentation were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedWorldObject(
        WorldObject worldObject,
        GreyboxInteractionTarget target,
        string expectedObjectId,
        string expectedDisplayName)
    {
        return string.Equals(target.ObjectId.Value, expectedObjectId, StringComparison.Ordinal)
            && !ReferenceEquals(worldObject.Identity.Id, target.ObjectId)
            && worldObject.Identity.Id == target.ObjectId
            && string.Equals(
                worldObject.Identity.DisplayName,
                expectedDisplayName,
                StringComparison.Ordinal)
            && string.Equals(worldObject.Description.Text, string.Empty, StringComparison.Ordinal);
    }

    private static bool HasExpectedMarkerPropertySurface()
    {
        return typeof(GreyboxInteractionTarget).GetProperty("DisplayName") is null
            && typeof(GreyboxInteractionTarget).GetProperty("ObjectIdText") is not null;
    }

    private bool HasUnchangedTargets(
        IReadOnlyList<GreyboxInteractionTarget> expectedTargets,
        IReadOnlyList<string> expectedObjectIdTexts,
        IReadOnlyList<ObjectId> expectedTargetObjectIds,
        IReadOnlyList<Vector2> expectedTargetPositions,
        IReadOnlyList<bool> expectedTargetVisibility,
        IReadOnlyList<bool> expectedInteractionVisibility,
        IReadOnlyList<bool> expectedInteractionEligibility,
        IReadOnlyList<bool> expectedFocusPresentation,
        IReadOnlyList<Label?> expectedMarkerLabels,
        IReadOnlyList<string?> expectedMarkerLabelTexts)
    {
        return expectedTargets.Select(
                (target, index) =>
                    ReferenceEquals(focusController.GetTarget(index), target)
                    && string.Equals(
                        target.ObjectIdText,
                        expectedObjectIdTexts[index],
                        StringComparison.Ordinal)
                    && ReferenceEquals(target.ObjectId, expectedTargetObjectIds[index])
                    && target.Position == expectedTargetPositions[index]
                    && target.Visible == expectedTargetVisibility[index]
                    && target.IsInteractionVisible == expectedInteractionVisibility[index]
                    && target.IsInteractionEnabled == expectedInteractionEligibility[index]
                    && target.IsFocused == expectedFocusPresentation[index]
                    && ReferenceEquals(
                        target.GetNodeOrNull<Label>("Label"),
                        expectedMarkerLabels[index])
                    && string.Equals(
                        expectedMarkerLabels[index]?.Text,
                        expectedMarkerLabelTexts[index],
                        StringComparison.Ordinal))
            .All(isUnchanged => isUnchanged);
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
