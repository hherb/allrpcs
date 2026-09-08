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
    /// Materializes one complete version-1 snapshot, rejecting malformed or unexpected structure.
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

    private static SaveSnapshot ReadSnapshot(JsonElement element)
    {
        RequireObject(element, "schemaVersion", "discovery", "worldEvents");
        SaveSchemaVersion version = ReadVersion(element.GetProperty("schemaVersion"));
        DiscoverySaveState discovery = ReadDiscovery(element.GetProperty("discovery"));
        WorldEventSaveState worldEvents = ReadWorldEvents(element.GetProperty("worldEvents"));
        return new SaveSnapshot(version, discovery, worldEvents);
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

        SaveSchemaVersion version = new(value);
        if (!SaveSchemaVersionSupportPolicy.CanLoad(version))
        {
            throw InvalidDocument();
        }

        return version;
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

    private static string ReadString(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw InvalidDocument();
        }

        return element.GetString() ?? throw InvalidDocument();
    }

    private static void RequireArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw InvalidDocument();
        }
    }

    private static void RequireObject(JsonElement element, params string[] propertyNames)
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

    private static InvalidDataException InvalidDocument() =>
        new("Unexpected save-snapshot JSON structure or value.");
}
