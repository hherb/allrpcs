using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes one stable JSON representation of an immutable save snapshot.</summary>
public static class SaveSnapshotJsonSerializer
{
    /// <summary>Serializes the complete snapshot without mutating or normalizing its values.</summary>
    public static string Serialize(SaveSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        bool isVersionOne = snapshot.SchemaVersion == SaveSchemaVersion.FirstSupported;
        bool hasCharacterMemory =
            snapshot.SchemaVersion == SaveSchemaVersion.CharacterMemory
            || snapshot.SchemaVersion == SaveSchemaVersion.FirstPatient
            || snapshot.SchemaVersion == SaveSchemaVersion.DialogueDelivery
            || snapshot.SchemaVersion == SaveSchemaVersion.Journal
            || snapshot.SchemaVersion == SaveSchemaVersion.Map
            || snapshot.SchemaVersion == SaveSchemaVersion.InvestigationRoutes;
        bool hasFirstPatient = snapshot.SchemaVersion == SaveSchemaVersion.FirstPatient
            || snapshot.SchemaVersion == SaveSchemaVersion.DialogueDelivery
            || snapshot.SchemaVersion == SaveSchemaVersion.Journal
            || snapshot.SchemaVersion == SaveSchemaVersion.Map
            || snapshot.SchemaVersion == SaveSchemaVersion.InvestigationRoutes;
        if (!isVersionOne && !hasCharacterMemory)
        {
            throw new ArgumentException(
                "The save snapshot schema does not have a supported JSON representation.",
                nameof(snapshot));
        }

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", snapshot.SchemaVersion.Value);
            WriteDiscovery(writer, snapshot.Discovery);
            WriteWorldEvents(writer, snapshot.WorldEvents);
            if (hasCharacterMemory)
            {
                CharacterMemoryJsonWriter.Write(
                    writer,
                    snapshot.CharacterMemory,
                    snapshot.SchemaVersion == SaveSchemaVersion.DialogueDelivery
                        || snapshot.SchemaVersion == SaveSchemaVersion.Journal
                        || snapshot.SchemaVersion == SaveSchemaVersion.Map
                        || snapshot.SchemaVersion == SaveSchemaVersion.InvestigationRoutes);
            }

            if (hasFirstPatient)
            {
                FirstPatientJsonWriter.Write(writer, snapshot.FirstPatient!);
            }

            if (snapshot.SchemaVersion == SaveSchemaVersion.Journal
                || snapshot.SchemaVersion == SaveSchemaVersion.Map
                || snapshot.SchemaVersion == SaveSchemaVersion.InvestigationRoutes)
            {
                JournalSaveStateJsonWriter.Write(writer, snapshot.Journal!);
            }

            if (snapshot.SchemaVersion == SaveSchemaVersion.Map
                || snapshot.SchemaVersion == SaveSchemaVersion.InvestigationRoutes)
            {
                MapSaveStateJsonWriter.Write(writer, snapshot.Map!);
            }

            if (snapshot.SchemaVersion == SaveSchemaVersion.InvestigationRoutes)
            {
                InvestigationRouteSaveStateJsonWriter.Write(writer, snapshot.InvestigationRoutes!);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteDiscovery(Utf8JsonWriter writer, DiscoverySaveState state)
    {
        writer.WritePropertyName("discovery");
        writer.WriteStartObject();
        WritePerception(writer, state.Perception);
        WriteActivities(writer, state.FocusedSearchActivities);
        WriteOutcomes(writer, state.FocusedSearchOutcomes);
        WritePassiveCandidates(writer, state.PassiveNoticingCandidates);
        WriteReinterpretationCandidates(writer, state.ObservationReinterpretationCandidates);
        writer.WriteEndObject();
    }

    private static void WritePerception(
        Utf8JsonWriter writer,
        CharacterPerceptionSnapshot perception)
    {
        writer.WritePropertyName("perception");
        writer.WriteStartObject();
        writer.WriteString("characterId", perception.CharacterId.Value);
        writer.WritePropertyName("entries");
        writer.WriteStartArray();
        foreach (WorldObjectVisibility entry in perception.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("objectId", entry.ObjectId.Value);
            writer.WriteString("visibilityState", entry.VisibilityState.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteActivities(
        Utf8JsonWriter writer,
        FocusedSearchActivitySnapshot activities)
    {
        writer.WritePropertyName("focusedSearchActivities");
        writer.WriteStartArray();
        foreach (FocusedSearchActivity activity in activities.Entries)
        {
            writer.WriteStartObject();
            WriteRequest(writer, activity.Request);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteOutcomes(
        Utf8JsonWriter writer,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        writer.WritePropertyName("focusedSearchOutcomes");
        writer.WriteStartArray();
        foreach (FocusedSearchOutcome outcome in outcomes.Entries)
        {
            writer.WriteStartObject();
            WriteRequest(writer, outcome.Request);
            writer.WriteString(
                "resultingVisibilityState",
                outcome.ResultingVisibilityState.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteRequest(Utf8JsonWriter writer, FocusedSearchRequest request)
    {
        writer.WriteString("searcherCharacterId", request.SearcherCharacterId.Value);
        writer.WriteString("targetObjectId", request.TargetObjectId.Value);
    }

    private static void WritePassiveCandidates(
        Utf8JsonWriter writer,
        PassiveNoticingCandidateSnapshot candidates)
    {
        writer.WritePropertyName("passiveNoticingCandidates");
        writer.WriteStartArray();
        foreach (PassiveNoticingCandidate candidate in candidates.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("observerCharacterId", candidate.ObserverCharacterId.Value);
            writer.WriteString("targetObjectId", candidate.TargetObjectId.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteReinterpretationCandidates(
        Utf8JsonWriter writer,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        writer.WritePropertyName("observationReinterpretationCandidates");
        writer.WriteStartArray();
        foreach (ObservationReinterpretationCandidate candidate in candidates.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("observerCharacterId", candidate.ObserverCharacterId.Value);
            writer.WriteString("targetObjectId", candidate.TargetObjectId.Value);
            writer.WriteString("triggerKnowledgeId", candidate.TriggerKnowledgeId.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteWorldEvents(Utf8JsonWriter writer, WorldEventSaveState state)
    {
        writer.WritePropertyName("worldEvents");
        writer.WriteStartObject();
        writer.WritePropertyName("eventLog");
        writer.WriteStartArray();
        foreach (Allrpcs.Domain.Identifiers.WorldEventId eventId in state.EventLog.Entries)
        {
            writer.WriteStringValue(eventId.Value);
        }

        writer.WriteEndArray();
        writer.WriteString(
            "currentTimeEventId",
            state.WorldTime.CurrentEventId?.Value);
        writer.WriteEndObject();
    }
}
