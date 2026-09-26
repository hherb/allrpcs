using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Binds exact authored hypotheses to canonical post-testimony journal provenance.</summary>
public static class JournalHypothesisSectionProjection
{
    private const int HypothesesSectionIndex = 2;

    /// <summary>Returns a fresh binding without copying or interpreting hypothesis content.</summary>
    public static JournalHypothesisSectionBinding Bind(
        JournalSnapshot journal,
        InvestigationHypothesisSnapshot hypotheses)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(hypotheses);

        JournalSectionDefinition canonicalSection =
            journal.Catalogue.Sections[HypothesesSectionIndex];
        if (journal.Testimony is null
            || journal.PendingSections.Count == 0
            || !ReferenceEquals(journal.PendingSections[0], canonicalSection))
        {
            throw new ArgumentException(
                "The journal must retain hypotheses as its exact next pending section.",
                nameof(journal));
        }

        return new JournalHypothesisSectionBinding(
            journal,
            canonicalSection,
            hypotheses);
    }
}
