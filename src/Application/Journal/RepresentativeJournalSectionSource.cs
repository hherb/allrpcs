namespace Allrpcs.Application.Journal;

/// <summary>Authors the content-free journal sections required by the opening vertical slice.</summary>
public static class RepresentativeJournalSectionSource
{
    /// <summary>Creates one fresh complete journal-section catalogue.</summary>
    public static JournalSectionCatalogue Create() => new(
    [
        new JournalSectionDefinition("observations", "Observations"),
        new JournalSectionDefinition("testimony", "Testimony"),
        new JournalSectionDefinition("hypotheses", "Hypotheses"),
        new JournalSectionDefinition("experiments", "Experiments"),
        new JournalSectionDefinition("open-questions", "Open Questions"),
    ]);
}
