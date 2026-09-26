namespace Allrpcs.Application.Journal;

/// <summary>Contains stable text projected only from one visibility-safe journal section.</summary>
public sealed class JournalSectionPresentationText
{
    internal JournalSectionPresentationText(string sectionLabel, string content)
    {
        SectionLabel = sectionLabel;
        Content = content;
    }

    /// <summary>Gets the exact authored display label of the selected section.</summary>
    public string SectionLabel { get; }

    /// <summary>Gets stable plain text containing only the selected section's safe content.</summary>
    public string Content { get; }
}
