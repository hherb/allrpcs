namespace Allrpcs.Application.Journal;

/// <summary>Retains one freshly restored safe journal and its optional canonical selection.</summary>
public sealed class JournalSaveStateRestoration
{
    internal JournalSaveStateRestoration(
        JournalSnapshot journal,
        JournalSectionDefinition? selectedSection)
    {
        Journal = journal;
        SelectedSection = selectedSection;
    }

    /// <summary>Gets the fresh complete safe journal.</summary>
    public JournalSnapshot Journal { get; }

    /// <summary>Gets the exact selected definition from the restored journal, or null.</summary>
    public JournalSectionDefinition? SelectedSection { get; }
}
