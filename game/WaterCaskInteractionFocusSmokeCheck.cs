using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks exact focus-only binding for the authored main-scene water cask.</summary>
public partial class WaterCaskInteractionFocusSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WATER_CASK_INTERACTION_FOCUS_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int WaterCaskTargetIndex = 2;
    private const string WaterCaskObjectIdText = "village-water-cask";
    private const string WaterCaskCapabilityKey = "inspect-water-sources";
    private const string WaterCaskCapabilityDescription = "Inspects water sources for tampering.";
    private const string WaterCaskFocusText = "Interaction focus: Village water cask";
    private const string NoFocusText = "Interaction focus: NONE";

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        InteractionFocusController focus =
            GetNode<InteractionFocusController>("../InteractionFocusController");
        TemporaryWorldObjectSource worldSource =
            GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        WorldObjectController worldState =
            GetNode<WorldObjectController>("../WorldObjectController");
        TemporaryCharacterObjectSabotageSource sabotageSource =
            GetNode<TemporaryCharacterObjectSabotageSource>(
                "../TemporaryCharacterObjectSabotageSource");
        CharacterObjectSabotageConcealmentController sabotageState =
            GetNode<CharacterObjectSabotageConcealmentController>(
                "../CharacterObjectSabotageConcealmentController");
        CharacterObjectSabotageConcealmentPresentationController sabotagePresentation =
            GetNode<CharacterObjectSabotageConcealmentPresentationController>(
                "../CharacterObjectSabotageConcealmentPresentationController");
        FocusedSearchActivityController focusedSearch =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        TemporaryFocusedSearchContextSource focusedSearchContext =
            GetNode<TemporaryFocusedSearchContextSource>(
                "../TemporaryFocusedSearchContextSource");
        Label focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");

        WorldObjectSnapshot expectedWorldObjects = worldSource.WorldObjects;
        WorldObject waterCask = expectedWorldObjects.Entries[^1];
        GreyboxInteractionTarget waterCaskTarget = focus.GetTarget(WaterCaskTargetIndex);
        GreyboxInteractionTarget[] expectedTargets = Enumerable.Range(0, focus.TargetCount)
            .Select(focus.GetTarget)
            .ToArray();
        ObjectId[] expectedTargetObjectIds = expectedTargets
            .Select(target => target.ObjectId)
            .ToArray();
        Vector2[] expectedTargetPositions = expectedTargets
            .Select(target => target.GlobalPosition)
            .ToArray();
        bool[] expectedTargetVisibility = expectedTargets
            .Select(target => target.Visible)
            .ToArray();
        bool[] expectedInteractionVisibility = expectedTargets
            .Select(target => target.IsInteractionVisible)
            .ToArray();
        bool[] expectedInteractionEnablement = expectedTargets
            .Select(target => target.IsInteractionEnabled)
            .ToArray();
        Label?[] expectedTargetLabels = expectedTargets
            .Select(target => target.GetNodeOrNull<Label>("Label"))
            .ToArray();
        string?[] expectedTargetLabelText = expectedTargetLabels
            .Select(label => label?.Text)
            .ToArray();
        DiscoveryEvidenceSnapshot expectedSourceEvidence = sabotageSource.Evidence;
        WorldObjectConcealmentSnapshot expectedSourceConcealment = sabotageSource.Concealment;
        DiscoveryEvidenceSnapshot? expectedStateEvidence = sabotageState.Evidence;
        WorldObjectConcealmentSnapshot? expectedStateConcealment = sabotageState.Concealment;
        ObjectId? expectedPresentationTarget = sabotagePresentation.TargetObjectId;
        WorldObjectConcealmentSnapshot? expectedPresentedConcealment =
            sabotagePresentation.LastPresentedConcealment;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearch.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearch.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearch.Perception;
        DiscoveryConditionSet expectedConditions = focusedSearchContext.Conditions;
        CharacterCapabilitySet expectedCapabilities = focusedSearchContext.Capabilities;
        CharacterInventory expectedInventory = focusedSearchContext.Inventory;
        Node2D waterCaskBody = GetNode<Node2D>("../InteractionCandidates/WaterCask");
        Vector2 expectedWaterCaskPosition = waterCaskBody.GlobalPosition;
        NumericsVector2 inRangePosition = new(
            expectedWaterCaskPosition.X,
            expectedWaterCaskPosition.Y);
        NumericsVector2 outOfRangePosition = inRangePosition
            + new NumericsVector2(InteractionFocusController.InteractionRangePixels + 1.0f, 0.0f);

        if (!AssertWaterCaskInvestigationContext(
                waterCask,
                expectedConditions,
                expectedCapabilities,
                expectedInventory))
        {
            Fail("The main scene did not retain exact water-cask investigation context.");
            return;
        }

        focus.RefreshFocusFrom(inRangePosition);
        if (focus.TargetCount != 5
            || !string.Equals(
                waterCaskTarget.ObjectId.Value,
                WaterCaskObjectIdText,
                StringComparison.Ordinal)
            || ReferenceEquals(waterCaskTarget.ObjectId, waterCask.Identity.Id)
            || waterCaskTarget.ObjectId != waterCask.Identity.Id
            || focus.FocusedTargetIndex != WaterCaskTargetIndex
            || !ReferenceEquals(focus.FocusedTargetObjectId, waterCask.Identity.Id)
            || !string.Equals(focusStatus.Text, WaterCaskFocusText, StringComparison.Ordinal)
            || !waterCaskTarget.IsFocused
            || !AssertOnlyExpectedFocus(focus, WaterCaskTargetIndex)
            || !AssertStateUnchanged())
        {
            Fail("The main-scene water cask did not retain exact focus-only binding.");
            return;
        }

        focus.RefreshFocusFrom(outOfRangePosition);
        if (focus.FocusedTargetIndex is not null
            || focus.FocusedTargetObjectId is not null
            || !string.Equals(focusStatus.Text, NoFocusText, StringComparison.Ordinal)
            || !AssertOnlyExpectedFocus(focus, expectedIndex: null)
            || !AssertStateUnchanged())
        {
            Fail("Out-of-range water-cask refresh did not clear only focus presentation.");
            return;
        }

        GD.Print(
            "Water-cask interaction-focus smoke passed: the stable eligible tail target retained "
            + "an independently parsed value-equal identifier, focus presented the exact "
            + "authoritative object identity and display name, its exact appended investigation "
            + "condition was retained, out-of-range refresh cleared focus, and no actor, sabotage, "
            + "focused-search, world-object, or target state changed.");
        GetTree().Quit();

        bool AssertStateUnchanged()
        {
            if (!ReferenceEquals(worldState.WorldObjects, expectedWorldObjects)
                || !ReferenceEquals(worldSource.WorldObjects, expectedWorldObjects)
                || !ReferenceEquals(sabotageSource.WorldObjects, expectedWorldObjects)
                || !ReferenceEquals(sabotageSource.Evidence, expectedSourceEvidence)
                || !ReferenceEquals(sabotageSource.Concealment, expectedSourceConcealment)
                || !ReferenceEquals(sabotageState.Evidence, expectedStateEvidence)
                || !ReferenceEquals(sabotageState.Concealment, expectedStateConcealment)
                || !ReferenceEquals(
                    sabotagePresentation.TargetObjectId,
                    expectedPresentationTarget)
                || !ReferenceEquals(
                    sabotagePresentation.LastPresentedConcealment,
                    expectedPresentedConcealment)
                || !ReferenceEquals(focusedSearch.Activities, expectedActivities)
                || !ReferenceEquals(focusedSearch.Outcomes, expectedOutcomes)
                || !ReferenceEquals(focusedSearch.Perception, expectedPerception)
                || !ReferenceEquals(focusedSearchContext.Conditions, expectedConditions)
                || !ReferenceEquals(focusedSearchContext.Capabilities, expectedCapabilities)
                || !ReferenceEquals(focusedSearchContext.Inventory, expectedInventory)
                || !waterCaskBody.Visible
                || waterCaskBody.GlobalPosition != expectedWaterCaskPosition)
            {
                return false;
            }

            for (int index = 0; index < expectedTargets.Length; index++)
            {
                GreyboxInteractionTarget target = focus.GetTarget(index);
                if (!ReferenceEquals(target, expectedTargets[index])
                    || !ReferenceEquals(target.ObjectId, expectedTargetObjectIds[index])
                    || target.GlobalPosition != expectedTargetPositions[index]
                    || target.Visible != expectedTargetVisibility[index]
                    || target.IsInteractionVisible != expectedInteractionVisibility[index]
                    || target.IsInteractionEnabled != expectedInteractionEnablement[index]
                    || !ReferenceEquals(
                        target.GetNodeOrNull<Label>("Label"),
                        expectedTargetLabels[index])
                    || !string.Equals(
                        target.GetNodeOrNull<Label>("Label")?.Text,
                        expectedTargetLabelText[index],
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private static bool AssertWaterCaskInvestigationContext(
        WorldObject waterCask,
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory)
    {
        if (conditions.CapabilityRequirements.Entries.Count != 2
            || conditions.ToolRequirements.Entries.Count != 1
            || capabilities.Entries.Count != 2
            || inventory.ObjectReferences.Count != 1)
        {
            return false;
        }

        DiscoveryCapabilityRequirement requirement =
            conditions.CapabilityRequirements.Entries[1];
        CharacterCapability capability = capabilities.Entries[1];
        return ReferenceEquals(requirement.ObjectId, waterCask.Identity.Id)
            && string.Equals(
                requirement.RequiredCapabilityKey,
                capability.Key,
                StringComparison.Ordinal)
            && string.Equals(capability.Key, WaterCaskCapabilityKey, StringComparison.Ordinal)
            && string.Equals(
                capability.Description,
                WaterCaskCapabilityDescription,
                StringComparison.Ordinal);
    }

    private static bool AssertOnlyExpectedFocus(
        InteractionFocusController focus,
        int? expectedIndex)
    {
        for (int index = 0; index < focus.TargetCount; index++)
        {
            if (focus.GetTarget(index).IsFocused != (index == expectedIndex))
            {
                return false;
            }
        }

        return true;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
