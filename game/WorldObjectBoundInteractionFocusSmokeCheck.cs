using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free checks for world-object-bound interaction focus when explicitly enabled.
/// </summary>
public partial class WorldObjectBoundInteractionFocusSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_WORLD_OBJECT_INTERACTION_FOCUS_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string NoFocusText = "Interaction focus: NONE";
    private static readonly Vector2 EqualDistancePosition = new(800.0f, 520.0f);
    private static readonly Vector2 FirstTargetPosition = new(720.0f, 520.0f);

    private InteractionFocusController focusController = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private TemporaryFocusedSearchContextSource focusedSearchContextSource = null!;
    private Label focusStatus = null!;
    private CharacterBody2D seeker = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
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

        focusController = GetNode<InteractionFocusController>("../InteractionFocusController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        focusedSearchContextSource = GetNode<TemporaryFocusedSearchContextSource>(
            "../TemporaryFocusedSearchContextSource");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldObjectSnapshot sourceWorldObjects = worldObjectSource.WorldObjects;
        if (!ReferenceEquals(worldObjectController.WorldObjects, sourceWorldObjects))
        {
            Fail("World-object-bound interaction-focus smoke found no initialized scene snapshot.");
            return;
        }

        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        DiscoveryConditionSet expectedConditions = focusedSearchContextSource.Conditions;
        CharacterCapabilitySet expectedCapabilities = focusedSearchContextSource.Capabilities;
        CharacterInventory expectedInventory = focusedSearchContextSource.Inventory;
        GreyboxInteractionTarget[] expectedTargets = Enumerable.Range(0, focusController.TargetCount)
            .Select(focusController.GetTarget)
            .ToArray();
        ObjectId[] expectedTargetObjectIds = expectedTargets.Select(target => target.ObjectId).ToArray();
        Label?[] expectedTargetLabels = expectedTargets
            .Select(target => target.GetNodeOrNull<Label>("Label"))
            .ToArray();
        WorldObject[] expectedSourceObjects = sourceWorldObjects.Entries.ToArray();
        WorldObjectIdentity[] expectedSourceIdentities = expectedSourceObjects
            .Select(worldObject => worldObject.Identity)
            .ToArray();
        WorldObjectDescription[] expectedSourceDescriptions = expectedSourceObjects
            .Select(worldObject => worldObject.Description)
            .ToArray();

        WorldObject firstWorldObject = expectedSourceObjects[0];
        WorldObject secondWorldObject = expectedSourceObjects[1];
        ObjectId valueEqualFirstObjectId = new(firstWorldObject.Identity.Id.Value);
        WorldObject valueEqualFirstWorldObject = new(
            new WorldObjectIdentity(valueEqualFirstObjectId, "Value-equal first object"),
            new WorldObjectDescription("Independent value-equal focus object."));
        WorldObjectIdentity valueEqualFirstIdentity = valueEqualFirstWorldObject.Identity;
        WorldObjectDescription valueEqualFirstDescription = valueEqualFirstWorldObject.Description;
        ObjectId valueEqualSecondObjectId = new(secondWorldObject.Identity.Id.Value);
        WorldObject valueEqualSecondWorldObject = new(
            new WorldObjectIdentity(valueEqualSecondObjectId, "Value-equal fallback object"),
            new WorldObjectDescription("Independent value-equal fallback object."));
        WorldObjectIdentity valueEqualSecondIdentity = valueEqualSecondWorldObject.Identity;
        WorldObjectDescription valueEqualSecondDescription = valueEqualSecondWorldObject.Description;
        WorldObject waterCaskWorldObject = expectedSourceObjects[2];
        ObjectId valueEqualWaterCaskObjectId = new(waterCaskWorldObject.Identity.Id.Value);
        WorldObject valueEqualWaterCaskWorldObject = new(
            new WorldObjectIdentity(valueEqualWaterCaskObjectId, "Value-equal water cask"),
            new WorldObjectDescription("Independent value-equal water cask."));
        WorldObject wagonClueWorldObject = expectedSourceObjects[3];
        ObjectId valueEqualWagonClueObjectId = new(wagonClueWorldObject.Identity.Id.Value);
        WorldObject valueEqualWagonClueWorldObject = new(
            new WorldObjectIdentity(valueEqualWagonClueObjectId, "Value-equal wagon clue"),
            new WorldObjectDescription("Independent value-equal wagon clue."));
        ObjectId valueEqualHiddenObjectId = new(expectedTargetObjectIds[4].Value);
        WorldObject valueEqualHiddenWorldObject = new(
            new WorldObjectIdentity(valueEqualHiddenObjectId, "Value-equal hidden object"),
            new WorldObjectDescription("Independent value-equal hidden object."));
        ObjectId valueEqualDisabledObjectId = new(expectedTargetObjectIds[5].Value);
        WorldObject valueEqualDisabledWorldObject = new(
            new WorldObjectIdentity(valueEqualDisabledObjectId, "Value-equal disabled object"),
            new WorldObjectDescription("Independent value-equal disabled object."));
        WorldObjectSnapshot valueEqualWorldObjects = new(
            [
                valueEqualFirstWorldObject,
                valueEqualSecondWorldObject,
                valueEqualWaterCaskWorldObject,
                valueEqualWagonClueWorldObject,
                valueEqualHiddenWorldObject,
                valueEqualDisabledWorldObject,
            ]);
        WorldObjectSnapshot missingNearestWorldObjects = new([valueEqualSecondWorldObject]);
        WorldObjectSnapshot emptyWorldObjects = new([]);

        seeker.GlobalPosition = FirstTargetPosition;
        focusController.RefreshFocus();
        if (!AssertFocus(
                expectedIndex: 0,
                firstWorldObject.Identity.Id,
                firstWorldObject.Identity.DisplayName,
                "exact world-object focus"))
        {
            return;
        }

        if (!AssertMarkerLabels(
                [
                    firstWorldObject.Identity.DisplayName,
                    secondWorldObject.Identity.DisplayName,
                    waterCaskWorldObject.Identity.DisplayName,
                    wagonClueWorldObject.Identity.DisplayName,
                    null,
                    string.Empty,
                ],
                "exact world-object labels"))
        {
            return;
        }

        SetControlledWorldObjects(valueEqualWorldObjects);
        focusController.RefreshFocus();
        if (!AssertFocus(
                expectedIndex: 0,
                valueEqualFirstObjectId,
                valueEqualFirstIdentity.DisplayName,
                "distinct value-equal world-object focus"))
        {
            return;
        }

        if (!AssertMarkerLabels(
                [
                    valueEqualFirstWorldObject.Identity.DisplayName,
                    valueEqualSecondWorldObject.Identity.DisplayName,
                    valueEqualWaterCaskWorldObject.Identity.DisplayName,
                    valueEqualWagonClueWorldObject.Identity.DisplayName,
                    null,
                    valueEqualDisabledWorldObject.Identity.DisplayName,
                ],
                "distinct value-equal world-object labels"))
        {
            return;
        }

        SetControlledWorldObjects(missingNearestWorldObjects);
        seeker.GlobalPosition = EqualDistancePosition;
        focusController.RefreshFocus();
        if (!AssertFocus(
                expectedIndex: 1,
                valueEqualSecondObjectId,
                valueEqualSecondIdentity.DisplayName,
                "missing-nearest fallback"))
        {
            return;
        }

        if (!AssertMarkerLabels(
                [
                    string.Empty,
                    valueEqualSecondWorldObject.Identity.DisplayName,
                    string.Empty,
                    string.Empty,
                    null,
                    string.Empty,
                ],
                "missing-object label clearing"))
        {
            return;
        }

        SetControlledWorldObjects(emptyWorldObjects);
        focusController.RefreshFocus();
        if (!AssertNoFocus("all-missing clearing"))
        {
            return;
        }

        if (!AssertMarkerLabels(
                [string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty],
                "all-missing label clearing"))
        {
            return;
        }

        SetControlledWorldObjects(sourceWorldObjects);
        focusController.RefreshFocus();
        if (!AssertFocus(
                expectedIndex: 0,
                firstWorldObject.Identity.Id,
                firstWorldObject.Identity.DisplayName,
                "restored authored-order focus")
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || !ReferenceEquals(focusedSearchContextSource.Conditions, expectedConditions)
            || !ReferenceEquals(focusedSearchContextSource.Capabilities, expectedCapabilities)
            || !ReferenceEquals(focusedSearchContextSource.Inventory, expectedInventory)
            || !ReferenceEquals(worldObjectSource.WorldObjects, sourceWorldObjects)
            || !ReferenceEquals(worldObjectController.WorldObjects, sourceWorldObjects)
            || !ReferenceEquals(valueEqualFirstWorldObject.Identity, valueEqualFirstIdentity)
            || !ReferenceEquals(valueEqualFirstWorldObject.Description, valueEqualFirstDescription)
            || !ReferenceEquals(valueEqualFirstIdentity.Id, valueEqualFirstObjectId)
            || !ReferenceEquals(valueEqualSecondWorldObject.Identity, valueEqualSecondIdentity)
            || !ReferenceEquals(valueEqualSecondWorldObject.Description, valueEqualSecondDescription)
            || !ReferenceEquals(valueEqualSecondIdentity.Id, valueEqualSecondObjectId)
            || ReferenceEquals(valueEqualFirstObjectId, expectedTargetObjectIds[0])
            || valueEqualFirstObjectId != expectedTargetObjectIds[0]
            || ReferenceEquals(valueEqualSecondObjectId, expectedTargetObjectIds[1])
            || valueEqualSecondObjectId != expectedTargetObjectIds[1]
            || !AssertMarkerLabels(
                [
                    firstWorldObject.Identity.DisplayName,
                    secondWorldObject.Identity.DisplayName,
                    waterCaskWorldObject.Identity.DisplayName,
                    wagonClueWorldObject.Identity.DisplayName,
                    null,
                    string.Empty,
                ],
                "restored world-object labels")
            || !AssertRetainedInputs(
                expectedTargets,
                expectedTargetObjectIds,
                expectedTargetLabels,
                sourceWorldObjects,
                expectedSourceObjects,
                expectedSourceIdentities,
                expectedSourceDescriptions))
        {
            Fail(
                "World-object-bound interaction-focus smoke changed state outside focus "
                + "presentation or failed restoration.");
            return;
        }

        GD.Print(
            "World-object-bound interaction-focus smoke passed: exact and distinct value-equal "
            + "object lookup, exact controller identifier retention, world-object status and "
            + "marker-label names, missing-object label clearing, hidden and disabled markers, "
            + "missing-nearest fallback, all-missing clearing, restoration, and unchanged "
            + "surrounding state were correct.");
        GetTree().Quit();
    }

    private bool AssertFocus(
        int expectedIndex,
        ObjectId expectedObjectId,
        string expectedDisplayName,
        string scenario)
    {
        if (focusController.FocusedTargetIndex != expectedIndex
            || !ReferenceEquals(focusController.FocusedTargetObjectId, expectedObjectId)
            || !string.Equals(
                focusStatus.Text,
                $"Interaction focus: {expectedDisplayName}",
                StringComparison.Ordinal))
        {
            Fail($"World-object-bound interaction-focus smoke failed {scenario} selection.");
            return false;
        }

        for (int index = 0; index < focusController.TargetCount; index++)
        {
            if (focusController.GetTarget(index).IsFocused != (index == expectedIndex))
            {
                Fail($"World-object-bound interaction-focus smoke failed {scenario} indicator.");
                return false;
            }
        }

        return true;
    }

    private bool AssertNoFocus(string scenario)
    {
        if (focusController.FocusedTargetIndex is not null
            || focusController.FocusedTargetObjectId is not null
            || !string.Equals(focusStatus.Text, NoFocusText, StringComparison.Ordinal))
        {
            Fail($"World-object-bound interaction-focus smoke failed {scenario} state.");
            return false;
        }

        for (int index = 0; index < focusController.TargetCount; index++)
        {
            if (focusController.GetTarget(index).IsFocused)
            {
                Fail($"World-object-bound interaction-focus smoke failed {scenario} indicator.");
                return false;
            }
        }

        return true;
    }

    private bool AssertMarkerLabels(IReadOnlyList<string?> expectedTexts, string scenario)
    {
        for (int index = 0; index < focusController.TargetCount; index++)
        {
            Label? label = focusController.GetTarget(index).GetNodeOrNull<Label>("Label");
            if ((label is null) != (expectedTexts[index] is null)
                || label is not null
                    && !string.Equals(label.Text, expectedTexts[index], StringComparison.Ordinal))
            {
                Fail($"World-object-bound interaction-focus smoke failed {scenario} at {index}.");
                return false;
            }
        }

        return true;
    }

    private bool AssertRetainedInputs(
        IReadOnlyList<GreyboxInteractionTarget> expectedTargets,
        IReadOnlyList<ObjectId> expectedTargetObjectIds,
        IReadOnlyList<Label?> expectedTargetLabels,
        WorldObjectSnapshot sourceWorldObjects,
        IReadOnlyList<WorldObject> expectedSourceObjects,
        IReadOnlyList<WorldObjectIdentity> expectedSourceIdentities,
        IReadOnlyList<WorldObjectDescription> expectedSourceDescriptions)
    {
        for (int index = 0; index < expectedTargets.Count; index++)
        {
            if (!ReferenceEquals(focusController.GetTarget(index), expectedTargets[index])
                || !ReferenceEquals(expectedTargets[index].ObjectId, expectedTargetObjectIds[index])
                || !ReferenceEquals(
                    expectedTargets[index].GetNodeOrNull<Label>("Label"),
                    expectedTargetLabels[index])
                || expectedTargetLabels[index] is { } label
                    && !string.Equals(
                        label.Text,
                        index < expectedSourceObjects.Count
                            ? expectedSourceObjects[index].Identity.DisplayName
                            : string.Empty,
                        StringComparison.Ordinal))
            {
                return false;
            }
        }

        for (int index = 0; index < expectedSourceObjects.Count; index++)
        {
            if (!ReferenceEquals(sourceWorldObjects.Entries[index], expectedSourceObjects[index])
                || !ReferenceEquals(expectedSourceObjects[index].Identity, expectedSourceIdentities[index])
                || !ReferenceEquals(
                    expectedSourceObjects[index].Description,
                    expectedSourceDescriptions[index]))
            {
                return false;
            }
        }

        return true;
    }

    private void SetControlledWorldObjects(WorldObjectSnapshot worldObjects)
    {
        System.Reflection.MethodInfo setter = typeof(WorldObjectController)
            .GetProperty(nameof(WorldObjectController.WorldObjects))?
            .GetSetMethod(nonPublic: true)
            ?? throw new InvalidOperationException(
                "The world-object-bound focus smoke requires the private snapshot setter.");

        setter.Invoke(worldObjectController, [worldObjects]);
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
