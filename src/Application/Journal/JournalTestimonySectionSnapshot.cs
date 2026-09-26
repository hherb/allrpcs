using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Exposes one immutable, audience-free Seeker testimony section.</summary>
public sealed class JournalTestimonySectionSnapshot
{
    internal JournalTestimonySectionSnapshot(
        JournalSectionDefinition section,
        CharacterId seekerId,
        IEnumerable<JournalTestimonyAuthoredCopy> entries)
    {
        Section = section;
        SeekerId = seekerId;
        Entries = entries.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact canonical testimony-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier owning the safely heard testimony.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets independent audience-free authored copies in heard-dialogue order.</summary>
    public IReadOnlyList<JournalTestimonyAuthoredCopy> Entries { get; }
}
