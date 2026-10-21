using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.Tests.Characters;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotTests
{
    [Fact]
    public void RetainsTheExactVersionAndStateCompositions()
    {
        SaveSchemaVersion version = SaveSchemaVersion.FirstSupported;
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();

        SaveSnapshot snapshot = new(version, discovery, worldEvents);

        Assert.Same(version, snapshot.SchemaVersion);
        Assert.Same(discovery, snapshot.Discovery);
        Assert.Same(worldEvents, snapshot.WorldEvents);
        Assert.Empty(snapshot.CharacterMemory.Characters);
        Assert.Null(snapshot.FirstPatient);
        Assert.Null(snapshot.Journal);
        Assert.Null(snapshot.Map);
        Assert.Null(snapshot.InvestigationRoutes);
    }

    [Fact]
    public void CharacterMemoryVersionRetainsTheExactMemorySnapshot()
    {
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();
        CharacterMemorySnapshot memory = new([CharacterMemorySnapshotTests.Empty("healer")]);

        SaveSnapshot snapshot = new(
            SaveSchemaVersion.CharacterMemory,
            discovery,
            worldEvents,
            memory);

        Assert.Same(SaveSchemaVersion.CharacterMemory, snapshot.SchemaVersion);
        Assert.Same(discovery, snapshot.Discovery);
        Assert.Same(worldEvents, snapshot.WorldEvents);
        Assert.Same(memory, snapshot.CharacterMemory);
        Assert.Null(snapshot.FirstPatient);
        Assert.Null(snapshot.Journal);
        Assert.Null(snapshot.Map);
    }

    [Fact]
    public void FirstPatientVersionRetainsTheExactFirstPatientState()
    {
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();
        CharacterMemorySnapshot memory = new([]);
        FirstPatientSaveState firstPatient = CreateFirstPatientState();

        SaveSnapshot snapshot = new(
            SaveSchemaVersion.FirstPatient,
            discovery,
            worldEvents,
            memory,
            firstPatient);

        Assert.Same(SaveSchemaVersion.FirstPatient, snapshot.SchemaVersion);
        Assert.Same(discovery, snapshot.Discovery);
        Assert.Same(worldEvents, snapshot.WorldEvents);
        Assert.Same(memory, snapshot.CharacterMemory);
        Assert.Same(firstPatient, snapshot.FirstPatient);
        Assert.Null(snapshot.Journal);
        Assert.Null(snapshot.Map);
    }

    [Fact]
    public void DialogueDeliveryVersionRetainsTheExactExistingState()
    {
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();
        CharacterMemorySnapshot memory = new([]);
        FirstPatientSaveState firstPatient = CreateFirstPatientState();

        SaveSnapshot snapshot = new(
            SaveSchemaVersion.DialogueDelivery,
            discovery,
            worldEvents,
            memory,
            firstPatient);

        Assert.Same(SaveSchemaVersion.DialogueDelivery, snapshot.SchemaVersion);
        Assert.Same(discovery, snapshot.Discovery);
        Assert.Same(worldEvents, snapshot.WorldEvents);
        Assert.Same(memory, snapshot.CharacterMemory);
        Assert.Same(firstPatient, snapshot.FirstPatient);
        Assert.Null(snapshot.Journal);
        Assert.Null(snapshot.Map);
    }

    [Fact]
    public void JournalVersionRetainsTheExactExistingState()
    {
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();
        CharacterMemorySnapshot memory = new([]);
        FirstPatientSaveState firstPatient = CreateFirstPatientState();
        JournalSaveState journal = EmptyJournal(new CharacterId("seeker"));

        SaveSnapshot snapshot = new(
            SaveSchemaVersion.Journal,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            journal);

        Assert.Same(SaveSchemaVersion.Journal, snapshot.SchemaVersion);
        Assert.Same(discovery, snapshot.Discovery);
        Assert.Same(worldEvents, snapshot.WorldEvents);
        Assert.Same(memory, snapshot.CharacterMemory);
        Assert.Same(firstPatient, snapshot.FirstPatient);
        Assert.Same(journal, snapshot.Journal);
        Assert.Null(snapshot.Map);
        Assert.NotSame(discovery.Perception.CharacterId, journal.SeekerId);
    }

    [Fact]
    public void MapVersionRetainsExactExistingJournalAndMapState()
    {
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();
        CharacterMemorySnapshot memory = new([]);
        FirstPatientSaveState firstPatient = CreateFirstPatientState();
        JournalSaveState journal = EmptyJournal(discovery.Perception.CharacterId);
        MapSaveState map = EmptyMap(discovery.Perception.CharacterId);

        SaveSnapshot snapshot = new(
            SaveSchemaVersion.Map,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            journal,
            map);

        Assert.Same(SaveSchemaVersion.Map, snapshot.SchemaVersion);
        Assert.Same(discovery, snapshot.Discovery);
        Assert.Same(worldEvents, snapshot.WorldEvents);
        Assert.Same(memory, snapshot.CharacterMemory);
        Assert.Same(firstPatient, snapshot.FirstPatient);
        Assert.Same(journal, snapshot.Journal);
        Assert.Same(map, snapshot.Map);
    }

    [Fact]
    public void FirstVersionRejectsPopulatedCharacterMemoryWithoutChangingIt()
    {
        CharacterMemorySnapshot memory = new([CharacterMemorySnapshotTests.Empty("healer")]);

        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.FirstSupported,
            CreateDiscoveryState(),
            CreateWorldEventState(),
            memory));
        Assert.Single(memory.Characters);
    }

    [Fact]
    public void OtherVersionsRejectFirstPatientStateAndFirstPatientVersionRequiresIt()
    {
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();
        CharacterMemorySnapshot memory = new([]);
        FirstPatientSaveState firstPatient = CreateFirstPatientState();
        DiseaseTruthSnapshot diseaseTruth = firstPatient.DiseaseTruth;

        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.FirstSupported,
            discovery,
            worldEvents,
            memory,
            firstPatient));
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.CharacterMemory,
            discovery,
            worldEvents,
            memory,
            firstPatient));
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            new SaveSchemaVersion(11),
            discovery,
            worldEvents,
            memory,
            firstPatient));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            SaveSchemaVersion.FirstPatient,
            discovery,
            worldEvents,
            memory));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            discovery,
            worldEvents,
            memory));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            SaveSchemaVersion.Journal,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            null));
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            EmptyJournal(discovery.Perception.CharacterId)));
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.Journal,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            EmptyJournal(new CharacterId("foreigner"))));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            SaveSchemaVersion.Map,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            EmptyJournal(discovery.Perception.CharacterId),
            null));
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.Map,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            EmptyJournal(discovery.Perception.CharacterId),
            EmptyMap(new CharacterId("foreigner"))));
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.Map,
            discovery,
            worldEvents,
            memory,
            firstPatient,
            EmptyJournal(new CharacterId("journal-owner")),
            EmptyMap(discovery.Perception.CharacterId)));

        Assert.Same(diseaseTruth, firstPatient.DiseaseTruth);
        Assert.Empty(memory.Characters);
    }

    [Fact]
    public void EqualityUsesTheRetainedVersionValueAndStateInstances()
    {
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();
        SaveSnapshot first = new(new SaveSchemaVersion(1), discovery, worldEvents);
        SaveSnapshot same = new(new SaveSchemaVersion(1), discovery, worldEvents);
        SaveSnapshot distinctState = new(
            new SaveSchemaVersion(1),
            CreateDiscoveryState(),
            CreateWorldEventState());
        SaveSnapshot future = new(new SaveSchemaVersion(2), discovery, worldEvents);

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, distinctState);
        Assert.NotEqual(first, future);
    }

    [Fact]
    public void RejectsEveryNullComponent()
    {
        SaveSchemaVersion version = SaveSchemaVersion.FirstSupported;
        DiscoverySaveState discovery = CreateDiscoveryState();
        WorldEventSaveState worldEvents = CreateWorldEventState();

        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            null!, discovery, worldEvents));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            version, null!, worldEvents));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            version, discovery, null!));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            version, discovery, worldEvents, null!));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            SaveSchemaVersion.FirstPatient,
            discovery,
            worldEvents,
            new CharacterMemorySnapshot([]),
            null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableSaveRootSurface()
    {
        Type snapshotType = typeof(SaveSnapshot);
        (string Name, Type Type)[] expectedProperties =
        [
            ("CharacterMemory", typeof(CharacterMemorySnapshot)),
            ("Continuation", typeof(SaveContinuationState)),
            ("Discovery", typeof(DiscoverySaveState)),
            ("FirstPatient", typeof(FirstPatientSaveState)),
            ("FirstPatientRecovery", typeof(FirstPatientRecoveryStage)),
            ("Inventory", typeof(Allrpcs.Domain.Inventory.InventoryOwnershipSnapshot)),
            ("InvestigationRoutes", typeof(InvestigationRouteSaveState)),
            ("Journal", typeof(JournalSaveState)),
            ("Map", typeof(MapSaveState)),
            ("SchemaVersion", typeof(SaveSchemaVersion)),
            ("WorldEvents", typeof(WorldEventSaveState)),
        ];

        System.Reflection.PropertyInfo[] properties = snapshotType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedProperties, properties.Select(property =>
            (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(snapshotType.GetEvents());
        Assert.True(snapshotType.IsSealed);
    }

    private static DiscoverySaveState CreateDiscoveryState() => new(
        new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
        new FocusedSearchActivitySnapshot([]),
        new FocusedSearchOutcomeSnapshot([]),
        new PassiveNoticingCandidateSnapshot([]),
        new ObservationReinterpretationCandidateSnapshot([]));

    private static WorldEventSaveState CreateWorldEventState() => new(
        new WorldEventLogSnapshot([]),
        new WorldTimeSnapshot(null));

    private static FirstPatientSaveState CreateFirstPatientState() => new(
        RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));

    private static JournalSaveState EmptyJournal(CharacterId seekerId) => new(
        seekerId,
        [],
        [],
        [],
        [],
        [],
        null);

    private static MapSaveState EmptyMap(CharacterId seekerId) => new(
        seekerId,
        [],
        [],
        []);
}
