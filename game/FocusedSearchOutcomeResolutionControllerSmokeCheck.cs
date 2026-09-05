using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free focused-search outcome resolution controller checks when enabled.
/// </summary>
public partial class FocusedSearchOutcomeResolutionControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_FOCUSED_SEARCH_OUTCOME_RESOLUTION_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityController controller = null!;
    private FocusedSearchActivityIndicator indicator = null!;
    private Line2D indicatorLine = null!;
    private Label resultStatus = null!;

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
        resultStatus = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivity retainedActivity = Activity("seeker", "concealed-herb");
        FocusedSearchActivitySnapshot activities = new([retainedActivity]);
        WorldObjectVisibility firstVisibility =
            Visibility("wagon-residue", VisibilityState.Identified);
        ObjectId targetObjectId = new("weathered-inscription");
        WorldObjectVisibility targetVisibility =
            new(targetObjectId, VisibilityState.Noticed);
        WorldObjectVisibility lastVisibility =
            Visibility("concealed-herb", VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = new(
            controller.SearcherCharacterId,
            [firstVisibility, targetVisibility, lastVisibility]);
        FocusedSearchOutcome firstOutcome =
            Outcome("sage", "wagon-residue", VisibilityState.Noticed);
        FocusedSearchOutcome retainedOutcome =
            Outcome("seeker", "weathered-inscription", VisibilityState.Understood);
        FocusedSearchOutcome lastOutcome =
            Outcome("healer", "concealed-herb", VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes =
            new([firstOutcome, retainedOutcome, lastOutcome]);
        FocusedSearchOutcome suppliedOutcome =
            Outcome("seeker", "weathered-inscription", VisibilityState.Understood);
        SetControllerState(activities, outcomes, perception);
        bool expectedIndicatorVisibility = indicatorLine.Visible;
        string expectedResultText = resultStatus.Text;

        if (ReferenceEquals(suppliedOutcome, retainedOutcome)
            || suppliedOutcome != retainedOutcome
            || !controller.TryResolveOutcome(suppliedOutcome))
        {
            Fail("Focused-search outcome resolution controller smoke failed eligible resolution.");
            return;
        }

        CharacterPerceptionSnapshot? resolvedPerception = controller.Perception;
        FocusedSearchOutcomeSnapshot resolvedOutcomes = controller.Outcomes;
        if (resolvedPerception is null
            || ReferenceEquals(resolvedPerception, perception)
            || ReferenceEquals(resolvedOutcomes, outcomes)
            || !ReferenceEquals(controller.Activities, activities))
        {
            Fail(
                "Focused-search outcome resolution controller smoke failed result snapshot "
                + "replacement or activity retention.");
            return;
        }

        if (resolvedPerception.Entries.Count != 3)
        {
            Fail(
                "Focused-search outcome resolution controller smoke failed resolved perception "
                + "entry count.");
            return;
        }

        WorldObjectVisibility resolvedTargetVisibility = resolvedPerception.Entries[1];
        if (!ReferenceEquals(resolvedPerception.CharacterId, perception.CharacterId)
            || ReferenceEquals(resolvedTargetVisibility, targetVisibility)
            || !ReferenceEquals(resolvedTargetVisibility.ObjectId, targetObjectId)
            || !ReferenceEquals(
                resolvedTargetVisibility.VisibilityState,
                VisibilityState.Understood))
        {
            Fail(
                "Focused-search outcome resolution controller smoke failed target replacement "
                + "or exact identifier and visibility retention.");
            return;
        }

        if (!AssertEntries(
                resolvedPerception.Entries,
                "resolved perception",
                firstVisibility,
                resolvedTargetVisibility,
                lastVisibility)
            || !AssertEntries(
                resolvedOutcomes.Entries,
                "remaining outcomes",
                firstOutcome,
                lastOutcome)
            || !AssertEntries(
                perception.Entries,
                "unchanged perception input",
                firstVisibility,
                targetVisibility,
                lastVisibility)
            || !AssertEntries(
                outcomes.Entries,
                "unchanged outcome input",
                firstOutcome,
                retainedOutcome,
                lastOutcome)
            || !AssertPresentation(
                expectedIndicatorVisibility,
                expectedResultText,
                "successful resolution"))
        {
            return;
        }

        FocusedSearchOutcome wrongCharacter =
            Outcome("healer", "concealed-herb", VisibilityState.Understood);
        if (!AssertRejected(
                wrongCharacter,
                activities,
                new FocusedSearchOutcomeSnapshot([wrongCharacter]),
                Perception(
                    "seeker",
                    Visibility("concealed-herb", VisibilityState.Noticed)),
                "wrong-character outcome"))
        {
            return;
        }

        FocusedSearchOutcome unretained =
            Outcome("seeker", "concealed-herb", VisibilityState.Understood);
        if (!AssertRejected(
                unretained,
                activities,
                new FocusedSearchOutcomeSnapshot([]),
                Perception(
                    "seeker",
                    Visibility("concealed-herb", VisibilityState.Noticed)),
                "unretained outcome"))
        {
            return;
        }

        FocusedSearchRequest unequalRequest =
            Request("seeker", "concealed-herb");
        FocusedSearchOutcome unequalResult =
            new(unequalRequest, VisibilityState.Understood);
        if (!AssertRejected(
                unequalResult,
                activities,
                new FocusedSearchOutcomeSnapshot(
                    [new FocusedSearchOutcome(unequalRequest, VisibilityState.Identified)]),
                Perception(
                    "seeker",
                    Visibility("concealed-herb", VisibilityState.Noticed)),
                "unequal-result outcome"))
        {
            return;
        }

        FocusedSearchOutcome unchanged =
            Outcome("seeker", "concealed-herb", VisibilityState.Identified);
        if (!AssertRejected(
                unchanged,
                activities,
                new FocusedSearchOutcomeSnapshot([unchanged]),
                Perception(
                    "seeker",
                    Visibility("concealed-herb", VisibilityState.Identified)),
                "unchanged-visibility outcome"))
        {
            return;
        }

        SetControllerState(activities, new FocusedSearchOutcomeSnapshot([unchanged]), null);
        if (!AssertCurrentStateRejected(
                () => controller.TryResolveOutcome(unchanged),
                activities,
                controller.Outcomes,
                null,
                "uninitialized perception"))
        {
            return;
        }

        CharacterPerceptionSnapshot nullAttemptPerception = Perception(
            "seeker",
            Visibility("concealed-herb", VisibilityState.Noticed));
        FocusedSearchOutcomeSnapshot nullAttemptOutcomes = new([unchanged]);
        SetControllerState(activities, nullAttemptOutcomes, nullAttemptPerception);
        if (!RejectsNullWithoutChangingState(
                activities,
                nullAttemptOutcomes,
                nullAttemptPerception))
        {
            return;
        }

        GD.Print(
            "Focused-search outcome resolution controller smoke passed: value-equal recorded "
            + "outcome resolution; exact stable-order retention; unchanged inputs, activity, "
            + "and presentation; and wrong-character, unretained, unequal-result, "
            + "unchanged-visibility, uninitialized, and null rejection were correct.");
        GetTree().Quit();
    }

    private bool AssertRejected(
        FocusedSearchOutcome outcome,
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes,
        CharacterPerceptionSnapshot perception,
        string scenario)
    {
        SetControllerState(activities, outcomes, perception);
        return AssertCurrentStateRejected(
            () => controller.TryResolveOutcome(outcome),
            activities,
            outcomes,
            perception,
            scenario);
    }

    private bool AssertCurrentStateRejected(
        Func<bool> attempt,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot? expectedPerception,
        string scenario)
    {
        bool expectedIndicatorVisibility = indicatorLine.Visible;
        string expectedResultText = resultStatus.Text;
        if (attempt())
        {
            Fail($"Focused-search outcome resolution controller smoke accepted {scenario}.");
            return false;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.Perception, expectedPerception))
        {
            Fail(
                $"Focused-search outcome resolution controller smoke changed {scenario} state.");
            return false;
        }

        return AssertPresentation(
            expectedIndicatorVisibility,
            expectedResultText,
            $"{scenario} rejection");
    }

    private bool RejectsNullWithoutChangingState(
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot expectedPerception)
    {
        bool expectedIndicatorVisibility = indicatorLine.Visible;
        string expectedResultText = resultStatus.Text;
        try
        {
            controller.TryResolveOutcome(null!);
        }
        catch (ArgumentNullException)
        {
            if (!ReferenceEquals(controller.Activities, expectedActivities)
                || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
                || !ReferenceEquals(controller.Perception, expectedPerception))
            {
                Fail(
                    "Focused-search outcome resolution controller smoke changed null rejection "
                    + "state.");
                return false;
            }

            return AssertPresentation(
                expectedIndicatorVisibility,
                expectedResultText,
                "null rejection");
        }

        Fail("Focused-search outcome resolution controller smoke failed null rejection.");
        return false;
    }

    private void SetControllerState(
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes,
        CharacterPerceptionSnapshot? perception)
    {
        SetControllerProperty(nameof(FocusedSearchActivityController.Activities), activities);
        SetControllerProperty(nameof(FocusedSearchActivityController.Outcomes), outcomes);
        SetControllerProperty(nameof(FocusedSearchActivityController.Perception), perception);
        indicator.Present(controller.SearcherCharacterId, activities);
    }

    private void SetControllerProperty(string propertyName, object? value)
    {
        System.Reflection.PropertyInfo property =
            typeof(FocusedSearchActivityController).GetProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Controller {propertyName} property was not found.");
        property.SetValue(controller, value);
    }

    private bool AssertPresentation(
        bool expectedIndicatorVisibility,
        string expectedResultText,
        string scenario)
    {
        if (indicatorLine.Visible == expectedIndicatorVisibility
            && resultStatus.Text == expectedResultText)
        {
            return true;
        }

        Fail(
            $"Focused-search outcome resolution controller smoke changed {scenario} "
            + "presentation.");
        return false;
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

        Fail($"Focused-search outcome resolution controller smoke failed {scenario} retention.");
        return false;
    }

    private static FocusedSearchActivity Activity(string characterId, string targetObjectId) =>
        new(Request(characterId, targetObjectId));

    private static FocusedSearchOutcome Outcome(
        string characterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(Request(characterId, targetObjectId), resultingVisibilityState);

    private static FocusedSearchRequest Request(string characterId, string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));

    private static CharacterPerceptionSnapshot Perception(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
