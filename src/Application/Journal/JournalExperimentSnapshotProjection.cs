namespace Allrpcs.Application.Journal;

/// <summary>Installs exact safe experiments into a fresh journal snapshot.</summary>
public static class JournalExperimentSnapshotProjection
{
    private const int ExperimentsSectionIndex = 3;

    /// <summary>Returns a fresh journal with experiments replacing only its pending definition.</summary>
    public static JournalSnapshot Compose(
        JournalSnapshot journal,
        JournalExperimentSectionSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(experiments);

        JournalSectionDefinition canonicalSection =
            journal.Catalogue.Sections[ExperimentsSectionIndex];
        if (journal.Hypotheses is null
            || journal.Experiments is not null
            || journal.PendingSections.Count == 0
            || !ReferenceEquals(journal.PendingSections[0], canonicalSection)
            || !ReferenceEquals(experiments.Section, canonicalSection)
            || !ReferenceEquals(experiments.SeekerId, journal.SeekerId))
        {
            throw new ArgumentException(
                "Experiments must retain the journal's exact pending definition and Seeker.",
                nameof(experiments));
        }

        IReadOnlyList<JournalSectionDefinition> remainingPendingSections =
            journal.PendingSections.Skip(1).ToList().AsReadOnly();
        return new JournalSnapshot(
            journal.Catalogue,
            journal.Observations,
            journal.Testimony,
            journal.Hypotheses,
            experiments,
            journal.OpenQuestions,
            remainingPendingSections);
    }
}
