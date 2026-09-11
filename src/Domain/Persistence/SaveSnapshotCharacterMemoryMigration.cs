namespace Allrpcs.Domain.Persistence;

/// <summary>Migrates the first save root to the character-memory schema.</summary>
public static class SaveSnapshotCharacterMemoryMigration
{
    /// <summary>
    /// Creates a character-memory-version root retaining the first-version root's exact state.
    /// </summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.SchemaVersion != SaveSchemaVersion.FirstSupported)
        {
            throw new ArgumentException(
                "Only the first save schema can migrate to the character-memory schema.",
                nameof(source));
        }

        return new SaveSnapshot(
            SaveSchemaVersion.CharacterMemory,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory);
    }
}
