namespace Allrpcs.Domain.Persistence;

/// <summary>
/// Decides whether this build can load a structurally valid save-schema version.
/// </summary>
public static class SaveSchemaVersionSupportPolicy
{
    /// <summary>Returns whether the supplied version has a supported load contract.</summary>
    public static bool CanLoad(SaveSchemaVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        return version == SaveSchemaVersion.FirstSupported
            || version == SaveSchemaVersion.CharacterMemory
            || version == SaveSchemaVersion.FirstPatient
            || version == SaveSchemaVersion.DialogueDelivery
            || version == SaveSchemaVersion.Journal
            || version == SaveSchemaVersion.Map
            || version == SaveSchemaVersion.InvestigationRoutes
            || version == SaveSchemaVersion.FirstPatientRecovery
            || version == SaveSchemaVersion.Continuation;
    }
}
