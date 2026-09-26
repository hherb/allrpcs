using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit bounded note editing over only safe known map locations.</summary>
public partial class SeekerMapNoteEditingSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_MAP_NOTE_EDITING_CHECK";
    private const string EnabledEnvironmentValue = "1";

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

        SeekerMapController state = GetNode<SeekerMapController>("../SeekerMapController");
        SeekerMapPresentationController presentations =
            GetNode<SeekerMapPresentationController>("../SeekerMapPresentationController");
        SeekerMapPanel panel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        SeekerMapNoteEditingController editing =
            GetNode<SeekerMapNoteEditingController>("../SeekerMapNoteEditingController");
        OptionButton locations = panel.GetNode<OptionButton>("Panel/Locations");
        LineEdit note = panel.GetNode<LineEdit>("Panel/Note");
        Button save = panel.GetNode<Button>("Panel/SaveNote");
        Button remove = panel.GetNode<Button>("Panel/RemoveNote");
        Label content = panel.GetNode<Label>("Panel/Scroll/Content");
        JournalController journal = GetNode<JournalController>("../JournalController");
        JournalSnapshot? initialJournal = journal.Journal;

        CharacterId seekerId = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource").Character.Identity.Id;
        if (!state.TryInitialize(seekerId)
            || state.Catalogue is not { } catalogue
            || state.Map is not { } emptyMap
            || state.Notes is not { } emptyNotes
            || !presentations.TryProject(emptyMap, emptyNotes)
            || presentations.Presentation is not { } emptyPresentation
            || !editing.TryPresent(emptyMap, emptyNotes, emptyPresentation)
            || !panel.TryPresent(emptyPresentation)
            || locations.ItemCount != 1
            || !locations.IsItemDisabled(0)
            || locations.GetItemText(0) != "No known locations"
            || note.Text.Length != 0
            || !note.Editable
            || !save.Disabled
            || !remove.Disabled
            || note.MaxLength != SeekerMapNote.MaximumTextLength)
        {
            Fail("Map note editing failed the safe empty-location presentation.");
            return;
        }

        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerId,
            "The stream and abandoned laundry lie east of the village.");
        MapLocationDefinition stream = catalogue.Locations[4];
        MapLocationDefinition laundry = catalogue.Locations[5];
        if (!state.TryReport(emptyMap, testimony, testimony.Entries[0], laundry)
            || state.Map is not { } laundryMap
            || !state.TryReport(laundryMap, testimony, testimony.Entries[0], stream)
            || state.Map is not { } knownMap
            || !presentations.TryRefresh(knownMap, emptyNotes, emptyPresentation)
            || presentations.Presentation is not { } knownPresentation
            || !editing.TryPresent(knownMap, emptyNotes, knownPresentation)
            || !panel.TryPresent(knownPresentation)
            || locations.ItemCount != 2
            || locations.GetItemText(0) != stream.DisplayLabel
            || locations.GetItemText(1) != laundry.DisplayLabel
            || locations.Selected != 0
            || note.Text.Length != 0
            || save.Disabled
            || !remove.Disabled)
        {
            Fail("Map note editing exposed hidden locations or lost canonical known order.");
            return;
        }

        note.Text = " \t ";
        save.EmitSignal(BaseButton.SignalName.Pressed);
        if (!ReferenceEquals(state.Notes, emptyNotes)
            || !ReferenceEquals(presentations.Presentation, knownPresentation)
            || !ReferenceEquals(panel.Presentation, knownPresentation))
        {
            Fail("Map note editing accepted whitespace or changed exact state on rejection.");
            return;
        }

        note.Text = "Inspect the far bank.";
        save.EmitSignal(BaseButton.SignalName.Pressed);
        SeekerMapNoteSnapshot streamNotes = state.Notes
            ?? throw new InvalidOperationException("Map note editing did not retain notes.");
        SeekerMapPresentationSnapshot streamPresentation = presentations.Presentation
            ?? throw new InvalidOperationException("Map note editing did not refresh presentation.");
        if (ReferenceEquals(streamNotes, emptyNotes)
            || streamNotes.Notes.Count != 1
            || !ReferenceEquals(streamNotes.Notes[0].Location, stream)
            || streamNotes.Notes[0].Text != "Inspect the far bank."
            || ReferenceEquals(streamPresentation, knownPresentation)
            || !ReferenceEquals(panel.Presentation, streamPresentation)
            || locations.Selected != 0
            || note.Text != "Inspect the far bank."
            || remove.Disabled
            || content.Text != "Reported: Stream\nNote: Inspect the far bank.\n\nReported: Abandoned Laundry")
        {
            Fail("Map note editing failed exact bounded save and safe refresh.");
            return;
        }

        locations.Select(1);
        locations.EmitSignal(OptionButton.SignalName.ItemSelected, 1L);
        if (note.Text.Length != 0 || remove.Disabled is false)
        {
            Fail("Map note editing did not load the exact selected known location.");
            return;
        }

        note.Text = new string('n', SeekerMapNote.MaximumTextLength);
        save.EmitSignal(BaseButton.SignalName.Pressed);
        SeekerMapNoteSnapshot bothNotes = state.Notes
            ?? throw new InvalidOperationException("Second bounded note was not retained.");
        if (bothNotes.Notes.Count != 2
            || !ReferenceEquals(bothNotes.Notes[1].Location, laundry)
            || bothNotes.Notes[1].Text.Length != SeekerMapNote.MaximumTextLength
            || note.Text.Length != SeekerMapNote.MaximumTextLength)
        {
            Fail("Map note editing failed the inclusive maximum note length.");
            return;
        }

        locations.Select(0);
        locations.EmitSignal(OptionButton.SignalName.ItemSelected, 0L);
        remove.EmitSignal(BaseButton.SignalName.Pressed);
        SeekerMapNoteSnapshot laundryOnlyNotes = state.Notes
            ?? throw new InvalidOperationException("Map note removal lost note state.");
        SeekerMapPresentationSnapshot laundryOnlyPresentation = presentations.Presentation
            ?? throw new InvalidOperationException("Map note removal lost presentation.");
        if (laundryOnlyNotes.Notes.Count != 1
            || !ReferenceEquals(laundryOnlyNotes.Notes[0].Location, laundry)
            || note.Text.Length != 0
            || remove.Disabled is false
            || !ReferenceEquals(panel.Presentation, laundryOnlyPresentation))
        {
            Fail("Map note editing failed exact removal and selection retention.");
            return;
        }

        if (!state.TrySetNote(
                knownMap,
                laundryOnlyNotes,
                stream,
                "Externally changed note."))
        {
            Fail("Map note editing smoke could not arrange stale editor state.");
            return;
        }

        SeekerMapNoteSnapshot externallyChanged = state.Notes!;
        bool stalePresentationAccepted = editing.TryPresent(
            knownMap,
            externallyChanged,
            laundryOnlyPresentation);
        note.Text = "Must be rejected while stale.";
        save.EmitSignal(BaseButton.SignalName.Pressed);
        remove.EmitSignal(BaseButton.SignalName.Pressed);
        if (stalePresentationAccepted
            || !ReferenceEquals(state.Notes, externallyChanged)
            || !ReferenceEquals(presentations.Presentation, laundryOnlyPresentation)
            || !ReferenceEquals(panel.Presentation, laundryOnlyPresentation)
            || !ReferenceEquals(state.Map, knownMap)
            || !ReferenceEquals(journal.Journal, initialJournal)
            || !HasClosedSurface())
        {
            Fail("Map note editing accepted stale UI or changed unrelated state.");
            return;
        }

        GD.Print(
            "Seeker map note editing smoke passed: safe empty state, known-only canonical "
            + "selection, exact save, maximum-length replacement, removal, refreshed rendering, "
            + "whitespace and stale rejection, closed surface, and unrelated isolation were "
            + "correct.");
        GetTree().Quit();
    }

    private static JournalTestimonySectionSnapshot Testimony(CharacterId seekerId, string text)
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
        CharacterId speakerId = new("healer");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(speakerId, text),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerId, [seekerId]));
        return JournalTestimonySectionSnapshotProjection.Project(
            JournalTestimonySectionProjection.Bind(journal, [dialogueEvent]));
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(SeekerMapNoteEditingController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(["TryPresent", "_Ready"])
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
