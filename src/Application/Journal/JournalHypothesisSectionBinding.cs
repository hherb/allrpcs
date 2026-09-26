using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Binds one safe post-testimony journal to exact authored hypotheses.</summary>
public sealed class JournalHypothesisSectionBinding
{
    internal JournalHypothesisSectionBinding(
        JournalSnapshot journal,
        JournalSectionDefinition section,
        InvestigationHypothesisSnapshot hypotheses)
    {
        Journal = journal;
        Section = section;
        Hypotheses = hypotheses;
    }

    /// <summary>Gets the exact safe journal snapshot receiving hypothesis provenance.</summary>
    public JournalSnapshot Journal { get; }

    /// <summary>Gets the catalogue's exact canonical hypotheses-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier retained by the safe journal.</summary>
    public CharacterId SeekerId => Journal.SeekerId;

    /// <summary>Gets the exact existing immutable authored-hypothesis snapshot.</summary>
    public InvestigationHypothesisSnapshot Hypotheses { get; }
}
