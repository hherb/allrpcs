using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves exact ordinary stream-residue noticing and optional route submission.</summary>
public partial class OpeningRegionStreamResiduePassiveNoticingSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_STREAM_RESIDUE_ROUTE_CHECK";
    private const string MissingProgressMode = "missing";
    private const string ActiveRouteMode = "route";

    private OpeningRegionStreamResiduePassiveNoticingCoordinator coordinator = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private CharacterPerceptionSnapshot initialPerception = null!;
    private PassiveNoticingCandidateSnapshot initialCandidates = null!;
    private PassiveNoticingCandidate candidate = null!;
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
        coordinator = GetNode<OpeningRegionStreamResiduePassiveNoticingCoordinator>(
            "../OpeningRegionStreamResiduePassiveNoticingCoordinator");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        initialPerception = coordinator.Perception;
        initialCandidates = coordinator.Candidates;
        candidate = coordinator.Candidate;

        if (coordinator.TryResolve(
                new CharacterPerceptionSnapshot(
                    initialPerception.CharacterId,
                    initialPerception.Entries),
                initialCandidates,
                candidate)
            || coordinator.TryResolve(
                initialPerception,
                new PassiveNoticingCandidateSnapshot(initialCandidates.Entries),
                candidate)
            || coordinator.TryResolve(
                initialPerception,
                initialCandidates,
                new PassiveNoticingCandidate(
                    new CharacterId("travelling-healer"),
                    candidate.TargetObjectId))
            || !ReferenceEquals(coordinator.Perception, initialPerception)
            || !ReferenceEquals(coordinator.Candidates, initialCandidates)
            || coordinator.EvidenceView is not null)
        {
            Fail("Rejected stream-residue inputs changed the exact noticing state.");
            return;
        }

        if (activeRouteMode)
        {
            InitializeNearlyCompleteStreamRoute();
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        OpeningRegionOldShrineSceneSource source = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        OpeningRegionInvestigationRouteCompletionCoordinator completions = GetNode<
            OpeningRegionInvestigationRouteCompletionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionCoordinator");
        OpeningRegionInvestigationRouteCompletionConsumptionCoordinator consumption = GetNode<
            OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
                "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");
        CharacterPerceptionSnapshot resolvedPerception = coordinator.Perception;
        CharacterEvidenceViewSnapshot? evidenceView = coordinator.EvidenceView;
        WorldObjectVisibility resolvedVisibility = resolvedPerception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, coordinator.StreamResidue.Identity.Id));
        OpeningRegionInvestigationRouteProgressSnapshot? completedProgress = routeProgress.Progress;
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> accepted =
            completions.AcceptedCompletions;

        CharacterPerceptionSnapshot retainedPerception = coordinator.Perception;
        PassiveNoticingCandidateSnapshot retainedCandidates = coordinator.Candidates;
        CharacterEvidenceViewSnapshot? retainedView = coordinator.EvidenceView;
        OpeningRegionInvestigationRouteProgressSnapshot? retainedProgress = routeProgress.Progress;
        if (coordinator.TryResolve(initialPerception, initialCandidates, candidate)
            || !ReferenceEquals(coordinator.Perception, retainedPerception)
            || !ReferenceEquals(coordinator.Candidates, retainedCandidates)
            || !ReferenceEquals(coordinator.EvidenceView, retainedView)
            || !ReferenceEquals(routeProgress.Progress, retainedProgress)
            || !ReferenceEquals(completions.AcceptedCompletions, accepted))
        {
            Fail("Repeated stream-residue noticing changed completed state.");
            return;
        }

        if (!ReferenceEquals(coordinator.Scenario, source.DiscoveryScenario)
            || !ReferenceEquals(candidate, source.DiscoveryScenario.PassiveNoticingCandidates.Entries.Single())
            || !ReferenceEquals(candidate.TargetObjectId, coordinator.StreamResidue.Identity.Id)
            || !ReferenceEquals(coordinator.ResidueEvidence.ObjectId, candidate.TargetObjectId)
            || coordinator.ResidueEvidence.EvidenceId.Value != "residue"
            || !ReferenceEquals(
                coordinator.ResidueEvidence.MinimumVisibilityState,
                VisibilityState.Noticed)
            || ReferenceEquals(resolvedPerception, initialPerception)
            || !ReferenceEquals(resolvedPerception.CharacterId, initialPerception.CharacterId)
            || !ReferenceEquals(resolvedVisibility.VisibilityState, VisibilityState.Noticed)
            || coordinator.Candidates.Entries.Count != 0
            || evidenceView is null
            || !ReferenceEquals(evidenceView.CharacterId, source.DiscoveryScenario.ObserverCharacterId)
            || evidenceView.EvidenceIds.Count != 1
            || !ReferenceEquals(evidenceView.EvidenceIds.Single(), coordinator.ResidueEvidence.EvidenceId)
            || consumption.ConsumedCompletion is not null
            || !HasExpectedRouteOutcome(completedProgress, accepted))
        {
            Fail("Ordinary stream-residue noticing lost exact scenario or route provenance.");
            return;
        }

        GD.Print(
            "Opening-region stream-residue route smoke passed: exact representative candidate, "
            + "atomic unnoticed-to-noticed resolution, filtered residue evidence, inert rejected "
            + "and repeated paths, optional exact route recording, and no completion consumption.");
        GetTree().Quit();
    }

    private void InitializeNearlyCompleteStreamRoute()
    {
        OpeningRegionOldShrineSceneSource source = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(
                locations,
                source.DiscoveryScenario);
        if (!routeProgress.TryInitialize(routes, locations, source.DiscoveryScenario))
        {
            Fail("The stream-residue route smoke could not initialize route progress.");
            return;
        }

        OpeningRegionInvestigationRoute streamRoute = routes.Routes.Single(route =>
            route.Key == "stream-laundry");
        foreach (LocationId locationId in streamRoute.LocationIds)
        {
            OpeningRegionInvestigationRouteProgressSnapshot expected = routeProgress.Progress!;
            if (!routeProgress.TryRecordLocation(expected, streamRoute, locationId))
            {
                Fail("The stream-residue route smoke could not seed exact route locations.");
                return;
            }
        }
    }

    private bool HasExpectedRouteOutcome(
        OpeningRegionInvestigationRouteProgressSnapshot? completed,
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> accepted)
    {
        if (!activeRouteMode)
        {
            return completed is null && accepted.Count == 0;
        }

        if (completed is null)
        {
            return false;
        }

        OpeningRegionInvestigationRouteProgress stream = completed.Entries.Single(entry =>
            entry.Route.Key == "stream-laundry");
        return stream.RecordedLocationIds.Count == stream.Route.LocationIds.Count
            && stream.RecordedEvidenceIds.Count == 1
            && ReferenceEquals(
                stream.RecordedEvidenceIds.Single(),
                coordinator.ResidueEvidence.EvidenceId)
            && completed.Entries
                .Where(entry => !ReferenceEquals(entry, stream))
                .All(entry => entry.RecordedLocationIds.Count == 0
                    && entry.RecordedEvidenceIds.Count == 0)
            && accepted.Count == 1
            && ReferenceEquals(accepted[0].Route, stream.Route)
            && ReferenceEquals(accepted[0].ProgressSnapshot, completed);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
