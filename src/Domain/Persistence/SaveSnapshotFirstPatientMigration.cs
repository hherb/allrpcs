namespace Allrpcs.Domain.Persistence;

/// <summary>Migrates the character-memory save root to the first-patient schema.</summary>
public static class SaveSnapshotFirstPatientMigration
{
    /// <summary>Creates a first-patient-version root retaining exact supplied state.</summary>
    public static SaveSnapshot Migrate(
        SaveSnapshot source,
        FirstPatientSaveState firstPatient)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(firstPatient);

        if (source.SchemaVersion != SaveSchemaVersion.CharacterMemory)
        {
            throw new ArgumentException(
                "Only the character-memory schema can migrate to the first-patient schema.",
                nameof(source));
        }

        return new SaveSnapshot(
            SaveSchemaVersion.FirstPatient,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            firstPatient);
    }
}
