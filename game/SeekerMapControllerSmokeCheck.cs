using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free exact scene-owned map-state checks when explicitly enabled.</summary>
public partial class SeekerMapControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_MAP_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const float VisitDistancePixels = 20.0f;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        SeekerMapController controller = GetNode<SeekerMapController>("../SeekerMapController");
        OpeningRegionProjection projection = GetNode<OpeningRegionProjectionSource>(
            "../OpeningRegionProjectionSource").Projection;
        JournalController unrelatedJournal = GetNode<JournalController>("../JournalController");
        JournalSnapshot? initialJournal = unrelatedJournal.Journal;

        if (controller.Catalogue is not null
            || controller.Map is not null
            || controller.Notes is not null
            || !RejectsNull(() => controller.TryInitialize(null!)))
        {
            Fail("Seeker map controller failed inert startup or null initialization rejection.");
            return;
        }

        CharacterId seekerId = new("seeker");
        if (!controller.TryInitialize(seekerId)
            || controller.Catalogue is not { } catalogue
            || controller.Map is not { } emptyMap
            || controller.Notes is not { } emptyNotes
            || !ReferenceEquals(emptyMap.SeekerId, seekerId)
            || !ReferenceEquals(emptyNotes.SeekerId, seekerId)
            || emptyMap.ReportedLocations.Count != 0
            || emptyMap.VerifiedLocations.Count != 0
            || emptyNotes.Notes.Count != 0
            || controller.TryInitialize(seekerId)
            || controller.TryInitialize(new CharacterId("seeker")))
        {
            Fail("Seeker map controller failed exact atomic one-shot initialization.");
            return;
        }

        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerId,
            "The abandoned laundry lies beyond the stream.");
        JournalTestimonySectionSnapshot rebuiltTestimony = Testimony(
            seekerId,
            "The abandoned laundry lies beyond the stream.");
        MapLocationDefinition stream = catalogue.Locations[4];
        MapLocationDefinition laundry = catalogue.Locations[5];
        if (controller.TryReport(
                emptyMap,
                testimony,
                rebuiltTestimony.Entries[0],
                stream)
            || !StateIs(controller, catalogue, emptyMap, emptyNotes)
            || !controller.TryReport(
                emptyMap,
                testimony,
                testimony.Entries[0],
                stream)
            || controller.Map is not { } reportedMap
            || !ReferenceEquals(AssertSingle(reportedMap.ReportedLocations), stream)
            || !ReferenceEquals(controller.Notes, emptyNotes)
            || controller.TryReport(
                emptyMap,
                testimony,
                testimony.Entries[0],
                laundry)
            || controller.TryReport(
                reportedMap,
                testimony,
                testimony.Entries[0],
                stream))
        {
            Fail("Seeker map controller failed exact testimony-report compare-and-swap behavior.");
            return;
        }

        OpeningRegionAnchor streamAnchor = projection.Anchors[4];
        OpeningRegionLocationVisit visit = OpeningRegionLocationVisitProjection.Project(
            projection,
            seekerId,
            streamAnchor,
            streamAnchor.Position,
            VisitDistancePixels)!;
        OpeningRegionLocationVisit rebuiltOwnerVisit = OpeningRegionLocationVisitProjection.Project(
            projection,
            new CharacterId("seeker"),
            streamAnchor,
            streamAnchor.Position,
            VisitDistancePixels)!;
        if (controller.TryVerifyVisit(
                reportedMap,
                projection,
                rebuiltOwnerVisit,
                stream)
            || controller.TryVerifyVisit(
                reportedMap,
                projection,
                visit,
                laundry)
            || !StateIs(controller, catalogue, reportedMap, emptyNotes)
            || !controller.TryVerifyVisit(
                reportedMap,
                projection,
                visit,
                stream)
            || controller.Map is not { } verifiedMap
            || verifiedMap.ReportedLocations.Count != 0
            || !ReferenceEquals(AssertSingle(verifiedMap.VerifiedLocations), stream)
            || !ReferenceEquals(controller.Notes, emptyNotes)
            || controller.TryVerifyVisit(
                reportedMap,
                projection,
                visit,
                stream)
            || controller.TryVerifyVisit(
                verifiedMap,
                projection,
                visit,
                stream))
        {
            Fail("Seeker map controller failed exact visit-verification compare-and-swap behavior.");
            return;
        }

        if (controller.TrySetNote(
                verifiedMap,
                emptyNotes,
                laundry,
                "Hidden location note.")
            || controller.TrySetNote(
                verifiedMap,
                emptyNotes,
                stream,
                " \t\r\n")
            || !StateIs(controller, catalogue, verifiedMap, emptyNotes)
            || !controller.TrySetNote(
                verifiedMap,
                emptyNotes,
                stream,
                "Ask the healer about the water.")
            || controller.Notes is not { } firstNotes
            || !ReferenceEquals(AssertSingle(firstNotes.Notes).Location, stream)
            || AssertSingle(firstNotes.Notes).Text != "Ask the healer about the water."
            || !ReferenceEquals(controller.Map, verifiedMap)
            || controller.TrySetNote(
                verifiedMap,
                emptyNotes,
                stream,
                "Stale replacement.")
            || !controller.TrySetNote(
                verifiedMap,
                firstNotes,
                stream,
                "Recheck the stream bank."))
        {
            Fail("Seeker map controller failed exact note set or replacement behavior.");
            return;
        }

        SeekerMapNoteSnapshot replacedNotes = controller.Notes!;
        if (AssertSingle(replacedNotes.Notes).Text != "Recheck the stream bank."
            || controller.TryRemoveNote(verifiedMap, firstNotes, stream)
            || controller.TryRemoveNote(verifiedMap, replacedNotes, laundry)
            || !StateIs(controller, catalogue, verifiedMap, replacedNotes)
            || !controller.TryRemoveNote(verifiedMap, replacedNotes, stream)
            || controller.Notes is not { } removedNotes
            || removedNotes.Notes.Count != 0
            || !ReferenceEquals(controller.Map, verifiedMap)
            || controller.TryRemoveNote(verifiedMap, replacedNotes, stream)
            || controller.TryRemoveNote(verifiedMap, removedNotes, stream)
            || !ReferenceEquals(unrelatedJournal.Journal, initialJournal)
            || !HasClosedSurface())
        {
            Fail("Seeker map controller failed exact note removal or scene-state isolation.");
            return;
        }

        if (!RejectsNull(() => controller.TryReport(null!, testimony, testimony.Entries[0], stream))
            || !RejectsNull(() => controller.TryVerifyVisit(null!, projection, visit, stream))
            || !RejectsNull(() => controller.TrySetNote(verifiedMap, null!, stream, "Note."))
            || !RejectsNull(() => controller.TryRemoveNote(verifiedMap, null!, stream)))
        {
            Fail("Seeker map controller failed null operation rejection.");
            return;
        }

        GD.Print(
            "Seeker map controller smoke passed: inert startup, atomic exact initialization, "
            + "testimony reporting, visit verification, note set, replacement, removal, "
            + "stale, rebuilt, foreign, repeated, hidden, invalid, and null rejection, "
            + "closed public surface, and unrelated state isolation were correct.");
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

    private static bool StateIs(
        SeekerMapController controller,
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes) =>
        ReferenceEquals(controller.Catalogue, catalogue)
        && ReferenceEquals(controller.Map, map)
        && ReferenceEquals(controller.Notes, notes);

    private static T AssertSingle<T>(IReadOnlyList<T> values) => values.Count == 1
        ? values[0]
        : throw new InvalidOperationException("Expected one retained value.");

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
        Type type = typeof(SeekerMapController);
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
        return properties.Length == 3
            && properties.All(property => property.CanRead && property.SetMethod?.IsPrivate == true)
            && methods.Length == 5
            && methods.All(method => method.ReturnType == typeof(bool));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
