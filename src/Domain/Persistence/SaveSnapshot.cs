namespace Allrpcs.Domain.Persistence;

/// <summary>Composes the versioned immutable root state required for later save restoration.</summary>
public sealed record SaveSnapshot
{
    /// <summary>Creates a save root from an exact version and existing state compositions.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents)
    {
        ArgumentNullException.ThrowIfNull(schemaVersion);
        ArgumentNullException.ThrowIfNull(discovery);
        ArgumentNullException.ThrowIfNull(worldEvents);

        SchemaVersion = schemaVersion;
        Discovery = discovery;
        WorldEvents = worldEvents;
    }

    /// <summary>Gets the exact retained root schema-version discriminator.</summary>
    public SaveSchemaVersion SchemaVersion { get; }

    /// <summary>Gets the exact retained discovery save state.</summary>
    public DiscoverySaveState Discovery { get; }

    /// <summary>Gets the exact retained world-event save state.</summary>
    public WorldEventSaveState WorldEvents { get; }
}
