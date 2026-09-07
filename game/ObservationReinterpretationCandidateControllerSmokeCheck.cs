using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free observation-reinterpretation candidate controller checks when enabled.
/// </summary>
public partial class ObservationReinterpretationCandidateControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_REINTERPRETATION_CANDIDATE_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityController controller = null!;
    private TemporarySeekerCharacterSource characterSource = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
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
        characterSource = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
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
        CharacterPerceptionSnapshot? expectedPerception = controller.Perception;
        Character expectedCharacter = characterSource.Character;
        WorldObjectSnapshot expectedWorldObjects = worldObjectSource.WorldObjects;
        bool expectedIndicatorVisibility = searchIndicator.Visible;
        string expectedResultText = focusedSearchResult.Text;
        ObservationReinterpretationCandidateSnapshot initialCandidates = new([]);
        SetCandidates(initialCandidates);
        WorldObject target = expectedWorldObjects.Entries[1];
        WorldObject unrelatedWorldObject = expectedWorldObjects.Entries[0];
        ObservationReinterpretationCandidate candidate = new(
            new CharacterId(controller.SearcherCharacterId.Value),
            new ObjectId(target.Identity.Id.Value),
            new KnowledgeId("stone-marker-meaning"));
        ObservationReinterpretationCandidate wrongObserver = new(
            new CharacterId("travelling-healer"),
            candidate.TargetObjectId,
            candidate.TriggerKnowledgeId);
        ObservationReinterpretationCandidate wrongTarget = new(
            candidate.ObserverCharacterId,
            new ObjectId("absent-object"),
            candidate.TriggerKnowledgeId);
        Character unrelatedCharacter = CreateCharacter(new CharacterId("travelling-healer"));

        if (initialCandidates.Entries.Count != 0
            || controller.TryProduceObservationReinterpretationCandidate(
                wrongObserver,
                expectedCharacter,
                target)
            || controller.TryProduceObservationReinterpretationCandidate(
                wrongTarget,
                expectedCharacter,
                target)
            || controller.TryProduceObservationReinterpretationCandidate(
                candidate,
                unrelatedCharacter,
                target)
            || controller.TryProduceObservationReinterpretationCandidate(
                candidate,
                expectedCharacter,
                unrelatedWorldObject)
            || !ReferenceEquals(controller.ObservationReinterpretationCandidates, initialCandidates))
        {
            Fail("Observation-reinterpretation controller failed closed production eligibility.");
            return;
        }

        if (!controller.TryProduceObservationReinterpretationCandidate(
                candidate,
                expectedCharacter,
                target))
        {
            Fail("Observation-reinterpretation controller rejected eligible production.");
            return;
        }

        ObservationReinterpretationCandidateSnapshot produced =
            controller.ObservationReinterpretationCandidates;
        ObservationReinterpretationCandidate duplicate = new(
            new CharacterId(candidate.ObserverCharacterId.Value),
            new ObjectId(candidate.TargetObjectId.Value),
            new KnowledgeId(candidate.TriggerKnowledgeId.Value));
        if (ReferenceEquals(produced, initialCandidates)
            || produced.Entries.Count != 1
            || !ReferenceEquals(produced.Entries[0], candidate)
            || controller.TryProduceObservationReinterpretationCandidate(
                duplicate,
                expectedCharacter,
                target)
            || !ReferenceEquals(controller.ObservationReinterpretationCandidates, produced))
        {
            Fail("Observation-reinterpretation controller failed exact production or duplicate rejection.");
            return;
        }

        AssertNullRejection(candidate, expectedCharacter, target);

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.PassiveNoticingCandidates, expectedPassiveCandidates)
            || !ReferenceEquals(controller.Perception, expectedPerception)
            || !ReferenceEquals(characterSource.Character, expectedCharacter)
            || !ReferenceEquals(worldObjectSource.WorldObjects, expectedWorldObjects)
            || searchIndicator.Visible != expectedIndicatorVisibility
            || !string.Equals(focusedSearchResult.Text, expectedResultText, StringComparison.Ordinal))
        {
            Fail("Observation-reinterpretation controller changed inputs or unrelated state.");
            return;
        }

        GD.Print(
            "Observation-reinterpretation candidate controller smoke passed: empty ownership; "
            + "exact and value-equal production; fail-closed mismatches, duplicates, and nulls; "
            + "exact candidate retention; and unchanged unrelated state were correct.");
        GetTree().Quit();
    }

    private void AssertNullRejection(
        ObservationReinterpretationCandidate candidate,
        Character character,
        WorldObject worldObject)
    {
        AssertThrowsArgumentNull(
            () => controller.TryProduceObservationReinterpretationCandidate(
                null!,
                character,
                worldObject));
        AssertThrowsArgumentNull(
            () => controller.TryProduceObservationReinterpretationCandidate(
                candidate,
                null!,
                worldObject));
        AssertThrowsArgumentNull(
            () => controller.TryProduceObservationReinterpretationCandidate(
                candidate,
                character,
                null!));
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

    private Character CreateCharacter(CharacterId characterId)
    {
        System.Reflection.MethodInfo method = typeof(TemporarySeekerCharacterSource).GetMethod(
            "CreateCharacter",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "Temporary Seeker character source factory was not found.");

        return (Character)(method.Invoke(null, [characterId])
            ?? throw new InvalidOperationException(
                "Temporary Seeker character source returned no character."));
    }

    private void SetCandidates(ObservationReinterpretationCandidateSnapshot candidates)
    {
        System.Reflection.PropertyInfo property = typeof(FocusedSearchActivityController)
            .GetProperty(nameof(FocusedSearchActivityController.ObservationReinterpretationCandidates))
            ?? throw new InvalidOperationException(
                "Focused-search controller observation-reinterpretation candidates were not found.");
        property.SetValue(controller, candidates);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
