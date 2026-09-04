using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free focused-search activity controller checks when explicitly enabled.
/// </summary>
public partial class FocusedSearchActivityControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FOCUSED_SEARCH_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityController controller = null!;
    private Line2D indicator = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        indicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivitySnapshot initialSnapshot = controller.Activities;
        if (controller.SearcherCharacterId.Value != "seeker"
            || !AssertState(initialSnapshot, expectedVisible: false, "empty initial state"))
        {
            return;
        }

        CharacterId unrelatedCharacterId = new("healer");
        ObjectId unrelatedActivityTargetId = new("weathered-inscription");
        FocusedSearchRequest unrelatedActivityRequest = new(
            unrelatedCharacterId,
            unrelatedActivityTargetId);
        FocusedSearchActivity unrelatedActivity = new(unrelatedActivityRequest);
        FocusedSearchActivitySnapshot unrelatedSnapshot = new([unrelatedActivity]);
        SetActivitiesForSmokeCheck(unrelatedSnapshot);

        ObjectId capabilityTargetId = new("concealed-herb");
        ObjectId toolTargetId = new("partial-inscription");
        ObjectId combinedTargetId = new("wagon-residue");
        ObjectId absentTargetId = new("unmarked-cache");
        ObjectId unsatisfiedTargetId = new("faded-marking");
        FocusedSearchRequest capabilityRequest = new(
            controller.SearcherCharacterId,
            capabilityTargetId);
        FocusedSearchRequest toolRequest = new(controller.SearcherCharacterId, toolTargetId);
        FocusedSearchRequest combinedRequest = new(
            controller.SearcherCharacterId,
            combinedTargetId);
        FocusedSearchRequest absentRequest = new(controller.SearcherCharacterId, absentTargetId);
        FocusedSearchRequest unsatisfiedRequest = new(
            controller.SearcherCharacterId,
            unsatisfiedTargetId);
        FocusedSearchRequest wrongCharacterRequest = new(
            unrelatedCharacterId,
            capabilityTargetId);

        DiscoveryCapabilityRequirement unrelatedCapabilityRequirement = new(
            unrelatedActivityTargetId,
            "read-ancient-script");
        DiscoveryCapabilityRequirement capabilityRequirement = new(
            capabilityTargetId,
            "identify-healing-plants");
        DiscoveryCapabilityRequirement combinedCapabilityRequirement = new(
            combinedTargetId,
            "analyze-residue");
        DiscoveryToolRequirement unrelatedToolRequirement = new(
            unrelatedActivityTargetId,
            new ObjectId("charcoal-stick"));
        ObjectId fieldLensId = new("field-lens");
        ObjectId sampleVialId = new("sample-vial");
        DiscoveryToolRequirement toolRequirement = new(toolTargetId, fieldLensId);
        DiscoveryToolRequirement combinedToolRequirement = new(combinedTargetId, sampleVialId);
        DiscoveryCapabilityRequirementSet capabilityRequirements = new(
            [
                unrelatedCapabilityRequirement,
                capabilityRequirement,
                combinedCapabilityRequirement,
            ]);
        DiscoveryToolRequirementSet toolRequirements = new(
            [unrelatedToolRequirement, toolRequirement, combinedToolRequirement]);
        DiscoveryConditionSet conditions = new(capabilityRequirements, toolRequirements);

        CharacterCapability identifyCapability = new(
            "identify-healing-plants",
            "Identifies healing plants.");
        CharacterCapability analysisCapability = new(
            "analyze-residue",
            "Analyzes residue.");
        CharacterCapabilitySet capabilities = new([identifyCapability, analysisCapability]);
        CharacterInventory inventory = new([fieldLensId, sampleVialId]);
        DiscoveryConditionSet unsatisfiedConditions = new(
            new DiscoveryCapabilityRequirementSet(
                [new DiscoveryCapabilityRequirement(unsatisfiedTargetId, "read-ancient-script")]),
            new DiscoveryToolRequirementSet([]));

        if (!AssertState(unrelatedSnapshot, expectedVisible: false, "unrelated activity setup")
            || controller.TryStartSearch(
                wrongCharacterRequest,
                conditions,
                capabilities,
                inventory)
            || !AssertUnchanged(
                unrelatedSnapshot,
                [unrelatedActivity],
                expectedVisible: false,
                "wrong-character start")
            || controller.TryStartSearch(absentRequest, conditions, capabilities, inventory)
            || !AssertUnchanged(
                unrelatedSnapshot,
                [unrelatedActivity],
                expectedVisible: false,
                "absent-condition start")
            || controller.TryStartSearch(
                unsatisfiedRequest,
                unsatisfiedConditions,
                capabilities,
                inventory)
            || !AssertUnchanged(
                unrelatedSnapshot,
                [unrelatedActivity],
                expectedVisible: false,
                "unsatisfied-condition start"))
        {
            return;
        }

        if (!controller.TryStartSearch(capabilityRequest, conditions, capabilities, inventory))
        {
            Fail("Focused-search controller smoke failed capability-only start.");
            return;
        }

        FocusedSearchActivitySnapshot capabilitySnapshot = controller.Activities;
        FocusedSearchActivity capabilityActivity = capabilitySnapshot.Entries[1];
        if (!ReferenceEquals(capabilityActivity.Request, capabilityRequest)
            || !AssertEntries(capabilitySnapshot, unrelatedActivity, capabilityActivity)
            || !AssertState(capabilitySnapshot, expectedVisible: true, "capability-only start")
            || controller.TryStartSearch(toolRequest, conditions, capabilities, inventory)
            || !AssertUnchanged(
                capabilitySnapshot,
                [unrelatedActivity, capabilityActivity],
                expectedVisible: true,
                "already-active start")
            || !controller.TryCancelSearch(capabilityRequest)
            || !AssertEntries(controller.Activities, unrelatedActivity)
            || !AssertState(
                controller.Activities,
                expectedVisible: false,
                "successful cancellation"))
        {
            return;
        }

        if (!controller.TryStartSearch(toolRequest, conditions, capabilities, inventory))
        {
            Fail("Focused-search controller smoke failed tool-only start.");
            return;
        }

        FocusedSearchActivitySnapshot toolSnapshot = controller.Activities;
        FocusedSearchActivity toolActivity = toolSnapshot.Entries[1];
        if (!ReferenceEquals(toolActivity.Request, toolRequest)
            || !AssertEntries(toolSnapshot, unrelatedActivity, toolActivity)
            || !AssertState(toolSnapshot, expectedVisible: true, "tool-only start")
            || !controller.TryCancelSearch(toolRequest)
            || !AssertEntries(controller.Activities, unrelatedActivity)
            || !AssertState(
                controller.Activities,
                expectedVisible: false,
                "tool-only cancellation"))
        {
            return;
        }

        if (!controller.TryStartSearch(combinedRequest, conditions, capabilities, inventory))
        {
            Fail("Focused-search controller smoke failed combined-condition start.");
            return;
        }

        FocusedSearchActivitySnapshot completionSnapshot = controller.Activities;
        FocusedSearchActivity combinedActivity = completionSnapshot.Entries[1];
        if (!ReferenceEquals(combinedActivity.Request, combinedRequest)
            || !AssertEntries(completionSnapshot, unrelatedActivity, combinedActivity)
            || !AssertState(completionSnapshot, expectedVisible: true, "combined-condition start")
            || controller.TryCompleteSearch(toolRequest)
            || !AssertUnchanged(
                completionSnapshot,
                [unrelatedActivity, combinedActivity],
                expectedVisible: true,
                "failed completion")
            || !controller.TryCompleteSearch(combinedRequest)
            || !AssertEntries(controller.Activities, unrelatedActivity)
            || !AssertState(
                controller.Activities,
                expectedVisible: false,
                "successful completion")
            || !AssertInputsUnchanged(
                unrelatedActivityRequest,
                capabilityRequest,
                toolRequest,
                combinedRequest,
                wrongCharacterRequest,
                capabilityRequirements,
                toolRequirements,
                conditions,
                capabilities,
                inventory,
                unrelatedCapabilityRequirement,
                capabilityRequirement,
                combinedCapabilityRequirement,
                unrelatedToolRequirement,
                toolRequirement,
                combinedToolRequirement,
                identifyCapability,
                analysisCapability,
                fieldLensId,
                sampleVialId))
        {
            return;
        }

        GD.Print(
            "Focused-search activity controller smoke passed: condition-aware capability-only, "
            + "tool-only, and combined starts; fail-closed absent, unsatisfied, already-active, "
            + "and wrong-character starts; unrelated entries; lifecycle presentation; exact "
            + "instance retention; and unchanged inputs were correct.");
        GetTree().Quit();
    }

    private void SetActivitiesForSmokeCheck(FocusedSearchActivitySnapshot activities)
    {
        System.Reflection.PropertyInfo property =
            typeof(FocusedSearchActivityController).GetProperty(
                nameof(FocusedSearchActivityController.Activities))
            ?? throw new InvalidOperationException("Controller activities property was not found.");
        property.SetValue(controller, activities);
    }

    private bool AssertUnchanged(
        FocusedSearchActivitySnapshot expectedSnapshot,
        FocusedSearchActivity[] expectedActivities,
        bool expectedVisible,
        string scenario)
    {
        if (!ReferenceEquals(controller.Activities, expectedSnapshot)
            || !AssertEntries(controller.Activities, expectedActivities))
        {
            Fail($"Focused-search controller smoke failed {scenario}: state changed.");
            return false;
        }

        return AssertState(expectedSnapshot, expectedVisible, scenario);
    }

    private bool AssertEntries(
        FocusedSearchActivitySnapshot snapshot,
        params FocusedSearchActivity[] expectedActivities)
    {
        if (snapshot.Entries.Count == expectedActivities.Length
            && snapshot.Entries
                .Zip(expectedActivities)
                .All(pair => ReferenceEquals(pair.First, pair.Second)))
        {
            return true;
        }

        Fail("Focused-search controller smoke failed exact activity retention.");
        return false;
    }

    private bool AssertState(
        FocusedSearchActivitySnapshot expectedSnapshot,
        bool expectedVisible,
        string scenario)
    {
        if (!ReferenceEquals(controller.Activities, expectedSnapshot))
        {
            Fail($"Focused-search controller smoke failed {scenario}: snapshot mismatch.");
            return false;
        }

        if (indicator.Visible != expectedVisible)
        {
            Fail(
                $"Focused-search controller smoke failed {scenario}: expected indicator "
                + $"visibility {expectedVisible}, received {indicator.Visible}.");
            return false;
        }

        return true;
    }

    private bool AssertInputsUnchanged(
        FocusedSearchRequest unrelatedActivityRequest,
        FocusedSearchRequest capabilityRequest,
        FocusedSearchRequest toolRequest,
        FocusedSearchRequest combinedRequest,
        FocusedSearchRequest wrongCharacterRequest,
        DiscoveryCapabilityRequirementSet capabilityRequirements,
        DiscoveryToolRequirementSet toolRequirements,
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory,
        DiscoveryCapabilityRequirement unrelatedCapabilityRequirement,
        DiscoveryCapabilityRequirement capabilityRequirement,
        DiscoveryCapabilityRequirement combinedCapabilityRequirement,
        DiscoveryToolRequirement unrelatedToolRequirement,
        DiscoveryToolRequirement toolRequirement,
        DiscoveryToolRequirement combinedToolRequirement,
        CharacterCapability identifyCapability,
        CharacterCapability analysisCapability,
        ObjectId fieldLensId,
        ObjectId sampleVialId)
    {
        bool unchanged = ReferenceEquals(
                unrelatedActivityRequest.SearcherCharacterId,
                wrongCharacterRequest.SearcherCharacterId)
            && ReferenceEquals(capabilityRequest.SearcherCharacterId, controller.SearcherCharacterId)
            && ReferenceEquals(toolRequest.SearcherCharacterId, controller.SearcherCharacterId)
            && ReferenceEquals(combinedRequest.SearcherCharacterId, controller.SearcherCharacterId)
            && ReferenceEquals(capabilityRequest.TargetObjectId, capabilityRequirement.ObjectId)
            && ReferenceEquals(toolRequest.TargetObjectId, toolRequirement.ObjectId)
            && ReferenceEquals(combinedRequest.TargetObjectId, combinedCapabilityRequirement.ObjectId)
            && ReferenceEquals(combinedRequest.TargetObjectId, combinedToolRequirement.ObjectId)
            && ReferenceEquals(conditions.CapabilityRequirements, capabilityRequirements)
            && ReferenceEquals(conditions.ToolRequirements, toolRequirements)
            && SameEntries(
                capabilityRequirements.Entries,
                unrelatedCapabilityRequirement,
                capabilityRequirement,
                combinedCapabilityRequirement)
            && SameEntries(
                toolRequirements.Entries,
                unrelatedToolRequirement,
                toolRequirement,
                combinedToolRequirement)
            && SameEntries(capabilities.Entries, identifyCapability, analysisCapability)
            && SameEntries(inventory.ObjectReferences, fieldLensId, sampleVialId);
        if (!unchanged)
        {
            Fail("Focused-search activity controller changed a supplied input or its order.");
        }

        return unchanged;
    }

    private static bool SameEntries<T>(IReadOnlyList<T> actual, params T[] expected)
        where T : class
    {
        return actual.Count == expected.Length
            && actual.Zip(expected).All(pair => ReferenceEquals(pair.First, pair.Second));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
