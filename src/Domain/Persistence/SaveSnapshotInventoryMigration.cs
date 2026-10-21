namespace Allrpcs.Domain.Persistence;

/// <summary>Moves a continuation-version save to the inventory schema.</summary>
public static class SaveSnapshotInventoryMigration
{
    /// <summary>Creates a current root without inventory: it restores the authored starting inventory.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.SchemaVersion != SaveSchemaVersion.Continuation)
        {
            throw new ArgumentException(
                "Only the continuation schema can migrate to inventory.",
                nameof(source));
        }

        return new SaveSnapshot(
            SaveSchemaVersion.Inventory,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            source.Map,
            source.InvestigationRoutes,
            source.FirstPatientRecovery,
            source.Continuation,
            inventory: null);
    }
}
