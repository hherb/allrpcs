using Allrpcs.Application.Journal;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one explicit content-safe projection of the scene's selected journal section.</summary>
public partial class JournalSectionPresentationController : Node
{
    private JournalController journal = null!;
    private JournalSectionSelectionController selection = null!;
    private JournalSnapshot? projectedJournal;

    /// <summary>Gets the exact projected selected-section presentation, or null before projection.</summary>
    public JournalSectionPresentationSnapshot? Presentation { get; private set; }

    /// <summary>Gets whether the presentation shows the current selection of the current journal.</summary>
    public bool IsCurrent => Presentation is { } current
        && ReferenceEquals(current.Section, selection.Selection)
        && ReferenceEquals(projectedJournal, journal.Journal);

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
            projectedJournal = currentJournal;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Replaces projected state after an exact selection or journal change.</summary>
    public bool TryRefresh()
    {
        JournalSectionPresentationSnapshot? currentPresentation = Presentation;
        JournalSnapshot? currentJournal = journal.Journal;
        JournalSectionDefinition? currentSelection = selection.Selection;
        if (currentPresentation is null
            || currentJournal is null
            || currentSelection is null
            || (ReferenceEquals(currentPresentation.Section, currentSelection)
                && ReferenceEquals(projectedJournal, currentJournal)))
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
            projectedJournal = currentJournal;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
