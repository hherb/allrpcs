using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free passive-noticing resolution controller checks when explicitly enabled.
/// </summary>
public partial class PassiveNoticingResolutionControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_PASSIVE_RESOLUTION_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityController controller = null!;
    private Line2D searchIndicator = null!;
    private Label focusedSearchResult = null!;

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
        searchIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        bool expectedIndicatorVisibility = searchIndicator.Visible;
        string expectedResultText = focusedSearchResult.Text;
        PassiveNoticingCandidateSnapshot emptyCandidates = new([]);
        SetControllerState(perception: null, emptyCandidates);
        if (controller.TryResolveNextPassiveNoticingCandidate()
            || controller.Perception is not null
            || !ReferenceEquals(controller.PassiveNoticingCandidates, emptyCandidates))
        {
            Fail("Passive-noticing controller changed uninitialized perception state.");
            return;
        }

        ObjectId firstTargetId = new("first-target");
        WorldObjectVisibility alreadyNoticed = new(firstTargetId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot unchangedPerception = new(
            controller.SearcherCharacterId,
            [alreadyNoticed]);
        PassiveNoticingCandidate ineligible = new(
            controller.SearcherCharacterId,
            firstTargetId);
        PassiveNoticingCandidateSnapshot ineligibleCandidates = new([ineligible]);
        SetControllerState(unchangedPerception, ineligibleCandidates);
        if (controller.TryResolveNextPassiveNoticingCandidate()
            || !ReferenceEquals(controller.Perception, unchangedPerception)
            || !ReferenceEquals(
                controller.PassiveNoticingCandidates,
                ineligibleCandidates))
        {
            Fail("Passive-noticing controller changed state without an eligible candidate.");
            return;
        }

        ObjectId laterTargetId = new("later-target");
        WorldObjectVisibility firstUnnoticed = new(firstTargetId, VisibilityState.Unnoticed);
        WorldObjectVisibility laterUnnoticed = new(laterTargetId, VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = new(
            new CharacterId(controller.SearcherCharacterId.Value),
            [firstUnnoticed, laterUnnoticed]);
        PassiveNoticingCandidate unrelated = new(
            new CharacterId("travelling-healer"),
            firstTargetId);
        PassiveNoticingCandidate firstEligible = new(
            new CharacterId(controller.SearcherCharacterId.Value),
            new ObjectId(laterTargetId.Value));
        PassiveNoticingCandidate laterEligible = new(
            controller.SearcherCharacterId,
            firstTargetId);
        PassiveNoticingCandidateSnapshot candidates = new(
            [unrelated, firstEligible, laterEligible]);
        SetControllerState(perception, candidates);

        if (!controller.TryResolveNextPassiveNoticingCandidate())
        {
            Fail("Passive-noticing controller rejected the first eligible candidate.");
            return;
        }

        CharacterPerceptionSnapshot resolvedPerception = controller.Perception!;
        PassiveNoticingCandidateSnapshot remaining = controller.PassiveNoticingCandidates;
        if (ReferenceEquals(resolvedPerception, perception)
            || !ReferenceEquals(resolvedPerception.CharacterId, perception.CharacterId)
            || resolvedPerception.Entries.Count != 2
            || !ReferenceEquals(resolvedPerception.Entries[0], firstUnnoticed)
            || !ReferenceEquals(resolvedPerception.Entries[1].ObjectId, laterTargetId)
            || !ReferenceEquals(
                resolvedPerception.Entries[1].VisibilityState,
                VisibilityState.Noticed)
            || ReferenceEquals(resolvedPerception.Entries[1], laterUnnoticed)
            || remaining.Entries.Count != 2
            || !ReferenceEquals(remaining.Entries[0], unrelated)
            || !ReferenceEquals(remaining.Entries[1], laterEligible)
            || remaining.Entries.Any(entry => ReferenceEquals(entry, firstEligible)))
        {
            Fail("Passive-noticing controller failed stable atomic next-candidate resolution.");
            return;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || searchIndicator.Visible != expectedIndicatorVisibility
            || !string.Equals(
                focusedSearchResult.Text,
                expectedResultText,
                StringComparison.Ordinal)
            || !ReferenceEquals(perception.Entries[0], firstUnnoticed)
            || !ReferenceEquals(perception.Entries[1], laterUnnoticed)
            || !ReferenceEquals(candidates.Entries[0], unrelated)
            || !ReferenceEquals(candidates.Entries[1], firstEligible)
            || !ReferenceEquals(candidates.Entries[2], laterEligible))
        {
            Fail("Passive-noticing controller changed inputs or unrelated owned state.");
            return;
        }

        GD.Print(
            "Passive-noticing resolution controller smoke passed: uninitialized and ineligible "
            + "no-ops; stable first eligible selection; atomic target-only perception change "
            + "and exact candidate consumption; one perception owner; and unchanged inputs, "
            + "focused-search, and presentation state were correct.");
        GetTree().Quit();
    }

    private void SetControllerState(
        CharacterPerceptionSnapshot? perception,
        PassiveNoticingCandidateSnapshot candidates)
    {
        SetProperty(nameof(FocusedSearchActivityController.Perception), perception);
        SetProperty(
            nameof(FocusedSearchActivityController.PassiveNoticingCandidates),
            candidates);
    }

    private void SetProperty(string propertyName, object? value)
    {
        System.Reflection.PropertyInfo property = typeof(FocusedSearchActivityController)
            .GetProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Focused-search controller property {propertyName} was not found.");
        property.SetValue(controller, value);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
