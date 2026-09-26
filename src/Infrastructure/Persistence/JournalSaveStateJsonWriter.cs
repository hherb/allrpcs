using System.Text.Json;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes the canonical content-safe journal save-state JSON object.</summary>
internal static class JournalSaveStateJsonWriter
{
    /// <summary>Writes one complete journal property in stable canonical order.</summary>
    internal static void Write(Utf8JsonWriter writer, JournalSaveState journal)
    {
        writer.WritePropertyName("journal");
        writer.WriteStartObject();
        writer.WriteString("seekerCharacterId", journal.SeekerId.Value);
        writer.WriteString("selectedSectionKey", journal.SelectedSectionKey);
        WriteObservations(writer, journal.Observations);
        WriteTestimony(writer, journal.Testimony);
        WriteHypotheses(writer, journal.Hypotheses);
        WriteExperiments(writer, journal.Experiments);
        WriteOpenQuestions(writer, journal.OpenQuestions);
        writer.WriteEndObject();
    }

    private static void WriteObservations(
        Utf8JsonWriter writer,
        IReadOnlyList<JournalObservationSaveEntry> entries)
    {
        writer.WritePropertyName("observations");
        writer.WriteStartArray();
        foreach (JournalObservationSaveEntry entry in entries)
        {
            writer.WriteStartObject();
            writer.WriteString("objectId", entry.ObjectId.Value);
            writer.WriteString("displayName", entry.DisplayName);
            writer.WriteString("description", entry.Description);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteTestimony(
        Utf8JsonWriter writer,
        IReadOnlyList<JournalTestimonySaveEntry> entries)
    {
        writer.WritePropertyName("testimony");
        writer.WriteStartArray();
        foreach (JournalTestimonySaveEntry entry in entries)
        {
            writer.WriteStartObject();
            writer.WriteString("speakerCharacterId", entry.SpeakerCharacterId.Value);
            writer.WriteString("text", entry.Text);
            writer.WriteString("deliveryMode", entry.DeliveryMode.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteHypotheses(
        Utf8JsonWriter writer,
        IReadOnlyList<JournalHypothesisSaveEntry> entries)
    {
        writer.WritePropertyName("hypotheses");
        writer.WriteStartArray();
        foreach (JournalHypothesisSaveEntry entry in entries)
        {
            writer.WriteStartObject();
            writer.WriteString("hypothesisId", entry.HypothesisId.Value);
            writer.WriteString("questionId", entry.QuestionId.Value);
            writer.WriteString("questionPrompt", entry.QuestionPrompt);
            writer.WriteString("statement", entry.Statement);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteExperiments(
        Utf8JsonWriter writer,
        IReadOnlyList<JournalExperimentSaveEntry> entries)
    {
        writer.WritePropertyName("experiments");
        writer.WriteStartArray();
        foreach (JournalExperimentSaveEntry entry in entries)
        {
            writer.WriteStartObject();
            writer.WriteString("experimentId", entry.ExperimentId.Value);
            writer.WriteString("procedure", entry.Procedure);
            writer.WriteString("observedOutcome", entry.ObservedOutcome);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteOpenQuestions(
        Utf8JsonWriter writer,
        IReadOnlyList<JournalOpenQuestionSaveEntry> entries)
    {
        writer.WritePropertyName("openQuestions");
        writer.WriteStartArray();
        foreach (JournalOpenQuestionSaveEntry entry in entries)
        {
            writer.WriteStartObject();
            writer.WriteString("questionId", entry.QuestionId.Value);
            writer.WriteString("prompt", entry.Prompt);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
