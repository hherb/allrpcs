using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Binds one safe post-hypotheses journal to exact experiment records.</summary>
public sealed class JournalExperimentSectionBinding
{
    internal JournalExperimentSectionBinding(
        JournalSnapshot journal,
        JournalSectionDefinition section,
        ExperimentJournalSnapshot experiments)
    {
        Journal = journal;
        Section = section;
        Experiments = experiments;
    }

    /// <summary>Gets the exact safe journal snapshot receiving experiment provenance.</summary>
    public JournalSnapshot Journal { get; }

    /// <summary>Gets the catalogue's exact canonical experiments-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier retained by the safe journal.</summary>
    public CharacterId SeekerId => Journal.SeekerId;

    /// <summary>Gets the exact existing immutable experiment journal.</summary>
    public ExperimentJournalSnapshot Experiments { get; }
}
