using System.Text.Json;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly reads one canonical content-safe journal save-state JSON object.</summary>
internal static class JournalSaveStateJsonReader
{
    /// <summary>Reads one complete journal object with no unknown properties.</summary>
    internal static JournalSaveState Read(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireObject(
            element,
            "seekerCharacterId",
            "selectedSectionKey",
            "observations",
            "testimony",
            "hypotheses",
            "experiments",
            "openQuestions");
        return new JournalSaveState(
            new CharacterId(ReadString(element, "seekerCharacterId")),
            ReadObservations(element.GetProperty("observations")),
            ReadTestimony(element.GetProperty("testimony")),
            ReadHypotheses(element.GetProperty("hypotheses")),
            ReadExperiments(element.GetProperty("experiments")),
            ReadOpenQuestions(element.GetProperty("openQuestions")),
            ReadNullableString(element.GetProperty("selectedSectionKey")));
    }

    private static List<JournalObservationSaveEntry> ReadObservations(JsonElement element)
    {
        List<JournalObservationSaveEntry> entries = [];
        foreach (JsonElement entry in RequireBoundedArray(element))
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                entry,
                "objectId",
                "displayName",
                "description");
            entries.Add(new JournalObservationSaveEntry(
                new ObjectId(ReadString(entry, "objectId")),
                ReadString(entry, "displayName"),
                ReadString(entry, "description")));
        }

        return entries;
    }

    private static List<JournalTestimonySaveEntry> ReadTestimony(JsonElement element)
    {
        List<JournalTestimonySaveEntry> entries = [];
        foreach (JsonElement entry in RequireBoundedArray(element))
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                entry,
                "speakerCharacterId",
                "text",
                "deliveryMode");
            entries.Add(new JournalTestimonySaveEntry(
                new CharacterId(ReadString(entry, "speakerCharacterId")),
                ReadString(entry, "text"),
                DialogueDeliveryMode.Parse(ReadString(entry, "deliveryMode"))));
        }

        return entries;
    }

    private static List<JournalHypothesisSaveEntry> ReadHypotheses(JsonElement element)
    {
        List<JournalHypothesisSaveEntry> entries = [];
        foreach (JsonElement entry in RequireBoundedArray(element))
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                entry,
                "hypothesisId",
                "questionId",
                "questionPrompt",
                "statement");
            entries.Add(new JournalHypothesisSaveEntry(
                new InvestigationHypothesisId(ReadString(entry, "hypothesisId")),
                new InvestigationQuestionId(ReadString(entry, "questionId")),
                ReadString(entry, "questionPrompt"),
                ReadString(entry, "statement")));
        }

        return entries;
    }

    private static List<JournalExperimentSaveEntry> ReadExperiments(JsonElement element)
    {
        List<JournalExperimentSaveEntry> entries = [];
        foreach (JsonElement entry in RequireBoundedArray(element))
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                entry,
                "experimentId",
                "procedure",
                "observedOutcome");
            entries.Add(new JournalExperimentSaveEntry(
                new ExperimentId(ReadString(entry, "experimentId")),
                ReadString(entry, "procedure"),
                ReadString(entry, "observedOutcome")));
        }

        return entries;
    }

    private static List<JournalOpenQuestionSaveEntry> ReadOpenQuestions(JsonElement element)
    {
        List<JournalOpenQuestionSaveEntry> entries = [];
        foreach (JsonElement entry in RequireBoundedArray(element))
        {
            SaveSnapshotJsonDeserializer.RequireObject(entry, "questionId", "prompt");
            entries.Add(new JournalOpenQuestionSaveEntry(
                new InvestigationQuestionId(ReadString(entry, "questionId")),
                ReadString(entry, "prompt")));
        }

        return entries;
    }

    private static JsonElement.ArrayEnumerator RequireBoundedArray(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        if (element.GetArrayLength() > JournalSaveState.MaximumEntriesPerSection)
        {
            throw SaveSnapshotJsonDeserializer.InvalidDocument();
        }

        return element.EnumerateArray();
    }

    private static string ReadString(JsonElement element, string propertyName) =>
        SaveSnapshotJsonDeserializer.ReadString(element.GetProperty(propertyName));

    private static string? ReadNullableString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => SaveSnapshotJsonDeserializer.ReadString(element),
        _ => throw SaveSnapshotJsonDeserializer.InvalidDocument(),
    };
}
