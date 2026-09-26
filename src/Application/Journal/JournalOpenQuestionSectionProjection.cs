using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Binds exact open questions to canonical post-experiments journal provenance.</summary>
public static class JournalOpenQuestionSectionProjection
{
    private const int OpenQuestionsSectionIndex = 4;

    /// <summary>Returns a fresh binding without copying or interpreting question content.</summary>
    public static JournalOpenQuestionSectionBinding Bind(
        JournalSnapshot journal,
        OpenInvestigationQuestionSnapshot openQuestions)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(openQuestions);

        JournalSectionDefinition canonicalSection =
            journal.Catalogue.Sections[OpenQuestionsSectionIndex];
        if (journal.Experiments is null
            || journal.PendingSections.Count == 0
            || !ReferenceEquals(journal.PendingSections[0], canonicalSection))
        {
            throw new ArgumentException(
                "The journal must retain open questions as its exact next pending section.",
                nameof(journal));
        }

        return new JournalOpenQuestionSectionBinding(
            journal,
            canonicalSection,
            openQuestions);
    }
}
