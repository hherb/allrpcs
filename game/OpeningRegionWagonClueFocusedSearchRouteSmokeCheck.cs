using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves ordinary tool-gated wagon discovery and optional route submission.</summary>
public partial class OpeningRegionWagonClueFocusedSearchRouteSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WAGON_CLUE_ROUTE_CHECK";
    private const string MissingProgressMode = "missing";
    private const string ActiveRouteMode = "route";
    private const string StartAction = "focused_search_start";
    private const string CompleteAction = "focused_search_complete";
    private const string ResolveAction = "focused_search_resolve";
    private const int WagonTargetIndex = 3;

    private bool activeRouteMode;

    /// <inheritdoc />
    public override void _Ready()
    {
        string mode = OS.GetEnvironment(CheckEnvironmentVariable);
        bool enabled = mode is MissingProgressMode or ActiveRouteMode;
        SetProcess(enabled);
        if (!enabled)
        {
            return;
        }

        activeRouteMode = mode == ActiveRouteMode;
        if (activeRouteMode)
        {
            InitializeNearlyCompleteWagonRoute();
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        Main main = GetNode<Main>("..");
        InteractionFocusController focus = GetNode<InteractionFocusController>(
            "../InteractionFocusController");
        FocusedSearchActivityController searches = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        TemporaryWorldObjectSource worldSource = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource");
        OpeningRegionOldShrineSceneSource scenarioSource = GetNode<
            OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        OpeningRegionWagonClueFocusedSearchRoutingCoordinator coordinator = GetNode<
            OpeningRegionWagonClueFocusedSearchRoutingCoordinator>(
                "../OpeningRegionWagonClueFocusedSearchRoutingCoordinator");
        OpeningRegionInvestigationRouteProgressController routeProgress = GetNode<
            OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController");
        OpeningRegionInvestigationRouteCompletionCoordinator completions = GetNode<
            OpeningRegionInvestigationRouteCompletionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionCoordinator");
        OpeningRegionInvestigationRouteCompletionConsumptionCoordinator consumption = GetNode<
            OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");

        GreyboxInteractionTarget wagonTarget = focus.GetTarget(WagonTargetIndex);
        Marker2D wagonMarker = GetNode<OpeningRegionSouthernRoadWagonSceneSource>(
            "../OpeningRegionSouthernRoadWagonSceneSource").AbandonedWagonMarker;
        WorldObject exactWagon = scenarioSource.DiscoveryScenario.WorldObjects.Entries.Single(
            candidate => candidate.Identity.Id.Value == "wagon-clue");
        CharacterPerceptionSnapshot initialPerception = searches.Perception
            ?? throw new InvalidOperationException("Missing live Seeker perception.");
        WorldObjectVisibility initialVisibility = initialPerception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, exactWagon.Identity.Id));
        if (!ReferenceEquals(worldSource.WorldObjects.Entries[WagonTargetIndex], exactWagon)
            || wagonTarget.ObjectId != exactWagon.Identity.Id
            || wagonTarget.GlobalPosition != wagonMarker.GlobalPosition
            || !ReferenceEquals(initialVisibility.VisibilityState, VisibilityState.Unnoticed)
            || coordinator.EvidenceView is not null)
        {
            Fail("The wagon clue was not bound to exact inert representative state.");
            return;
        }

        OpeningRegionInvestigationRouteProgressSnapshot? beforeWagonProgress =
            routeProgress.Progress;

        focus.RefreshFocusFrom(new System.Numerics.Vector2(
            wagonTarget.GlobalPosition.X,
            wagonTarget.GlobalPosition.Y));
        main._UnhandledInput(Action(StartAction));
        FocusedSearchActivity started = searches.Activities.Entries.Single();
        if (!ReferenceEquals(started.Request.TargetObjectId, exactWagon.Identity.Id))
        {
            Fail("The exact wagon clue did not start through ordinary focused search.");
            return;
        }

        main._UnhandledInput(Action(CompleteAction));
        FocusedSearchOutcome outcome = searches.Outcomes.Entries.Single();
        if (!ReferenceEquals(outcome.Request, started.Request)
            || !ReferenceEquals(outcome.ResultingVisibilityState, VisibilityState.Identified))
        {
            Fail("The exact wagon focused search did not produce its identified outcome.");
            return;
        }

        main._UnhandledInput(Action(ResolveAction));
        CharacterPerceptionSnapshot resolvedPerception = searches.Perception!;
        CharacterEvidenceViewSnapshot? evidenceView = coordinator.EvidenceView;
        WorldObjectVisibility resolvedVisibility = resolvedPerception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, exactWagon.Identity.Id));
        OpeningRegionInvestigationRouteProgressSnapshot? completedProgress = routeProgress.Progress;
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> accepted =
            completions.AcceptedCompletions;
        OpeningRegionInvestigationRouteProgressSnapshot? retainedProgress = routeProgress.Progress;
        CharacterEvidenceViewSnapshot? retainedView = coordinator.EvidenceView;

        main._UnhandledInput(Action(ResolveAction));
        if (!ReferenceEquals(searches.Perception, resolvedPerception)
            || searches.Outcomes.Entries.Count != 0
            || !ReferenceEquals(coordinator.EvidenceView, retainedView)
            || !ReferenceEquals(routeProgress.Progress, retainedProgress)
            || !ReferenceEquals(completions.AcceptedCompletions, accepted)
            || !ReferenceEquals(coordinator.Scenario, scenarioSource.DiscoveryScenario)
            || !ReferenceEquals(coordinator.WagonClue, exactWagon)
            || !ReferenceEquals(coordinator.ToolGatedEvidence.ObjectId, exactWagon.Identity.Id)
            || coordinator.ToolGatedEvidence.EvidenceId.Value != "tool-gated-clue"
            || ReferenceEquals(resolvedPerception, initialPerception)
            || !ReferenceEquals(resolvedVisibility.VisibilityState, VisibilityState.Identified)
            || evidenceView is null
            || evidenceView.EvidenceIds.Count != 1
            || !ReferenceEquals(
                evidenceView.EvidenceIds.Single(),
                coordinator.ToolGatedEvidence.EvidenceId)
            || consumption.ConsumedCompletion is not null
            || !HasExpectedRouteOutcome(beforeWagonProgress, completedProgress, accepted))
        {
            Fail("The wagon clue lost exact focused-search or route provenance.");
            return;
        }

        GD.Print(
            "Opening-region wagon-clue route smoke passed: exact representative world object, "
            + "field-lens-gated ordinary F/E/R discovery, identified filtered evidence, optional "
            + "exact route recording, repeated resolution inertness, and no completion consumption.");
        GetTree().Quit();
    }

    private void InitializeNearlyCompleteWagonRoute()
    {
        OpeningRegionInvestigationRouteProgressController progress = GetNode<
            OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController");
        DiscoveryScenario scenario = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource").DiscoveryScenario;
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        if (!progress.TryInitialize(routes, locations, scenario))
        {
            Fail("The wagon-clue route smoke could not initialize route progress.");
            return;
        }

        OpeningRegionInvestigationRoute wagonRoute = routes.Routes.Single(route =>
            route.Key == "road-wagon");
        foreach (LocationId locationId in wagonRoute.LocationIds)
        {
            if (!progress.TryRecordLocation(progress.Progress!, wagonRoute, locationId))
            {
                Fail("The wagon-clue route smoke could not seed exact route locations.");
                return;
            }
        }
    }

    private bool HasExpectedRouteOutcome(
        OpeningRegionInvestigationRouteProgressSnapshot? beforeWagon,
        OpeningRegionInvestigationRouteProgressSnapshot? completed,
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> accepted)
    {
        if (!activeRouteMode)
        {
            return beforeWagon is null && completed is null && accepted.Count == 0;
        }

        if (beforeWagon is null || completed is null)
        {
            return false;
        }

        OpeningRegionInvestigationRouteProgress wagon = completed.Entries.Single(entry =>
            entry.Route.Key == "road-wagon");
        return wagon.RecordedLocationIds.Count == wagon.Route.LocationIds.Count
            && wagon.RecordedEvidenceIds.Count == 1
            && ReferenceEquals(
                wagon.RecordedEvidenceIds.Single(),
                GetNode<OpeningRegionWagonClueFocusedSearchRoutingCoordinator>(
                    "../OpeningRegionWagonClueFocusedSearchRoutingCoordinator")
                    .ToolGatedEvidence.EvidenceId)
            && completed.Entries
                .Where(entry => !ReferenceEquals(entry, wagon))
                .All(entry => ReferenceEquals(
                    entry,
                    beforeWagon.Entries.Single(before =>
                        ReferenceEquals(before.Route, entry.Route))))
            && accepted.Count == 1
            && ReferenceEquals(accepted[0].Route, wagon.Route)
            && ReferenceEquals(accepted[0].ProgressSnapshot, completed);
    }

    private static InputEventAction Action(string action) => new()
    {
        Action = action,
        Pressed = true,
    };

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
