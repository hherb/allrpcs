namespace Allrpcs.Domain.Persistence;

/// <summary>Migrates the map save root to initialized empty investigation-route state.</summary>
public static class SaveSnapshotInvestigationRoutesMigration
{
    private static readonly string[] CanonicalRouteKeys =
    [
        "stream-laundry",
        "shrine",
        "road-wagon",
    ];

    /// <summary>Creates a route-version root with fresh empty canonical progress.</summary>
    public static SaveSnapshot Migrate(SaveSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.SchemaVersion != SaveSchemaVersion.Map)
        {
            throw new ArgumentException("Only the map schema can migrate to route state.", nameof(source));
        }

        InvestigationRouteSaveState routes = new(
            CanonicalRouteKeys.Select(key => new InvestigationRouteProgressSaveEntry(key, [], [])),
            [],
            null);
        return new SaveSnapshot(
            SaveSchemaVersion.InvestigationRoutes,
            source.Discovery,
            source.WorldEvents,
            source.CharacterMemory,
            source.FirstPatient,
            source.Journal,
            source.Map,
            routes);
    }
}
