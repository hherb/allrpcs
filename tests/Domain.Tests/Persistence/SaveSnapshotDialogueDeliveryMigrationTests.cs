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

public sealed class SaveSnapshotDialogueDeliveryMigrationTests
{
    [Fact]
    public void MigratesFirstPatientVersionWithEveryExactExistingComposition()
    {
        SaveSnapshot source = FirstPatientVersion();

        SaveSnapshot result = SaveSnapshotDialogueDeliveryMigration.Migrate(source);

        Assert.NotSame(source, result);
        Assert.Same(SaveSchemaVersion.DialogueDelivery, result.SchemaVersion);
        Assert.Same(source.Discovery, result.Discovery);
        Assert.Same(source.WorldEvents, result.WorldEvents);
        Assert.Same(source.CharacterMemory, result.CharacterMemory);
        Assert.Same(source.FirstPatient, result.FirstPatient);
        Assert.Same(SaveSchemaVersion.FirstPatient, source.SchemaVersion);
    }

    [Fact]
    public void RejectsEveryOtherVersionAndNullWithoutChangingInputs()
    {
        SaveSnapshot source = FirstPatientVersion();
        SaveSnapshot current = new(
            SaveSchemaVersion.DialogueDelivery,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotDialogueDeliveryMigration.Migrate(new SaveSnapshot(
                SaveSchemaVersion.FirstSupported,
                source.Discovery,
                source.WorldEvents)));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotDialogueDeliveryMigration.Migrate(current));
        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotDialogueDeliveryMigration.Migrate(null!));
        Assert.Same(SaveSchemaVersion.DialogueDelivery, current.SchemaVersion);
        Assert.Same(source.FirstPatient, current.FirstPatient);
    }

    [Fact]
    public void ExposesOnlyTheStaticMigrationOperation()
    {
        Type migrationType = typeof(SaveSnapshotDialogueDeliveryMigration);
        System.Reflection.MethodInfo method = Assert.Single(
            migrationType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(SaveSnapshotDialogueDeliveryMigration.Migrate), method.Name);
        Assert.Equal(typeof(SaveSnapshot), method.ReturnType);
        Assert.Equal(
            [typeof(SaveSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(migrationType.GetProperties());
        Assert.Empty(migrationType.GetEvents());
    }

    private static SaveSnapshot FirstPatientVersion()
    {
        FirstPatientSaveState firstPatient = new(
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        return new SaveSnapshot(
            SaveSchemaVersion.FirstPatient,
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
