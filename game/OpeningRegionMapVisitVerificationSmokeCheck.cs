using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks one explicit exact live opening-region visit-verification flow.</summary>
public partial class OpeningRegionMapVisitVerificationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_MAP_VISIT_VERIFICATION_CHECK";
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

        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        OpeningRegionMapVisitVerificationCoordinator coordinator =
            GetNode<OpeningRegionMapVisitVerificationCoordinator>(
                "../OpeningRegionMapVisitVerificationCoordinator");
        OpeningRegionProjection projection = GetNode<OpeningRegionProjectionSource>(
            "../OpeningRegionProjectionSource").Projection;
        TemporarySeekerCharacterSource character = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        Seeker seeker = GetNode<Seeker>("../Seeker");
        SeekerMapPresentationController presentations =
            GetNode<SeekerMapPresentationController>("../SeekerMapPresentationController");
        SeekerMapPanel panel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        JournalController journal = GetNode<JournalController>("../JournalController");
        OpeningRegionInvestigationRouteProgressController routeProgress = GetNode<
            OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController");
        OpeningRegionInvestigationRouteCompletionCoordinator routeCompletions = GetNode<
            OpeningRegionInvestigationRouteCompletionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionCoordinator");
        CharacterPerceptionSnapshot? perception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
        WorldObjectSnapshot? objects = GetNode<WorldObjectController>(
            "../WorldObjectController").WorldObjects;

        CharacterId seekerId = character.Character.Identity.Id;
        SeekerMapPresentationSnapshot? initialPresentation = presentations.Presentation;
        SeekerMapPresentationSnapshot? initialPanelPresentation = panel.Presentation;
        JournalSnapshot? initialJournal = journal.Journal;
        if (map.Map is not null
            || map.Catalogue is not null
            || map.Notes is not null
            || routeProgress.Progress is not null
            || routeCompletions.AcceptedCompletions.Count != 0
            || initialPresentation is not null
            || initialPanelPresentation is not null
            || panel.Visible)
        {
            Fail("Map visit verification changed state during ordinary startup.");
            return;
        }

        if (!map.TryInitialize(seekerId)
            || map.Catalogue is not { } catalogue
            || map.Map is not { } emptyMap
            || map.Notes is not { } emptyNotes)
        {
            Fail("Map visit verification smoke could not arrange exact map state.");
            return;
        }

        // The live scenario, so the stream arrival's residue routes into these exact routes.
        DiscoveryScenario scenario = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource").DiscoveryScenario;
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(catalogue, scenario);
        if (!routeProgress.TryInitialize(routes, catalogue, scenario)
            || routeProgress.Progress is not { } emptyRouteProgress)
        {
            Fail("Map visit verification smoke could not arrange route progress.");
            return;
        }

        JournalTestimonySectionSnapshot testimony = Testimony(
            seekerId,
            "The stream lies east of the village.");
        MapLocationDefinition stream = catalogue.Locations[4];
        MapLocationDefinition laundry = catalogue.Locations[5];
        if (!map.TryReport(emptyMap, testimony, testimony.Entries[0], stream)
            || map.Map is not { } reportedMap)
        {
            Fail("Map visit verification smoke could not arrange a reported location.");
            return;
        }

        OpeningRegionAnchor streamAnchor = projection.Anchors[4];
        OpeningRegionAnchor laundryAnchor = projection.Anchors[5];
        seeker.Position = ToGodot(streamAnchor.Position);
        NumericsVector2 streamPosition = ToNumerics(seeker.Position);
        OpeningRegionProjection foreignProjection = new(
            projection.Topology,
            projection.WorldBounds,
            projection.MinimumPathClearancePixels,
            projection.MinimumLandmarkClearancePixels,
            projection.Anchors);
        OpeningRegionAnchor foreignAnchor = new(
            streamAnchor.Location,
            streamAnchor.Position);

        if (coordinator.TryVerifyVisit(
                reportedMap,
                new CharacterId("seeker"),
                streamPosition,
                projection,
                streamAnchor,
                stream)
            || coordinator.TryVerifyVisit(
                reportedMap,
                seekerId,
                streamPosition,
                foreignProjection,
                foreignProjection.Anchors[4],
                stream)
            || coordinator.TryVerifyVisit(
                reportedMap,
                seekerId,
                streamPosition,
                projection,
                foreignAnchor,
                stream)
            || coordinator.TryVerifyVisit(
                reportedMap,
                seekerId,
                streamPosition,
                projection,
                streamAnchor,
                laundry)
            || coordinator.TryVerifyVisit(
                reportedMap,
                seekerId,
                new NumericsVector2(float.NaN, streamPosition.Y),
                projection,
                streamAnchor,
                stream)
            || coordinator.TryVerifyVisit(
                reportedMap,
                seekerId,
                streamPosition + new NumericsVector2(1.0f, 0.0f),
                projection,
                streamAnchor,
                stream)
            || !Unchanged(map, reportedMap, emptyNotes)
            || !ReferenceEquals(routeProgress.Progress, emptyRouteProgress))
        {
            Fail("Map visit verification accepted invalid live provenance or range.");
            return;
        }

        seeker.Position = ToGodot(streamPosition + new NumericsVector2(50.0f, 0.0f));
        NumericsVector2 outOfRangePosition = ToNumerics(seeker.Position);
        if (coordinator.TryVerifyVisit(
                reportedMap,
                seekerId,
                outOfRangePosition,
                projection,
                streamAnchor,
                stream)
            || !Unchanged(map, reportedMap, emptyNotes)
            || !ReferenceEquals(routeProgress.Progress, emptyRouteProgress))
        {
            Fail("Map visit verification accepted the exact current out-of-range position.");
            return;
        }

        seeker.Position = ToGodot(streamAnchor.Position);
        if (!coordinator.TryVerifyCurrentVisit()
            || !ReferenceEquals(map.Notes, emptyNotes)
            || map.Map is null)
        {
            Fail("Map visit verification failed reported-to-verified promotion or repetition.");
            return;
        }

        SeekerMapSnapshot streamVerified = map.Map;
        OpeningRegionInvestigationRouteProgressSnapshot streamRouteProgress =
            routeProgress.Progress!;
        if (streamVerified.ReportedLocations.Count != 0
            || streamVerified.VerifiedLocations.Count != 1
            || !ReferenceEquals(streamVerified.VerifiedLocations[0], stream)
            || streamRouteProgress.Entries[0].RecordedLocationIds.Count != 1
            || !ReferenceEquals(streamRouteProgress.Entries[0].RecordedLocationIds[0], stream.Id)
            || streamRouteProgress.Entries[0].RecordedEvidenceIds.Count != 1
            || streamRouteProgress.Entries[0].RecordedEvidenceIds[0].Value != "residue"
            || routeCompletions.AcceptedCompletions.Count != 0
            || ReferenceEquals(streamVerified, reportedMap)
            || coordinator.TryVerifyVisit(
                reportedMap,
                seekerId,
                streamPosition,
                projection,
                streamAnchor,
                stream)
            || coordinator.TryVerifyVisit(
                streamVerified,
                seekerId,
                streamPosition,
                projection,
                streamAnchor,
                stream)
            || !ReferenceEquals(routeProgress.Progress, streamRouteProgress))
        {
            Fail("Map visit verification accepted stale or repeated visit state.");
            return;
        }

        seeker.Position = ToGodot(laundryAnchor.Position);
        NumericsVector2 laundryPosition = ToNumerics(seeker.Position);
        if (!coordinator.TryVerifyCurrentVisit()
            || !ReferenceEquals(map.Notes, emptyNotes)
            || map.Map is null)
        {
            Fail("Map visit verification failed hidden verification.");
            return;
        }

        SeekerMapSnapshot bothVerified = map.Map;
        OpeningRegionInvestigationRouteProgressSnapshot bothRouteProgress =
            routeProgress.Progress!;
        if (bothVerified.ReportedLocations.Count != 0
            || bothVerified.VerifiedLocations.Count != 2
            || !ReferenceEquals(bothVerified.VerifiedLocations[0], stream)
            || !ReferenceEquals(bothVerified.VerifiedLocations[1], laundry)
            || bothRouteProgress.Entries[0].RecordedLocationIds.Count != 2
            || !ReferenceEquals(bothRouteProgress.Entries[0].RecordedLocationIds[0], stream.Id)
            || !ReferenceEquals(bothRouteProgress.Entries[0].RecordedLocationIds[1], laundry.Id)
            || routeCompletions.AcceptedCompletions.Count != 1
            || !ReferenceEquals(
                routeCompletions.AcceptedCompletions[0].Route,
                bothRouteProgress.Entries[0].Route)
            || !ReferenceEquals(reportedMap.ReportedLocations[0], stream)
            || streamVerified.VerifiedLocations.Count != 1
            || !ReferenceEquals(streamVerified.VerifiedLocations[0], stream)
            || !RejectsNull(() => coordinator.TryVerifyVisit(
                null!,
                seekerId,
                laundryPosition,
                projection,
                laundryAnchor,
                laundry))
            || !ReferenceEquals(presentations.Presentation, initialPresentation)
            || !ReferenceEquals(panel.Presentation, initialPanelPresentation)
            || panel.Visible
            || !ReferenceEquals(journal.Journal, initialJournal)
            || !ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                perception)
            || !ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                objects)
            || !HasClosedSurface())
        {
            Fail("Map visit verification failed hidden verification or unrelated isolation.");
            return;
        }

        GD.Print(
            "Opening-region map visit verification smoke passed: inert startup, exact live "
            + "provenance, reported promotion, hidden addition, canonical order, immutable "
            + "sources, explicit route-observation submission, stream residue and route completion, "
            + "invalid and repeated rejection, "
            + "closed surface, and unrelated isolation were correct.");
        GetTree().Quit();
    }

    private static bool Unchanged(
        SeekerMapController map,
        SeekerMapSnapshot expectedMap,
        SeekerMapNoteSnapshot expectedNotes) =>
        ReferenceEquals(map.Map, expectedMap)
        && ReferenceEquals(map.Notes, expectedNotes);

    private static NumericsVector2 ToNumerics(Godot.Vector2 position) =>
        new(position.X, position.Y);

    private static Godot.Vector2 ToGodot(NumericsVector2 position) =>
        new(position.X, position.Y);

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
        Type type = typeof(OpeningRegionMapVisitVerificationCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(["TryVerifyVisit", "_PhysicsProcess", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
