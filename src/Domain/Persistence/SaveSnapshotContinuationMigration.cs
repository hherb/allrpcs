namespace Allrpcs.Domain.Persistence;

/// <summary>Adds an empty continuation to a recovery-version save.</summary>
public static class SaveSnapshotContinuationMigration
{
    /// <summary>Creates a current root that restores no Seeker profile or positions.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.SchemaVersion != SaveSchemaVersion.FirstPatientRecovery)
        {
            throw new ArgumentException(
                "Only the first-patient-recovery schema can migrate to continuation.",
                nameof(source));
        }

        return new SaveSnapshot(
            SaveSchemaVersion.Continuation,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            source.Map,
            source.InvestigationRoutes,
            source.FirstPatientRecovery,
            new SaveContinuationState(null, []));
    }
}
