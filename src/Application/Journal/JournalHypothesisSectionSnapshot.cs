using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Exposes one immutable Seeker-authored hypotheses section.</summary>
public sealed class JournalHypothesisSectionSnapshot
{
    internal JournalHypothesisSectionSnapshot(
        JournalSectionDefinition section,
        CharacterId seekerId,
        IEnumerable<JournalHypothesisAuthoredCopy> entries)
    {
        Section = section;
        SeekerId = seekerId;
        Entries = entries.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact canonical hypotheses-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier owning the authored hypotheses.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets independent authored copies in stable source order.</summary>
    public IReadOnlyList<JournalHypothesisAuthoredCopy> Entries { get; }
}
