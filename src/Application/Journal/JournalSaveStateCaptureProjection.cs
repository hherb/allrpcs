using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Journal;

/// <summary>Captures a complete exact safe journal as closed persistence values.</summary>
public static class JournalSaveStateCaptureProjection
{
    private const int ObservationsSectionIndex = 0;
    private const int TestimonySectionIndex = 1;
    private const int HypothesesSectionIndex = 2;
    private const int ExperimentsSectionIndex = 3;
    private const int OpenQuestionsSectionIndex = 4;

    /// <summary>Returns one fresh immutable save state with an optional exact selection.</summary>
    public static JournalSaveState Capture(
        JournalSnapshot journal,
        JournalSectionDefinition? selection)
    {
        ArgumentNullException.ThrowIfNull(journal);
        RequireCompleteExactJournal(journal);

        if (selection is not null
            && !journal.Catalogue.Sections.Any(section => ReferenceEquals(section, selection)))
        {
            throw new ArgumentException(
                "A journal selection must be one exact canonical section from the journal.",
                nameof(selection));
        }

        return new JournalSaveState(
            journal.SeekerId,
            journal.Observations.Entries.Select(entry => new JournalObservationSaveEntry(
                entry.ObjectId,
                entry.DisplayName,
                entry.Description)),
            journal.Testimony!.Entries.Select(entry => new JournalTestimonySaveEntry(
                entry.SpeakerCharacterId,
                entry.Text,
                entry.DeliveryMode)),
            journal.Hypotheses!.Entries.Select(entry => new JournalHypothesisSaveEntry(
                entry.HypothesisId,
                entry.QuestionId,
                entry.QuestionPrompt,
                entry.Statement)),
            journal.Experiments!.Entries.Select(entry => new JournalExperimentSaveEntry(
                entry.ExperimentId,
                entry.Procedure,
                entry.ObservedOutcome)),
            journal.OpenQuestions!.Entries.Select(entry => new JournalOpenQuestionSaveEntry(
                entry.QuestionId,
                entry.Prompt)),
            selection?.Key);
    }

    private static void RequireCompleteExactJournal(JournalSnapshot journal)
    {
        IReadOnlyList<JournalSectionDefinition> sections = journal.Catalogue.Sections;
        if (journal.PendingSections.Count != 0
            || journal.Testimony is null
            || journal.Hypotheses is null
            || journal.Experiments is null
            || journal.OpenQuestions is null
            || !HasExactOwner(
                journal.Observations.Section,
                journal.Observations.SeekerId,
                ObservationsSectionIndex)
            || !HasExactOwner(
                journal.Testimony.Section,
                journal.Testimony.SeekerId,
                TestimonySectionIndex)
            || !HasExactOwner(
                journal.Hypotheses.Section,
                journal.Hypotheses.SeekerId,
                HypothesesSectionIndex)
            || !HasExactOwner(
                journal.Experiments.Section,
                journal.Experiments.SeekerId,
                ExperimentsSectionIndex)
            || !HasExactOwner(
                journal.OpenQuestions.Section,
                journal.OpenQuestions.SeekerId,
                OpenQuestionsSectionIndex))
        {
            throw new ArgumentException(
                "Journal persistence requires every exact canonical safe section.",
                nameof(journal));
        }

        bool HasExactOwner(
            JournalSectionDefinition section,
            CharacterId seekerId,
            int sectionIndex) =>
            ReferenceEquals(section, sections[sectionIndex])
            && ReferenceEquals(seekerId, journal.SeekerId);
    }
}
