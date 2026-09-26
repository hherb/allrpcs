using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit scene-owned authority-bound route progress when enabled.</summary>
public partial class OpeningRegionInvestigationRouteAuthorityCoordinatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_AUTHORITY_CHECK";
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
        OpeningRegionInvestigationRouteAuthorityCoordinator coordinator = GetNode<
            OpeningRegionInvestigationRouteAuthorityCoordinator>(
                "../OpeningRegionInvestigationRouteAuthorityCoordinator");
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        JournalController journal = GetNode<JournalController>("../JournalController");
        JournalSnapshot? initialJournal = journal.Journal;
        if (progress.Progress is not null || map.Map is not null)
        {
            Fail("Route authority coordination changed state during ordinary startup.");
            return;
        }

        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        OpeningRegionInvestigationRoute route = routes.Routes[0];
        MapLocationDefinition stream = FindLocation(locations, route.LocationIds[0]);
        MapLocationDefinition laundry = FindLocation(locations, route.LocationIds[1]);
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot currentMap = SeekerMapSnapshotProjection.Project(
            locations,
            seekerId,
            [],
            [stream, laundry]);
        ((IMapSaveStateRestorationTarget)map).InstallRestored(
            locations,
            currentMap,
            SeekerMapNoteSnapshotProjection.CreateEmpty(locations, seekerId));
        if (!progress.TryInitialize(routes, locations, scenario)
            || progress.Progress is not { } empty)
        {
            Fail("Route authority coordination could not arrange exact current state.");
            return;
        }

        SeekerMapSnapshot rebuiltMap = SeekerMapSnapshotProjection.Project(
            locations,
            seekerId,
            [],
            [stream, laundry]);
        MapLocationDefinition rebuiltStream = new(stream.Id, stream.DisplayLabel);
        if (coordinator.TryRecordVerifiedLocation(empty, route, rebuiltMap, stream)
            || coordinator.TryRecordVerifiedLocation(empty, route, currentMap, rebuiltStream)
            || !coordinator.TryRecordVerifiedLocation(empty, route, currentMap, stream)
            || progress.Progress is not { } withLocation
            || !ReferenceEquals(withLocation.Entries[0].RecordedLocationIds[0], stream.Id)
            || coordinator.TryRecordVerifiedLocation(empty, route, currentMap, laundry)
            || coordinator.TryRecordVerifiedLocation(withLocation, route, currentMap, stream))
        {
            Fail("Route authority coordination failed exact current verified-location recording.");
            return;
        }

        EvidenceId evidenceId = route.EvidenceIds[0];
        CharacterEvidenceViewSnapshot visible = new(seekerId, [evidenceId]);
        CharacterEvidenceViewSnapshot unseen = new(seekerId, []);
        if (coordinator.TryRecordVisibleEvidence(withLocation, route, unseen, evidenceId)
            || coordinator.TryRecordVisibleEvidence(
                withLocation,
                route,
                visible,
                new EvidenceId(evidenceId.Value))
            || !coordinator.TryRecordVisibleEvidence(
                withLocation,
                route,
                visible,
                evidenceId)
            || progress.Progress is not { } withEvidence
            || !ReferenceEquals(withEvidence.Entries[0].RecordedEvidenceIds[0], evidenceId)
            || coordinator.TryRecordVisibleEvidence(
                withLocation,
                route,
                visible,
                evidenceId)
            || coordinator.TryRecordVisibleEvidence(
                withEvidence,
                route,
                visible,
                evidenceId)
            || !ReferenceEquals(map.Map, currentMap)
            || !ReferenceEquals(journal.Journal, initialJournal)
            || !HasClosedSurface())
        {
            Fail("Route authority coordination failed exact visible-evidence or isolation checks.");
            return;
        }

        GD.Print(
            "Opening-region investigation-route authority smoke passed: inert startup, exact "
            + "current map and progress compare-and-swap, verified locations, visible evidence, "
            + "stale, rebuilt, unseen and repeated rejection, closed surface, and map/journal "
            + "isolation were correct.");
        GetTree().Quit();
    }

    private static MapLocationDefinition FindLocation(
        OpeningRegionMapLocationCatalogue locations,
        LocationId locationId) => locations.Locations.Single(location =>
            ReferenceEquals(location.Id, locationId));

    private static bool HasClosedSurface()
    {
        Type type = typeof(OpeningRegionInvestigationRouteAuthorityCoordinator);
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
