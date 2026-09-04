using System.Globalization;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free interaction-focus presentation checks when explicitly enabled.
/// </summary>
public partial class InteractionFocusSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_INTERACTION_FOCUS_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private static readonly string[] ExpectedObjectIdValues =
    [
        "herb-patch",
        "stone-marker",
        "hidden-marker",
        "excluded-marker",
    ];
    private static readonly Vector2 FirstTargetPosition = new(720.0f, 520.0f);
    private static readonly Vector2 EqualDistancePosition = new(800.0f, 520.0f);
    private static readonly Vector2 SecondTargetPosition = new(880.0f, 520.0f);
    private static readonly Vector2 OutOfRangePosition = new(500.0f, 800.0f);
    private static readonly Vector2 HiddenTargetPosition = new(800.0f, 280.0f);
    private static readonly Vector2 DisabledTargetPosition = new(1040.0f, 440.0f);

    private InteractionFocusController controller = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private Line2D focusedSearchIndicator = null!;
    private CharacterBody2D seeker = null!;
    private ObjectId[] authoredObjectIds = [];
    private SmokeStage stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<InteractionFocusController>("../InteractionFocusController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        focusedSearchIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        seeker = GetNode<CharacterBody2D>("../Seeker");

        if (!AssertAuthoredObjectIdentifiers())
        {
            SetProcess(false);
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;

        switch (stage)
        {
            case SmokeStage.RepresentativeSelection:
                CheckRepresentativeSelection();
                break;
            case SmokeStage.EqualDistanceTie:
                CheckEqualDistanceTie();
                break;
            case SmokeStage.FocusChange:
                CheckFocusChange();
                break;
            case SmokeStage.OutOfRangeClearing:
                CheckOutOfRangeClearing();
                break;
            case SmokeStage.HiddenTarget:
                CheckHiddenTarget();
                break;
            case SmokeStage.DisabledTarget:
                CheckDisabledTarget();
                break;
            default:
                Fail("Interaction-focus smoke entered an unknown stage.");
                break;
        }
    }

    private void CheckRepresentativeSelection()
    {
        seeker.GlobalPosition = FirstTargetPosition;
        controller.RefreshFocus();
        if (!AssertOnlyTargetFocused(expectedIndex: 0, "representative nearest selection"))
        {
            return;
        }

        if (!AssertEligibleFocusedSearchStart(expectedTargetIndex: 0))
        {
            return;
        }

        stage = SmokeStage.EqualDistanceTie;
    }

    private void CheckEqualDistanceTie()
    {
        seeker.GlobalPosition = EqualDistancePosition;
        controller.RefreshFocus();
        if (!AssertOnlyTargetFocused(expectedIndex: 0, "authored-order equal-distance tie"))
        {
            return;
        }

        stage = SmokeStage.FocusChange;
    }

    private void CheckFocusChange()
    {
        seeker.GlobalPosition = SecondTargetPosition;
        controller.RefreshFocus();
        if (!AssertOnlyTargetFocused(expectedIndex: 1, "deterministic focus change"))
        {
            return;
        }

        if (!AssertIneligibleFocusedSearchStart(expectedTargetIndex: 1))
        {
            return;
        }

        stage = SmokeStage.OutOfRangeClearing;
    }

    private void CheckOutOfRangeClearing()
    {
        seeker.GlobalPosition = OutOfRangePosition;
        controller.RefreshFocus();
        if (!AssertNoTargetFocused("out-of-range clearing"))
        {
            return;
        }

        stage = SmokeStage.HiddenTarget;
    }

    private void CheckHiddenTarget()
    {
        seeker.GlobalPosition = HiddenTargetPosition;
        controller.RefreshFocus();
        if (!AssertNoTargetFocused("hidden-target exclusion"))
        {
            return;
        }

        GreyboxInteractionTarget hiddenTarget = controller.GetTarget(2);
        if (hiddenTarget.Visible)
        {
            Fail("The hidden interaction target remained visible in the scene.");
            return;
        }

        stage = SmokeStage.DisabledTarget;
    }

    private void CheckDisabledTarget()
    {
        seeker.GlobalPosition = DisabledTargetPosition;
        controller.RefreshFocus();
        if (!AssertNoTargetFocused("disabled-target exclusion"))
        {
            return;
        }

        GD.Print(
            "Interaction-focus smoke passed: nearest selection, authored-order tie-breaking, "
            + "focus change, out-of-range clearing, hidden and disabled exclusions, and exact "
            + "stable request-identifier retention; eligible and ineligible focused-search "
            + "start composition; unchanged inputs; failure-state identity; and activity "
            + "presentation were correct.");
        SetProcess(false);
        GetTree().Quit();
    }

    private bool AssertOnlyTargetFocused(int expectedIndex, string scenario)
    {
        if (controller.FocusedTargetIndex != expectedIndex)
        {
            string actualIndex = controller.FocusedTargetIndex is int value
                ? value.ToString(CultureInfo.InvariantCulture)
                : "none";
            Fail(
                $"Interaction-focus smoke failed {scenario}: expected {expectedIndex}, "
                + $"received {actualIndex}.");
            return false;
        }

        if (!ReferenceEquals(controller.GetTarget(expectedIndex).ObjectId, authoredObjectIds[expectedIndex])
            || !ReferenceEquals(controller.FocusedTargetObjectId, authoredObjectIds[expectedIndex]))
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: the focused object identifier "
                + "was not the exact authored target instance.");
            return false;
        }

        FocusedSearchRequest? request = controller.CreateFocusedSearchRequest(
            focusedSearchController.SearcherCharacterId);
        if (request is null
            || !ReferenceEquals(
                request.SearcherCharacterId,
                focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(request.TargetObjectId, authoredObjectIds[expectedIndex]))
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: the request did not retain the "
                + "exact searcher and focused target identifier instances.");
            return false;
        }

        for (int index = 0; index < controller.TargetCount; index++)
        {
            bool expectedFocus = index == expectedIndex;
            if (controller.GetTarget(index).IsFocused != expectedFocus)
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} indicator "
                    + $"state was {controller.GetTarget(index).IsFocused}.");
                return false;
            }
        }

        return true;
    }

    private bool AssertNoTargetFocused(string scenario)
    {
        if (controller.FocusedTargetIndex is not null)
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: unexpected index "
                + $"{controller.FocusedTargetIndex}.");
            return false;
        }

        if (controller.FocusedTargetObjectId is not null)
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: unexpected object identifier "
                + $"{controller.FocusedTargetObjectId}.");
            return false;
        }

        if (controller.CreateFocusedSearchRequest(focusedSearchController.SearcherCharacterId)
            is not null)
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: a request was created without "
                + "a focused target.");
            return false;
        }

        FocusedSearchActivitySnapshot expectedSnapshot = focusedSearchController.Activities;
        if (controller.TryStartFocusedSearch(null!, null!, null!)
            || !ReferenceEquals(focusedSearchController.Activities, expectedSnapshot)
            || focusedSearchIndicator.Visible)
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: the absent-focus start attempt "
                + "invoked or changed focused-search state.");
            return false;
        }

        for (int index = 0; index < controller.TargetCount; index++)
        {
            if (!ReferenceEquals(controller.GetTarget(index).ObjectId, authoredObjectIds[index]))
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} replaced its "
                    + "authored object identifier.");
                return false;
            }

            if (controller.GetTarget(index).IsFocused)
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} still showed "
                    + "its focus indicator.");
                return false;
            }
        }

        return true;
    }

    private bool AssertEligibleFocusedSearchStart(int expectedTargetIndex)
    {
        ObjectId targetObjectId = authoredObjectIds[expectedTargetIndex];
        DiscoveryCapabilityRequirement capabilityRequirement = new(
            targetObjectId,
            "identify-healing-plants");
        ObjectId toolObjectId = new("field-lens");
        DiscoveryToolRequirement toolRequirement = new(targetObjectId, toolObjectId);
        DiscoveryCapabilityRequirementSet capabilityRequirements = new([capabilityRequirement]);
        DiscoveryToolRequirementSet toolRequirements = new([toolRequirement]);
        DiscoveryConditionSet conditions = new(capabilityRequirements, toolRequirements);
        CharacterCapability capability = new(
            "identify-healing-plants",
            "Identifies healing plants.");
        CharacterCapabilitySet capabilities = new([capability]);
        CharacterInventory inventory = new([toolObjectId]);

        FocusedSearchActivitySnapshot initialSnapshot = focusedSearchController.Activities;
        if (initialSnapshot.Entries.Count != 0 || focusedSearchIndicator.Visible)
        {
            Fail("Interaction-focus smoke failed eligible start setup: state was not empty.");
            return false;
        }

        if (!controller.TryStartFocusedSearch(conditions, capabilities, inventory))
        {
            Fail("Interaction-focus smoke failed eligible focused-search start composition.");
            return false;
        }

        FocusedSearchActivitySnapshot startedSnapshot = focusedSearchController.Activities;
        if (ReferenceEquals(startedSnapshot, initialSnapshot)
            || startedSnapshot.Entries.Count != 1
            || !focusedSearchIndicator.Visible)
        {
            Fail("Interaction-focus smoke failed eligible start state and presentation.");
            return false;
        }

        FocusedSearchRequest request = startedSnapshot.Entries[0].Request;
        if (!ReferenceEquals(request.SearcherCharacterId, focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(request.TargetObjectId, targetObjectId)
            || !AssertInputsUnchanged(
                conditions,
                capabilityRequirements,
                toolRequirements,
                capabilities,
                inventory,
                capabilityRequirement,
                toolRequirement,
                capability,
                toolObjectId,
                "eligible start"))
        {
            return false;
        }

        if (controller.TryStartFocusedSearch(conditions, capabilities, inventory)
            || !ReferenceEquals(focusedSearchController.Activities, startedSnapshot)
            || !focusedSearchIndicator.Visible)
        {
            Fail("Interaction-focus smoke failed already-active start state identity.");
            return false;
        }

        if (!focusedSearchController.TryCancelSearch(request)
            || focusedSearchController.Activities.Entries.Count != 0
            || focusedSearchIndicator.Visible)
        {
            Fail("Interaction-focus smoke failed eligible start cleanup.");
            return false;
        }

        return true;
    }

    private bool AssertIneligibleFocusedSearchStart(int expectedTargetIndex)
    {
        ObjectId targetObjectId = authoredObjectIds[expectedTargetIndex];
        DiscoveryCapabilityRequirement capabilityRequirement = new(
            targetObjectId,
            "read-ancient-script");
        DiscoveryCapabilityRequirementSet capabilityRequirements = new([capabilityRequirement]);
        DiscoveryToolRequirementSet toolRequirements = new([]);
        DiscoveryConditionSet conditions = new(capabilityRequirements, toolRequirements);
        CharacterCapabilitySet capabilities = new([]);
        CharacterInventory inventory = new([]);
        FocusedSearchActivitySnapshot expectedSnapshot = focusedSearchController.Activities;

        if (controller.TryStartFocusedSearch(conditions, capabilities, inventory)
            || !ReferenceEquals(focusedSearchController.Activities, expectedSnapshot)
            || focusedSearchIndicator.Visible
            || !AssertInputsUnchanged(
                conditions,
                capabilityRequirements,
                toolRequirements,
                capabilities,
                inventory,
                capabilityRequirement,
                toolRequirement: null,
                capability: null,
                toolObjectId: null,
                "ineligible start"))
        {
            return false;
        }

        return true;
    }

    private bool AssertInputsUnchanged(
        DiscoveryConditionSet conditions,
        DiscoveryCapabilityRequirementSet capabilityRequirements,
        DiscoveryToolRequirementSet toolRequirements,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory,
        DiscoveryCapabilityRequirement capabilityRequirement,
        DiscoveryToolRequirement? toolRequirement,
        CharacterCapability? capability,
        ObjectId? toolObjectId,
        string scenario)
    {
        bool unchanged = ReferenceEquals(conditions.CapabilityRequirements, capabilityRequirements)
            && ReferenceEquals(conditions.ToolRequirements, toolRequirements)
            && SameEntries(capabilityRequirements.Entries, capabilityRequirement)
            && SameEntries(toolRequirements.Entries, toolRequirement)
            && SameEntries(capabilities.Entries, capability)
            && SameEntries(inventory.ObjectReferences, toolObjectId);
        if (!unchanged)
        {
            Fail($"Interaction-focus smoke failed {scenario}: a supplied input changed.");
        }

        return unchanged;
    }

    private static bool SameEntries<T>(IReadOnlyList<T> actual, T? expected)
        where T : class
    {
        return expected is null
            ? actual.Count == 0
            : actual.Count == 1 && ReferenceEquals(actual[0], expected);
    }

    private bool AssertAuthoredObjectIdentifiers()
    {
        if (controller.TargetCount != ExpectedObjectIdValues.Length)
        {
            Fail(
                "Interaction-focus smoke failed stable identifier authoring: expected "
                + $"{ExpectedObjectIdValues.Length} targets, received {controller.TargetCount}.");
            return false;
        }

        authoredObjectIds = new ObjectId[controller.TargetCount];
        HashSet<string> distinctValues = new(StringComparer.Ordinal);
        for (int index = 0; index < controller.TargetCount; index++)
        {
            GreyboxInteractionTarget target = controller.GetTarget(index);
            authoredObjectIds[index] = target.ObjectId;
            if (!string.Equals(
                    target.ObjectId.Value,
                    ExpectedObjectIdValues[index],
                    StringComparison.Ordinal))
            {
                Fail(
                    $"Interaction-focus smoke failed stable identifier authoring: target {index} "
                    + $"exposed {target.ObjectId}.");
                return false;
            }

            if (!distinctValues.Add(target.ObjectId.Value))
            {
                Fail(
                    $"Interaction-focus smoke failed stable identifier authoring: target {index} "
                    + "duplicated an existing identifier.");
                return false;
            }
        }

        return true;
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private enum SmokeStage
    {
        RepresentativeSelection,
        EqualDistanceTie,
        FocusChange,
        OutOfRangeClearing,
        HiddenTarget,
        DisabledTarget,
    }
}
