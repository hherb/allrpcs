using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates explicit safe testimony-to-location reporting for the current Seeker.</summary>
public partial class SeekerMapTestimonyReportingCoordinator : Node
{
    private JournalController journal = null!;
    private SeekerMapController map = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        journal = GetNode<JournalController>("../JournalController");
        map = GetNode<SeekerMapController>("../SeekerMapController");
        seekerCharacter = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
    }

    /// <summary>Reports one explicitly paired canonical location from exact current safe testimony.</summary>
    public bool TryReport(
        SeekerMapSnapshot expectedMap,
        JournalSnapshot expectedJournal,
        JournalTestimonySectionSnapshot testimony,
        CharacterId seekerId,
        JournalTestimonyAuthoredCopy sourceEntry,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(expectedJournal);
        ArgumentNullException.ThrowIfNull(testimony);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(sourceEntry);
        ArgumentNullException.ThrowIfNull(location);

        return ReferenceEquals(map.Map, expectedMap)
            && ReferenceEquals(journal.Journal, expectedJournal)
            && ReferenceEquals(expectedJournal.Testimony, testimony)
            && ReferenceEquals(seekerCharacter.Character.Identity.Id, seekerId)
            && map.TryReport(expectedMap, testimony, sourceEntry, location);
    }
}
