using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Persistence;

/// <summary>Composes the versioned immutable root state required for later save restoration.</summary>
public sealed record SaveSnapshot
{
    private static readonly CharacterMemorySnapshot EmptyCharacterMemory = new([]);

    /// <summary>Creates a save root from an exact version and existing state compositions.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents)
        : this(
            schemaVersion,
            discovery,
            worldEvents,
            EmptyCharacterMemory,
            firstPatient: null,
            journal: null,
            map: null,
            investigationRoutes: null,
            firstPatientRecovery: null)
    {
    }

    /// <summary>Creates a save root retaining an exact character-memory snapshot.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents,
        CharacterMemorySnapshot characterMemory)
        : this(
            schemaVersion,
            discovery,
            worldEvents,
            characterMemory,
            firstPatient: null,
            journal: null,
            map: null,
            investigationRoutes: null,
            firstPatientRecovery: null)
    {
    }

    /// <summary>Creates a save root retaining exact character-memory and first-patient state.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents,
        CharacterMemorySnapshot characterMemory,
        FirstPatientSaveState? firstPatient)
        : this(
            schemaVersion,
            discovery,
            worldEvents,
            characterMemory,
            firstPatient,
            journal: null,
            map: null,
            investigationRoutes: null,
            firstPatientRecovery: null)
    {
    }

    /// <summary>Creates a save root retaining exact first-patient and journal state.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents,
        CharacterMemorySnapshot characterMemory,
        FirstPatientSaveState? firstPatient,
        JournalSaveState? journal)
        : this(
            schemaVersion,
            discovery,
            worldEvents,
            characterMemory,
            firstPatient,
            journal,
            map: null,
            investigationRoutes: null,
            firstPatientRecovery: null)
    {
    }

    /// <summary>Creates a save root retaining exact journal and visibility-safe map state.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents,
        CharacterMemorySnapshot characterMemory,
        FirstPatientSaveState? firstPatient,
        JournalSaveState? journal,
        MapSaveState? map)
        : this(
            schemaVersion,
            discovery,
            worldEvents,
            characterMemory,
            firstPatient,
            journal,
            map,
            investigationRoutes: null,
            firstPatientRecovery: null)
    {
    }

    /// <summary>
    /// Creates a save root retaining exact map, ID-only route lifecycle, first-patient recovery,
    /// continuation and inventory state, each only on the schemas that require it.
    /// </summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents,
        CharacterMemorySnapshot characterMemory,
        FirstPatientSaveState? firstPatient,
        JournalSaveState? journal,
        MapSaveState? map,
        InvestigationRouteSaveState? investigationRoutes,
        FirstPatientRecoveryStage? firstPatientRecovery = null,
        SaveContinuationState? continuation = null,
        InventoryOwnershipSnapshot? inventory = null)
    {
        ArgumentNullException.ThrowIfNull(schemaVersion);
        ArgumentNullException.ThrowIfNull(discovery);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(characterMemory);

        bool retainsFirstPatient = schemaVersion == SaveSchemaVersion.FirstPatient
            || schemaVersion == SaveSchemaVersion.DialogueDelivery
            || schemaVersion == SaveSchemaVersion.Journal
            || schemaVersion == SaveSchemaVersion.Map
            || schemaVersion == SaveSchemaVersion.InvestigationRoutes
            || schemaVersion == SaveSchemaVersion.FirstPatientRecovery
            || schemaVersion == SaveSchemaVersion.Continuation
            || schemaVersion == SaveSchemaVersion.Inventory
            || schemaVersion == SaveSchemaVersion.OptionalMap;
        if (retainsFirstPatient && firstPatient is null)
        {
            throw new ArgumentNullException(nameof(firstPatient));
        }

        if (!retainsFirstPatient && firstPatient is not null)
        {
            throw new ArgumentException(
                "Only a first-patient-capable save schema can retain first-patient state.",
                nameof(firstPatient));
        }

        if (schemaVersion == SaveSchemaVersion.FirstSupported
            && characterMemory.Characters.Count > 0)
        {
            throw new ArgumentException(
                "The first save schema cannot retain character memory.",
                nameof(characterMemory));
        }

        bool retainsJournal = schemaVersion == SaveSchemaVersion.Journal
            || schemaVersion == SaveSchemaVersion.Map
            || schemaVersion == SaveSchemaVersion.InvestigationRoutes
            || schemaVersion == SaveSchemaVersion.FirstPatientRecovery
            || schemaVersion == SaveSchemaVersion.Continuation
            || schemaVersion == SaveSchemaVersion.Inventory
            || schemaVersion == SaveSchemaVersion.OptionalMap;
        if (retainsJournal && journal is null)
        {
            throw new ArgumentNullException(nameof(journal));
        }

        if (!retainsJournal && journal is not null)
        {
            throw new ArgumentException(
                "Only the journal save schema can retain journal state.",
                nameof(journal));
        }

        if (journal is not null
            && journal.SeekerId != discovery.Perception.CharacterId)
        {
            throw new ArgumentException(
                "Journal state must belong to the saved discovery character.",
                nameof(journal));
        }

        bool retainsMap = schemaVersion == SaveSchemaVersion.Map
            || schemaVersion == SaveSchemaVersion.InvestigationRoutes
            || schemaVersion == SaveSchemaVersion.FirstPatientRecovery
            || schemaVersion == SaveSchemaVersion.Continuation
            || schemaVersion == SaveSchemaVersion.Inventory
            || schemaVersion == SaveSchemaVersion.OptionalMap;
        // Schema 11 omits both the map and the routes until the map is first opened.
        bool mapIsOptional = schemaVersion == SaveSchemaVersion.OptionalMap;
        if (mapIsOptional && (map is null) != (investigationRoutes is null))
        {
            throw new ArgumentException(
                "An optional-map save retains the map and the investigation routes together or neither.",
                nameof(investigationRoutes));
        }

        if (retainsMap && map is null && !mapIsOptional)
        {
            throw new ArgumentNullException(nameof(map));
        }

        if (!retainsMap && map is not null)
        {
            throw new ArgumentException(
                "Only the map save schema can retain map state.",
                nameof(map));
        }

        if (map is not null
            && (map.SeekerId != discovery.Perception.CharacterId
                || map.SeekerId != journal!.SeekerId))
        {
            throw new ArgumentException(
                "Map state must belong to the saved discovery character and journal owner.",
                nameof(map));
        }

        bool retainsRoutes = schemaVersion == SaveSchemaVersion.InvestigationRoutes
            || schemaVersion == SaveSchemaVersion.FirstPatientRecovery
            || schemaVersion == SaveSchemaVersion.Continuation
            || schemaVersion == SaveSchemaVersion.Inventory
            || schemaVersion == SaveSchemaVersion.OptionalMap;
        if (retainsRoutes
            && investigationRoutes is null
            && !mapIsOptional)
        {
            throw new ArgumentNullException(nameof(investigationRoutes));
        }

        if (!retainsRoutes
            && investigationRoutes is not null)
        {
            throw new ArgumentException(
                "Only the investigation-route schema can retain route state.",
                nameof(investigationRoutes));
        }

        bool retainsRecovery = schemaVersion == SaveSchemaVersion.FirstPatientRecovery
            || schemaVersion == SaveSchemaVersion.Continuation
            || schemaVersion == SaveSchemaVersion.Inventory
            || schemaVersion == SaveSchemaVersion.OptionalMap;
        if (retainsRecovery && firstPatientRecovery is null)
        {
            throw new ArgumentNullException(nameof(firstPatientRecovery));
        }

        if (!retainsRecovery && firstPatientRecovery is not null)
        {
            throw new ArgumentException(
                "Only the first-patient-recovery, continuation and inventory schemas can retain recovery progress.",
                nameof(firstPatientRecovery));
        }

        // A continuation or inventory schema root without continuation state restores no profile
        // or positions; rejected-restore checks copy a captured root this way.
        if (schemaVersion != SaveSchemaVersion.Continuation
            && schemaVersion != SaveSchemaVersion.Inventory
            && schemaVersion != SaveSchemaVersion.OptionalMap
            && continuation is not null)
        {
            throw new ArgumentException(
                "Only the continuation and inventory schemas can retain the Seeker profile and positions.",
                nameof(continuation));
        }

        // An inventory schema root without inventory restores the authored starting inventory plus
        // any gathered herb its history still holds, as a migrated schema 9 save does.
        if (schemaVersion != SaveSchemaVersion.Inventory
            && schemaVersion != SaveSchemaVersion.OptionalMap
            && inventory is not null)
        {
            throw new ArgumentException(
                "Only the inventory schema can retain inventory ownership.",
                nameof(inventory));
        }

        SchemaVersion = schemaVersion;
        Discovery = discovery;
        WorldEvents = worldEvents;
        CharacterMemory = characterMemory;
        FirstPatient = firstPatient;
        Journal = journal;
        Map = map;
        InvestigationRoutes = investigationRoutes;
        FirstPatientRecovery = firstPatientRecovery;
        Continuation = continuation;
        Inventory = inventory;
    }

    /// <summary>Gets the exact retained root schema-version discriminator.</summary>
    public SaveSchemaVersion SchemaVersion { get; }

    /// <summary>Gets the exact retained discovery save state.</summary>
    public DiscoverySaveState Discovery { get; }

    /// <summary>Gets the exact retained world-event save state.</summary>
    public WorldEventSaveState WorldEvents { get; }

    /// <summary>Gets the exact retained canonical all-character memory snapshot.</summary>
    public CharacterMemorySnapshot CharacterMemory { get; }

    /// <summary>Gets exact first-patient state when retained by the schema version.</summary>
    public FirstPatientSaveState? FirstPatient { get; }

    /// <summary>Gets exact content-safe journal state when retained by the schema version.</summary>
    public JournalSaveState? Journal { get; }

    /// <summary>
    /// Gets exact visibility-safe map state when retained by the schema version; at the optional-map
    /// version it is null, with <see cref="InvestigationRoutes"/>, until the map is first opened.
    /// </summary>
    public MapSaveState? Map { get; }

    /// <summary>Gets exact ID-only investigation-route lifecycle state when retained; null exactly when <see cref="Map"/> is.</summary>
    public InvestigationRouteSaveState? InvestigationRoutes { get; }

    /// <summary>Gets durable first-patient recovery dialogue progress when retained.</summary>
    public FirstPatientRecoveryStage? FirstPatientRecovery { get; }

    /// <summary>Gets the saved Seeker profile and body positions when retained.</summary>
    public SaveContinuationState? Continuation { get; }

    /// <summary>Gets who carries which world objects when retained.</summary>
    public InventoryOwnershipSnapshot? Inventory { get; }
}
