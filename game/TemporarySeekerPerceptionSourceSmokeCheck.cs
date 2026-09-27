using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free temporary Seeker perception source checks when explicitly enabled.
/// </summary>
public partial class TemporarySeekerPerceptionSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_PERCEPTION_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporarySeekerPerceptionSource source = null!;
    private TemporaryPassiveNoticingCandidateSource candidateSource = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
    private FocusedSearchActivityController controller = null!;
    private GreyboxInteractionTarget herbPatch = null!;
    private GreyboxInteractionTarget stoneMarker = null!;
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

        source = GetNode<TemporarySeekerPerceptionSource>("../TemporarySeekerPerceptionSource");
        candidateSource = GetNode<TemporaryPassiveNoticingCandidateSource>(
            "../TemporaryPassiveNoticingCandidateSource");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        herbPatch = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/HerbPatch");
        stoneMarker = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/StoneMarker");
        indicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        PassiveNoticingCandidateSnapshot expectedCandidates =
            controller.PassiveNoticingCandidates;
        bool expectedIndicatorVisibility = indicator.Visible;
        PassiveNoticingCandidate candidate = candidateSource.Candidate;
        CharacterPerceptionSnapshot controllerPerception = controller.Perception
            ?? throw new InvalidOperationException(
                "Temporary Seeker perception source smoke found no live perception.");
        CharacterPerceptionSnapshot first = source.Perception;
        CharacterPerceptionSnapshot second = source.Perception;
        WorldObjectSnapshot sourceWorldObjects = worldObjectSource.WorldObjects;
        WorldObject sourceHerbPatch = sourceWorldObjects.Entries[0];
        WorldObject sourceStoneMarker = sourceWorldObjects.Entries[1];
        WorldObject sourceWaterCask = sourceWorldObjects.Entries[2];
        WorldObjectIdentity sourceHerbPatchIdentity = sourceHerbPatch.Identity;
        WorldObjectDescription sourceHerbPatchDescription = sourceHerbPatch.Description;
        WorldObjectIdentity sourceStoneMarkerIdentity = sourceStoneMarker.Identity;
        WorldObjectDescription sourceStoneMarkerDescription = sourceStoneMarker.Description;

        if (expectedActivities.Entries.Count != 0
            || expectedOutcomes.Entries.Count != 0
            || expectedIndicatorVisibility)
        {
            Fail(
                "Temporary Seeker perception source smoke found changed initial controller or "
                + "presentation state.");
            return;
        }

        if (!ReferenceEquals(first, second)
            || ReferenceEquals(first, controllerPerception)
            || !ReferenceEquals(first.CharacterId, candidate.ObserverCharacterId)
            || !ReferenceEquals(first.CharacterId, controller.SearcherCharacterId))
        {
            Fail("Temporary Seeker perception source smoke failed stable snapshot retention.");
            return;
        }

        if (first.Entries.Count != 4
            || !ReferenceEquals(first.Entries[0].ObjectId, sourceHerbPatch.Identity.Id)
            || !ReferenceEquals(first.Entries[0].ObjectId, candidate.TargetObjectId)
            || !ReferenceEquals(first.Entries[0].VisibilityState, VisibilityState.Unnoticed)
            || !ReferenceEquals(first.Entries[1].ObjectId, sourceStoneMarker.Identity.Id)
            || !ReferenceEquals(first.Entries[1].VisibilityState, VisibilityState.Noticed)
            || !ReferenceEquals(first.Entries[2].ObjectId, sourceWaterCask.Identity.Id)
            || !ReferenceEquals(first.Entries[2].VisibilityState, VisibilityState.Noticed)
            || ReferenceEquals(sourceHerbPatch.Identity.Id, herbPatch.ObjectId)
            || sourceHerbPatch.Identity.Id != herbPatch.ObjectId
            || ReferenceEquals(sourceStoneMarker.Identity.Id, stoneMarker.ObjectId)
            || sourceStoneMarker.Identity.Id != stoneMarker.ObjectId)
        {
            Fail(
                "Temporary Seeker perception source smoke failed exact ordered perception "
                + "retention.");
            return;
        }

        CharacterId independentCharacterId = new("independent-seeker");
        ObjectId independentFirstObjectId = new("independent-first-object");
        ObjectId independentLastObjectId = new("independent-last-object");
        ObjectId valueEqualCandidateTargetId = new("independent-last-object");
        WorldObject independentFirstObject = CreateWorldObject(
            independentFirstObjectId,
            "Independent first object");
        WorldObject independentLastObject = CreateWorldObject(
            independentLastObjectId,
            "Independent last object");
        WorldObjectIdentity independentFirstIdentity = independentFirstObject.Identity;
        WorldObjectDescription independentFirstDescription = independentFirstObject.Description;
        WorldObjectIdentity independentLastIdentity = independentLastObject.Identity;
        WorldObjectDescription independentLastDescription = independentLastObject.Description;
        WorldObjectSnapshot emptyWorldObjects = new([]);
        WorldObjectSnapshot independentWorldObjects = new(
            [independentFirstObject, independentLastObject]);
        PassiveNoticingCandidate independentCandidate = new(
            independentCharacterId,
            valueEqualCandidateTargetId);
        CharacterPerceptionSnapshot independentPerception = CreatePerception(
            independentCandidate,
            independentWorldObjects);

        if (independentPerception.Entries.Count != 2
            || !ReferenceEquals(independentPerception.CharacterId, independentCharacterId)
            || !ReferenceEquals(
                independentPerception.Entries[0].ObjectId,
                independentFirstObjectId)
            || !ReferenceEquals(
                independentPerception.Entries[0].VisibilityState,
                VisibilityState.Noticed)
            || !ReferenceEquals(
                independentPerception.Entries[1].ObjectId,
                independentLastObjectId)
            || !ReferenceEquals(
                independentPerception.Entries[1].VisibilityState,
                VisibilityState.Unnoticed)
            || ReferenceEquals(
                independentPerception.Entries[1].ObjectId,
                valueEqualCandidateTargetId)
            || !ReturnsMissingTargetFailure(independentCandidate, emptyWorldObjects))
        {
            Fail(
                "Temporary Seeker perception source smoke failed candidate-aligned "
                + "world-object snapshot derivation.");
            return;
        }

        if (!ReferenceEquals(worldObjectSource.WorldObjects, sourceWorldObjects)
            || !ReferenceEquals(sourceWorldObjects.Entries[0], sourceHerbPatch)
            || !ReferenceEquals(sourceWorldObjects.Entries[1], sourceStoneMarker)
            || !ReferenceEquals(sourceWorldObjects.Entries[2], sourceWaterCask)
            || !ReferenceEquals(sourceHerbPatch.Identity, sourceHerbPatchIdentity)
            || !ReferenceEquals(sourceHerbPatch.Description, sourceHerbPatchDescription)
            || !ReferenceEquals(sourceStoneMarker.Identity, sourceStoneMarkerIdentity)
            || !ReferenceEquals(sourceStoneMarker.Description, sourceStoneMarkerDescription)
            || !ReferenceEquals(independentWorldObjects.Entries[0], independentFirstObject)
            || !ReferenceEquals(independentWorldObjects.Entries[1], independentLastObject)
            || !ReferenceEquals(independentFirstObject.Identity, independentFirstIdentity)
            || !ReferenceEquals(
                independentFirstObject.Description,
                independentFirstDescription)
            || !ReferenceEquals(independentLastObject.Identity, independentLastIdentity)
            || !ReferenceEquals(independentLastObject.Description, independentLastDescription)
            || !ReferenceEquals(independentFirstObject.Identity.Id, independentFirstObjectId)
            || !ReferenceEquals(independentLastObject.Identity.Id, independentLastObjectId)
            || !ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.Perception, controllerPerception)
            || !ReferenceEquals(
                controllerPerception.Entries.Single(
                    entry => entry.ObjectId == candidate.TargetObjectId).VisibilityState,
                VisibilityState.Noticed)
            || !ReferenceEquals(controller.PassiveNoticingCandidates, expectedCandidates)
            || expectedCandidates.Entries.Count != 0
            || indicator.Visible != expectedIndicatorVisibility)
        {
            Fail(
                "Temporary Seeker perception source smoke changed controller state or "
                + "presentation.");
            return;
        }

        GD.Print(
            "Temporary Seeker perception source smoke passed: exact Seeker and world-object "
            + "identifier retention; candidate-aligned independent snapshot derivation; "
            + "explicit missing-target failure; stable order and visibility states; unchanged "
            + "nested inputs, controller candidates, focused-search state, targets, "
            + "and presentation; and one stable scene snapshot were correct.");
        GetTree().Quit();
    }

    private CharacterPerceptionSnapshot CreatePerception(
        PassiveNoticingCandidate candidate,
        WorldObjectSnapshot worldObjects)
    {
        System.Reflection.MethodInfo? method = typeof(TemporarySeekerPerceptionSource).GetMethod(
            "CreatePerception",
            System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);
        if (method is null)
        {
            Fail("Temporary Seeker perception source smoke found no snapshot derivation method.");
            return null!;
        }

        return (CharacterPerceptionSnapshot)(method.Invoke(source, [candidate, worldObjects])
            ?? throw new InvalidOperationException(
                "The temporary Seeker perception source returned no derived snapshot."));
    }

    private bool ReturnsMissingTargetFailure(
        PassiveNoticingCandidate candidate,
        WorldObjectSnapshot worldObjects)
    {
        try
        {
            _ = CreatePerception(candidate, worldObjects);
            return false;
        }
        catch (System.Reflection.TargetInvocationException exception)
        {
            return exception.InnerException is InvalidOperationException;
        }
    }

    private static WorldObject CreateWorldObject(ObjectId objectId, string displayName)
    {
        return new WorldObject(
            new WorldObjectIdentity(objectId, displayName),
            new WorldObjectDescription($"Description for {displayName}."));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
