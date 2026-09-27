using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Routes one exact resolved representative wagon focused-search clue.</summary>
public partial class OpeningRegionWagonClueFocusedSearchRoutingCoordinator : Node
{
    private const string ToolGatedEvidenceId = "tool-gated-clue";
    private const string WagonClueObjectId = "wagon-clue";
    private FocusedSearchActivityController searches = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteObservationRoutingCoordinator routing = null!;

    /// <summary>Gets the exact representative discovery scenario retained by this owner.</summary>
    public DiscoveryScenario Scenario { get; private set; } = null!;

    /// <summary>Gets the exact retained wagon-clue world object.</summary>
    public WorldObject WagonClue { get; private set; } = null!;

    /// <summary>Gets the exact retained tool-gated evidence definition.</summary>
    public DiscoveryEvidence ToolGatedEvidence { get; private set; } = null!;

    /// <summary>Gets the exact filtered clue view, or null before successful resolution.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        Scenario = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource").DiscoveryScenario;
        searches = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        routing = GetNode<OpeningRegionInvestigationRouteObservationRoutingCoordinator>(
            "../OpeningRegionInvestigationRouteObservationRoutingCoordinator");
        WagonClue = Scenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == WagonClueObjectId);
        GreyboxInteractionTarget target = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/WagonClue");
        Marker2D wagonMarker = GetNode<OpeningRegionSouthernRoadWagonSceneSource>(
            "../OpeningRegionSouthernRoadWagonSceneSource").AbandonedWagonMarker;
        ToolGatedEvidence = Scenario.Evidence.Entries.Single(candidate =>
            candidate.EvidenceId.Value == ToolGatedEvidenceId);
        DiscoveryToolRequirement requirement = Scenario.Conditions.ToolRequirements.Entries.Single(
            candidate => ReferenceEquals(candidate.ObjectId, WagonClue.Identity.Id));
        if (!ReferenceEquals(ToolGatedEvidence.ObjectId, WagonClue.Identity.Id)
            || target.ObjectId != WagonClue.Identity.Id
            || target.GlobalPosition != wagonMarker.GlobalPosition
            || !ReferenceEquals(
                ToolGatedEvidence.MinimumVisibilityState,
                VisibilityState.Identified)
            || !Scenario.AvailableInventory.ObjectReferences.Any(candidate =>
                ReferenceEquals(candidate, requirement.RequiredToolObjectId)))
        {
            throw new InvalidOperationException(
                "The representative wagon clue lacks its exact tool-gated evidence graph.");
        }

        EvidenceView = null;
    }

    /// <summary>Filters and optionally routes one exact already-resolved wagon outcome.</summary>
    public bool TryRouteResolvedOutcome(
        FocusedSearchOutcome outcome,
        WorldObject worldObject,
        CharacterPerceptionSnapshot resolvedPerception)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(worldObject);
        ArgumentNullException.ThrowIfNull(resolvedPerception);

        if (EvidenceView is not null
            || !ReferenceEquals(worldObject, WagonClue)
            || !ReferenceEquals(outcome.Request.TargetObjectId, WagonClue.Identity.Id)
            || outcome.Request.SearcherCharacterId != Scenario.ObserverCharacterId
            || !ReferenceEquals(outcome.ResultingVisibilityState, VisibilityState.Identified)
            || !ReferenceEquals(searches.Perception, resolvedPerception)
            || searches.Outcomes.Entries.Any(candidate => ReferenceEquals(candidate, outcome)))
        {
            return false;
        }

        WorldObjectVisibility visibility = resolvedPerception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, WagonClue.Identity.Id));
        CharacterEvidenceViewSnapshot projected = DiscoveryEvidenceProjection.ForCharacter(
            Scenario.Evidence,
            resolvedPerception);
        if (!ReferenceEquals(visibility.VisibilityState, VisibilityState.Identified)
            || !projected.EvidenceIds.Any(candidate =>
                ReferenceEquals(candidate, ToolGatedEvidence.EvidenceId)))
        {
            return false;
        }

        CharacterEvidenceViewSnapshot filtered = new(
            Scenario.ObserverCharacterId,
            [ToolGatedEvidence.EvidenceId]);
        EvidenceView = filtered;
        if (routeProgress.Progress is { } currentRouteProgress)
        {
            _ = routing.TryRouteVisibleEvidence(
                currentRouteProgress,
                filtered,
                ToolGatedEvidence.EvidenceId);
        }

        return true;
    }
}
