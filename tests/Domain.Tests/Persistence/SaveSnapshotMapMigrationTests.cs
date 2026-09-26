using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotMapMigrationTests
{
    [Fact]
    public void MigratesJournalRootToFreshEmptySameSeekerMapState()
    {
        SaveSnapshot source = JournalVersion();

        SaveSnapshot migrated = SaveSnapshotMapMigration.Migrate(source);

        Assert.NotSame(source, migrated);
        Assert.Same(SaveSchemaVersion.Map, migrated.SchemaVersion);
        Assert.Same(source.Discovery, migrated.Discovery);
        Assert.Same(source.WorldEvents, migrated.WorldEvents);
        Assert.Same(source.CharacterMemory, migrated.CharacterMemory);
        Assert.Same(source.FirstPatient, migrated.FirstPatient);
        Assert.Same(source.Journal, migrated.Journal);
        Assert.NotNull(migrated.Map);
        Assert.Same(source.Discovery.Perception.CharacterId, migrated.Map.SeekerId);
        Assert.Empty(migrated.Map.ReportedLocationIds);
        Assert.Empty(migrated.Map.VerifiedLocationIds);
        Assert.Empty(migrated.Map.Notes);
    }

    [Fact]
    public void ReturnsFreshIndependentEmptyMapCollections()
    {
        SaveSnapshot source = JournalVersion();

        MapSaveState first = SaveSnapshotMapMigration.Migrate(source).Map!;
        MapSaveState second = SaveSnapshotMapMigration.Migrate(source).Map!;

        Assert.NotSame(first, second);
        Assert.NotSame(first.ReportedLocationIds, second.ReportedLocationIds);
        Assert.NotSame(first.VerifiedLocationIds, second.VerifiedLocationIds);
        Assert.NotSame(first.Notes, second.Notes);
    }

    [Fact]
    public void RejectsWrongVersionAndNullWithoutChangingSource()
    {
        SaveSnapshot source = JournalVersion();
        SaveSnapshot current = SaveSnapshotMapMigration.Migrate(source);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotMapMigration.Migrate(new SaveSnapshot(
                SaveSchemaVersion.FirstSupported,
                source.Discovery,
                source.WorldEvents)));
        Assert.Throws<ArgumentException>(() => SaveSnapshotMapMigration.Migrate(current));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotMapMigration.Migrate(null!));
        Assert.Same(SaveSchemaVersion.Journal, source.SchemaVersion);
        Assert.Null(source.Map);
        Assert.NotNull(source.Journal);
    }

    private static SaveSnapshot JournalVersion()
    {
        DiscoverySaveState discovery = new(
            new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));
        FirstPatientSaveState firstPatient = new(
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        JournalSaveState journal = new(
            discovery.Perception.CharacterId,
            [],
            [],
            [],
            [],
            [],
            null);
        return new SaveSnapshot(
            SaveSchemaVersion.Journal,
            discovery,
            new WorldEventSaveState(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            firstPatient,
            journal);
    }
}
