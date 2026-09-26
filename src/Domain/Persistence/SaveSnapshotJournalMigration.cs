namespace Allrpcs.Domain.Persistence;

/// <summary>Migrates the dialogue-delivery save root to the safe-journal schema.</summary>
public static class SaveSnapshotJournalMigration
{
    /// <summary>Creates a journal-version root with fresh empty journal state.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.SchemaVersion != SaveSchemaVersion.DialogueDelivery)
        {
            throw new ArgumentException(
                "Only the dialogue-delivery schema can migrate to the journal schema.",
                nameof(source));
        }

        JournalSaveState journal = new(
            source.Discovery.Perception.CharacterId,
            [],
            [],
            [],
            [],
            [],
            selectedSectionKey: null);
        return new SaveSnapshot(
            SaveSchemaVersion.Journal,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            journal);
    }
}
