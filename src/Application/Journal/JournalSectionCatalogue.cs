namespace Allrpcs.Application.Journal;

/// <summary>Retains the complete canonical journal-section catalogue in stable authored order.</summary>
public sealed class JournalSectionCatalogue
{
    private static readonly string[] CanonicalKeys =
    [
        "observations",
        "testimony",
        "hypotheses",
        "experiments",
        "open-questions",
    ];

    /// <summary>Creates an immutable catalogue from exact canonical section definitions.</summary>
    public JournalSectionCatalogue(IEnumerable<JournalSectionDefinition> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);

        List<JournalSectionDefinition> retainedSections = sections.ToList();
        if (retainedSections.Count != CanonicalKeys.Length)
        {
            throw new ArgumentException(
                "A journal-section catalogue must contain every canonical section exactly once.",
                nameof(sections));
        }

        for (int index = 0; index < CanonicalKeys.Length; index++)
        {
            JournalSectionDefinition? section = retainedSections[index];
            if (section is null
                || !string.Equals(section.Key, CanonicalKeys[index], StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Journal sections must use the complete canonical key order.",
                    nameof(sections));
            }
        }

        Sections = retainedSections.AsReadOnly();
    }

    /// <summary>Gets the exact section definitions in stable canonical order.</summary>
    public IReadOnlyList<JournalSectionDefinition> Sections { get; }
}
