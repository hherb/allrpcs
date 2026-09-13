namespace Allrpcs.Domain.Persistence;

/// <summary>Migrates the first-patient save root to the dialogue-delivery schema.</summary>
public static class SaveSnapshotDialogueDeliveryMigration
{
    /// <summary>Creates a dialogue-delivery-version root retaining every exact composition.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.SchemaVersion != SaveSchemaVersion.FirstPatient)
        {
            throw new ArgumentException(
                "Only the first-patient schema can migrate to the dialogue-delivery schema.",
                nameof(source));
        }

        return new SaveSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient);
    }
}
