namespace Allrpcs.Application.Journal;

/// <summary>Installs exact safe hypotheses into a fresh journal snapshot.</summary>
public static class JournalHypothesisSnapshotProjection
{
    private const int HypothesesSectionIndex = 2;

    /// <summary>Returns a fresh journal with hypotheses replacing only its pending definition.</summary>
    public static JournalSnapshot Compose(
        JournalSnapshot journal,
        JournalHypothesisSectionSnapshot hypotheses)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(hypotheses);

        JournalSectionDefinition canonicalSection =
            journal.Catalogue.Sections[HypothesesSectionIndex];
        if (journal.Testimony is null
            || journal.Hypotheses is not null
            || journal.PendingSections.Count == 0
            || !ReferenceEquals(journal.PendingSections[0], canonicalSection)
            || !ReferenceEquals(hypotheses.Section, canonicalSection)
            || !ReferenceEquals(hypotheses.SeekerId, journal.SeekerId))
        {
            throw new ArgumentException(
                "Hypotheses must retain the journal's exact pending definition and Seeker.",
                nameof(hypotheses));
        }

        IReadOnlyList<JournalSectionDefinition> remainingPendingSections =
            journal.PendingSections.Skip(1).ToList().AsReadOnly();
        return new JournalSnapshot(
            journal.Catalogue,
            journal.Observations,
            journal.Testimony,
            hypotheses,
            journal.Experiments,
            journal.OpenQuestions,
            remainingPendingSections);
    }
}
