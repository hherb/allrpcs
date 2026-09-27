using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit one-shot consumption of one exact accepted route completion.</summary>
public partial class OpeningRegionInvestigationRouteCompletionConsumptionCoordinatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_CONSUMPTION_CHECK";
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
        OpeningRegionInvestigationRouteCompletionCoordinator completions = GetNode<
            OpeningRegionInvestigationRouteCompletionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionCoordinator");
        OpeningRegionInvestigationRouteCompletionConsumptionCoordinator consumption = GetNode<
            OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        JournalController journal = GetNode<JournalController>("../JournalController");
        SeekerMapSnapshot? initialMap = map.Map;
        JournalSnapshot? initialJournal = journal.Journal;
        if (progress.Progress is not null
            || completions.AcceptedCompletions.Count != 0
            || consumption.ConsumedCompletion is not null)
        {
            Fail("Route completion consumption changed state during ordinary startup.");
            return;
        }

        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        OpeningRegionInvestigationRoute first = routes.Routes[0];
        OpeningRegionInvestigationRoute second = routes.Routes[1];
        if (!progress.TryInitialize(routes, locations, scenario))
        {
            Fail("Route completion consumption could not arrange route progress.");
            return;
        }

        Complete(progress, first);
        OpeningRegionInvestigationRouteProgressSnapshot firstComplete = progress.Progress!;
        OpeningRegionInvestigationRouteCompletion unaccepted =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(firstComplete, first)!;
        bool nullRejected = ThrowsArgumentNull(() => consumption.TryConsume(null!));
        bool firstObserved = completions.TryObserve(firstComplete, first);
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> accepted =
            completions.AcceptedCompletions;
        if (!nullRejected
            || consumption.TryConsume(unaccepted)
            || !firstObserved
            || accepted.Count != 1
            || !consumption.TryConsume(accepted[0])
            || !ReferenceEquals(consumption.ConsumedCompletion, accepted[0])
            || !ReferenceEquals(progress.Progress, firstComplete)
            || completions.AcceptedCompletions.Count != 1
            || consumption.TryConsume(accepted[0]))
        {
            Fail("Route completion consumption failed exact one-shot acceptance or rejection.");
            return;
        }

        OpeningRegionInvestigationRouteCompletion retained = consumption.ConsumedCompletion;
        Complete(progress, second);
        OpeningRegionInvestigationRouteProgressSnapshot secondComplete = progress.Progress!;
        OpeningRegionInvestigationRouteCompletion unacceptedSecond =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(secondComplete, second)!;
        bool secondObserved = completions.TryObserve(secondComplete, second);
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> bothAccepted =
            completions.AcceptedCompletions;
        if (consumption.TryConsume(unacceptedSecond)
            || !secondObserved
            || bothAccepted.Count != 2
            || consumption.TryConsume(bothAccepted[1])
            || !ReferenceEquals(consumption.ConsumedCompletion, retained)
            || !ReferenceEquals(progress.Progress, secondComplete)
            || !ReferenceEquals(map.Map, initialMap)
            || !ReferenceEquals(journal.Journal, initialJournal)
            || !HasClosedSurface())
        {
            Fail("Route completion consumption failed retention, rejection, or isolation checks.");
            return;
        }

        GD.Print(
            "Opening-region investigation-route completion consumption smoke passed: inert "
            + "startup, exact accepted-token binding, one-shot retention, unaccepted, repeated, "
            + "later-route and null rejection, closed surface, and progress, completion, map and "
            + "journal isolation were correct.");
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
        Type type = typeof(OpeningRegionInvestigationRouteCompletionConsumptionCoordinator);
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
                == typeof(OpeningRegionInvestigationRouteCompletion)
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
