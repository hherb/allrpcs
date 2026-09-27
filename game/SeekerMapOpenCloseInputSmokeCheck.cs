using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit map open and close input with exact modal ownership.</summary>
public partial class SeekerMapOpenCloseInputSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_MAP_OPEN_CLOSE_INPUT_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string JournalToggleAction = "journal_toggle";
    private const string MapToggleAction = "map_toggle";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        SeekerMapOpenCloseInputController input =
            GetNode<SeekerMapOpenCloseInputController>("../SeekerMapOpenCloseInputController");
        SeekerMapController state = GetNode<SeekerMapController>("../SeekerMapController");
        OpeningRegionInvestigationRouteProgressController routeProgress = GetNode<
            OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController");
        OpeningRegionInvestigationRouteCompletionCoordinator routeCompletions = GetNode<
            OpeningRegionInvestigationRouteCompletionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionCoordinator");
        OpeningRegionOldShrineSceneSource shrineSource = GetNode<
            OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        SeekerMapPresentationController presentations =
            GetNode<SeekerMapPresentationController>("../SeekerMapPresentationController");
        SeekerMapPanel panel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        Button close = panel.GetNode<Button>("Panel/Close");
        Label title = panel.GetNode<Label>("Panel/Title");
        Label content = panel.GetNode<Label>("Panel/Scroll/Content");
        JournalOpenCloseInputController journalInput =
            GetNode<JournalOpenCloseInputController>("../JournalOpenCloseInputController");
        JournalController journal = GetNode<JournalController>("../JournalController");
        JournalSectionPanel journalPanel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        Button journalClose = journalPanel.GetNode<Button>("Panel/Close");
        TemporarySeekerCharacterSource seekerSource =
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        Seeker seeker = GetNode<Seeker>("../Seeker");
        LineEdit priorFocus = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
        FocusedSearchActivityController focusedSearch =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        WorldObjectController worldObjects = GetNode<WorldObjectController>(
            "../WorldObjectController");
        DialogueEventController dialogue = GetNode<DialogueEventController>(
            "../DialogueEventController");

        CharacterPerceptionSnapshot perception = focusedSearch.Perception
            ?? throw new InvalidOperationException("Missing initialized Seeker perception.");
        WorldObjectSnapshot objects = worldObjects.WorldObjects
            ?? throw new InvalidOperationException("Missing initialized world objects.");
        DialogueEvent? lastDialogue = dialogue.LastOccurredEvent;

        input._UnhandledInput(Action("map_unrelated", pressed: true));
        input._UnhandledInput(Action(MapToggleAction, pressed: false));
        if (state.Catalogue is not null
            || state.Map is not null
            || state.Notes is not null
            || routeProgress.Catalogue is not null
            || routeProgress.Progress is not null
            || routeCompletions.AcceptedCompletions.Count != 0
            || presentations.Presentation is not null
            || panel.Presentation is not null
            || panel.Visible
            || close.HasFocus()
            || !seeker.IsPhysicsProcessing()
            || !HasOnlyPhysicalKey(MapToggleAction, Key.M)
            || !HasOnlyPhysicalKey("healer_move", Key.H)
            || !RejectsNull(() => input._UnhandledInput(null!)))
        {
            Fail("Map input changed state during inert startup or has incorrect key mappings.");
            return;
        }

        journalInput._UnhandledInput(Action(JournalToggleAction, pressed: true));
        JournalSnapshot openedJournal = journal.Journal
            ?? throw new InvalidOperationException("Journal did not open for overlap rejection.");
        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        if (!journalPanel.Visible
            || !journalClose.HasFocus()
            || state.Map is not null
            || state.Notes is not null
            || routeProgress.Progress is not null
            || presentations.Presentation is not null
            || panel.Visible
            || seeker.IsPhysicsProcessing())
        {
            Fail("Map input changed state while the journal owned modal interaction.");
            return;
        }

        journalClose.EmitSignal(BaseButton.SignalName.Pressed);
        if (journalPanel.Visible || !seeker.IsPhysicsProcessing())
        {
            Fail("Journal overlap arrangement did not close cleanly.");
            return;
        }

        priorFocus.GrabFocus();
        seeker.Velocity = new Vector2(24.0f, -12.0f);
        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        SeekerMapSnapshot emptyMap = state.Map
            ?? throw new InvalidOperationException("Map input did not initialize map state.");
        SeekerMapNoteSnapshot emptyNotes = state.Notes
            ?? throw new InvalidOperationException("Map input did not initialize notes.");
        SeekerMapPresentationSnapshot emptyPresentation = presentations.Presentation
            ?? throw new InvalidOperationException("Map input did not project empty state.");
        OpeningRegionInvestigationRouteProgressSnapshot emptyRouteProgress =
            routeProgress.Progress
            ?? throw new InvalidOperationException("Map input did not initialize route progress.");
        if (!ReferenceEquals(emptyMap.SeekerId, seekerSource.Character.Identity.Id)
            || !ReferenceEquals(emptyNotes.SeekerId, seekerSource.Character.Identity.Id)
            || emptyMap.ReportedLocations.Count != 0
            || emptyMap.VerifiedLocations.Count != 0
            || emptyNotes.Notes.Count != 0
            || emptyPresentation.Locations.Count != 0
            || !HasExactEmptyRoutes(
                emptyRouteProgress,
                state.Catalogue!,
                shrineSource.DiscoveryScenario)
            || routeCompletions.AcceptedCompletions.Count != 0
            || !ReferenceEquals(panel.Presentation, emptyPresentation)
            || !panel.Visible
            || title.Text != SeekerMapPresentationTextProjection.Title
            || content.Text != SeekerMapPresentationTextProjection.EmptyContent
            || seeker.IsPhysicsProcessing()
            || seeker.Velocity != Vector2.Zero
            || !close.HasFocus()
            || journalPanel.Visible)
        {
            Fail("Map input did not open exact empty current-Seeker state modally.");
            return;
        }

        input._UnhandledInput(Action("map_unrelated", pressed: true));
        input._UnhandledInput(Action(MapToggleAction, pressed: false));
        if (!panel.Visible
            || !ReferenceEquals(state.Map, emptyMap)
            || !ReferenceEquals(state.Notes, emptyNotes)
            || !ReferenceEquals(routeProgress.Progress, emptyRouteProgress)
            || !ReferenceEquals(presentations.Presentation, emptyPresentation)
            || !ReferenceEquals(panel.Presentation, emptyPresentation)
            || !close.HasFocus()
            || seeker.IsPhysicsProcessing())
        {
            Fail("Released or unrelated input changed the open exact map state.");
            return;
        }

        close.EmitSignal(BaseButton.SignalName.Pressed);
        close.EmitSignal(BaseButton.SignalName.Pressed);
        if (panel.Visible
            || !seeker.IsPhysicsProcessing()
            || !priorFocus.HasFocus()
            || !ReferenceEquals(panel.Presentation, emptyPresentation))
        {
            Fail("Map close control did not restore exact prior focus and movement state.");
            return;
        }

        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerSource.Character.Identity.Id,
            "The stream crosses the eastern path.");
        MapLocationDefinition stream = state.Catalogue!.Locations[4];
        if (!state.TryReport(emptyMap, testimony, testimony.Entries[0], stream)
            || state.Map is not { } reportedMap
            || !state.TrySetNote(reportedMap, emptyNotes, stream, "Inspect the far bank.")
            || state.Notes is not { } notes)
        {
            Fail("Map input smoke could not arrange changed closed map state.");
            return;
        }

        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        SeekerMapPresentationSnapshot changedPresentation = presentations.Presentation
            ?? throw new InvalidOperationException("Map input did not refresh changed state.");
        if (ReferenceEquals(changedPresentation, emptyPresentation)
            || !ReferenceEquals(panel.Presentation, changedPresentation)
            || changedPresentation.Locations.Count != 1
            || content.Text != "Reported: Stream\nNote: Inspect the far bank."
            || !panel.Visible
            || !close.HasFocus())
        {
            Fail("Map input did not refresh and render exact changed sources on reopen.");
            return;
        }

        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        if (panel.Visible || !priorFocus.HasFocus() || !seeker.IsPhysicsProcessing())
        {
            Fail("Map toggle did not close and restore exact prior ownership.");
            return;
        }

        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        if (!panel.Visible
            || !ReferenceEquals(presentations.Presentation, changedPresentation)
            || !ReferenceEquals(panel.Presentation, changedPresentation))
        {
            Fail("Map input did not reopen the exact retained unchanged presentation.");
            return;
        }

        journalInput._UnhandledInput(Action(JournalToggleAction, pressed: true));
        if (!panel.Visible
            || journalPanel.Visible
            || !ReferenceEquals(journal.Journal, openedJournal)
            || !ReferenceEquals(state.Map, reportedMap)
            || !ReferenceEquals(state.Notes, notes)
            || !ReferenceEquals(routeProgress.Progress, emptyRouteProgress)
            || !ReferenceEquals(presentations.Presentation, changedPresentation))
        {
            Fail("Journal input changed state while the map owned modal interaction.");
            return;
        }

        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        seeker.SetPhysicsProcess(false);
        priorFocus.ReleaseFocus();
        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        input._UnhandledInput(Action(MapToggleAction, pressed: true));
        bool restoredSuspendedMovement = !seeker.IsPhysicsProcessing();
        seeker.SetPhysicsProcess(true);

        if (!restoredSuspendedMovement
            || GetViewport().GuiGetFocusOwner() is not null
            || !ReferenceEquals(state.Map, reportedMap)
            || !ReferenceEquals(state.Notes, notes)
            || !ReferenceEquals(presentations.Presentation, changedPresentation)
            || !ReferenceEquals(panel.Presentation, changedPresentation)
            || !ReferenceEquals(routeProgress.Progress, emptyRouteProgress)
            || routeCompletions.AcceptedCompletions.Count != 0
            || !ReferenceEquals(focusedSearch.Perception, perception)
            || !ReferenceEquals(worldObjects.WorldObjects, objects)
            || !ReferenceEquals(dialogue.LastOccurredEvent, lastDialogue)
            || !HasClosedSurface())
        {
            Fail("Map input changed gameplay state or prior movement and focus ownership.");
            return;
        }

        GD.Print(
            "Seeker map open-close input smoke passed: inert startup, exact current-Seeker "
            + "map and investigation-route initialization, changed-source refresh, unchanged "
            + "retained reopen, physical M, "
            + "focus and movement ownership, close and toggle paths, journal exclusion, released "
            + "and unrelated rejection, closed surface, and gameplay isolation were correct.");
        GetTree().Quit();
    }

    private static JournalTestimonySectionSnapshot Testimony(
        Allrpcs.Domain.Identifiers.CharacterId seekerId,
        string text)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalSnapshot journal = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
        Allrpcs.Domain.Identifiers.CharacterId speakerId = new("healer");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(speakerId, text),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerId, [seekerId]));
        return JournalTestimonySectionSnapshotProjection.Project(
            JournalTestimonySectionProjection.Bind(journal, [dialogueEvent]));
    }

    private static InputEventAction Action(string action, bool pressed) => new()
    {
        Action = action,
        Pressed = pressed,
    };

    private static bool HasExactEmptyRoutes(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionMapLocationCatalogue locations,
        Allrpcs.Domain.Discovery.DiscoveryScenario scenario) =>
        progress.Entries.Count == 3
        && progress.Entries.All(entry => entry.RecordedLocationIds.Count == 0
            && entry.RecordedEvidenceIds.Count == 0
            && entry.Route.LocationIds.All(locationId => locations.Locations.Any(location =>
                ReferenceEquals(location.Id, locationId)))
            && entry.Route.EvidenceIds.All(evidenceId => scenario.Evidence.Entries.Any(evidence =>
                ReferenceEquals(evidence.EvidenceId, evidenceId))));

    private static bool HasOnlyPhysicalKey(string actionName, Key expectedKey)
    {
        StringName action = new(actionName);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey keyEvent
            && keyEvent.PhysicalKeycode == expectedKey;
    }

    private static bool RejectsNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(SeekerMapOpenCloseInputController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(["_Ready", "_UnhandledInput"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
