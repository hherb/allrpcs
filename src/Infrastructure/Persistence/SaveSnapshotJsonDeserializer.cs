using System.Globalization;
using System.Text.Json;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly materializes the supported immutable save snapshot JSON schema.</summary>
public static class SaveSnapshotJsonDeserializer
{
    private const int MaximumJsonDepth = 16;

    /// <summary>
    /// Materializes one complete known snapshot, rejecting malformed or unexpected structure.
    /// </summary>
    public static SaveSnapshot Deserialize(string serialized)
    {
        ArgumentNullException.ThrowIfNull(serialized);

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                serialized,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth,
                });
            return ReadSnapshot(document.RootElement);
        }
        catch (Exception exception) when (
            exception is JsonException
                or InvalidDataException
                or ArgumentException
                or InvalidOperationException
                or OverflowException)
        {
            throw new ArgumentException(
                "Value must be a complete supported save-snapshot JSON document.",
                nameof(serialized),
                exception);
        }
    }

    /// <summary>
    /// Materializes one known document and migrates older versions to current state.
    /// </summary>
    public static SaveSnapshot DeserializeCurrent(
        string serialized,
        FirstPatientSaveState initialFirstPatient)
    {
        ArgumentNullException.ThrowIfNull(initialFirstPatient);

        SaveSnapshot snapshot = Deserialize(serialized);
        if (snapshot.SchemaVersion == SaveSchemaVersion.FirstSupported)
        {
            snapshot = SaveSnapshotCharacterMemoryMigration.Migrate(snapshot);
        }

        if (snapshot.SchemaVersion == SaveSchemaVersion.CharacterMemory)
        {
            snapshot = SaveSnapshotFirstPatientMigration.Migrate(snapshot, initialFirstPatient);
        }

        if (snapshot.SchemaVersion == SaveSchemaVersion.FirstPatient)
        {
            snapshot = SaveSnapshotDialogueDeliveryMigration.Migrate(snapshot);
        }

        if (snapshot.SchemaVersion == SaveSchemaVersion.DialogueDelivery)
        {
            snapshot = SaveSnapshotJournalMigration.Migrate(snapshot);
        }

        if (snapshot.SchemaVersion == SaveSchemaVersion.Journal)
        {
            snapshot = SaveSnapshotMapMigration.Migrate(snapshot);
        }

        if (snapshot.SchemaVersion == SaveSchemaVersion.Map)
        {
            snapshot = SaveSnapshotInvestigationRoutesMigration.Migrate(snapshot);
        }

        if (snapshot.SchemaVersion == SaveSchemaVersion.InvestigationRoutes)
        {
            snapshot = SaveSnapshotFirstPatientRecoveryMigration.Migrate(snapshot);
        }

        if (snapshot.SchemaVersion == SaveSchemaVersion.FirstPatientRecovery)
        {
            snapshot = SaveSnapshotContinuationMigration.Migrate(snapshot);
        }

        return snapshot.SchemaVersion == SaveSchemaVersion.Continuation
            ? SaveSnapshotInventoryMigration.Migrate(snapshot)
            : snapshot;
    }

    private static SaveSnapshot ReadSnapshot(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty("schemaVersion", out JsonElement versionElement))
        {
            throw InvalidDocument();
        }

        SaveSchemaVersion version = ReadVersion(versionElement);
        if (version == SaveSchemaVersion.FirstSupported)
        {
            RequireObject(element, "schemaVersion", "discovery", "worldEvents");
        }
        else if (version == SaveSchemaVersion.Inventory)
        {
            RequireObject(
                element,
                "schemaVersion",
                "discovery",
                "worldEvents",
                "characterMemory",
                "firstPatient",
                "journal",
                "map",
                "investigationRoutes",
                "firstPatientRecoveryStage",
                "continuation",
                "inventory");
        }
        else if (version == SaveSchemaVersion.Continuation)
        {
            RequireObject(
                element,
                "schemaVersion",
                "discovery",
                "worldEvents",
                "characterMemory",
                "firstPatient",
                "journal",
                "map",
                "investigationRoutes",
                "firstPatientRecoveryStage",
                "continuation");
        }
        else if (version == SaveSchemaVersion.FirstPatientRecovery)
        {
            RequireObject(
                element,
                "schemaVersion",
                "discovery",
                "worldEvents",
                "characterMemory",
                "firstPatient",
                "journal",
                "map",
                "investigationRoutes",
                "firstPatientRecoveryStage");
        }
        else if (version == SaveSchemaVersion.InvestigationRoutes)
        {
            RequireObject(
                element,
                "schemaVersion",
                "discovery",
                "worldEvents",
                "characterMemory",
                "firstPatient",
                "journal",
                "map",
                "investigationRoutes");
        }
        else if (version == SaveSchemaVersion.Map)
        {
            RequireObject(
                element,
                "schemaVersion",
                "discovery",
                "worldEvents",
                "characterMemory",
                "firstPatient",
                "journal",
                "map");
        }
        else if (version == SaveSchemaVersion.Journal)
        {
            RequireObject(
                element,
                "schemaVersion",
                "discovery",
                "worldEvents",
                "characterMemory",
                "firstPatient",
                "journal");
        }
        else if (version == SaveSchemaVersion.FirstPatient
            || version == SaveSchemaVersion.DialogueDelivery)
        {
            RequireObject(
                element,
                "schemaVersion",
                "discovery",
                "worldEvents",
                "characterMemory",
                "firstPatient");
        }
        else
        {
            RequireObject(element, "schemaVersion", "discovery", "worldEvents", "characterMemory");
        }

        DiscoverySaveState discovery = ReadDiscovery(element.GetProperty("discovery"));
        WorldEventSaveState worldEvents = ReadWorldEvents(element.GetProperty("worldEvents"));
        if (version == SaveSchemaVersion.FirstSupported)
        {
            return new SaveSnapshot(version, discovery, worldEvents);
        }

        Allrpcs.Domain.Characters.CharacterMemorySnapshot characterMemory =
            CharacterMemoryJsonReader.Read(
                element.GetProperty("characterMemory"),
                version == SaveSchemaVersion.DialogueDelivery
                    || version == SaveSchemaVersion.Journal
                    || version == SaveSchemaVersion.Map
                    || version == SaveSchemaVersion.InvestigationRoutes
                    || version == SaveSchemaVersion.FirstPatientRecovery
                    || version == SaveSchemaVersion.Continuation
                    || version == SaveSchemaVersion.Inventory);
        if (version == SaveSchemaVersion.FirstPatientRecovery
            || version == SaveSchemaVersion.Continuation
            || version == SaveSchemaVersion.Inventory)
        {
            return new SaveSnapshot(
                version,
                discovery,
                worldEvents,
                characterMemory,
                FirstPatientJsonReader.Read(element.GetProperty("firstPatient")),
                JournalSaveStateJsonReader.Read(element.GetProperty("journal")),
                MapSaveStateJsonReader.Read(element.GetProperty("map")),
                InvestigationRouteSaveStateJsonReader.Read(
                    element.GetProperty("investigationRoutes")),
                FirstPatientRecoveryStage.Parse(
                    ReadString(element.GetProperty("firstPatientRecoveryStage"))),
                version == SaveSchemaVersion.FirstPatientRecovery
                    ? null
                    : SaveContinuationJsonReader.Read(element.GetProperty("continuation")),
                version == SaveSchemaVersion.Inventory
                    ? InventoryOwnershipJsonReader.Read(element.GetProperty("inventory"))
                    : null);
        }

        if (version == SaveSchemaVersion.InvestigationRoutes)
        {
            return new SaveSnapshot(
                version,
                discovery,
                worldEvents,
                characterMemory,
                FirstPatientJsonReader.Read(element.GetProperty("firstPatient")),
                JournalSaveStateJsonReader.Read(element.GetProperty("journal")),
                MapSaveStateJsonReader.Read(element.GetProperty("map")),
                InvestigationRouteSaveStateJsonReader.Read(
                    element.GetProperty("investigationRoutes")));
        }

        if (version == SaveSchemaVersion.Map)
        {
            return new SaveSnapshot(
                version,
                discovery,
                worldEvents,
                characterMemory,
                FirstPatientJsonReader.Read(element.GetProperty("firstPatient")),
                JournalSaveStateJsonReader.Read(element.GetProperty("journal")),
                MapSaveStateJsonReader.Read(element.GetProperty("map")));
        }

        if (version == SaveSchemaVersion.Journal)
        {
            return new SaveSnapshot(
                version,
                discovery,
                worldEvents,
                characterMemory,
                FirstPatientJsonReader.Read(element.GetProperty("firstPatient")),
                JournalSaveStateJsonReader.Read(element.GetProperty("journal")));
        }

        return version == SaveSchemaVersion.FirstPatient
                || version == SaveSchemaVersion.DialogueDelivery
            ? new SaveSnapshot(
                version,
                discovery,
                worldEvents,
                characterMemory,
                FirstPatientJsonReader.Read(element.GetProperty("firstPatient")))
            : new SaveSnapshot(version, discovery, worldEvents, characterMemory);
    }

    private static SaveSchemaVersion ReadVersion(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Number
            || !element.TryGetInt32(out int value)
            || !string.Equals(
                element.GetRawText(),
                value.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
        {
            throw InvalidDocument();
        }

        return value switch
        {
            1 => SaveSchemaVersion.FirstSupported,
            2 => SaveSchemaVersion.CharacterMemory,
            3 => SaveSchemaVersion.FirstPatient,
            4 => SaveSchemaVersion.DialogueDelivery,
            5 => SaveSchemaVersion.Journal,
            6 => SaveSchemaVersion.Map,
            7 => SaveSchemaVersion.InvestigationRoutes,
            8 => SaveSchemaVersion.FirstPatientRecovery,
            9 => SaveSchemaVersion.Continuation,
            10 => SaveSchemaVersion.Inventory,
            _ => throw InvalidDocument(),
        };
    }

    private static DiscoverySaveState ReadDiscovery(JsonElement element)
    {
        RequireObject(
            element,
            "perception",
            "focusedSearchActivities",
            "focusedSearchOutcomes",
            "passiveNoticingCandidates",
            "observationReinterpretationCandidates");
        return new DiscoverySaveState(
            ReadPerception(element.GetProperty("perception")),
            ReadActivities(element.GetProperty("focusedSearchActivities")),
            ReadOutcomes(element.GetProperty("focusedSearchOutcomes")),
            ReadPassiveCandidates(element.GetProperty("passiveNoticingCandidates")),
            ReadReinterpretationCandidates(
                element.GetProperty("observationReinterpretationCandidates")));
    }

    private static CharacterPerceptionSnapshot ReadPerception(JsonElement element)
    {
        RequireObject(element, "characterId", "entries");
        CharacterId characterId = new(ReadString(element.GetProperty("characterId")));
        JsonElement entries = element.GetProperty("entries");
        RequireArray(entries);

        List<WorldObjectVisibility> visibilityEntries = new();
        foreach (JsonElement entry in entries.EnumerateArray())
        {
            RequireObject(entry, "objectId", "visibilityState");
            visibilityEntries.Add(
                new WorldObjectVisibility(
                    new ObjectId(ReadString(entry.GetProperty("objectId"))),
                    VisibilityState.Parse(ReadString(entry.GetProperty("visibilityState")))));
        }

        return new CharacterPerceptionSnapshot(characterId, visibilityEntries);
    }

    private static FocusedSearchActivitySnapshot ReadActivities(JsonElement element)
    {
        RequireArray(element);
        List<FocusedSearchActivity> entries = new();
        foreach (JsonElement entry in element.EnumerateArray())
        {
            RequireObject(entry, "searcherCharacterId", "targetObjectId");
            entries.Add(new FocusedSearchActivity(ReadRequest(entry)));
        }

        return new FocusedSearchActivitySnapshot(entries);
    }

    private static FocusedSearchOutcomeSnapshot ReadOutcomes(JsonElement element)
    {
        RequireArray(element);
        List<FocusedSearchOutcome> entries = new();
        foreach (JsonElement entry in element.EnumerateArray())
        {
            RequireObject(
                entry,
                "searcherCharacterId",
                "targetObjectId",
                "resultingVisibilityState");
            entries.Add(
                new FocusedSearchOutcome(
                    ReadRequest(entry),
                    VisibilityState.Parse(
                        ReadString(entry.GetProperty("resultingVisibilityState")))));
        }

        return new FocusedSearchOutcomeSnapshot(entries);
    }

    private static FocusedSearchRequest ReadRequest(JsonElement element) => new(
        new CharacterId(ReadString(element.GetProperty("searcherCharacterId"))),
        new ObjectId(ReadString(element.GetProperty("targetObjectId"))));

    private static PassiveNoticingCandidateSnapshot ReadPassiveCandidates(JsonElement element)
    {
        RequireArray(element);
        List<PassiveNoticingCandidate> entries = new();
        foreach (JsonElement entry in element.EnumerateArray())
        {
            RequireObject(entry, "observerCharacterId", "targetObjectId");
            entries.Add(
                new PassiveNoticingCandidate(
                    new CharacterId(ReadString(entry.GetProperty("observerCharacterId"))),
                    new ObjectId(ReadString(entry.GetProperty("targetObjectId")))));
        }

        return new PassiveNoticingCandidateSnapshot(entries);
    }

    private static ObservationReinterpretationCandidateSnapshot
        ReadReinterpretationCandidates(JsonElement element)
    {
        RequireArray(element);
        List<ObservationReinterpretationCandidate> entries = new();
        foreach (JsonElement entry in element.EnumerateArray())
        {
            RequireObject(
                entry,
                "observerCharacterId",
                "targetObjectId",
                "triggerKnowledgeId");
            entries.Add(
                new ObservationReinterpretationCandidate(
                    new CharacterId(ReadString(entry.GetProperty("observerCharacterId"))),
                    new ObjectId(ReadString(entry.GetProperty("targetObjectId"))),
                    new KnowledgeId(ReadString(entry.GetProperty("triggerKnowledgeId")))));
        }

        return new ObservationReinterpretationCandidateSnapshot(entries);
    }

    private static WorldEventSaveState ReadWorldEvents(JsonElement element)
    {
        RequireObject(element, "eventLog", "currentTimeEventId");
        JsonElement eventLogElement = element.GetProperty("eventLog");
        RequireArray(eventLogElement);
        List<WorldEventId> eventLogEntries = new();
        foreach (JsonElement entry in eventLogElement.EnumerateArray())
        {
            eventLogEntries.Add(new WorldEventId(ReadString(entry)));
        }

        JsonElement currentTimeElement = element.GetProperty("currentTimeEventId");
        WorldEventId? currentTimeEventId = currentTimeElement.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => new WorldEventId(ReadString(currentTimeElement)),
            _ => throw InvalidDocument(),
        };
        return new WorldEventSaveState(
            new WorldEventLogSnapshot(eventLogEntries),
            new WorldTimeSnapshot(currentTimeEventId));
    }

    internal static int ReadInt32(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Number
            || !element.TryGetInt32(out int value)
            || !string.Equals(
                element.GetRawText(),
                value.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
        {
            throw InvalidDocument();
        }

        return value;
    }

    internal static string ReadString(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return element.GetString() ?? throw InvalidDocument();
    }

    internal static void RequireArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw InvalidDocument();
        }
    }

    internal static void RequireObject(JsonElement element, params string[] propertyNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw InvalidDocument();
        }

        HashSet<string> expected = new(propertyNames, StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !actual.Add(property.Name))
            {
                throw InvalidDocument();
            }
        }

        if (!actual.SetEquals(expected))
        {
            throw InvalidDocument();
        }
    }

    internal static InvalidDataException InvalidDocument() =>
        new("Unexpected save-snapshot JSON structure or value.");
}
