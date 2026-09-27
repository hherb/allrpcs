using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact ordinary stream-residue passive-noticing resolution.</summary>
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

    /// <summary>Gets the exact retained passive-noticing candidate.</summary>
    public PassiveNoticingCandidate Candidate { get; private set; } = null!;

    /// <summary>Gets the exact current representative perception.</summary>
    public CharacterPerceptionSnapshot Perception { get; private set; } = null!;

    /// <summary>Gets the exact current representative passive-noticing candidates.</summary>
    public PassiveNoticingCandidateSnapshot Candidates { get; private set; } = null!;

    /// <summary>Gets the exact filtered residue view, or null before successful noticing.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

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
        Candidate = Scenario.PassiveNoticingCandidates.Entries.Single();
        WorldObjectVisibility initialVisibility = Scenario.InitialPerception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, StreamResidue.Identity.Id));
        if (!ReferenceEquals(Candidate.ObserverCharacterId, Scenario.ObserverCharacterId)
            || !ReferenceEquals(Candidate.TargetObjectId, StreamResidue.Identity.Id)
            || !ReferenceEquals(ResidueEvidence.ObjectId, StreamResidue.Identity.Id)
            || !ReferenceEquals(ResidueEvidence.MinimumVisibilityState, VisibilityState.Noticed)
            || !ReferenceEquals(initialVisibility.VisibilityState, VisibilityState.Unnoticed))
        {
            throw new InvalidOperationException(
                "The representative stream-residue passive-noticing graph is inconsistent.");
        }

        Perception = Scenario.InitialPerception;
        Candidates = Scenario.PassiveNoticingCandidates;
        EvidenceView = null;
    }

    /// <summary>Resolves and optionally routes the one exact current residue candidate.</summary>
    public bool TryResolve(
        CharacterPerceptionSnapshot expectedPerception,
        PassiveNoticingCandidateSnapshot expectedCandidates,
        PassiveNoticingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(expectedPerception);
        ArgumentNullException.ThrowIfNull(expectedCandidates);
        ArgumentNullException.ThrowIfNull(candidate);

        if (EvidenceView is not null
            || !ReferenceEquals(Perception, expectedPerception)
            || !ReferenceEquals(Candidates, expectedCandidates)
            || !ReferenceEquals(Candidate, candidate)
            || !ReferenceEquals(
                PassiveNoticingCandidateResolutionSelection.SelectNext(
                    Scenario.ObserverCharacterId,
                    expectedPerception,
                    expectedCandidates),
                candidate))
        {
            return false;
        }

        PassiveNoticingCandidateResolutionResult result =
            PassiveNoticingCandidateResolutionSequence.ResolveNext(
                Scenario.ObserverCharacterId,
                expectedPerception,
                expectedCandidates);
        WorldObjectVisibility resolvedVisibility = result.Perception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, StreamResidue.Identity.Id));
        CharacterEvidenceViewSnapshot projected = DiscoveryEvidenceProjection.ForCharacter(
            Scenario.Evidence,
            result.Perception);
        if (!ReferenceEquals(resolvedVisibility.VisibilityState, VisibilityState.Noticed)
            || !projected.EvidenceIds.Any(candidateEvidenceId =>
                ReferenceEquals(candidateEvidenceId, ResidueEvidence.EvidenceId)))
        {
            return false;
        }

        CharacterEvidenceViewSnapshot filtered = new(
            Scenario.ObserverCharacterId,
            [ResidueEvidence.EvidenceId]);
        Perception = result.Perception;
        Candidates = result.Candidates;
        EvidenceView = filtered;

        if (routeProgress.Progress is { } currentRouteProgress)
        {
            _ = routing.TryRouteVisibleEvidence(
                currentRouteProgress,
                filtered,
                ResidueEvidence.EvidenceId);
        }

        return true;
    }
}
