using Allrpcs.Domain.Characters;
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
    private const string FocusedSearchResultText = "Focused search: NO ATTEMPT";
    private const string FocusStatusText = "Interaction focus: HERB PATCH";
    private const string HerbPatchObjectId = "herb-patch";
    private const string HerbPatchDisplayName = "HERB PATCH";
    private const string HerbPatchDescription =
        "A cluster of narrow leaves gives off a sharp scent.";
    private const string StoneMarkerObjectId = "stone-marker";
    private const string StoneMarkerDisplayName = "STONE MARKER";
    private const string StoneMarkerDescription =
        "Weathered carvings mark one face of the standing stone.";
    private const string WaterCaskObjectId = "village-water-cask";
    private const string WaterCaskDisplayName = "Village water cask";
    private const string WaterCaskDescription = "A covered cask beside the village well.";
    private const string InitialResolvedDescriptionText = "";

    private TemporaryWorldObjectSource source = null!;
    private InteractionFocusController focusController = null!;
    private TemporaryFocusedSearchContextSource focusedSearchContextSource = null!;
    private FocusedSearchActivityController controller = null!;
    private GreyboxInteractionTarget herbPatch = null!;
    private GreyboxInteractionTarget stoneMarker = null!;
    private Line2D searchIndicator = null!;
    private Label herbPatchResolvedIndicator = null!;
    private Label stoneMarkerResolvedIndicator = null!;
    private Label focusStatus = null!;
    private Label focusedSearchResultStatus = null!;

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
        focusedSearchContextSource = GetNode<TemporaryFocusedSearchContextSource>(
            "../TemporaryFocusedSearchContextSource");
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
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResultStatus = GetNode<Label>(
            "../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = controller.Perception;
        DiscoveryConditionSet expectedConditions = focusedSearchContextSource.Conditions;
        CharacterCapabilitySet expectedCapabilities = focusedSearchContextSource.Capabilities;
        CharacterInventory expectedInventory = focusedSearchContextSource.Inventory;
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
        string expectedHerbPatchIndicatorText = herbPatchResolvedIndicator.Text;
        string expectedStoneMarkerIndicatorText = stoneMarkerResolvedIndicator.Text;
        string expectedFocusStatusText = focusStatus.Text;
        string expectedFocusedSearchResultText = focusedSearchResultStatus.Text;
        WorldObjectSnapshot first = source.WorldObjects;
        WorldObjectSnapshot second = source.WorldObjects;

        if (!HasExpectedPropertySurface() || !HasExpectedMarkerPropertySurface())
        {
            Fail("Temporary world-object source smoke failed its bounded snapshot surface.");
            return;
        }

        if (!ReferenceEquals(first, second) || first.Entries.Count != 3)
        {
            Fail("Temporary world-object source smoke failed stable ordered snapshot retention.");
            return;
        }

        WorldObject herbPatchObject = first.Entries[0];
        WorldObject stoneMarkerObject = first.Entries[1];
        WorldObject waterCaskObject = first.Entries[2];
        WorldObjectIdentity herbPatchIdentity = herbPatchObject.Identity;
        WorldObjectIdentity stoneMarkerIdentity = stoneMarkerObject.Identity;
        WorldObjectDescription herbPatchDescription = herbPatchObject.Description;
        WorldObjectDescription stoneMarkerDescription = stoneMarkerObject.Description;
        WorldObjectIdentity waterCaskIdentity = waterCaskObject.Identity;
        WorldObjectDescription waterCaskDescription = waterCaskObject.Description;
        string herbPatchName = herbPatchIdentity.DisplayName;
        string stoneMarkerName = stoneMarkerIdentity.DisplayName;
        if (ReferenceEquals(herbPatchObject, stoneMarkerObject)
            || ReferenceEquals(herbPatchIdentity, stoneMarkerIdentity)
            || ReferenceEquals(herbPatchDescription, stoneMarkerDescription)
            || ReferenceEquals(waterCaskObject, herbPatchObject)
            || ReferenceEquals(waterCaskObject, stoneMarkerObject)
            || ReferenceEquals(waterCaskIdentity, herbPatchIdentity)
            || ReferenceEquals(waterCaskIdentity, stoneMarkerIdentity)
            || ReferenceEquals(waterCaskDescription, herbPatchDescription)
            || ReferenceEquals(waterCaskDescription, stoneMarkerDescription)
            || !HasExpectedWorldObject(
                herbPatchObject,
                herbPatch,
                HerbPatchObjectId,
                HerbPatchDisplayName,
                HerbPatchDescription)
            || !HasExpectedWorldObject(
                stoneMarkerObject,
                stoneMarker,
                StoneMarkerObjectId,
                StoneMarkerDisplayName,
                StoneMarkerDescription)
            || !HasExpectedWorldObject(
                waterCaskObject,
                WaterCaskObjectId,
                WaterCaskDisplayName,
                WaterCaskDescription))
        {
            Fail("Temporary world-object source smoke failed exact authored target retention.");
            return;
        }

        if (!HasExpectedMarkerLabelTexts(expectedMarkerLabelTexts)
            || !string.Equals(
                expectedHerbPatchIndicatorText,
                InitialResolvedDescriptionText,
                StringComparison.Ordinal)
            || !string.Equals(
                expectedStoneMarkerIndicatorText,
                StoneMarkerDescription,
                StringComparison.Ordinal)
            || !string.Equals(expectedFocusStatusText, FocusStatusText, StringComparison.Ordinal)
            || !string.Equals(
                expectedFocusedSearchResultText,
                FocusedSearchResultText,
                StringComparison.Ordinal)
            || !HasExpectedDescriptionFreeFocusedSearchInputs(
                expectedConditions,
                expectedCapabilities,
                expectedInventory,
                herbPatchIdentity.Id,
                stoneMarkerIdentity.Id,
                HerbPatchDescription,
                StoneMarkerDescription))
        {
            Fail(
                "Temporary world-object source smoke copied a description into focused-search "
                + "inputs or failed exact reinterpretation presentation.");
            return;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.Perception, expectedPerception)
            || !ReferenceEquals(focusedSearchContextSource.Conditions, expectedConditions)
            || !ReferenceEquals(focusedSearchContextSource.Capabilities, expectedCapabilities)
            || !ReferenceEquals(focusedSearchContextSource.Inventory, expectedInventory)
            || focusController.FocusedTargetIndex != expectedFocusedTargetIndex
            || !ReferenceEquals(
                focusController.FocusedTargetObjectId,
                expectedFocusedTargetObjectId)
            || !ReferenceEquals(first.Entries[0].Identity, herbPatchIdentity)
            || !ReferenceEquals(first.Entries[1].Identity, stoneMarkerIdentity)
            || !ReferenceEquals(first.Entries[0].Identity.DisplayName, herbPatchName)
            || !ReferenceEquals(first.Entries[1].Identity.DisplayName, stoneMarkerName)
            || !ReferenceEquals(first.Entries[0].Description, herbPatchDescription)
            || !ReferenceEquals(first.Entries[1].Description, stoneMarkerDescription)
            || !ReferenceEquals(first.Entries[2].Identity, waterCaskIdentity)
            || !ReferenceEquals(first.Entries[2].Description, waterCaskDescription)
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
            || stoneMarkerResolvedIndicator.Visible != expectedStoneMarkerIndicatorVisibility
            || !string.Equals(
                herbPatchResolvedIndicator.Text,
                expectedHerbPatchIndicatorText,
                StringComparison.Ordinal)
            || !string.Equals(
                stoneMarkerResolvedIndicator.Text,
                expectedStoneMarkerIndicatorText,
                StringComparison.Ordinal)
            || !string.Equals(focusStatus.Text, expectedFocusStatusText, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResultStatus.Text,
                expectedFocusedSearchResultText,
                StringComparison.Ordinal))
        {
            Fail(
                "Temporary world-object source smoke changed controller state or presentation.");
            return;
        }

        GD.Print(
            "Temporary world-object source smoke passed: bounded surface, stable ordered snapshot, "
            + "distinct objects, independently authored value-equal identifiers, exact names, "
            + "distinct exact descriptions, stable nested source values, description-free "
            + "focused-search inputs, exact identified-only resolved-description presentation, and "
            + "unchanged marker order, geometry, visibility, focus, controller snapshots, and "
            + "presentation were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedWorldObject(
        WorldObject worldObject,
        GreyboxInteractionTarget target,
        string expectedObjectId,
        string expectedDisplayName,
        string expectedDescription)
    {
        return string.Equals(target.ObjectId.Value, expectedObjectId, StringComparison.Ordinal)
            && !ReferenceEquals(worldObject.Identity.Id, target.ObjectId)
            && worldObject.Identity.Id == target.ObjectId
            && string.Equals(
                worldObject.Identity.DisplayName,
                expectedDisplayName,
                StringComparison.Ordinal)
            && string.Equals(
                worldObject.Description.Text,
                expectedDescription,
                StringComparison.Ordinal);
    }

    private static bool HasExpectedWorldObject(
        WorldObject worldObject,
        string expectedObjectId,
        string expectedDisplayName,
        string expectedDescription)
    {
        return string.Equals(
                worldObject.Identity.Id.Value,
                expectedObjectId,
                StringComparison.Ordinal)
            && string.Equals(
                worldObject.Identity.DisplayName,
                expectedDisplayName,
                StringComparison.Ordinal)
            && string.Equals(
                worldObject.Description.Text,
                expectedDescription,
                StringComparison.Ordinal);
    }

    private static bool HasExpectedDescriptionFreeFocusedSearchInputs(
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory,
        ObjectId herbPatchObjectId,
        ObjectId stoneMarkerObjectId,
        params string[] descriptions)
    {
        if (conditions.CapabilityRequirements.Entries.Count != 1
            || conditions.ToolRequirements.Entries.Count != 1
            || capabilities.Entries.Count != 1
            || inventory.ObjectReferences.Count != 1
            || !ReferenceEquals(
                conditions.CapabilityRequirements.Entries[0].ObjectId,
                herbPatchObjectId)
            || !ReferenceEquals(
                conditions.ToolRequirements.Entries[0].ObjectId,
                stoneMarkerObjectId))
        {
            return false;
        }

        IEnumerable<string> inputTexts = conditions.CapabilityRequirements.Entries
            .SelectMany(
                requirement => new[]
                {
                    requirement.ObjectId.Value,
                    requirement.RequiredCapabilityKey,
                })
            .Concat(
                conditions.ToolRequirements.Entries.SelectMany(
                    requirement => new[]
                    {
                        requirement.ObjectId.Value,
                        requirement.RequiredToolObjectId.Value,
                    }))
            .Concat(capabilities.Entries.SelectMany(entry => new[] { entry.Key, entry.Description }))
            .Concat(inventory.ObjectReferences.Select(reference => reference.Value));

        return inputTexts.All(
            input => !descriptions.Contains(input, StringComparer.Ordinal));
    }

    private static bool HasExpectedMarkerLabelTexts(IReadOnlyList<string?> markerLabelTexts)
    {
        string?[] expectedTexts = [HerbPatchDisplayName, StoneMarkerDisplayName, null, string.Empty];

        return markerLabelTexts.Count == expectedTexts.Length
            && markerLabelTexts.Select(
                    (text, index) => string.Equals(
                        text,
                        expectedTexts[index],
                        StringComparison.Ordinal))
                .All(isExpected => isExpected);
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
