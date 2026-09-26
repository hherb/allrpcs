namespace Allrpcs.Application.Journal;

/// <summary>Projects one exact selected complete-journal section into safe presentation state.</summary>
public static class JournalSectionPresentationProjection
{
    /// <summary>Returns a presentation snapshot containing only the exact selected safe section.</summary>
    public static JournalSectionPresentationSnapshot Project(
        JournalSnapshot journal,
        JournalSectionDefinition section)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(section);

        if (journal.Testimony is null
            || journal.Hypotheses is null
            || journal.Experiments is null
            || journal.OpenQuestions is null
            || journal.PendingSections.Count != 0)
        {
            throw new InvalidOperationException(
                "A selected journal section can be projected only from a complete journal.");
        }

        IReadOnlyList<JournalSectionDefinition> sections = journal.Catalogue.Sections;
        if (ReferenceEquals(section, sections[0]))
        {
            return new(section, journal.Observations, null, null, null, null);
        }

        if (ReferenceEquals(section, sections[1]))
        {
            return new(section, null, journal.Testimony, null, null, null);
        }

        if (ReferenceEquals(section, sections[2]))
        {
            return new(section, null, null, journal.Hypotheses, null, null);
        }

        if (ReferenceEquals(section, sections[3]))
        {
            return new(section, null, null, null, journal.Experiments, null);
        }

        if (ReferenceEquals(section, sections[4]))
        {
            return new(section, null, null, null, null, journal.OpenQuestions);
        }

        throw new InvalidOperationException(
            "A journal section presentation requires an exact canonical section from the journal.");
    }
}
