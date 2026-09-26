using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Exposes one immutable Seeker-owned open-questions section.</summary>
public sealed class JournalOpenQuestionSectionSnapshot
{
    internal JournalOpenQuestionSectionSnapshot(
        JournalSectionDefinition section,
        CharacterId seekerId,
        IEnumerable<JournalOpenQuestionAuthoredCopy> entries)
    {
        Section = section;
        SeekerId = seekerId;
        Entries = entries.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact canonical open-questions definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier owning the open questions.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets independent authored copies in stable source order.</summary>
    public IReadOnlyList<JournalOpenQuestionAuthoredCopy> Entries { get; }
}
