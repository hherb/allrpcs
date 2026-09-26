namespace Allrpcs.Domain.Persistence;

/// <summary>Migrates the safe-journal save root to the visibility-safe map schema.</summary>
public static class SaveSnapshotMapMigration
{
    /// <summary>Creates a map-version root with fresh empty same-Seeker map state.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.SchemaVersion != SaveSchemaVersion.Journal)
        {
            throw new ArgumentException(
                "Only the journal schema can migrate to the map schema.",
                nameof(source));
        }

        MapSaveState map = new(
            source.Discovery.Perception.CharacterId,
            [],
            [],
            []);
        return new SaveSnapshot(
            SaveSchemaVersion.Map,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            map);
    }
}
