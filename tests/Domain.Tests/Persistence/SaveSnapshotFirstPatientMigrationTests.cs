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

public sealed class SaveSnapshotFirstPatientMigrationTests
{
    [Fact]
    public void MigratesCharacterMemoryVersionWithExactExistingAndAuthoredState()
    {
        SaveSnapshot source = CharacterMemoryVersion();
        FirstPatientSaveState firstPatient = FirstPatientState();

        SaveSnapshot result = SaveSnapshotFirstPatientMigration.Migrate(source, firstPatient);

        Assert.NotSame(source, result);
        Assert.Same(SaveSchemaVersion.FirstPatient, result.SchemaVersion);
        Assert.Same(source.Discovery, result.Discovery);
        Assert.Same(source.WorldEvents, result.WorldEvents);
        Assert.Same(source.CharacterMemory, result.CharacterMemory);
        Assert.Same(firstPatient, result.FirstPatient);
        Assert.Same(SaveSchemaVersion.CharacterMemory, source.SchemaVersion);
        Assert.Null(source.FirstPatient);
    }

    [Fact]
    public void RejectsEveryOtherVersionAndNullWithoutChangingInputs()
    {
        SaveSnapshot versionOne = new(
            SaveSchemaVersion.FirstSupported,
            CharacterMemoryVersion().Discovery,
            CharacterMemoryVersion().WorldEvents);
        FirstPatientSaveState firstPatient = FirstPatientState();
        SaveSnapshot versionThree = new(
            SaveSchemaVersion.FirstPatient,
            versionOne.Discovery,
            versionOne.WorldEvents,
            new CharacterMemorySnapshot([]),
            firstPatient);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotFirstPatientMigration.Migrate(versionOne, firstPatient));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotFirstPatientMigration.Migrate(versionThree, firstPatient));
        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotFirstPatientMigration.Migrate(null!, firstPatient));
        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotFirstPatientMigration.Migrate(CharacterMemoryVersion(), null!));
        Assert.Same(SaveSchemaVersion.FirstSupported, versionOne.SchemaVersion);
        Assert.Same(SaveSchemaVersion.FirstPatient, versionThree.SchemaVersion);
        Assert.Same(firstPatient, versionThree.FirstPatient);
    }

    [Fact]
    public void ExposesOnlyTheStaticMigrationOperation()
    {
        Type migrationType = typeof(SaveSnapshotFirstPatientMigration);
        System.Reflection.MethodInfo method = Assert.Single(
            migrationType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(SaveSnapshotFirstPatientMigration.Migrate), method.Name);
        Assert.Equal(typeof(SaveSnapshot), method.ReturnType);
        Assert.Equal(
            [typeof(SaveSnapshot), typeof(FirstPatientSaveState)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(migrationType.GetProperties());
        Assert.Empty(migrationType.GetEvents());
    }

    private static SaveSnapshot CharacterMemoryVersion() => new(
        SaveSchemaVersion.CharacterMemory,
        new DiscoverySaveState(
            new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([])),
        new WorldEventSaveState(
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null)),
        new CharacterMemorySnapshot([]));

    private static FirstPatientSaveState FirstPatientState() => new(
        RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));
}
