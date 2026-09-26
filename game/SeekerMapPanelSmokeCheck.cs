using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit exact read-only map-panel rendering when enabled.</summary>
public partial class SeekerMapPanelSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_MAP_PANEL_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        SeekerMapController state = GetNode<SeekerMapController>("../SeekerMapController");
        SeekerMapPresentationController presentations =
            GetNode<SeekerMapPresentationController>("../SeekerMapPresentationController");
        SeekerMapPanel panel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        Label title = panel.GetNode<Label>("Panel/Title");
        Label content = panel.GetNode<Label>("Panel/Scroll/Content");
        CanvasLayer journalPanel = GetNode<CanvasLayer>("../JournalSectionPanel");
        bool journalVisibility = journalPanel.Visible;

        if (panel.Visible
            || panel.Presentation is not null
            || title.Text.Length != 0
            || content.Text.Length != 0
            || !RejectsNull(() => panel.TryPresent(null!))
            || panel.TryClose())
        {
            Fail("Seeker map panel failed hidden empty inert startup.");
            return;
        }

        CharacterId seekerId = new("seeker");
        if (!state.TryInitialize(seekerId)
            || state.Catalogue is not { } catalogue
            || state.Map is not { } emptyMap
            || state.Notes is not { } emptyNotes
            || !presentations.TryProject(emptyMap, emptyNotes)
            || presentations.Presentation is not { } emptyPresentation
            || !panel.TryPresent(emptyPresentation)
            || !panel.Visible
            || title.Text != SeekerMapPresentationTextProjection.Title
            || content.Text != SeekerMapPresentationTextProjection.EmptyContent
            || panel.TryPresent(emptyPresentation)
            || !panel.TryClose()
            || panel.Visible
            || !panel.TryPresent(emptyPresentation))
        {
            Fail("Seeker map panel failed exact empty presentation, close, or reopen behavior.");
            return;
        }

        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerId,
            "The stream crosses the eastern path.");
        MapLocationDefinition stream = catalogue.Locations[4];
        if (!state.TryReport(emptyMap, testimony, testimony.Entries[0], stream)
            || state.Map is not { } reportedMap
            || !state.TrySetNote(
                reportedMap,
                emptyNotes,
                stream,
                "Inspect the far bank.")
            || state.Notes is not { } notes
            || !presentations.TryRefresh(reportedMap, notes, emptyPresentation)
            || presentations.Presentation is not { } currentPresentation)
        {
            Fail("Seeker map panel smoke could not arrange one safe known location.");
            return;
        }

        SeekerMapPresentationSnapshot rebuilt = SeekerMapPresentationProjection.Project(
            reportedMap,
            notes);
        string priorTitle = title.Text;
        string priorContent = content.Text;
        if (panel.TryPresent(emptyPresentation)
            || panel.TryPresent(rebuilt)
            || title.Text != priorTitle
            || content.Text != priorContent
            || !ReferenceEquals(panel.Presentation, emptyPresentation)
            || !panel.TryPresent(currentPresentation)
            || panel.Presentation is not { } rendered
            || !ReferenceEquals(rendered, currentPresentation)
            || title.Text != "Opening Region Map"
            || content.Text != "Reported: Stream\nNote: Inspect the far bank."
            || !panel.Visible
            || panel.TryPresent(currentPresentation)
            || !ReferenceEquals(state.Map, reportedMap)
            || !ReferenceEquals(state.Notes, notes)
            || journalPanel.Visible != journalVisibility
            || !HasClosedSurface())
        {
            Fail("Seeker map panel accepted stale state or failed safe exact rendering.");
            return;
        }

        GD.Print(
            "Seeker map panel smoke passed: hidden empty startup, exact safe empty and known-location "
            + "rendering, explicit close and reopen, stale, rebuilt, repeated, and null rejection, "
            + "closed surface, and map, note, and unrelated UI isolation were correct.");
        GetTree().Quit();
    }

    private static JournalTestimonySectionSnapshot Testimony(CharacterId seekerId, string text)
    {
        JournalSectionCatalogue journalCatalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            journalCatalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalSnapshot journal = JournalSnapshotProjection.Compose(
            journalCatalogue,
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
        Type type = typeof(SeekerMapPanel);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly);
        return methods.SequenceEqual(["TryClose", "TryPresent", "_Ready"])
            && properties.Length == 1
            && properties[0].Name == nameof(SeekerMapPanel.Presentation)
            && properties[0].PropertyType == typeof(SeekerMapPresentationSnapshot)
            && properties[0].SetMethod?.IsPrivate == true;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
