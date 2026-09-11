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
            new SaveSchemaVersion(4),
            discovery,
            worldEvents,
            memory,
            firstPatient));
        Assert.Throws<ArgumentNullException>(() => new SaveSnapshot(
            SaveSchemaVersion.FirstPatient,
            discovery,
            worldEvents,
            memory));

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
            ("Discovery", typeof(DiscoverySaveState)),
            ("FirstPatient", typeof(FirstPatientSaveState)),
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
}
