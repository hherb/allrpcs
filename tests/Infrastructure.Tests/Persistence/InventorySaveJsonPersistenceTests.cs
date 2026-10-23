using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class InventorySaveJsonPersistenceTests
{
    [Fact]
    public void RoundTripsWhoCarriesWhichObjectInOrder()
    {
        SaveSnapshot source = InventoryVersion(new InventoryOwnershipSnapshot(
            [
                new InventoryOwnership(new ObjectId("moonwort-leaves"), new CharacterId("healer")),
                new InventoryOwnership(new ObjectId("field-lens"), new CharacterId("seeker")),
            ]));

        string serialized = SaveSnapshotJsonSerializer.Serialize(source);
        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.StartsWith("{\"schemaVersion\":10,", serialized, StringComparison.Ordinal);
        Assert.EndsWith(
            "\"inventory\":[{\"objectId\":\"moonwort-leaves\",\"ownerCharacterId\":\"healer\"},"
            + "{\"objectId\":\"field-lens\",\"ownerCharacterId\":\"seeker\"}]}",
            serialized,
            StringComparison.Ordinal);
        Assert.Same(SaveSchemaVersion.Inventory, restored.SchemaVersion);
        Assert.Equal(source.Inventory!.Entries, restored.Inventory!.Entries);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(restored));
    }

    [Fact]
    public void ANullInventoryMeansTheAuthoredStartAndAContinuationSaveMigratesToIt()
    {
        string serialized = SaveSnapshotJsonSerializer.Serialize(InventoryVersion(null));
        SaveSnapshot recovery = FirstPatientRecoverySaveStateJsonPersistenceTests.RecoveryVersionState(
            FirstPatientRecoveryStage.NotHeard);
        string versionNine = SaveSnapshotJsonSerializer.Serialize(
            SaveSnapshotContinuationMigration.Migrate(recovery));

        SaveSnapshot current = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            versionNine,
            recovery.FirstPatient!);

        Assert.EndsWith(",\"inventory\":null}", serialized, StringComparison.Ordinal);
        Assert.Null(SaveSnapshotJsonDeserializer.Deserialize(serialized).Inventory);
        Assert.Same(SaveSchemaVersion.OptionalMap, current.SchemaVersion);
        Assert.Null(current.Inventory);
        Assert.NotNull(current.Continuation);
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            SaveSchemaVersion.Continuation,
            recovery.Discovery,
            recovery.WorldEvents,
            recovery.CharacterMemory,
            recovery.FirstPatient,
            recovery.Journal,
            recovery.Map,
            recovery.InvestigationRoutes,
            recovery.FirstPatientRecovery,
            null,
            new InventoryOwnershipSnapshot([])));
    }

    [Theory]
    [InlineData(",\"inventory\":[]", "")]
    [InlineData("\"inventory\":[]", "\"inventory\":{}")]
    [InlineData("\"inventory\":[]", "\"inventory\":[{\"objectId\":\"lens\"}]")]
    [InlineData("\"inventory\":[]", "\"inventory\":[{\"objectId\":\"lens\",\"ownerCharacterId\":\"seeker\",\"extra\":1}]")]
    [InlineData("\"inventory\":[]", "\"inventory\":[{\"objectId\":\"lens\",\"ownerCharacterId\":\"seeker\"},{\"objectId\":\"lens\",\"ownerCharacterId\":\"healer\"}]")]
    [InlineData("\"inventory\":[]", "\"inventory\":[{\"objectId\":\"Lens!\",\"ownerCharacterId\":\"seeker\"}]")]
    public void RejectsMalformedInventory(string original, string replacement)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(InventoryVersion(new InventoryOwnershipSnapshot([])));
        string malformed = valid.Replace(original, replacement, StringComparison.Ordinal);

        Assert.NotEqual(valid, malformed);
        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    private static SaveSnapshot InventoryVersion(InventoryOwnershipSnapshot? inventory)
    {
        SaveSnapshot recovery = FirstPatientRecoverySaveStateJsonPersistenceTests.RecoveryVersionState(
            FirstPatientRecoveryStage.NotHeard);
        return new SaveSnapshot(
            SaveSchemaVersion.Inventory,
            recovery.Discovery,
            recovery.WorldEvents,
            recovery.CharacterMemory,
            recovery.FirstPatient,
            recovery.Journal,
            recovery.Map,
            recovery.InvestigationRoutes,
            recovery.FirstPatientRecovery,
            new SaveContinuationState(null, []),
            inventory);
    }
}
