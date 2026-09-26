using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene-owned safe map presentation when explicitly enabled.</summary>
public partial class SeekerMapPresentationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_MAP_PRESENTATION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        SeekerMapController state = GetNode<SeekerMapController>("../SeekerMapController");
        SeekerMapPresentationController presentation =
            GetNode<SeekerMapPresentationController>("../SeekerMapPresentationController");
        CanvasLayer journalPanel = GetNode<CanvasLayer>("../JournalSectionPanel");
        bool initialJournalVisibility = journalPanel.Visible;

        if (presentation.Presentation is not null
            || !RejectsNull(() => presentation.TryProject(null!, null!))
            || !RejectsNull(() => presentation.TryRefresh(null!, null!, null!)))
        {
            Fail("Map presentation controller failed inert startup or null rejection.");
            return;
        }

        CharacterId seekerId = new("seeker");
        if (!state.TryInitialize(seekerId)
            || state.Catalogue is not { } catalogue
            || state.Map is not { } emptyMap
            || state.Notes is not { } emptyNotes)
        {
            Fail("Map presentation smoke could not initialize exact map state.");
            return;
        }

        OpeningRegionMapLocationCatalogue foreignCatalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        SeekerMapSnapshot foreignMap = SeekerMapSnapshotProjection.Project(
            foreignCatalogue,
            seekerId,
            [],
            []);
        SeekerMapNoteSnapshot foreignNotes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            foreignCatalogue,
            seekerId);
        if (presentation.TryProject(foreignMap, foreignNotes)
            || !presentation.TryProject(emptyMap, emptyNotes)
            || presentation.Presentation is not { } emptyPresentation
            || emptyPresentation.Locations.Count != 0
            || !ReferenceEquals(emptyPresentation.SeekerId, seekerId)
            || presentation.TryProject(emptyMap, emptyNotes))
        {
            Fail("Map presentation controller failed exact one-shot empty projection.");
            return;
        }

        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerId,
            "The stream crosses the eastern path.");
        MapLocationDefinition stream = catalogue.Locations[4];
        if (!state.TryReport(emptyMap, testimony, testimony.Entries[0], stream)
            || state.Map is not { } reportedMap
            || presentation.TryRefresh(
                emptyMap,
                emptyNotes,
                emptyPresentation)
            || presentation.TryRefresh(
                reportedMap,
                emptyNotes,
                SeekerMapPresentationProjection.Project(reportedMap, emptyNotes))
            || !ReferenceEquals(presentation.Presentation, emptyPresentation)
            || !presentation.TryRefresh(
                reportedMap,
                emptyNotes,
                emptyPresentation)
            || presentation.Presentation is not { } reportedPresentation
            || ReferenceEquals(reportedPresentation, emptyPresentation)
            || reportedPresentation.Locations.Count != 1
            || !ReferenceEquals(reportedPresentation.Locations[0].Location, stream)
            || reportedPresentation.Locations[0].IsVerified
            || reportedPresentation.Locations[0].NoteText is not null
            || presentation.TryRefresh(
                reportedMap,
                emptyNotes,
                reportedPresentation))
        {
            Fail("Map presentation controller failed exact report-state refresh.");
            return;
        }

        if (!state.TrySetNote(
                reportedMap,
                emptyNotes,
                stream,
                "Check the far bank.")
            || state.Notes is not { } noted
            || presentation.TryRefresh(
                reportedMap,
                noted,
                emptyPresentation)
            || !presentation.TryRefresh(
                reportedMap,
                noted,
                reportedPresentation)
            || presentation.Presentation is not { } notedPresentation
            || notedPresentation.Locations.Count != 1
            || notedPresentation.Locations[0].NoteText != "Check the far bank."
            || reportedPresentation.Locations[0].NoteText is not null
            || emptyPresentation.Locations.Count != 0
            || !ReferenceEquals(state.Map, reportedMap)
            || !ReferenceEquals(state.Notes, noted)
            || journalPanel.Visible != initialJournalVisibility
            || !HasClosedSurface())
        {
            Fail("Map presentation controller failed note refresh or state isolation.");
            return;
        }

        GD.Print(
            "Seeker map presentation controller smoke passed: inert startup, exact one-shot "
            + "projection, explicit report and note refresh, stale, rebuilt, unchanged, repeated, "
            + "and null rejection, immutable prior presentations, closed surface, and UI isolation "
            + "were correct.");
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
        Type type = typeof(SeekerMapPresentationController);
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        return properties.Length == 1
            && properties[0].Name == nameof(SeekerMapPresentationController.Presentation)
            && properties[0].PropertyType == typeof(SeekerMapPresentationSnapshot)
            && properties[0].CanRead
            && properties[0].SetMethod?.IsPrivate == true
            && methods.Length == 3
            && methods.Any(method => method.Name == nameof(Node._Ready))
            && methods.Any(method =>
                method.Name == nameof(SeekerMapPresentationController.TryProject)
                && method.ReturnType == typeof(bool)
                && method.GetParameters().Length == 2)
            && methods.Any(method =>
                method.Name == nameof(SeekerMapPresentationController.TryRefresh)
                && method.ReturnType == typeof(bool)
                && method.GetParameters().Length == 3);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
