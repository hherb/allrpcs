using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks catalogue-ordered routing of exact authoritative observations when enabled.</summary>
public partial class OpeningRegionInvestigationRouteObservationRoutingCoordinatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_OBSERVATION_ROUTING_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        OpeningRegionInvestigationRouteProgressController progress = GetNode<
            OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController");
        OpeningRegionInvestigationRouteObservationRoutingCoordinator routing = GetNode<
            OpeningRegionInvestigationRouteObservationRoutingCoordinator>(
                "../OpeningRegionInvestigationRouteObservationRoutingCoordinator");
        OpeningRegionInvestigationRouteCompletionCoordinator completions = GetNode<
            OpeningRegionInvestigationRouteCompletionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionCoordinator");
        OpeningRegionInvestigationRouteCompletionConsumptionCoordinator consumption = GetNode<
            OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        JournalController journal = GetNode<JournalController>("../JournalController");
        FocusedSearchActivityController discovery = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        JournalSnapshot? initialJournal = journal.Journal;
        CharacterPerceptionSnapshot? initialPerception = discovery.Perception;
        if (progress.Progress is not null
            || completions.AcceptedCompletions.Count != 0
            || consumption.ConsumedCompletion is not null
            || map.Map is not null)
        {
            Fail("Observation routing changed state during ordinary startup.");
            return;
        }

        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes = CreateSharedRoutes(locations, scenario);
        OpeningRegionInvestigationRoute first = routes.Routes[0];
        OpeningRegionInvestigationRoute second = routes.Routes[1];
        OpeningRegionInvestigationRoute third = routes.Routes[2];
        MapLocationDefinition stream = FindLocation(locations, "stream");
        MapLocationDefinition laundry = FindLocation(locations, "abandoned-laundry");
        MapLocationDefinition shrine = FindLocation(locations, "old-shrine");
        MapLocationDefinition road = FindLocation(locations, "southern-road");
        MapLocationDefinition nonmemberLocation = FindLocation(locations, "sickhouse");
        EvidenceId residue = FindEvidence(scenario, "residue");
        EvidenceId partialInscription = FindEvidence(scenario, "partial-inscription");
        EvidenceId nonmemberEvidence = FindEvidence(scenario, "visible-herb");
        CharacterId seekerId = scenario.ObserverCharacterId;
        SeekerMapSnapshot currentMap = SeekerMapSnapshotProjection.Project(
            locations,
            seekerId,
            [],
            [stream, laundry, shrine]);
        ((IMapSaveStateRestorationTarget)map).InstallRestored(
            locations,
            currentMap,
            SeekerMapNoteSnapshotProjection.CreateEmpty(locations, seekerId));
        if (!progress.TryInitialize(routes, locations, scenario)
            || progress.Progress is not { } empty
            || !progress.TryRecordLocation(empty, first, laundry.Id)
            || !progress.TryRecordLocation(progress.Progress!, second, shrine.Id)
            || !progress.TryRecordEvidence(progress.Progress!, second, partialInscription)
            || progress.Progress is not { } beforeLocation)
        {
            Fail("Observation routing could not arrange shared-route progress.");
            return;
        }

        SeekerMapSnapshot rebuiltMap = SeekerMapSnapshotProjection.Project(
            locations,
            seekerId,
            [],
            [stream, laundry, shrine]);
        MapLocationDefinition rebuiltStream = new(stream.Id, stream.DisplayLabel);
        OpeningRegionMapLocationCatalogue foreignLocations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        MapLocationDefinition foreignStream = FindLocation(foreignLocations, "stream");
        if (!ThrowsArgumentNull(() => routing.TryRouteVerifiedLocation(
                null!,
                currentMap,
                stream))
            || !ThrowsArgumentNull(() => routing.TryRouteVerifiedLocation(
                beforeLocation,
                null!,
                stream))
            || !ThrowsArgumentNull(() => routing.TryRouteVerifiedLocation(
                beforeLocation,
                currentMap,
                null!))
            || routing.TryRouteVerifiedLocation(beforeLocation, rebuiltMap, stream)
            || routing.TryRouteVerifiedLocation(beforeLocation, currentMap, rebuiltStream)
            || routing.TryRouteVerifiedLocation(beforeLocation, currentMap, foreignStream)
            || routing.TryRouteVerifiedLocation(beforeLocation, currentMap, road)
            || routing.TryRouteVerifiedLocation(beforeLocation, currentMap, nonmemberLocation)
            || !routing.TryRouteVerifiedLocation(beforeLocation, currentMap, stream))
        {
            Fail("Observation routing rejected exact catalogue-ordered location routing.");
            return;
        }

        OpeningRegionInvestigationRouteProgressSnapshot afterLocation = progress.Progress!;
        if (!HasRecordedLocation(afterLocation, first, stream.Id)
            || !HasRecordedLocation(afterLocation, second, stream.Id)
            || afterLocation.Entries.Single(entry => ReferenceEquals(entry.Route, third))
                .RecordedLocationIds.Count != 0
            || completions.AcceptedCompletions.Count != 0)
        {
            Fail("Observation routing failed exact catalogue-ordered location routing.");
            return;
        }

        if (routing.TryRouteVerifiedLocation(beforeLocation, currentMap, stream)
            || routing.TryRouteVerifiedLocation(afterLocation, currentMap, stream)
            || !ReferenceEquals(progress.Progress, afterLocation)
            || completions.AcceptedCompletions.Count != 0)
        {
            Fail("Observation routing changed state for stale or repeated locations.");
            return;
        }

        CharacterEvidenceViewSnapshot visible = new(seekerId, [residue]);
        CharacterEvidenceViewSnapshot unseen = new(seekerId, []);
        DiscoveryScenario foreignScenario = RepresentativeDiscoveryScenarioSource.Create();
        EvidenceId foreignResidue = FindEvidence(foreignScenario, "residue");
        if (!ThrowsArgumentNull(() => routing.TryRouteVisibleEvidence(null!, visible, residue))
            || !ThrowsArgumentNull(() => routing.TryRouteVisibleEvidence(
                afterLocation,
                null!,
                residue))
            || !ThrowsArgumentNull(() => routing.TryRouteVisibleEvidence(
                afterLocation,
                visible,
                null!))
            || routing.TryRouteVisibleEvidence(afterLocation, unseen, residue)
            || routing.TryRouteVisibleEvidence(
                afterLocation,
                visible,
                new EvidenceId(residue.Value))
            || routing.TryRouteVisibleEvidence(afterLocation, visible, foreignResidue)
            || routing.TryRouteVisibleEvidence(afterLocation, visible, nonmemberEvidence)
            || !routing.TryRouteVisibleEvidence(afterLocation, visible, residue))
        {
            Fail("Observation routing rejected exact catalogue-ordered evidence routing.");
            return;
        }

        OpeningRegionInvestigationRouteProgressSnapshot afterEvidence = progress.Progress!;
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> accepted =
            completions.AcceptedCompletions;
        if (!HasRecordedEvidence(afterEvidence, first, residue)
            || !HasRecordedEvidence(afterEvidence, second, residue)
            || afterEvidence.Entries.Single(entry => ReferenceEquals(entry.Route, third))
                .RecordedEvidenceIds.Count != 0
            || accepted.Count != 2
            || !ReferenceEquals(accepted[0].Route, first)
            || !ReferenceEquals(accepted[1].Route, second)
            || !ReferenceEquals(accepted[1].ProgressSnapshot, afterEvidence))
        {
            Fail("Observation routing failed exact evidence routing or completion observation.");
            return;
        }

        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> retained =
            completions.AcceptedCompletions;
        if (routing.TryRouteVisibleEvidence(afterLocation, visible, residue)
            || routing.TryRouteVisibleEvidence(afterEvidence, visible, residue)
            || !ReferenceEquals(progress.Progress, afterEvidence)
            || !ReferenceEquals(completions.AcceptedCompletions, retained)
            || consumption.ConsumedCompletion is not null
            || !ReferenceEquals(map.Map, currentMap)
            || !ReferenceEquals(journal.Journal, initialJournal)
            || !ReferenceEquals(discovery.Perception, initialPerception)
            || !HasClosedSurface())
        {
            Fail("Observation routing failed rejection, isolation, or surface checks.");
            return;
        }

        GD.Print(
            "Opening-region investigation-route observation-routing smoke passed: inert startup, "
            + "exact current authority, catalogue-ordered multi-route location and evidence "
            + "recording, ordered completion observation, null, stale, rebuilt, foreign, "
            + "unverified, unseen, nonmember and repeated rejection, closed surface, and progress, "
            + "completion-consumption, map, discovery and journal isolation were correct.");
        GetTree().Quit();
    }

    private static OpeningRegionInvestigationRouteCatalogue CreateSharedRoutes(
        OpeningRegionMapLocationCatalogue locations,
        DiscoveryScenario scenario)
    {
        LocationId stream = FindLocation(locations, "stream").Id;
        EvidenceId residue = FindEvidence(scenario, "residue");
        return new OpeningRegionInvestigationRouteCatalogue(
        [
            new(
                "stream-laundry",
                [stream, FindLocation(locations, "abandoned-laundry").Id],
                [residue]),
            new(
                "shrine",
                [stream, FindLocation(locations, "old-shrine").Id],
                [FindEvidence(scenario, "partial-inscription"), residue]),
            new(
                "road-wagon",
                [
                    FindLocation(locations, "southern-road").Id,
                    FindLocation(locations, "abandoned-wagon").Id,
                ],
                [FindEvidence(scenario, "tool-gated-clue")]),
        ]);
    }

    private static MapLocationDefinition FindLocation(
        OpeningRegionMapLocationCatalogue locations,
        string value) => locations.Locations.Single(location =>
            string.Equals(location.Id.Value, value, StringComparison.Ordinal));

    private static EvidenceId FindEvidence(
        DiscoveryScenario scenario,
        string value) => scenario.Evidence.Entries.Single(evidence =>
            string.Equals(evidence.EvidenceId.Value, value, StringComparison.Ordinal)).EvidenceId;

    private static bool HasRecordedLocation(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route,
        LocationId locationId) => progress.Entries.Single(entry => ReferenceEquals(entry.Route, route))
            .RecordedLocationIds.Any(recorded => ReferenceEquals(recorded, locationId));

    private static bool HasRecordedEvidence(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route,
        EvidenceId evidenceId) => progress.Entries.Single(entry => ReferenceEquals(entry.Route, route))
            .RecordedEvidenceIds.Any(recorded => ReferenceEquals(recorded, evidenceId));

    private static bool ThrowsArgumentNull(Action action)
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
        Type type = typeof(OpeningRegionInvestigationRouteObservationRoutingCoordinator);
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName && method.Name != nameof(_Ready))
            .ToArray();
        return properties.Length == 0
            && methods.Length == 2
            && methods.All(method => method.ReturnType == typeof(bool));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
