namespace Allrpcs.Application.Journal;

/// <summary>Installs exact safe testimony into a fresh journal snapshot.</summary>
public static class JournalTestimonySnapshotProjection
{
    private const int TestimonySectionIndex = 1;

    /// <summary>Returns a fresh journal with testimony replacing only its pending definition.</summary>
    public static JournalSnapshot Compose(
        JournalSnapshot journal,
        JournalTestimonySectionSnapshot testimony)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(testimony);

        JournalSectionDefinition canonicalSection =
            journal.Catalogue.Sections[TestimonySectionIndex];
        if (journal.Testimony is not null
            || journal.PendingSections.Count == 0
            || !ReferenceEquals(journal.PendingSections[0], canonicalSection)
            || !ReferenceEquals(testimony.Section, canonicalSection)
            || !ReferenceEquals(testimony.SeekerId, journal.SeekerId))
        {
            throw new ArgumentException(
                "Testimony must retain the journal's exact pending definition and Seeker.",
                nameof(testimony));
        }

        IReadOnlyList<JournalSectionDefinition> remainingPendingSections =
            journal.PendingSections.Skip(1).ToList().AsReadOnly();
        return new JournalSnapshot(
            journal.Catalogue,
            journal.Observations,
            testimony,
            journal.Hypotheses,
            journal.Experiments,
            journal.OpenQuestions,
            remainingPendingSections);
    }
}
