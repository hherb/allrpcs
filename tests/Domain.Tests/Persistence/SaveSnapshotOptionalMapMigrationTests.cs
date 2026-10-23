using Allrpcs.Domain.Persistence;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotOptionalMapMigrationTests
{
    [Fact]
    public void MigratesAnInventorySaveUnchangedWithItsOpenedMap()
    {
        SaveSnapshot source = InventoryVersion();

        SaveSnapshot result = SaveSnapshotOptionalMapMigration.Migrate(source);

        Assert.Same(SaveSchemaVersion.OptionalMap, result.SchemaVersion);
        Assert.Same(source.Discovery, result.Discovery);
        Assert.Same(source.WorldEvents, result.WorldEvents);
        Assert.Same(source.CharacterMemory, result.CharacterMemory);
        Assert.Same(source.FirstPatient, result.FirstPatient);
        Assert.Same(source.Journal, result.Journal);
        Assert.Same(source.Map, result.Map);
        Assert.Same(source.InvestigationRoutes, result.InvestigationRoutes);
        Assert.Same(source.FirstPatientRecovery, result.FirstPatientRecovery);
        Assert.Same(source.Continuation, result.Continuation);
        Assert.Same(source.Inventory, result.Inventory);
    }

    [Fact]
    public void RejectsEveryOtherVersionAndNull()
    {
        SaveSnapshot current = SaveSnapshotOptionalMapMigration.Migrate(InventoryVersion());

        Assert.Throws<ArgumentException>(() => SaveSnapshotOptionalMapMigration.Migrate(current));
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotOptionalMapMigration.Migrate(null!));
    }

    [Fact]
    public void OnlyTheOptionalMapVersionMayOmitTheMapAndRoutesAndOnlyTogether()
    {
        SaveSnapshot opened = SaveSnapshotOptionalMapMigration.Migrate(InventoryVersion());

        SaveSnapshot unopened = With(SaveSchemaVersion.OptionalMap, opened, map: null, routes: null);

        Assert.Null(unopened.Map);
        Assert.Null(unopened.InvestigationRoutes);
        Assert.Throws<ArgumentNullException>(() =>
            With(SaveSchemaVersion.Inventory, opened, map: null, routes: null));
        Assert.Throws<ArgumentException>(() =>
            With(SaveSchemaVersion.OptionalMap, opened, map: opened.Map, routes: null));
        Assert.Throws<ArgumentException>(() =>
            With(SaveSchemaVersion.OptionalMap, opened, map: null, routes: opened.InvestigationRoutes));
    }

    internal static SaveSnapshot InventoryVersion()
    {
        SaveSnapshot routes = SaveSnapshotFirstPatientRecoveryMigrationTests.RouteVersion();
        return SaveSnapshotInventoryMigration.Migrate(
            SaveSnapshotContinuationMigration.Migrate(
                SaveSnapshotFirstPatientRecoveryMigration.Migrate(routes)));
    }

    private static SaveSnapshot With(
        SaveSchemaVersion version,
        SaveSnapshot source,
        MapSaveState? map,
        InvestigationRouteSaveState? routes) => new(
            version,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            map,
            routes,
            source.FirstPatientRecovery,
            source.Continuation,
            source.Inventory);
}
