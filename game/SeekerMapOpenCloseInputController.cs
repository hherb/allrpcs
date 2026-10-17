using Allrpcs.Application.Investigations;
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
    private OpeningRegionInvestigationRouteObservationRoutingCoordinator routing = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionOldShrineSceneSource shrineSource = null!;
    private SeekerMapController state = null!;
    private OpeningRegionStreamResiduePassiveNoticingCoordinator streamResidue = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;
    private Seeker seeker = null!;
    private Control? priorFocusOwner;
    private SeekerMapSnapshot? presentedMap;
    private SeekerMapNoteSnapshot? presentedNotes;
    private bool isOpen;

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
        routing = GetNode<OpeningRegionInvestigationRouteObservationRoutingCoordinator>(
            "../OpeningRegionInvestigationRouteObservationRoutingCoordinator");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        shrineSource = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        state = GetNode<SeekerMapController>("../SeekerMapController");
        streamResidue = GetNode<OpeningRegionStreamResiduePassiveNoticingCoordinator>(
            "../OpeningRegionStreamResiduePassiveNoticingCoordinator");
        seekerCharacter = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        seeker = GetNode<Seeker>("../Seeker");
        close.Pressed += OnClosePressed;
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.TogglesAllowed(this)) { return; }
        ArgumentNullException.ThrowIfNull(@event);

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
            || state.Catalogue is not { } locationCatalogue
            || map.SeekerId != seekerId
            || notes.SeekerId != seekerId)
        {
            return false;
        }

        OpeningRegionInvestigationRouteCatalogue? routesToInitialize = null;
        if (routeProgress.Progress is null)
        {
            if (routeProgress.Catalogue is not null)
            {
                return false;
            }

            routesToInitialize =
                RepresentativeOpeningRegionInvestigationRouteSource.Create(
                    locationCatalogue,
                    shrineSource.DiscoveryScenario);
        }
        else if (routeProgress.Catalogue is not { } routes
            || !ReferenceEquals(routeProgress.Progress.Catalogue, routes)
            || !OpeningRegionInvestigationRouteValidationPolicy.IsValid(
                routes,
                locationCatalogue,
                shrineSource.DiscoveryScenario))
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

        if (currentPresentation is null)
        {
            return false;
        }

        if (routesToInitialize is not null
            && !routeProgress.TryInitialize(
                routesToInitialize,
                locationCatalogue,
                shrineSource.DiscoveryScenario))
        {
            return false;
        }

        if (!TryEnsureRetainedStreamResidueIsRouted())
        {
            return false;
        }

        if (!noteEditing.TryPresent(map, notes, currentPresentation)
            || !panel.TryPresent(currentPresentation))
        {
            return false;
        }

        priorFocusOwner = GetViewport().GuiGetFocusOwner();
        seeker.SuspendForModal(this);
        close.GrabFocus();
        presentedMap = map;
        presentedNotes = notes;
        isOpen = true;
        return true;
    }

    private bool TryEnsureRetainedStreamResidueIsRouted()
    {
        if (routeProgress.Progress is not { } progress
            || streamResidue.EvidenceView is not { } evidenceView)
        {
            return false;
        }

        Allrpcs.Domain.Identifiers.EvidenceId residueId =
            streamResidue.ResidueEvidence.EvidenceId;
        return progress.Entries.Any(entry => entry.RecordedEvidenceIds.Any(candidate =>
                ReferenceEquals(candidate, residueId)))
            || routing.TryRouteVisibleEvidence(progress, evidenceView, residueId);
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
            priorFocusOwner = null;
        }, () =>
        {
            // Resume first so a presentation failure cannot leave the Seeker suspended.
            if (wasOpen) { seeker.ResumeAfterModal(this); }
            panel.Visible = false;
            if (wasOpen) { GetViewport().GuiGetFocusOwner()?.ReleaseFocus(); }
        });
    }

    private bool TryClose()
    {
        if (!isOpen || !panel.TryClose())
        {
            return false;
        }

        GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
        if (GodotObject.IsInstanceValid(priorFocusOwner) && priorFocusOwner!.IsVisibleInTree())
        { priorFocusOwner.GrabFocus(); }
        priorFocusOwner = null;
        seeker.ResumeAfterModal(this);
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
