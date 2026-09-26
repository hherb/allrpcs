using Allrpcs.Domain.Experiments;

namespace Allrpcs.Application.Journal;

/// <summary>Binds exact experiment records to canonical post-hypotheses journal provenance.</summary>
public static class JournalExperimentSectionProjection
{
    private const int ExperimentsSectionIndex = 3;

    /// <summary>Returns a fresh binding without copying or interpreting experiment content.</summary>
    public static JournalExperimentSectionBinding Bind(
        JournalSnapshot journal,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(experiments);

        JournalSectionDefinition canonicalSection =
            journal.Catalogue.Sections[ExperimentsSectionIndex];
        if (journal.Hypotheses is null
            || journal.PendingSections.Count == 0
            || !ReferenceEquals(journal.PendingSections[0], canonicalSection))
        {
            throw new ArgumentException(
                "The journal must retain experiments as its exact next pending section.",
                nameof(journal));
        }

        return new JournalExperimentSectionBinding(
            journal,
            canonicalSection,
            experiments);
    }
}
