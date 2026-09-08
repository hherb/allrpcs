using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class DiscoverySaveStateRestorationPolicyTests
{
    [Fact]
    public void AcceptsOwnedStateWhoseTargetsAllResolveByIdentifierValue()
    {
        CharacterId retainedSearcherId = new("seeker");
        ObjectId firstRetainedTargetId = new("herb-patch");
        ObjectId secondRetainedTargetId = new("stone-marker");
        DiscoverySaveState state = CreateState(
            retainedSearcherId,
            firstRetainedTargetId,
            secondRetainedTargetId);
        WorldObject firstWorldObject = WorldObject("herb-patch");
        WorldObject secondWorldObject = WorldObject("stone-marker");
        WorldObjectSnapshot worldObjects = new([firstWorldObject, secondWorldObject]);
        CharacterId suppliedSearcherId = new("seeker");

        bool canRestore = DiscoverySaveStateRestorationPolicy.CanRestore(
            state,
            suppliedSearcherId,
            worldObjects);

        Assert.True(canRestore);
        Assert.NotSame(retainedSearcherId, suppliedSearcherId);
        Assert.Same(firstRetainedTargetId, state.Perception.Entries[0].ObjectId);
        Assert.Same(secondRetainedTargetId, state.Perception.Entries[1].ObjectId);
        Assert.Collection(
            worldObjects.Entries,
            entry => Assert.Same(firstWorldObject, entry),
            entry => Assert.Same(secondWorldObject, entry));
    }

    [Fact]
    public void RejectsEveryStateCategoryOwnedByAnotherCharacter()
    {
        CharacterId searcherId = new("seeker");
        CharacterId otherId = new("travelling-healer");
        ObjectId firstTargetId = new("herb-patch");
        ObjectId secondTargetId = new("stone-marker");
        WorldObjectSnapshot worldObjects = WorldObjects("herb-patch", "stone-marker");
        DiscoverySaveState valid = CreateState(searcherId, firstTargetId, secondTargetId);

        Assert.False(CanRestore(
            ReplacePerception(
                valid,
                new CharacterPerceptionSnapshot(otherId, valid.Perception.Entries)),
            searcherId,
            worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            activityOwner: otherId), searcherId, worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            outcomeOwner: otherId), searcherId, worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            passiveOwner: otherId), searcherId, worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            reinterpretationOwner: otherId), searcherId, worldObjects));
    }

    [Fact]
    public void RejectsAnUnknownTargetInEveryStateCategory()
    {
        CharacterId searcherId = new("seeker");
        ObjectId firstTargetId = new("herb-patch");
        ObjectId secondTargetId = new("stone-marker");
        ObjectId missingTargetId = new("missing-object");
        WorldObjectSnapshot worldObjects = WorldObjects("herb-patch", "stone-marker");
        DiscoverySaveState valid = CreateState(searcherId, firstTargetId, secondTargetId);

        Assert.False(CanRestore(
            ReplacePerception(
                valid,
                new CharacterPerceptionSnapshot(
                    searcherId,
                    [new WorldObjectVisibility(missingTargetId, VisibilityState.Noticed)])),
            searcherId,
            worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            activityTarget: missingTargetId), searcherId, worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            outcomeTarget: missingTargetId), searcherId, worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            passiveTarget: missingTargetId), searcherId, worldObjects));
        Assert.False(CanRestore(CreateState(
            searcherId,
            firstTargetId,
            secondTargetId,
            reinterpretationTarget: missingTargetId), searcherId, worldObjects));
    }

    [Fact]
    public void AcceptsEmptyOwnedCollections()
    {
        CharacterId searcherId = new("seeker");
        DiscoverySaveState state = new(
            new CharacterPerceptionSnapshot(searcherId, []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));

        Assert.True(CanRestore(state, searcherId, new WorldObjectSnapshot([])));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId searcherId = new("seeker");
        DiscoverySaveState state = CreateState(
            searcherId,
            new ObjectId("herb-patch"),
            new ObjectId("stone-marker"));
        WorldObjectSnapshot worldObjects = WorldObjects("herb-patch", "stone-marker");

        Assert.Throws<ArgumentNullException>(
            () => DiscoverySaveStateRestorationPolicy.CanRestore(
                null!, searcherId, worldObjects));
        Assert.Throws<ArgumentNullException>(
            () => DiscoverySaveStateRestorationPolicy.CanRestore(
                state, null!, worldObjects));
        Assert.Throws<ArgumentNullException>(
            () => DiscoverySaveStateRestorationPolicy.CanRestore(
                state, searcherId, null!));
    }

    [Fact]
    public void ExposesOnlyTheStaticRestorationDecision()
    {
        Type policyType = typeof(DiscoverySaveStateRestorationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(DiscoverySaveStateRestorationPolicy.CanRestore), method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(DiscoverySaveState), parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldObjectSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }

    private static DiscoverySaveState CreateState(
        CharacterId owner,
        ObjectId firstTarget,
        ObjectId secondTarget,
        CharacterId? activityOwner = null,
        CharacterId? outcomeOwner = null,
        CharacterId? passiveOwner = null,
        CharacterId? reinterpretationOwner = null,
        ObjectId? activityTarget = null,
        ObjectId? outcomeTarget = null,
        ObjectId? passiveTarget = null,
        ObjectId? reinterpretationTarget = null) => new(
            new CharacterPerceptionSnapshot(
                owner,
                [
                    new WorldObjectVisibility(firstTarget, VisibilityState.Noticed),
                    new WorldObjectVisibility(secondTarget, VisibilityState.Identified),
                ]),
            new FocusedSearchActivitySnapshot(
                [new FocusedSearchActivity(Request(activityOwner ?? owner, activityTarget ?? firstTarget))]),
            new FocusedSearchOutcomeSnapshot(
                [
                    new FocusedSearchOutcome(
                        Request(outcomeOwner ?? owner, outcomeTarget ?? secondTarget),
                        VisibilityState.Identified),
                ]),
            new PassiveNoticingCandidateSnapshot(
                [new PassiveNoticingCandidate(passiveOwner ?? owner, passiveTarget ?? firstTarget)]),
            new ObservationReinterpretationCandidateSnapshot(
                [
                    new ObservationReinterpretationCandidate(
                        reinterpretationOwner ?? owner,
                        reinterpretationTarget ?? secondTarget,
                        new KnowledgeId("stone-marker-meaning")),
                ]));

    private static FocusedSearchRequest Request(CharacterId owner, ObjectId target) =>
        new(owner, target);

    private static DiscoverySaveState ReplacePerception(
        DiscoverySaveState state,
        CharacterPerceptionSnapshot perception) => new(
            perception,
            state.FocusedSearchActivities,
            state.FocusedSearchOutcomes,
            state.PassiveNoticingCandidates,
            state.ObservationReinterpretationCandidates);

    private static bool CanRestore(
        DiscoverySaveState state,
        CharacterId searcherId,
        WorldObjectSnapshot worldObjects) =>
        DiscoverySaveStateRestorationPolicy.CanRestore(state, searcherId, worldObjects);

    private static WorldObjectSnapshot WorldObjects(params string[] ids) =>
        new(ids.Select(WorldObject));

    private static WorldObject WorldObject(string id) => new(
        new WorldObjectIdentity(new ObjectId(id), id),
        new WorldObjectDescription(string.Empty));
}
