using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit scene-owned observation of exact route completions when enabled.</summary>
public partial class OpeningRegionInvestigationRouteCompletionCoordinatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_CHECK";
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
        OpeningRegionInvestigationRouteCompletionCoordinator coordinator = GetNode<
            OpeningRegionInvestigationRouteCompletionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionCoordinator");
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        JournalController journal = GetNode<JournalController>("../JournalController");
        SeekerMapSnapshot? initialMap = map.Map;
        JournalSnapshot? initialJournal = journal.Journal;
        if (progress.Progress is not null || coordinator.AcceptedCompletions.Count != 0)
        {
            Fail("Route completion observation changed state during ordinary startup.");
            return;
        }

        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        OpeningRegionInvestigationRoute first = routes.Routes[0];
        OpeningRegionInvestigationRoute second = routes.Routes[1];
        if (!progress.TryInitialize(routes, locations, scenario)
            || progress.Progress is not { } empty
            || coordinator.TryObserve(empty, first))
        {
            Fail("Route completion observation accepted incomplete progress.");
            return;
        }

        Complete(progress, first);
        OpeningRegionInvestigationRouteProgressSnapshot firstComplete = progress.Progress!;
        OpeningRegionInvestigationRoute rebuilt = new(
            first.Key,
            first.LocationIds,
            first.EvidenceIds);
        OpeningRegionMapLocationCatalogue foreignLocations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario foreignScenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRoute foreign =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(
                foreignLocations,
                foreignScenario).Routes[0];
        bool nullProgressRejected = ThrowsArgumentNull(() =>
            coordinator.TryObserve(null!, first));
        bool nullRouteRejected = ThrowsArgumentNull(() =>
            coordinator.TryObserve(firstComplete, null!));
        if (!nullProgressRejected
            || !nullRouteRejected
            || coordinator.TryObserve(empty, first)
            || coordinator.TryObserve(firstComplete, rebuilt)
            || coordinator.TryObserve(firstComplete, foreign)
            || !coordinator.TryObserve(firstComplete, first)
            || coordinator.AcceptedCompletions is not { Count: 1 } firstAccepted
            || !ReferenceEquals(firstAccepted[0].ProgressSnapshot, firstComplete)
            || !ReferenceEquals(firstAccepted[0].RouteProgress, firstComplete.Entries[0])
            || !ReferenceEquals(firstAccepted[0].Route, first)
            || !ReferenceEquals(progress.Progress, firstComplete)
            || coordinator.TryObserve(firstComplete, first))
        {
            Fail("Route completion observation failed exact one-shot acceptance or rejection.");
            return;
        }

        OpeningRegionInvestigationRouteCompletion retainedFirst = firstAccepted[0];
        OpeningRegionInvestigationRouteProgressSnapshot beforeSecond = progress.Progress!;
        if (coordinator.TryObserve(beforeSecond, second))
        {
            Fail("Route completion observation accepted an unrelated partial route.");
            return;
        }

        Complete(progress, second);
        OpeningRegionInvestigationRouteProgressSnapshot secondComplete = progress.Progress!;
        if (!coordinator.TryObserve(secondComplete, second)
            || coordinator.AcceptedCompletions is not { Count: 2 } accepted
            || !ReferenceEquals(accepted[0], retainedFirst)
            || !ReferenceEquals(accepted[1].ProgressSnapshot, secondComplete)
            || !ReferenceEquals(accepted[1].Route, second)
            || !ReferenceEquals(progress.Progress, secondComplete)
            || !ReferenceEquals(map.Map, initialMap)
            || !ReferenceEquals(journal.Journal, initialJournal)
            || !HasClosedSurface())
        {
            Fail("Route completion observation failed ordered retention or isolation checks.");
            return;
        }

        GD.Print(
            "Opening-region investigation-route completion smoke passed: inert startup, exact "
            + "current complete progress, one-shot ordered token retention, partial, stale, "
            + "rebuilt, foreign, repeated and null rejection, closed surface, and progress, "
            + "map and journal isolation were correct.");
        GetTree().Quit();
    }

    private static void Complete(
        OpeningRegionInvestigationRouteProgressController progress,
        OpeningRegionInvestigationRoute route)
    {
        foreach (Allrpcs.Domain.Identifiers.LocationId locationId in route.LocationIds)
        {
            if (!progress.TryRecordLocation(progress.Progress!, route, locationId))
            {
                throw new InvalidOperationException("Could not arrange complete route locations.");
            }
        }

        foreach (Allrpcs.Domain.Identifiers.EvidenceId evidenceId in route.EvidenceIds)
        {
            if (!progress.TryRecordEvidence(progress.Progress!, route, evidenceId))
            {
                throw new InvalidOperationException("Could not arrange complete route evidence.");
            }
        }
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
        Type type = typeof(OpeningRegionInvestigationRouteCompletionCoordinator);
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
        return properties.Length == 1
            && properties[0].PropertyType
                == typeof(IReadOnlyList<OpeningRegionInvestigationRouteCompletion>)
            && properties[0].SetMethod?.IsPrivate == true
            && methods.Length == 1
            && methods[0].ReturnType == typeof(bool);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
