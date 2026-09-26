using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Exposes one immutable, visibility-safe Seeker observations section.</summary>
public sealed class JournalObservationSectionSnapshot
{
    internal JournalObservationSectionSnapshot(
        JournalSectionDefinition section,
        CharacterId seekerId,
        IEnumerable<JournalObservationAuthoredCopy> entries)
    {
        Section = section;
        SeekerId = seekerId;
        Entries = entries.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact canonical observations-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier owning the filtered source perception.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets independent visibility-safe authored copies in observation order.</summary>
    public IReadOnlyList<JournalObservationAuthoredCopy> Entries { get; }
}
