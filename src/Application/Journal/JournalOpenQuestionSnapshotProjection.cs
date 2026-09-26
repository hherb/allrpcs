namespace Allrpcs.Application.Journal;

/// <summary>Installs exact safe open questions into a fresh complete journal snapshot.</summary>
public static class JournalOpenQuestionSnapshotProjection
{
    private const int OpenQuestionsSectionIndex = 4;

    /// <summary>Returns a fresh journal with open questions replacing its final definition.</summary>
    public static JournalSnapshot Compose(
        JournalSnapshot journal,
        JournalOpenQuestionSectionSnapshot openQuestions)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(openQuestions);

        JournalSectionDefinition canonicalSection =
            journal.Catalogue.Sections[OpenQuestionsSectionIndex];
        if (journal.Experiments is null
            || journal.OpenQuestions is not null
            || journal.PendingSections.Count != 1
            || !ReferenceEquals(journal.PendingSections[0], canonicalSection)
            || !ReferenceEquals(openQuestions.Section, canonicalSection)
            || !ReferenceEquals(openQuestions.SeekerId, journal.SeekerId))
        {
            throw new ArgumentException(
                "Open questions must retain the journal's exact final definition and Seeker.",
                nameof(openQuestions));
        }

        return new JournalSnapshot(
            journal.Catalogue,
            journal.Observations,
            journal.Testimony,
            journal.Hypotheses,
            journal.Experiments,
            openQuestions,
            []);
    }
}
