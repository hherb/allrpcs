using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free observation-reinterpretation resolution controller checks when enabled.
/// </summary>
public partial class ObservationReinterpretationResolutionControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_REINTERPRETATION_RESOLUTION_CONTROLLER_CHECK";
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
        PassiveNoticingCandidateSnapshot expectedPassiveCandidates =
            controller.PassiveNoticingCandidates;
        bool expectedIndicatorVisibility = searchIndicator.Visible;
        string expectedResultText = focusedSearchResult.Text;
        KnowledgeId triggerId = new("stone-marker-meaning");
        Character character = CreateCharacter(
            new CharacterId(controller.SearcherCharacterId.Value),
            triggerId);
        ObservationReinterpretationCandidateSnapshot emptyCandidates = new([]);
        SetControllerState(perception: null, emptyCandidates);

        if (controller.TryResolveNextObservationReinterpretationCandidate(character)
            || controller.Perception is not null
            || !ReferenceEquals(
                controller.ObservationReinterpretationCandidates,
                emptyCandidates))
        {
            Fail("Observation-reinterpretation controller changed uninitialized state.");
            return;
        }

        ObjectId firstTargetId = new("first-target");
        WorldObjectVisibility alreadyIdentified = new(firstTargetId, VisibilityState.Identified);
        CharacterPerceptionSnapshot unchangedPerception = new(
            controller.SearcherCharacterId,
            [alreadyIdentified]);
        ObservationReinterpretationCandidate ineligible = new(
            controller.SearcherCharacterId,
            firstTargetId,
            triggerId);
        ObservationReinterpretationCandidateSnapshot ineligibleCandidates = new([ineligible]);
        SetControllerState(unchangedPerception, ineligibleCandidates);
        Character wrongCharacter = CreateCharacter(new CharacterId("travelling-healer"), triggerId);

        if (controller.TryResolveNextObservationReinterpretationCandidate(wrongCharacter)
            || controller.TryResolveNextObservationReinterpretationCandidate(character)
            || !ReferenceEquals(controller.Perception, unchangedPerception)
            || !ReferenceEquals(
                controller.ObservationReinterpretationCandidates,
                ineligibleCandidates))
        {
            Fail("Observation-reinterpretation controller changed ineligible state.");
            return;
        }

        ObjectId laterTargetId = new("later-target");
        WorldObjectVisibility firstNoticed = new(firstTargetId, VisibilityState.Noticed);
        WorldObjectVisibility laterNoticed = new(laterTargetId, VisibilityState.Noticed);
        CharacterPerceptionSnapshot perception = new(
            new CharacterId(controller.SearcherCharacterId.Value),
            [firstNoticed, laterNoticed]);
        ObservationReinterpretationCandidate unrelated = new(
            new CharacterId("travelling-healer"),
            firstTargetId,
            triggerId);
        ObservationReinterpretationCandidate firstEligible = new(
            new CharacterId(controller.SearcherCharacterId.Value),
            new ObjectId(laterTargetId.Value),
            new KnowledgeId(triggerId.Value));
        ObservationReinterpretationCandidate laterEligible = new(
            controller.SearcherCharacterId,
            firstTargetId,
            triggerId);
        ObservationReinterpretationCandidateSnapshot candidates = new(
            [unrelated, firstEligible, laterEligible]);
        SetControllerState(perception, candidates);

        if (!controller.TryResolveNextObservationReinterpretationCandidate(character))
        {
            Fail("Observation-reinterpretation controller rejected the first eligible candidate.");
            return;
        }

        CharacterPerceptionSnapshot resolvedPerception = controller.Perception!;
        ObservationReinterpretationCandidateSnapshot remaining =
            controller.ObservationReinterpretationCandidates;
        if (ReferenceEquals(resolvedPerception, perception)
            || !ReferenceEquals(resolvedPerception.CharacterId, perception.CharacterId)
            || resolvedPerception.Entries.Count != 2
            || !ReferenceEquals(resolvedPerception.Entries[0], firstNoticed)
            || !ReferenceEquals(resolvedPerception.Entries[1].ObjectId, laterTargetId)
            || !ReferenceEquals(
                resolvedPerception.Entries[1].VisibilityState,
                VisibilityState.Identified)
            || ReferenceEquals(resolvedPerception.Entries[1], laterNoticed)
            || remaining.Entries.Count != 2
            || !ReferenceEquals(remaining.Entries[0], unrelated)
            || !ReferenceEquals(remaining.Entries[1], laterEligible)
            || remaining.Entries.Any(entry => ReferenceEquals(entry, firstEligible)))
        {
            Fail("Observation-reinterpretation controller failed stable atomic resolution.");
            return;
        }

        AssertThrowsArgumentNull(
            () => controller.TryResolveNextObservationReinterpretationCandidate(null!));

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.PassiveNoticingCandidates, expectedPassiveCandidates)
            || searchIndicator.Visible != expectedIndicatorVisibility
            || !string.Equals(focusedSearchResult.Text, expectedResultText, StringComparison.Ordinal)
            || !ReferenceEquals(perception.Entries[0], firstNoticed)
            || !ReferenceEquals(perception.Entries[1], laterNoticed)
            || !ReferenceEquals(candidates.Entries[0], unrelated)
            || !ReferenceEquals(candidates.Entries[1], firstEligible)
            || !ReferenceEquals(candidates.Entries[2], laterEligible))
        {
            Fail("Observation-reinterpretation controller changed inputs or unrelated state.");
            return;
        }

        GD.Print(
            "Observation-reinterpretation resolution controller smoke passed: uninitialized, "
            + "wrong-character, and ineligible no-ops; stable first eligible selection; atomic "
            + "target-only identification and exact consumption; null rejection; and unchanged "
            + "inputs and unrelated state were correct.");
        GetTree().Quit();
    }

    private static Character CreateCharacter(CharacterId characterId, KnowledgeId knowledgeId)
    {
        return new Character(
            new CharacterIdentity(characterId, "TEST CHARACTER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.DeterministicTest),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([knowledgeId]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }

    private void SetControllerState(
        CharacterPerceptionSnapshot? perception,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        SetProperty(nameof(FocusedSearchActivityController.Perception), perception);
        SetProperty(
            nameof(FocusedSearchActivityController.ObservationReinterpretationCandidates),
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

    private static void AssertThrowsArgumentNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return;
        }

        throw new InvalidOperationException("Expected ArgumentNullException was not thrown.");
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
