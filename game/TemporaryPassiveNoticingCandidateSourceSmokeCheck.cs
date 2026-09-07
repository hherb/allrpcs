using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free temporary passive-noticing candidate source checks when enabled.
/// </summary>
public partial class TemporaryPassiveNoticingCandidateSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_PASSIVE_CANDIDATE_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporaryPassiveNoticingCandidateSource source = null!;
    private TemporarySeekerCharacterSource characterSource = null!;
    private TemporarySeekerPerceptionSource perceptionSource = null!;
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

        source = GetNode<TemporaryPassiveNoticingCandidateSource>(
            "../TemporaryPassiveNoticingCandidateSource");
        characterSource = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        perceptionSource = GetNode<TemporarySeekerPerceptionSource>(
            "../TemporarySeekerPerceptionSource");
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
        WorldObject expectedTarget = expectedWorldObjects.Entries[0];
        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = controller.Perception;
        CharacterPerceptionSnapshot sourcePerception = perceptionSource.Perception;
        PassiveNoticingCandidateSnapshot expectedCandidates =
            controller.PassiveNoticingCandidates;
        bool expectedIndicatorVisibility = searchIndicator.Visible;
        PassiveNoticingCandidate first = source.Candidate;
        PassiveNoticingCandidate second = source.Candidate;

        if (!HasExpectedPropertySurface()
            || !ReferenceEquals(first, second)
            || !ReferenceEquals(first.ObserverCharacterId, expectedCharacter.Identity.Id)
            || !ReferenceEquals(first.TargetObjectId, expectedTarget.Identity.Id)
            || !ReferenceEquals(
                expectedCharacter.ControllerAssignment.CharacterId,
                first.ObserverCharacterId))
        {
            Fail("Temporary passive-noticing source failed exact scene candidate retention.");
            return;
        }

        CharacterId independentCharacterId = new("independent-seeker");
        ObjectId independentObjectId = new("independent-object");
        Character independentCharacter = CreateCharacter(independentCharacterId);
        WorldObject independentWorldObject = new(
            new WorldObjectIdentity(independentObjectId, "INDEPENDENT OBJECT"),
            new WorldObjectDescription("An independent authored object."));
        PassiveNoticingCandidate independent = CreateCandidate(
            independentCharacter,
            independentWorldObject);
        if (!ReferenceEquals(independent.ObserverCharacterId, independentCharacterId)
            || !ReferenceEquals(independent.TargetObjectId, independentObjectId))
        {
            Fail("Temporary passive-noticing source failed independent input retention.");
            return;
        }

        if (!ReferenceEquals(characterSource.Character, expectedCharacter)
            || !ReferenceEquals(worldObjectSource.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(expectedWorldObjects.Entries[0], expectedTarget)
            || !ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.Perception, expectedPerception)
            || expectedPerception is null
            || ReferenceEquals(expectedPerception, sourcePerception)
            || !ReferenceEquals(
                controller.PassiveNoticingCandidates,
                expectedCandidates)
            || expectedCandidates.Entries.Count != 0
            || !ReferenceEquals(
                expectedPerception.Entries.Single(
                    entry => entry.ObjectId == first.TargetObjectId).VisibilityState,
                VisibilityState.Noticed)
            || !ReferenceEquals(
                sourcePerception.Entries.Single(
                    entry => entry.ObjectId == first.TargetObjectId).VisibilityState,
                VisibilityState.Unnoticed)
            || searchIndicator.Visible != expectedIndicatorVisibility)
        {
            Fail("Temporary passive-noticing source changed inputs or controller state.");
            return;
        }

        GD.Print(
            "Temporary passive-noticing candidate source smoke passed: one stable candidate; "
            + "exact scene and independent character/world-object identifier retention; no "
            + "source-owned controller mutation; exact ordinary-play production and atomic "
            + "resolution; unchanged source perception, focused-search, and presentation state "
            + "were correct.");
        GetTree().Quit();
    }

    private PassiveNoticingCandidate CreateCandidate(
        Character character,
        WorldObject worldObject)
    {
        System.Reflection.MethodInfo method =
            typeof(TemporaryPassiveNoticingCandidateSource).GetMethod(
                "CreateCandidate",
                System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "Temporary passive-noticing candidate factory was not found.");

        return (PassiveNoticingCandidate)(method.Invoke(null, [character, worldObject])
            ?? throw new InvalidOperationException(
                "Temporary passive-noticing candidate source returned no candidate."));
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

    private static bool HasExpectedPropertySurface()
    {
        System.Reflection.PropertyInfo[] properties =
            typeof(TemporaryPassiveNoticingCandidateSource).GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);

        return properties.Length == 1
            && string.Equals(properties[0].Name, "Candidate", StringComparison.Ordinal)
            && properties[0].PropertyType == typeof(PassiveNoticingCandidate)
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
