using Allrpcs.Application.Journal;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns explicit journal open and close input plus its modal movement suspension.</summary>
public partial class JournalOpenCloseInputController : Node
{
    private static readonly StringName ToggleAction = new("journal_toggle");

    private Button close = null!;
    private RepresentativeJournalCompositionCoordinator composition = null!;
    private JournalController journal = null!;
    private JournalSectionPanel panel = null!;
    private SeekerMapPanel mapPanel = null!;
    private JournalSectionPresentationController presentation = null!;
    private Button next = null!;
    private Button previous = null!;
    private Seeker seeker = null!;
    private JournalSectionSelectionController selection = null!;
    private bool isOpen;

    /// <inheritdoc />
    public override void _Ready()
    {
        close = GetNode<Button>("../JournalSectionPanel/Panel/Close");
        composition = GetNode<RepresentativeJournalCompositionCoordinator>(
            "../RepresentativeJournalCompositionCoordinator");
        journal = GetNode<JournalController>("../JournalController");
        panel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        mapPanel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        presentation = GetNode<JournalSectionPresentationController>(
            "../JournalSectionPresentationController");
        next = GetNode<Button>("../JournalSectionPanel/Panel/Next");
        previous = GetNode<Button>("../JournalSectionPanel/Panel/Previous");
        seeker = GetNode<Seeker>("../Seeker");
        selection = GetNode<JournalSectionSelectionController>(
            "../JournalSectionSelectionController");
        close.Pressed += OnClosePressed;
        next.Pressed += OnNextPressed;
        previous.Pressed += OnPreviousPressed;
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.TogglesAllowed(this)) { return; }
        if (!ControlInputMatcher.Pressed(@event, ToggleAction))
        {
            return;
        }

        bool changed = isOpen ? TryClose() : TryOpen();
        if (changed)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private bool TryOpen()
    {
        if (mapPanel.Visible
            || (journal.Journal is null && !composition.TryComposeAndInitialize()))
        {
            return false;
        }

        JournalSnapshot currentJournal = journal.Journal!;
        if (selection.Selection is null
            && !selection.TrySelect(currentJournal, currentJournal.Catalogue.Sections[0]))
        {
            return false;
        }

        if (presentation.Presentation is null && !presentation.TryProject())
        {
            return false;
        }

        if (presentation.Presentation is { } projected
            && !ReferenceEquals(projected.Section, selection.Selection)
            && !presentation.TryRefresh())
        {
            return false;
        }

        JournalSectionPresentationSnapshot currentPresentation = presentation.Presentation!;
        if (!panel.TryPresent(currentPresentation))
        {
            return false;
        }

        seeker.SuspendForModal(this);
        close.GrabFocus();
        isOpen = true;
        UpdateNavigationButtons();
        return true;
    }

    /// <summary>Closes this reading modal after a successful load, releasing its movement suspension.</summary>
    public void ResetAfterRestore()
    {
        PreparedRestore reset = PrepareSessionReset();
        reset.Commit();
        if (reset.Publish is { } publish) { RestoreParticipantRegistry.Publish(publish); }
    }

    internal PreparedRestore PrepareSessionReset()
    {
        bool wasOpen = isOpen;
        return new PreparedRestore(() =>
        {
            isOpen = false;
        }, () =>
        {
            panel.Visible = false;
            if (wasOpen)
            {
                GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
                seeker.ResumeAfterModal(this);
            }
        });
    }

    private bool TryClose()
    {
        if (!panel.TryClose())
        {
            return false;
        }

        GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
        seeker.ResumeAfterModal(this);
        isOpen = false;
        return true;
    }

    private bool TryNavigate(int offset)
    {
        JournalSnapshot? currentJournal = journal.Journal;
        JournalSectionPresentationSnapshot? currentPresentation = presentation.Presentation;
        if (!isOpen
            || currentJournal is null
            || currentPresentation is null
            || !ReferenceEquals(selection.Selection, currentPresentation.Section))
        {
            return false;
        }

        int currentIndex = FindSectionIndex(
            currentJournal.Catalogue.Sections,
            currentPresentation.Section);
        int targetIndex = currentIndex + offset;
        if (currentIndex < 0
            || targetIndex < 0
            || targetIndex >= currentJournal.Catalogue.Sections.Count)
        {
            return false;
        }

        JournalSectionDefinition target = currentJournal.Catalogue.Sections[targetIndex];
        if (!selection.TrySelect(currentJournal, target)
            || !presentation.TryRefresh()
            || presentation.Presentation is not { } replacement
            || !panel.TryPresent(replacement))
        {
            return false;
        }

        UpdateNavigationButtons();
        return true;
    }

    private void UpdateNavigationButtons()
    {
        JournalSnapshot? currentJournal = journal.Journal;
        JournalSectionPresentationSnapshot? currentPresentation = presentation.Presentation;
        int currentIndex = currentJournal is null || currentPresentation is null
            ? -1
            : FindSectionIndex(
                currentJournal.Catalogue.Sections,
                currentPresentation.Section);
        previous.Disabled = currentIndex <= 0;
        next.Disabled = currentJournal is null
            || currentIndex < 0
            || currentIndex >= currentJournal.Catalogue.Sections.Count - 1;
    }

    private static int FindSectionIndex(
        IReadOnlyList<JournalSectionDefinition> sections,
        JournalSectionDefinition section)
    {
        for (int index = 0; index < sections.Count; index++)
        {
            if (ReferenceEquals(sections[index], section))
            {
                return index;
            }
        }

        return -1;
    }

    private void OnClosePressed()
    {
        if (isOpen)
        {
            _ = TryClose();
        }
    }

    private void OnNextPressed()
    {
        _ = TryNavigate(1);
    }

    private void OnPreviousPressed()
    {
        _ = TryNavigate(-1);
    }
}
