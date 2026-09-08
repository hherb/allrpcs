using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotRestorationPolicyTests
{
    [Fact]
    public void AcceptsASupportedSnapshotWhoseDiscoveryAndEventStateCanRestore()
    {
        CharacterId searcherId = new("seeker");
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldObjectSnapshot worldObjects = new([worldObject]);
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        WorldEventSnapshot worldEvents = new([worldEvent]);
        SaveSnapshot snapshot = CreateSnapshot(
            SaveSchemaVersion.FirstSupported,
            searcherId,
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);

        bool canRestore = SaveSnapshotRestorationPolicy.CanRestore(
            snapshot,
            new CharacterId("seeker"),
            worldObjects,
            worldEvents);

        Assert.True(canRestore);
        Assert.Same(worldObject.Identity.Id, snapshot.Discovery.Perception.Entries[0].ObjectId);
        Assert.Same(worldEvent.EventId, snapshot.WorldEvents.EventLog.Entries[0]);
        Assert.Collection(worldObjects.Entries, entry => Assert.Same(worldObject, entry));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(worldEvent, entry));
    }

    [Fact]
    public void RejectsAnUnsupportedSchemaBeforeRestoration()
    {
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        SaveSnapshot snapshot = CreateSnapshot(
            new SaveSchemaVersion(2),
            new CharacterId("seeker"),
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);

        Assert.False(SaveSnapshotRestorationPolicy.CanRestore(
            snapshot,
            new CharacterId("seeker"),
            new WorldObjectSnapshot([worldObject]),
            new WorldEventSnapshot([worldEvent])));
    }

    [Fact]
    public void RejectsWhenTheDelegatedDiscoveryDecisionRejects()
    {
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        SaveSnapshot snapshot = CreateSnapshot(
            SaveSchemaVersion.FirstSupported,
            new CharacterId("travelling-healer"),
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);

        Assert.False(SaveSnapshotRestorationPolicy.CanRestore(
            snapshot,
            new CharacterId("seeker"),
            new WorldObjectSnapshot([worldObject]),
            new WorldEventSnapshot([worldEvent])));
    }

    [Fact]
    public void RejectsWhenTheDelegatedWorldEventDecisionRejects()
    {
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        SaveSnapshot snapshot = CreateSnapshot(
            SaveSchemaVersion.FirstSupported,
            new CharacterId("seeker"),
            worldObject.Identity.Id,
            worldEvent.EventId,
            null);

        Assert.False(SaveSnapshotRestorationPolicy.CanRestore(
            snapshot,
            new CharacterId("seeker"),
            new WorldObjectSnapshot([worldObject]),
            new WorldEventSnapshot([worldEvent])));
    }

    [Fact]
    public void RejectsEveryNullInputBeforeDelegation()
    {
        CharacterId searcherId = new("seeker");
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldObjectSnapshot worldObjects = new([worldObject]);
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        WorldEventSnapshot worldEvents = new([worldEvent]);
        SaveSnapshot snapshot = CreateSnapshot(
            SaveSchemaVersion.FirstSupported,
            searcherId,
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);

        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            null!, searcherId, worldObjects, worldEvents));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            snapshot, null!, worldObjects, worldEvents));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            snapshot, searcherId, null!, worldEvents));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            snapshot, searcherId, worldObjects, null!));
    }

    [Fact]
    public void ExposesOnlyTheStaticRestorationDecision()
    {
        Type policyType = typeof(SaveSnapshotRestorationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(SaveSnapshotRestorationPolicy.CanRestore), method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(SaveSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldObjectSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }

    private static SaveSnapshot CreateSnapshot(
        SaveSchemaVersion version,
        CharacterId owner,
        ObjectId target,
        WorldEventId loggedEvent,
        WorldEventId? currentTimeEvent) => new(
            version,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(
                    owner,
                    [new WorldObjectVisibility(target, VisibilityState.Identified)]),
                new FocusedSearchActivitySnapshot([]),
                new FocusedSearchOutcomeSnapshot([]),
                new PassiveNoticingCandidateSnapshot([]),
                new ObservationReinterpretationCandidateSnapshot([])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([loggedEvent]),
                new WorldTimeSnapshot(currentTimeEvent)));

    private static WorldObject CreateWorldObject(string id) => new(
        new WorldObjectIdentity(new ObjectId(id), id),
        new WorldObjectDescription(string.Empty));

    private static WorldEvent CreateWorldEvent(string id) =>
        new(new WorldEventId(id), WorldEventTimeEffect.Advance);
}
