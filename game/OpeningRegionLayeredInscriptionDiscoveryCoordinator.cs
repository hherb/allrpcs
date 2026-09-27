using Allrpcs.Application.Discovery;
using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Preflights and atomically commits one exact old-shrine inscription discovery.</summary>
public partial class OpeningRegionLayeredInscriptionDiscoveryCoordinator : Node
{
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteObservationRoutingCoordinator routing = null!;

    /// <summary>Gets the exact current old-shrine source retained by this scene coordinator.</summary>
    public OpeningRegionOldShrineSceneSource SceneSource { get; private set; } = null!;

    /// <summary>Gets the exact completed filtered evidence exposure, or null before discovery.</summary>
    public OpeningRegionLayeredInscriptionEvidenceExposure? CompletedExposure { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        SceneSource = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        routing = GetNode<OpeningRegionInvestigationRouteObservationRoutingCoordinator>(
            "../OpeningRegionInvestigationRouteObservationRoutingCoordinator");
        CompletedExposure = null;
    }

    /// <summary>Attempts one complete discovery while leaving every rejected path inert.</summary>
    public bool TryDiscover(Character? seeker)
    {
        OpeningRegionOldShrineSceneSource source = SceneSource;
        if (seeker is null
            || CompletedExposure is not null
            || !ReferenceEquals(seeker, source.KnowledgeableSeeker))
        {
            return false;
        }

        OpeningRegionLayeredInscriptionDiscoveryState initialState = source.DiscoveryState;
        try
        {
            NumericsVector2 seekerPosition = ToNumerics(
                source.GetNode<CharacterBody2D>("../Seeker").GlobalPosition);
            OpeningRegionLayeredInscriptionAccess access = new(
                source.InscriptionBinding,
                source.DiscoveryScenario.ObserverCharacterId,
                seekerPosition);
            ObservationReinterpretationCandidate candidate =
                source.DiscoveryScenario.ReinterpretationCandidates.Entries.Single();
            Allrpcs.Domain.Identifiers.KnowledgeId triggerKnowledgeId =
                source.DiscoveryScenario.AvailableKnowledge.KnowledgeReferences.Single();
            OpeningRegionLayeredInscriptionCandidateBinding candidateBinding = new(
                access,
                candidate,
                triggerKnowledgeId);
            OpeningRegionLayeredInscriptionResolution resolution =
                OpeningRegionLayeredInscriptionResolutionTransition.Resolve(
                    candidateBinding,
                    seeker);
            OpeningRegionLayeredInscriptionDiscoveryState appliedState =
                OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(
                    initialState,
                    resolution);
            OpeningRegionLayeredInscriptionPresentation presentation =
                OpeningRegionLayeredInscriptionPresentationTransition.Create(appliedState);
            OpeningRegionLayeredInscriptionEvidenceExposure evidenceExposure =
                OpeningRegionLayeredInscriptionEvidenceExposureTransition.Create(presentation);

            if (!source.TryCommitInscriptionDiscovery(
                    initialState,
                    appliedState,
                    presentation,
                    evidenceExposure))
            {
                return false;
            }

            CompletedExposure = evidenceExposure;
            if (routeProgress.Progress is { } currentRouteProgress)
            {
                _ = routing.TryRouteVisibleEvidence(
                    currentRouteProgress,
                    evidenceExposure.EvidenceView,
                    evidenceExposure.Evidence.EvidenceId);
            }

            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
