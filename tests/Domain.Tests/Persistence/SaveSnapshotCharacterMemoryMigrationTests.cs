using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotCharacterMemoryMigrationTests
{
    [Fact]
    public void MigratesFirstVersionToCharacterMemoryVersionWithExactExistingState()
    {
        SaveSnapshot source = FirstVersion();
        CharacterMemorySnapshot sourceMemory = source.CharacterMemory;

        SaveSnapshot result = SaveSnapshotCharacterMemoryMigration.Migrate(source);

        Assert.NotSame(source, result);
        Assert.Same(SaveSchemaVersion.CharacterMemory, result.SchemaVersion);
        Assert.Same(source.Discovery, result.Discovery);
        Assert.Same(source.WorldEvents, result.WorldEvents);
        Assert.Same(sourceMemory, result.CharacterMemory);
        Assert.Empty(result.CharacterMemory.Characters);
        Assert.Same(SaveSchemaVersion.FirstSupported, source.SchemaVersion);
    }

    [Fact]
    public void RejectsEveryOtherVersionAndNullWithoutChangingInput()
    {
        SaveSnapshot versionTwo = new(
            SaveSchemaVersion.CharacterMemory,
            FirstVersion().Discovery,
            FirstVersion().WorldEvents,
            new CharacterMemorySnapshot([]));
        SaveSnapshot future = new(
            new SaveSchemaVersion(11),
            FirstVersion().Discovery,
            FirstVersion().WorldEvents,
            new CharacterMemorySnapshot([]));

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotCharacterMemoryMigration.Migrate(versionTwo));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotCharacterMemoryMigration.Migrate(future));
        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotCharacterMemoryMigration.Migrate(null!));
        Assert.Same(SaveSchemaVersion.CharacterMemory, versionTwo.SchemaVersion);
        Assert.Equal(11, future.SchemaVersion.Value);
    }

    [Fact]
    public void ExposesOnlyTheStaticMigrationOperation()
    {
        Type migrationType = typeof(SaveSnapshotCharacterMemoryMigration);
        System.Reflection.MethodInfo method = Assert.Single(
            migrationType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(SaveSnapshotCharacterMemoryMigration.Migrate), method.Name);
        Assert.Equal(typeof(SaveSnapshot), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(SaveSnapshot), parameter.ParameterType));
        Assert.Empty(migrationType.GetProperties());
        Assert.Empty(migrationType.GetEvents());
    }

    private static SaveSnapshot FirstVersion() => new(
        SaveSchemaVersion.FirstSupported,
        new DiscoverySaveState(
            new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([])),
        new WorldEventSaveState(
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null)));
}
