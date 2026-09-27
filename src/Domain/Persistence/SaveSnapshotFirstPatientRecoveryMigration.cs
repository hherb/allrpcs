namespace Allrpcs.Domain.Persistence;

/// <summary>Adds inert first-patient recovery dialogue progress to a route-version save.</summary>
public static class SaveSnapshotFirstPatientRecoveryMigration
{
    /// <summary>Creates a current root whose recovery dialogue has not been heard.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.SchemaVersion != SaveSchemaVersion.InvestigationRoutes)
        {
            throw new ArgumentException(
                "Only the investigation-route schema can migrate to recovery progress.",
                nameof(source));
        }

        return new SaveSnapshot(
            SaveSchemaVersion.FirstPatientRecovery,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            source.Map,
            source.InvestigationRoutes,
            FirstPatientRecoveryStage.NotHeard);
    }
}
