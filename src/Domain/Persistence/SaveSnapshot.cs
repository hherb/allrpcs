using Allrpcs.Domain.Characters;

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
            map: null)
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
            map: null)
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
            map: null)
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
            map: null)
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
    {
        ArgumentNullException.ThrowIfNull(schemaVersion);
        ArgumentNullException.ThrowIfNull(discovery);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(characterMemory);

        bool retainsFirstPatient = schemaVersion == SaveSchemaVersion.FirstPatient
            || schemaVersion == SaveSchemaVersion.DialogueDelivery
            || schemaVersion == SaveSchemaVersion.Journal
            || schemaVersion == SaveSchemaVersion.Map;
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
            || schemaVersion == SaveSchemaVersion.Map;
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

        if (schemaVersion == SaveSchemaVersion.Map && map is null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        if (schemaVersion != SaveSchemaVersion.Map && map is not null)
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

        SchemaVersion = schemaVersion;
        Discovery = discovery;
        WorldEvents = worldEvents;
        CharacterMemory = characterMemory;
        FirstPatient = firstPatient;
        Journal = journal;
        Map = map;
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

    /// <summary>Gets exact visibility-safe map state when retained by the schema version.</summary>
    public MapSaveState? Map { get; }
}
