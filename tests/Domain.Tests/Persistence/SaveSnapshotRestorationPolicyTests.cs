using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotRestorationPolicyTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void AcceptsAKnownSnapshotWhoseDiscoveryAndEventStateCanRestore(int version)
    {
        CharacterId searcherId = new("seeker");
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldObjectSnapshot worldObjects = new([worldObject]);
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        WorldEventSnapshot worldEvents = new([worldEvent]);
        SaveSnapshot snapshot = CreateSnapshot(
            new SaveSchemaVersion(version),
            searcherId,
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);

        bool canRestore = SaveSnapshotRestorationPolicy.CanRestore(
            snapshot,
            new CharacterId("seeker"),
            worldObjects,
            worldEvents,
            Context());

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
            new SaveSchemaVersion(27),
            new CharacterId("seeker"),
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);

        Assert.False(SaveSnapshotRestorationPolicy.CanRestore(
            snapshot,
            new CharacterId("seeker"),
            new WorldObjectSnapshot([worldObject]),
            new WorldEventSnapshot([worldEvent]),
            Context()));
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
            new WorldEventSnapshot([worldEvent]),
            Context()));
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
            new WorldEventSnapshot([worldEvent]),
            Context()));
    }

    [Fact]
    public void AcceptsCompatibleAndRejectsIncompatibleFirstPatientState()
    {
        FirstPatientTreatmentContext context = Context();
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        SaveSnapshot baseline = CreateSnapshot(
            SaveSchemaVersion.CharacterMemory,
            new CharacterId("seeker"),
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);
        FirstPatientSaveState compatible = new(
            context.Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        FirstPatientSaveState incompatible = new(
            new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot([]), []),
            compatible.PreparedBatches,
            compatible.AdministrationExperiments,
            compatible.Experiments);
        SaveSnapshot valid = new(
            SaveSchemaVersion.FirstPatient,
            baseline.Discovery,
            baseline.WorldEvents,
            new CharacterMemorySnapshot([]),
            compatible);
        SaveSnapshot invalid = new(
            SaveSchemaVersion.FirstPatient,
            baseline.Discovery,
            baseline.WorldEvents,
            new CharacterMemorySnapshot([]),
            incompatible);
        WorldObjectSnapshot worldObjects = new([worldObject]);
        WorldEventSnapshot worldEvents = new([worldEvent]);

        Assert.True(SaveSnapshotRestorationPolicy.CanRestore(
            valid,
            new CharacterId("seeker"),
            worldObjects,
            worldEvents,
            context));
        Assert.False(SaveSnapshotRestorationPolicy.CanRestore(
            invalid,
            new CharacterId("seeker"),
            worldObjects,
            worldEvents,
            context));
    }

    [Fact]
    public void RejectsEveryNullInputBeforeDelegation()
    {
        CharacterId searcherId = new("seeker");
        WorldObject worldObject = CreateWorldObject("herb-patch");
        WorldObjectSnapshot worldObjects = new([worldObject]);
        WorldEvent worldEvent = CreateWorldEvent("herb-patch-identified");
        WorldEventSnapshot worldEvents = new([worldEvent]);
        FirstPatientTreatmentContext context = Context();
        SaveSnapshot snapshot = CreateSnapshot(
            SaveSchemaVersion.FirstSupported,
            searcherId,
            worldObject.Identity.Id,
            worldEvent.EventId,
            worldEvent.EventId);

        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            null!, searcherId, worldObjects, worldEvents, context));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            snapshot, null!, worldObjects, worldEvents, context));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            snapshot, searcherId, null!, worldEvents, context));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            snapshot, searcherId, worldObjects, null!, context));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotRestorationPolicy.CanRestore(
            snapshot, searcherId, worldObjects, worldEvents, null!));
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
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FirstPatientTreatmentContext),
                parameter.ParameterType));
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

    private static FirstPatientTreatmentContext Context() =>
        RepresentativeFirstPatientTreatmentContextSource.Create();
}
