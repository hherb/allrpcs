namespace Allrpcs.Domain.Persistence;

/// <summary>Moves an inventory-version save to the optional-map schema.</summary>
public static class SaveSnapshotOptionalMapMigration
{
    /// <summary>Creates a current root with the same state; an inventory save always has an opened map.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.SchemaVersion != SaveSchemaVersion.Inventory)
        {
            throw new ArgumentException(
                "Only the inventory schema can migrate to optional map.",
                nameof(source));
        }

        return new SaveSnapshot(
            SaveSchemaVersion.OptionalMap,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            source.Map,
            source.InvestigationRoutes,
            source.FirstPatientRecovery,
            source.Continuation,
            source.Inventory);
    }
}
