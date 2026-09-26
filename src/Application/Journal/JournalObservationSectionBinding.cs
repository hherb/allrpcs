using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Journal;

/// <summary>Binds the exact journal observations section to one filtered Seeker perception.</summary>
public sealed class JournalObservationSectionBinding
{
    internal JournalObservationSectionBinding(
        JournalSectionCatalogue catalogue,
        JournalSectionDefinition section,
        CharacterId seekerId,
        CharacterPerceptionSnapshot perception)
    {
        Catalogue = catalogue;
        Section = section;
        SeekerId = seekerId;
        Perception = perception;
    }

    /// <summary>Gets the exact canonical catalogue containing the observations section.</summary>
    public JournalSectionCatalogue Catalogue { get; }

    /// <summary>Gets the catalogue's exact observations-section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the exact Seeker identifier retained by the filtered perception.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets the exact Seeker perception after hidden entries were excluded.</summary>
    public CharacterPerceptionSnapshot Perception { get; }
}
