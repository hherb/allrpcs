using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free passive-noticing candidate controller checks when explicitly enabled.
/// </summary>
public partial class PassiveNoticingCandidateControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_PASSIVE_CANDIDATE_CONTROLLER_CHECK";
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
        CharacterPerceptionSnapshot? expectedPerception = controller.Perception;
        Character expectedCharacter = characterSource.Character;
        WorldObjectSnapshot expectedWorldObjects = worldObjectSource.WorldObjects;
        bool expectedIndicatorVisibility = searchIndicator.Visible;
        string expectedResultText = focusedSearchResult.Text;
        PassiveNoticingCandidateSnapshot initialCandidates =
            controller.PassiveNoticingCandidates;
        WorldObject target = expectedWorldObjects.Entries[0];
        WorldObject unrelatedWorldObject = expectedWorldObjects.Entries[1];
        PassiveNoticingCandidate candidate = new(
            new CharacterId(controller.SearcherCharacterId.Value),
            new ObjectId(target.Identity.Id.Value));
        PassiveNoticingCandidate wrongObserver = new(
            new CharacterId("travelling-healer"),
            candidate.TargetObjectId);
        PassiveNoticingCandidate wrongTarget = new(
            candidate.ObserverCharacterId,
            new ObjectId("absent-object"));
        Character unrelatedCharacter = CreateCharacter(new CharacterId("travelling-healer"));

        if (initialCandidates.Entries.Count != 0
            || controller.TryProducePassiveNoticingCandidate(
                wrongObserver,
                expectedCharacter,
                target)
            || controller.TryProducePassiveNoticingCandidate(
                wrongTarget,
                expectedCharacter,
                target)
            || controller.TryProducePassiveNoticingCandidate(
                candidate,
                unrelatedCharacter,
                target)
            || controller.TryProducePassiveNoticingCandidate(
                candidate,
                expectedCharacter,
                unrelatedWorldObject)
            || !ReferenceEquals(controller.PassiveNoticingCandidates, initialCandidates))
        {
            Fail("Passive-noticing controller failed closed production eligibility.");
            return;
        }

        if (!controller.TryProducePassiveNoticingCandidate(
                candidate,
                expectedCharacter,
                target))
        {
            Fail("Passive-noticing controller rejected eligible production.");
            return;
        }

        PassiveNoticingCandidateSnapshot produced = controller.PassiveNoticingCandidates;
        PassiveNoticingCandidate duplicate = new(
            new CharacterId(candidate.ObserverCharacterId.Value),
            new ObjectId(candidate.TargetObjectId.Value));
        if (ReferenceEquals(produced, initialCandidates)
            || produced.Entries.Count != 1
            || !ReferenceEquals(produced.Entries[0], candidate)
            || controller.TryProducePassiveNoticingCandidate(
                duplicate,
                expectedCharacter,
                target)
            || !ReferenceEquals(controller.PassiveNoticingCandidates, produced))
        {
            Fail("Passive-noticing controller failed exact production or duplicate rejection.");
            return;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.Perception, expectedPerception)
            || !ReferenceEquals(characterSource.Character, expectedCharacter)
            || !ReferenceEquals(worldObjectSource.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(candidate.ObserverCharacterId, produced.Entries[0].ObserverCharacterId)
            || !ReferenceEquals(candidate.TargetObjectId, produced.Entries[0].TargetObjectId)
            || searchIndicator.Visible != expectedIndicatorVisibility
            || !string.Equals(
                focusedSearchResult.Text,
                expectedResultText,
                StringComparison.Ordinal))
        {
            Fail("Passive-noticing controller changed inputs or unrelated owned state.");
            return;
        }

        GD.Print(
            "Passive-noticing candidate controller smoke passed: empty ownership; exact "
            + "value-equal Seeker and world-object production; fail-closed mismatches and "
            + "duplicates; exact candidate retention; and unchanged character, world-object, "
            + "focused-search, perception, and presentation state were correct.");
        GetTree().Quit();
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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
