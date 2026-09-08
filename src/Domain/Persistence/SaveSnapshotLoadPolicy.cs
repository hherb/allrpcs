namespace Allrpcs.Domain.Persistence;

/// <summary>Decides whether a save root has a schema version this build can load.</summary>
public static class SaveSnapshotLoadPolicy
{
    /// <summary>Returns whether the save root's schema version has a supported load contract.</summary>
    public static bool CanLoad(SaveSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return SaveSchemaVersionSupportPolicy.CanLoad(snapshot.SchemaVersion);
    }
}
