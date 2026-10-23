using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class OptionalMapSaveJsonPersistenceTests
{
    [Fact]
    public void RoundTripsASaveTakenBeforeTheMapWasOpened()
    {
        string serialized = SaveSnapshotJsonSerializer.Serialize(OptionalMapVersion(opened: false));
        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.StartsWith("{\"schemaVersion\":11,", serialized, StringComparison.Ordinal);
        Assert.Contains(",\"map\":null,\"investigationRoutes\":null,", serialized, StringComparison.Ordinal);
        Assert.Same(SaveSchemaVersion.OptionalMap, restored.SchemaVersion);
        Assert.Null(restored.Map);
        Assert.Null(restored.InvestigationRoutes);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(restored));
    }

    [Fact]
    public void RoundTripsASaveWithAnOpenedMapAsBefore()
    {
        SaveSnapshot source = OptionalMapVersion(opened: true);

        string serialized = SaveSnapshotJsonSerializer.Serialize(source);
        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.Equal(
            SaveSnapshotJsonSerializer.Serialize(Inventory()).Replace("{\"schemaVersion\":10,", "{\"schemaVersion\":11,", StringComparison.Ordinal),
            serialized);
        Assert.NotNull(restored.Map);
        Assert.NotNull(restored.InvestigationRoutes);
    }

    [Fact]
    public void AnInventorySaveLoadsAsTheCurrentVersionWithItsMap()
    {
        SaveSnapshot current = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            SaveSnapshotJsonSerializer.Serialize(Inventory()),
            Inventory().FirstPatient!);

        Assert.Same(SaveSchemaVersion.OptionalMap, current.SchemaVersion);
        Assert.NotNull(current.Map);
        Assert.NotNull(current.InvestigationRoutes);
    }

    [Theory]
    [InlineData("\"investigationRoutes\":null", "\"investigationRoutes\":{}")]
    [InlineData(",\"map\":null", "")]
    [InlineData("\"map\":null", "\"map\":{}")]
    public void RejectsAMalformedOrHalfOmittedMap(string original, string replacement)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(OptionalMapVersion(opened: false));
        string malformed = valid.Replace(original, replacement, StringComparison.Ordinal);

        Assert.NotEqual(valid, malformed);
        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    [Fact]
    public void AnInventorySaveCannotOmitTheMap()
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(Inventory());
        string start = valid[..valid.IndexOf(",\"map\":", StringComparison.Ordinal)];
        string end = valid[valid.IndexOf(",\"firstPatientRecoveryStage\":", StringComparison.Ordinal)..];

        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(
            start + ",\"map\":null,\"investigationRoutes\":null" + end));
    }

    private static SaveSnapshot Inventory()
    {
        SaveSnapshot recovery = FirstPatientRecoverySaveStateJsonPersistenceTests.RecoveryVersionState(
            FirstPatientRecoveryStage.NotHeard);
        return SaveSnapshotInventoryMigration.Migrate(SaveSnapshotContinuationMigration.Migrate(recovery));
    }

    private static SaveSnapshot OptionalMapVersion(bool opened)
    {
        SaveSnapshot source = Inventory();
        return new SaveSnapshot(
            SaveSchemaVersion.OptionalMap,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            opened ? source.Map : null,
            opened ? source.InvestigationRoutes : null,
            source.FirstPatientRecovery,
            source.Continuation,
            source.Inventory);
    }
}
