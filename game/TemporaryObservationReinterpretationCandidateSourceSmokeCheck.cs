using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free temporary observation-reinterpretation source checks when enabled.
/// </summary>
public partial class TemporaryObservationReinterpretationCandidateSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_REINTERPRETATION_CANDIDATE_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporaryObservationReinterpretationCandidateSource source = null!;
    private TemporarySeekerCharacterSource characterSource = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
    private FocusedSearchActivityController controller = null!;
    private Line2D searchIndicator = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporaryObservationReinterpretationCandidateSource>(
            "../TemporaryObservationReinterpretationCandidateSource");
        characterSource = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        searchIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        Character expectedCharacter = characterSource.Character;
        WorldObjectSnapshot expectedWorldObjects = worldObjectSource.WorldObjects;
        WorldObject expectedTarget = expectedWorldObjects.Entries[1];
        KnowledgeId expectedTrigger = expectedCharacter.Knowledge.KnowledgeReferences[0];
        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        PassiveNoticingCandidateSnapshot expectedPassiveCandidates =
            controller.PassiveNoticingCandidates;
        CharacterPerceptionSnapshot? expectedPerception = controller.Perception;
        ObservationReinterpretationCandidateSnapshot expectedCandidates =
            controller.ObservationReinterpretationCandidates;
        bool expectedIndicatorVisibility = searchIndicator.Visible;
        ObservationReinterpretationCandidate first = source.Candidate;
        ObservationReinterpretationCandidate second = source.Candidate;

        if (!HasExpectedPropertySurface()
            || !ReferenceEquals(first, second)
            || !ReferenceEquals(first.ObserverCharacterId, expectedCharacter.Identity.Id)
            || !ReferenceEquals(first.TargetObjectId, expectedTarget.Identity.Id)
            || !ReferenceEquals(first.TriggerKnowledgeId, expectedTrigger))
        {
            Fail("Temporary observation-reinterpretation source failed exact retention.");
            return;
        }

        CharacterId independentCharacterId = new("independent-seeker");
        Character independentCharacter = CreateCharacter(independentCharacterId);
        ObjectId independentObjectId = new("independent-object");
        WorldObject independentWorldObject = new(
            new WorldObjectIdentity(independentObjectId, "INDEPENDENT OBJECT"),
            new WorldObjectDescription("An independent authored object."));
        ObservationReinterpretationCandidate independent = CreateCandidate(
            independentCharacter,
            independentWorldObject);
        if (!ReferenceEquals(independent.ObserverCharacterId, independentCharacterId)
            || !ReferenceEquals(independent.TargetObjectId, independentObjectId)
            || !ReferenceEquals(
                independent.TriggerKnowledgeId,
                independentCharacter.Knowledge.KnowledgeReferences[0]))
        {
            Fail("Temporary observation-reinterpretation source failed independent retention.");
            return;
        }

        if (!ReferenceEquals(characterSource.Character, expectedCharacter)
            || !ReferenceEquals(worldObjectSource.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(expectedWorldObjects.Entries[1], expectedTarget)
            || !ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.PassiveNoticingCandidates, expectedPassiveCandidates)
            || !ReferenceEquals(controller.Perception, expectedPerception)
            || !ReferenceEquals(
                controller.ObservationReinterpretationCandidates,
                expectedCandidates)
            || expectedCandidates.Entries.Count != 0
            || expectedPerception is null
            || !ReferenceEquals(
                expectedPerception.Entries.Single(
                    entry => entry.ObjectId == first.TargetObjectId).VisibilityState,
                VisibilityState.Identified)
            || searchIndicator.Visible != expectedIndicatorVisibility)
        {
            Fail("Temporary observation-reinterpretation source changed inputs or live state.");
            return;
        }

        GD.Print(
            "Temporary observation-reinterpretation candidate source smoke passed: one stable "
            + "candidate; exact scene and independent character, world-object, and trigger "
            + "knowledge identifier retention; exact ordinary-play production and atomic "
            + "resolution; and unchanged unrelated state were correct.");
        GetTree().Quit();
    }

    private static ObservationReinterpretationCandidate CreateCandidate(
        Character character,
        WorldObject worldObject)
    {
        System.Reflection.MethodInfo method =
            typeof(TemporaryObservationReinterpretationCandidateSource).GetMethod(
                "CreateCandidate",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "Temporary observation-reinterpretation candidate factory was not found.");

        return (ObservationReinterpretationCandidate)(method.Invoke(null, [character, worldObject])
            ?? throw new InvalidOperationException(
                "Temporary observation-reinterpretation source returned no candidate."));
    }

    private static Character CreateCharacter(CharacterId characterId)
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

    private static bool HasExpectedPropertySurface()
    {
        System.Reflection.PropertyInfo[] properties =
            typeof(TemporaryObservationReinterpretationCandidateSource).GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);

        return properties.Length == 1
            && string.Equals(properties[0].Name, "Candidate", StringComparison.Ordinal)
            && properties[0].PropertyType == typeof(ObservationReinterpretationCandidate)
            && properties[0].CanRead
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
