using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Binds one safe post-experiments journal to exact open questions.</summary>
public sealed class JournalOpenQuestionSectionBinding
{
    internal JournalOpenQuestionSectionBinding(
        JournalSnapshot journal,
        JournalSectionDefinition section,
        OpenInvestigationQuestionSnapshot openQuestions)
    {
        Journal = journal;
        Section = section;
        OpenQuestions = openQuestions;
    }

    /// <summary>Gets the exact safe journal snapshot receiving open-question provenance.</summary>
    public JournalSnapshot Journal { get; }

    /// <summary>Gets the catalogue's exact canonical open-questions definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier retained by the safe journal.</summary>
    public CharacterId SeekerId => Journal.SeekerId;

    /// <summary>Gets the exact existing immutable open-question snapshot.</summary>
    public OpenInvestigationQuestionSnapshot OpenQuestions { get; }
}
