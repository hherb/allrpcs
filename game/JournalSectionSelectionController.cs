using Allrpcs.Application.Journal;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact canonical section selection for the scene's exact journal.</summary>
public partial class JournalSectionSelectionController : Node
{
    private JournalController journal = null!;

    /// <summary>Gets the exact selected canonical section, or null before explicit selection.</summary>
    public JournalSectionDefinition? Selection { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        journal = GetNode<JournalController>("../JournalController");
    }

    /// <summary>Selects or replaces the exact canonical section for the retained exact journal.</summary>
    public bool TrySelect(JournalSnapshot snapshot, JournalSectionDefinition section)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(section);

        if (!ReferenceEquals(journal.Journal, snapshot)
            || ReferenceEquals(Selection, section)
            || !snapshot.Catalogue.Sections.Any(candidate => ReferenceEquals(candidate, section)))
        {
            return false;
        }

        Selection = section;
        return true;
    }

    /// <summary>Installs one preflighted optional selection during root restoration.</summary>
    internal void InstallRestored(JournalSectionDefinition? section)
    {
        Selection = section;
    }
}
