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

public sealed class SaveSnapshotJournalMigrationTests
{
    [Fact]
    public void MigratesDialogueDeliveryRootToFreshEmptyJournalState()
    {
        SaveSnapshot source = DialogueDeliveryVersion();

        SaveSnapshot migrated = SaveSnapshotJournalMigration.Migrate(source);

        Assert.NotSame(source, migrated);
        Assert.Same(SaveSchemaVersion.Journal, migrated.SchemaVersion);
        Assert.Same(source.Discovery, migrated.Discovery);
        Assert.Same(source.WorldEvents, migrated.WorldEvents);
        Assert.Same(source.CharacterMemory, migrated.CharacterMemory);
        Assert.Same(source.FirstPatient, migrated.FirstPatient);
        Assert.NotNull(migrated.Journal);
        Assert.Same(source.Discovery.Perception.CharacterId, migrated.Journal.SeekerId);
        Assert.Empty(migrated.Journal.Observations);
        Assert.Empty(migrated.Journal.Testimony);
        Assert.Empty(migrated.Journal.Hypotheses);
        Assert.Empty(migrated.Journal.Experiments);
        Assert.Empty(migrated.Journal.OpenQuestions);
        Assert.Null(migrated.Journal.SelectedSectionKey);
    }

    [Fact]
    public void ReturnsFreshIndependentEmptyJournalCollections()
    {
        SaveSnapshot source = DialogueDeliveryVersion();

        JournalSaveState first = SaveSnapshotJournalMigration.Migrate(source).Journal!;
        JournalSaveState second = SaveSnapshotJournalMigration.Migrate(source).Journal!;

        Assert.NotSame(first, second);
        Assert.NotSame(first.Observations, second.Observations);
        Assert.NotSame(first.Testimony, second.Testimony);
        Assert.NotSame(first.Hypotheses, second.Hypotheses);
        Assert.NotSame(first.Experiments, second.Experiments);
        Assert.NotSame(first.OpenQuestions, second.OpenQuestions);
    }

    [Fact]
    public void RejectsWrongVersionAndNullWithoutChangingSource()
    {
        SaveSnapshot source = DialogueDeliveryVersion();
        SaveSnapshot current = SaveSnapshotJournalMigration.Migrate(source);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJournalMigration.Migrate(new SaveSnapshot(
                SaveSchemaVersion.FirstSupported,
                source.Discovery,
                source.WorldEvents)));
        Assert.Throws<ArgumentException>(() => SaveSnapshotJournalMigration.Migrate(current));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotJournalMigration.Migrate(null!));
        Assert.Same(SaveSchemaVersion.DialogueDelivery, source.SchemaVersion);
        Assert.Null(source.Journal);
    }

    private static SaveSnapshot DialogueDeliveryVersion()
    {
        FirstPatientSaveState firstPatient = new(
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        return new SaveSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
                new FocusedSearchActivitySnapshot([]),
                new FocusedSearchOutcomeSnapshot([]),
                new PassiveNoticingCandidateSnapshot([]),
                new ObservationReinterpretationCandidateSnapshot([])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            firstPatient);
    }
}
