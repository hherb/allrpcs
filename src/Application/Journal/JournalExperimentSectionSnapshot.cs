using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Exposes one immutable Seeker-owned experiments section.</summary>
public sealed class JournalExperimentSectionSnapshot
{
    internal JournalExperimentSectionSnapshot(
        JournalSectionDefinition section,
        CharacterId seekerId,
        IEnumerable<JournalExperimentAuthoredCopy> entries)
    {
        Section = section;
        SeekerId = seekerId;
        Entries = entries.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact canonical experiments-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier owning the recorded experiments.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets independent authored copies in stable recorded order.</summary>
    public IReadOnlyList<JournalExperimentAuthoredCopy> Entries { get; }
}
