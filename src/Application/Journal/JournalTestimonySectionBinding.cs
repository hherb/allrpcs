using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Binds one safe journal to exact in-world dialogue heard by its Seeker.</summary>
public sealed class JournalTestimonySectionBinding
{
    internal JournalTestimonySectionBinding(
        JournalSnapshot journal,
        JournalSectionDefinition section,
        IEnumerable<DialogueEvent> heardDialogueEvents)
    {
        Journal = journal;
        Section = section;
        HeardDialogueEvents = heardDialogueEvents.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact safe journal snapshot receiving testimony provenance.</summary>
    public JournalSnapshot Journal { get; }

    /// <summary>Gets the catalogue's exact canonical testimony-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier retained by the safe journal.</summary>
    public CharacterId SeekerId => Journal.SeekerId;

    /// <summary>Gets copied exact dialogue events whose audiences include the Seeker.</summary>
    public IReadOnlyList<DialogueEvent> HeardDialogueEvents { get; }
}
