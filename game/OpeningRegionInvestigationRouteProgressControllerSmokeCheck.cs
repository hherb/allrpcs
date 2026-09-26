using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks inert, explicit scene-owned investigation-route progress when enabled.</summary>
public partial class OpeningRegionInvestigationRouteProgressControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_PROGRESS_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        OpeningRegionInvestigationRouteProgressController controller = GetNode<
            OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController");
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        JournalController journal = GetNode<JournalController>("../JournalController");
        SeekerMapSnapshot? initialMap = map.Map;
        JournalSnapshot? initialJournal = journal.Journal;

        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        if (controller.Catalogue is not null
            || controller.Progress is not null
            || !ThrowsArgumentNull(() => controller.TryInitialize(null!, locations, scenario))
            || !controller.TryInitialize(routes, locations, scenario)
            || !ReferenceEquals(controller.Catalogue, routes)
            || controller.Progress is not { } empty
            || !ReferenceEquals(empty.Catalogue, routes)
            || empty.Entries.Any(entry =>
                entry.RecordedLocationIds.Count != 0 || entry.RecordedEvidenceIds.Count != 0)
            || controller.TryInitialize(routes, locations, scenario))
        {
            Fail("Route progress failed inert exact one-shot initialization.");
            return;
        }

        OpeningRegionInvestigationRoute stream = routes.Routes[0];
        if (controller.TryRecordLocation(empty, stream, new LocationId("stream"))
            || !controller.TryRecordLocation(empty, stream, stream.LocationIds[0])
            || controller.Progress is not { } withLocation
            || !ReferenceEquals(withLocation.Entries[0].RecordedLocationIds[0], stream.LocationIds[0])
            || controller.TryRecordLocation(empty, stream, stream.LocationIds[1])
            || controller.TryRecordLocation(withLocation, stream, stream.LocationIds[0])
            || !controller.TryRecordEvidence(withLocation, stream, stream.EvidenceIds[0])
            || controller.Progress is not { } withEvidence
            || !ReferenceEquals(withEvidence.Entries[0].RecordedEvidenceIds[0], stream.EvidenceIds[0])
            || controller.TryRecordEvidence(withLocation, stream, stream.EvidenceIds[0])
            || controller.TryRecordEvidence(
                withEvidence,
                stream,
                new EvidenceId(stream.EvidenceIds[0].Value)))
        {
            Fail("Route progress failed exact compare-and-swap recording.");
            return;
        }

        OpeningRegionMapLocationCatalogue foreignLocations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario foreignScenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue foreignRoutes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(
                foreignLocations,
                foreignScenario);
        OpeningRegionInvestigationRouteCatalogue repeatedShape = new(
        [
            stream,
            new(
                "shrine",
                stream.LocationIds,
                stream.EvidenceIds),
            routes.Routes[2],
        ]);
        OpeningRegionInvestigationRouteProgressController invalid = new();
        bool invalidAccepted = invalid.TryInitialize(repeatedShape, locations, scenario);
        bool invalidChanged = invalid.Catalogue is not null || invalid.Progress is not null;
        invalid.Free();
        if (controller.TryRecordLocation(
                withEvidence,
                foreignRoutes.Routes[0],
                foreignRoutes.Routes[0].LocationIds[0])
            || invalidAccepted
            || invalidChanged
            || !ReferenceEquals(map.Map, initialMap)
            || !ReferenceEquals(journal.Journal, initialJournal)
            || !HasClosedSurface())
        {
            Fail("Route progress failed foreign, invalid, isolation, or surface checks.");
            return;
        }

        GD.Print(
            "Opening-region investigation-route progress smoke passed: inert startup, exact "
            + "one-shot initialization, explicit location and evidence recording, stale, rebuilt, "
            + "foreign, repeated, invalid and null rejection, closed surface, and unrelated map "
            + "and journal isolation were correct.");
        GetTree().Quit();
    }

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
        Type type = typeof(OpeningRegionInvestigationRouteProgressController);
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
        return properties.Length == 2
            && properties.All(property => property.CanRead && property.SetMethod?.IsPrivate == true)
            && methods.Length == 3
            && methods.All(method => method.ReturnType == typeof(bool));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
