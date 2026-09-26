using Allrpcs.Application.Map;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns explicit map open and close input plus exact modal interaction restoration.</summary>
public partial class SeekerMapOpenCloseInputController : Node
{
    private static readonly StringName ToggleAction = new("map_toggle");

    private Button close = null!;
    private JournalSectionPanel journalPanel = null!;
    private SeekerMapPanel panel = null!;
    private SeekerMapNoteEditingController noteEditing = null!;
    private SeekerMapPresentationController presentations = null!;
    private SeekerMapController state = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;
    private Seeker seeker = null!;
    private Control? priorFocusOwner;
    private SeekerMapSnapshot? presentedMap;
    private SeekerMapNoteSnapshot? presentedNotes;
    private bool isOpen;
    private bool seekerWasPhysicsProcessing;

    /// <inheritdoc />
    public override void _Ready()
    {
        close = GetNode<Button>("../SeekerMapPanel/Panel/Close");
        journalPanel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        panel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        noteEditing = GetNode<SeekerMapNoteEditingController>(
            "../SeekerMapNoteEditingController");
        presentations = GetNode<SeekerMapPresentationController>(
            "../SeekerMapPresentationController");
        state = GetNode<SeekerMapController>("../SeekerMapController");
        seekerCharacter = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        seeker = GetNode<Seeker>("../Seeker");
        close.Pressed += OnClosePressed;
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!@event.IsActionPressed(ToggleAction))
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
        if (isOpen || panel.Visible || journalPanel.Visible)
        {
            return false;
        }

        Allrpcs.Domain.Identifiers.CharacterId seekerId =
            seekerCharacter.Character.Identity.Id;
        bool requiresInitialization = state.Map is null
            && state.Notes is null
            && state.Catalogue is null;
        if (requiresInitialization
            && (presentations.Presentation is not null || panel.Presentation is not null))
        {
            return false;
        }

        if (requiresInitialization)
        {
            if (!state.TryInitialize(seekerId))
            {
                return false;
            }
        }

        if (state.Map is not { } map
            || state.Notes is not { } notes
            || state.Catalogue is null
            || !ReferenceEquals(map.SeekerId, seekerId)
            || !ReferenceEquals(notes.SeekerId, seekerId))
        {
            return false;
        }

        SeekerMapPresentationSnapshot? currentPresentation = presentations.Presentation;
        if (panel.Presentation is not null
            && !ReferenceEquals(panel.Presentation, currentPresentation))
        {
            return false;
        }

        if (currentPresentation is null)
        {
            if (presentedMap is not null
                || presentedNotes is not null
                || !presentations.TryProject(map, notes))
            {
                return false;
            }

            currentPresentation = presentations.Presentation;
        }
        else if (!ReferenceEquals(presentedMap, map)
            || !ReferenceEquals(presentedNotes, notes))
        {
            if (presentedMap is null
                || presentedNotes is null
                || !presentations.TryRefresh(map, notes, currentPresentation))
            {
                return false;
            }

            currentPresentation = presentations.Presentation;
        }

        if (currentPresentation is null
            || !noteEditing.TryPresent(map, notes, currentPresentation)
            || !panel.TryPresent(currentPresentation))
        {
            return false;
        }

        priorFocusOwner = GetViewport().GuiGetFocusOwner();
        seekerWasPhysicsProcessing = seeker.IsPhysicsProcessing();
        seeker.Velocity = Vector2.Zero;
        seeker.SetPhysicsProcess(false);
        close.GrabFocus();
        presentedMap = map;
        presentedNotes = notes;
        isOpen = true;
        return true;
    }

    private bool TryClose()
    {
        if (!isOpen || !panel.TryClose())
        {
            return false;
        }

        GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
        priorFocusOwner?.GrabFocus();
        priorFocusOwner = null;
        seeker.SetPhysicsProcess(seekerWasPhysicsProcessing);
        isOpen = false;
        return true;
    }

    private void OnClosePressed()
    {
        if (isOpen)
        {
            _ = TryClose();
        }
    }
}
