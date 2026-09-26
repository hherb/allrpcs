using Allrpcs.Application.Journal;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one explicit content-safe projection of the scene's selected journal section.</summary>
public partial class JournalSectionPresentationController : Node
{
    private JournalController journal = null!;
    private JournalSectionSelectionController selection = null!;

    /// <summary>Gets the exact projected selected-section presentation, or null before projection.</summary>
    public JournalSectionPresentationSnapshot? Presentation { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        journal = GetNode<JournalController>("../JournalController");
        selection = GetNode<JournalSectionSelectionController>(
            "../JournalSectionSelectionController");
    }

    /// <summary>Projects the current exact journal selection once without presenting UI.</summary>
    public bool TryProject()
    {
        JournalSnapshot? currentJournal = journal.Journal;
        JournalSectionDefinition? currentSelection = selection.Selection;
        if (Presentation is not null
            || currentJournal is null
            || currentSelection is null)
        {
            return false;
        }

        try
        {
            Presentation = JournalSectionPresentationProjection.Project(
                currentJournal,
                currentSelection);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Replaces projected state for one explicit exact selection change.</summary>
    public bool TryRefresh()
    {
        JournalSectionPresentationSnapshot? currentPresentation = Presentation;
        JournalSnapshot? currentJournal = journal.Journal;
        JournalSectionDefinition? currentSelection = selection.Selection;
        if (currentPresentation is null
            || currentJournal is null
            || currentSelection is null
            || ReferenceEquals(currentPresentation.Section, currentSelection))
        {
            return false;
        }

        try
        {
            JournalSectionPresentationSnapshot replacement =
                JournalSectionPresentationProjection.Project(
                    currentJournal,
                    currentSelection);
            Presentation = replacement;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
