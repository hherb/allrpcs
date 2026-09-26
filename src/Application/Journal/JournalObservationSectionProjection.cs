using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Journal;

/// <summary>Binds only already-filtered Seeker perception to the canonical observations section.</summary>
public static class JournalObservationSectionProjection
{
    /// <summary>Retains exact catalogue and perception provenance without creating journal content.</summary>
    public static JournalObservationSectionBinding Bind(
        JournalSectionCatalogue catalogue,
        CharacterId seekerId,
        CharacterPerceptionSnapshot filteredPerception)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(filteredPerception);

        if (!ReferenceEquals(filteredPerception.CharacterId, seekerId))
        {
            throw new ArgumentException(
                "The filtered perception must retain the exact supplied Seeker identifier.",
                nameof(filteredPerception));
        }

        if (filteredPerception.Entries.Any(entry =>
                entry.VisibilityState == VisibilityState.Unnoticed))
        {
            throw new ArgumentException(
                "Filtered journal perception cannot contain unnoticed world objects.",
                nameof(filteredPerception));
        }

        JournalSectionDefinition observationsSection = catalogue.Sections[0];
        return new JournalObservationSectionBinding(
            catalogue,
            observationsSection,
            seekerId,
            filteredPerception);
    }
}
