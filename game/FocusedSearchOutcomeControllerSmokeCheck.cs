using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free focused-search outcome controller checks when explicitly enabled.
/// </summary>
public partial class FocusedSearchOutcomeControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_FOCUSED_SEARCH_OUTCOME_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityController controller = null!;
    private FocusedSearchActivityIndicator indicator = null!;
    private Line2D indicatorLine = null!;

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
        indicator = GetNode<FocusedSearchActivityIndicator>(
            "../Seeker/FocusedSearchActivityIndicator");
        indicatorLine = indicator.GetNode<Line2D>("Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        if (controller.Outcomes.Entries.Count != 0)
        {
            Fail("Focused-search outcome controller smoke failed empty initial outcome state.");
            return;
        }

        if (!AssertControllerState(
                controller.Activities,
                controller.Outcomes,
                expectedVisible: false,
                "empty initial outcome state"))
        {
            return;
        }

        FocusedSearchActivity firstActivity = Activity("healer", "concealed-herb");
        FocusedSearchOutcome suppliedOutcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivity matchingActivity = new(suppliedOutcome.Request);
        FocusedSearchActivity lastActivity = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot suppliedActivities = new(
            [firstActivity, matchingActivity, lastActivity]);
        FocusedSearchOutcome firstOutcome = Outcome(
            "archivist",
            "faded-map",
            VisibilityState.Noticed);
        FocusedSearchOutcome lastOutcome = Outcome(
            "scout",
            "hidden-trail",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot suppliedOutcomes = new([firstOutcome, lastOutcome]);
        SetControllerState(suppliedActivities, suppliedOutcomes);

        if (!controller.TryCompleteSearchWithOutcome(suppliedOutcome))
        {
            Fail("Focused-search outcome controller smoke failed eligible completion.");
            return;
        }

        FocusedSearchActivitySnapshot completedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot completedOutcomes = controller.Outcomes;
        if (ReferenceEquals(completedActivities, suppliedActivities)
            || ReferenceEquals(completedOutcomes, suppliedOutcomes)
            || !ReferenceEquals(suppliedOutcome.Request, matchingActivity.Request))
        {
            Fail("Focused-search outcome controller smoke failed exact result snapshot retention.");
            return;
        }

        if (!AssertEntries(
                completedActivities.Entries,
                "completed activities",
                firstActivity,
                lastActivity)
            || !AssertEntries(
                completedOutcomes.Entries,
                "completed outcomes",
                firstOutcome,
                lastOutcome,
                suppliedOutcome)
            || !AssertEntries(
                suppliedActivities.Entries,
                "unchanged input activities",
                firstActivity,
                matchingActivity,
                lastActivity)
            || !AssertEntries(
                suppliedOutcomes.Entries,
                "unchanged input outcomes",
                firstOutcome,
                lastOutcome)
            || !AssertControllerState(
                completedActivities,
                completedOutcomes,
                expectedVisible: false,
                "successful outcome completion"))
        {
            return;
        }

        FocusedSearchOutcome wrongCharacterOutcome = Outcome(
            "healer",
            "concealed-herb",
            VisibilityState.Identified);
        FocusedSearchActivity wrongCharacterActivity = new(wrongCharacterOutcome.Request);
        FocusedSearchActivitySnapshot wrongCharacterActivities = new([wrongCharacterActivity]);
        SetControllerState(wrongCharacterActivities, completedOutcomes);
        if (!AssertRejected(
                () => controller.TryCompleteSearchWithOutcome(wrongCharacterOutcome),
                wrongCharacterActivities,
                completedOutcomes,
                expectedVisible: false,
                "wrong-character outcome"))
        {
            return;
        }

        FocusedSearchOutcome inactiveOutcome = Outcome(
            "seeker",
            "unmarked-cache",
            VisibilityState.Noticed);
        if (!AssertRejected(
                () => controller.TryCompleteSearchWithOutcome(inactiveOutcome),
                wrongCharacterActivities,
                completedOutcomes,
                expectedVisible: false,
                "inactive-request outcome"))
        {
            return;
        }

        FocusedSearchOutcome duplicateOutcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Noticed);
        FocusedSearchActivity duplicateActivity = new(duplicateOutcome.Request);
        FocusedSearchActivitySnapshot duplicateActivities = new([duplicateActivity]);
        SetControllerState(duplicateActivities, completedOutcomes);
        if (!AssertRejected(
                () => controller.TryCompleteSearchWithOutcome(duplicateOutcome),
                duplicateActivities,
                completedOutcomes,
                expectedVisible: true,
                "duplicate-request outcome")
            || !RejectsNullWithoutChangingState(duplicateActivities, completedOutcomes))
        {
            return;
        }

        if (!controller.TryCompleteSearch(duplicateOutcome.Request))
        {
            Fail("Focused-search outcome controller smoke failed request-only completion.");
            return;
        }

        if (controller.Activities.Entries.Count != 0)
        {
            Fail("Focused-search outcome controller smoke retained request-only activity.");
            return;
        }

        if (!AssertControllerState(
                controller.Activities,
                completedOutcomes,
                expectedVisible: false,
                "request-only completion"))
        {
            return;
        }

        FocusedSearchRequest lifecycleRequest = new(
            controller.SearcherCharacterId,
            new ObjectId("partial-inscription"));
        CharacterCapability lifecycleCapability = new(
            "read-ancient-script",
            "Reads ancient script.");
        DiscoveryConditionSet lifecycleConditions = new(
            new DiscoveryCapabilityRequirementSet(
                [
                    new DiscoveryCapabilityRequirement(
                        lifecycleRequest.TargetObjectId,
                        lifecycleCapability.Key),
                ]),
            new DiscoveryToolRequirementSet([]));
        if (!controller.TryStartSearch(
                lifecycleRequest,
                lifecycleConditions,
                new CharacterCapabilitySet([lifecycleCapability]),
                new CharacterInventory([])))
        {
            Fail("Focused-search outcome controller smoke failed existing start.");
            return;
        }

        if (controller.Activities.Entries.Count != 1
            || !ReferenceEquals(controller.Activities.Entries[0].Request, lifecycleRequest))
        {
            Fail("Focused-search outcome controller smoke changed existing start behavior.");
            return;
        }

        if (!AssertControllerState(
                controller.Activities,
                completedOutcomes,
                expectedVisible: true,
                "existing start"))
        {
            return;
        }

        if (!controller.TryCancelSearch(lifecycleRequest))
        {
            Fail("Focused-search outcome controller smoke failed existing cancellation.");
            return;
        }

        if (controller.Activities.Entries.Count != 0)
        {
            Fail("Focused-search outcome controller smoke retained cancelled activity.");
            return;
        }

        if (!AssertControllerState(
                controller.Activities,
                completedOutcomes,
                expectedVisible: false,
                "existing cancellation"))
        {
            return;
        }

        GD.Print(
            "Focused-search outcome controller smoke passed: empty initial outcome state; exact "
            + "completion and recording; stable order and exact retained instances; unchanged "
            + "inputs; wrong-character, inactive, duplicate, and null rejection; and lifecycle "
            + "state and presentation were correct.");
        GetTree().Quit();
    }

    private bool RejectsNullWithoutChangingState(
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes)
    {
        try
        {
            controller.TryCompleteSearchWithOutcome(null!);
        }
        catch (ArgumentNullException)
        {
            return AssertControllerState(
                expectedActivities,
                expectedOutcomes,
                expectedVisible: true,
                "null rejection");
        }

        Fail("Focused-search outcome controller smoke failed null rejection.");
        return false;
    }

    private bool AssertRejected(
        Func<bool> attempt,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        bool expectedVisible,
        string scenario)
    {
        if (attempt())
        {
            Fail($"Focused-search outcome controller smoke accepted {scenario}.");
            return false;
        }

        return AssertControllerState(
            expectedActivities,
            expectedOutcomes,
            expectedVisible,
            $"{scenario} rejection");
    }

    private void SetControllerState(
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        SetControllerProperty(nameof(FocusedSearchActivityController.Activities), activities);
        SetControllerProperty(nameof(FocusedSearchActivityController.Outcomes), outcomes);
        indicator.Present(controller.SearcherCharacterId, activities);
    }

    private void SetControllerProperty(string propertyName, object value)
    {
        System.Reflection.PropertyInfo property =
            typeof(FocusedSearchActivityController).GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Controller {propertyName} property was not found.");
        property.SetValue(controller, value);
    }

    private bool AssertControllerState(
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        bool expectedVisible,
        string scenario)
    {
        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes))
        {
            Fail($"Focused-search outcome controller smoke failed {scenario}: state changed.");
            return false;
        }

        if (indicatorLine.Visible != expectedVisible)
        {
            Fail(
                $"Focused-search outcome controller smoke failed {scenario}: expected indicator "
                + $"visibility {expectedVisible}, received {indicatorLine.Visible}.");
            return false;
        }

        return true;
    }

    private bool AssertEntries<T>(
        IReadOnlyList<T> actual,
        string scenario,
        params T[] expected)
        where T : class
    {
        if (actual.Count == expected.Length
            && actual.Zip(expected).All(pair => ReferenceEquals(pair.First, pair.Second)))
        {
            return true;
        }

        Fail($"Focused-search outcome controller smoke failed {scenario} retention.");
        return false;
    }

    private static FocusedSearchActivity Activity(string characterId, string targetObjectId) =>
        new(new FocusedSearchRequest(new CharacterId(characterId), new ObjectId(targetObjectId)));

    private static FocusedSearchOutcome Outcome(
        string characterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(
            new FocusedSearchRequest(
                new CharacterId(characterId),
                new ObjectId(targetObjectId)),
            resultingVisibilityState);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
