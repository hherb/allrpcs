using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Notices the stream residue when a verified map visit reaches the stream (visits verify only once
/// the map exists), and routes it.
/// </summary>
/// <remarks>
/// Holds no session state: the residue recorded in the (saved) route progress is the only record,
/// so loading a save never leaves this node believing the residue was already noticed.
/// </remarks>
public partial class OpeningRegionStreamResiduePassiveNoticingCoordinator : Node
{
    private const string ResidueEvidenceId = "residue";
    private const string StreamResidueObjectId = "stream-residue";
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteObservationRoutingCoordinator routing = null!;

    /// <summary>Gets the exact representative discovery scenario retained by this owner.</summary>
    public DiscoveryScenario Scenario { get; private set; } = null!;

    /// <summary>Gets the exact retained stream-residue world object.</summary>
    public WorldObject StreamResidue { get; private set; } = null!;

    /// <summary>Gets the exact retained residue evidence definition.</summary>
    public DiscoveryEvidence ResidueEvidence { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Scenario = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource").DiscoveryScenario;
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        routing = GetNode<OpeningRegionInvestigationRouteObservationRoutingCoordinator>(
            "../OpeningRegionInvestigationRouteObservationRoutingCoordinator");

        StreamResidue = Scenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == StreamResidueObjectId);
        ResidueEvidence = Scenario.Evidence.Entries.Single(candidate =>
            candidate.EvidenceId.Value == ResidueEvidenceId);
        PassiveNoticingCandidate candidate = Scenario.PassiveNoticingCandidates.Entries.Single();
        WorldObjectVisibility initialVisibility = Scenario.InitialPerception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, StreamResidue.Identity.Id));
        if (!ReferenceEquals(candidate.ObserverCharacterId, Scenario.ObserverCharacterId)
            || !ReferenceEquals(candidate.TargetObjectId, StreamResidue.Identity.Id)
            || !ReferenceEquals(ResidueEvidence.ObjectId, StreamResidue.Identity.Id)
            || !ReferenceEquals(ResidueEvidence.MinimumVisibilityState, VisibilityState.Noticed)
            || !ReferenceEquals(initialVisibility.VisibilityState, VisibilityState.Unnoticed))
        {
            throw new InvalidOperationException(
                "The representative stream-residue passive-noticing graph is inconsistent.");
        }
    }

    /// <summary>
    /// Routes the residue into every matching route when the verified location is the stream (on
    /// arrival, and in the map-open catch-up). Returns false for any other location, before the
    /// routes exist, when the residue is already recorded, or when progress is stale.
    /// </summary>
    public bool TryNoticeOnArrival(LocationId verifiedLocationId)
    {
        ArgumentNullException.ThrowIfNull(verifiedLocationId);

        return routeProgress.Progress is { } progress
            && StreamResidueArrivalNoticing.Notice(Scenario, verifiedLocationId) is { } evidenceView
            && routing.TryRouteVisibleEvidence(progress, evidenceView, ResidueEvidence.EvidenceId);
    }
}
