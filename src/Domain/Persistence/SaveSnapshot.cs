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
        : this(schemaVersion, discovery, worldEvents, EmptyCharacterMemory, firstPatient: null)
    {
    }

    /// <summary>Creates a save root retaining an exact character-memory snapshot.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents,
        CharacterMemorySnapshot characterMemory)
        : this(schemaVersion, discovery, worldEvents, characterMemory, firstPatient: null)
    {
    }

    /// <summary>Creates a save root retaining exact character-memory and first-patient state.</summary>
    public SaveSnapshot(
        SaveSchemaVersion schemaVersion,
        DiscoverySaveState discovery,
        WorldEventSaveState worldEvents,
        CharacterMemorySnapshot characterMemory,
        FirstPatientSaveState? firstPatient)
    {
        ArgumentNullException.ThrowIfNull(schemaVersion);
        ArgumentNullException.ThrowIfNull(discovery);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(characterMemory);

        bool isFirstPatientVersion = schemaVersion == SaveSchemaVersion.FirstPatient;
        if (isFirstPatientVersion && firstPatient is null)
        {
            throw new ArgumentNullException(nameof(firstPatient));
        }

        if (!isFirstPatientVersion && firstPatient is not null)
        {
            throw new ArgumentException(
                "Only the first-patient save schema can retain first-patient state.",
                nameof(firstPatient));
        }

        if (schemaVersion == SaveSchemaVersion.FirstSupported
            && characterMemory.Characters.Count > 0)
        {
            throw new ArgumentException(
                "The first save schema cannot retain character memory.",
                nameof(characterMemory));
        }

        SchemaVersion = schemaVersion;
        Discovery = discovery;
        WorldEvents = worldEvents;
        CharacterMemory = characterMemory;
        FirstPatient = firstPatient;
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
}
