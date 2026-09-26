using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Discovery;

/// <summary>Exposes exact inscription evidence only after its exact identified presentation.</summary>
public static class OpeningRegionLayeredInscriptionEvidenceExposureTransition
{
    private const string PartialInscriptionEvidenceId = "partial-inscription";

    /// <summary>Returns a one-identifier evidence view without exposing other definitions.</summary>
    public static OpeningRegionLayeredInscriptionEvidenceExposure Create(
        OpeningRegionLayeredInscriptionPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        OpeningRegionLayeredInscriptionBinding binding =
            presentation.DiscoveryState.InscriptionBinding;
        DiscoveryScenario scenario = binding.DiscoveryScenario;
        DiscoveryEvidence evidence = scenario.Evidence.Entries.Single(definition =>
            definition.EvidenceId.Value == PartialInscriptionEvidenceId);
        if (!ReferenceEquals(evidence.ObjectId, binding.WorldObject.Identity.Id)
            || !ReferenceEquals(evidence.MinimumVisibilityState, VisibilityState.Identified))
        {
            throw new InvalidOperationException(
                "The presented inscription lacks its exact identified evidence definition.");
        }

        CharacterEvidenceViewSnapshot projected = DiscoveryEvidenceProjection.ForCharacter(
            scenario.Evidence,
            presentation.DiscoveryState.Perception);
        if (!projected.EvidenceIds.Any(candidate =>
                ReferenceEquals(candidate, evidence.EvidenceId)))
        {
            throw new InvalidOperationException(
                "The exact inscription evidence is not visible in the applied perception.");
        }

        CharacterEvidenceViewSnapshot filtered = new(
            scenario.ObserverCharacterId,
            [evidence.EvidenceId]);
        return new OpeningRegionLayeredInscriptionEvidenceExposure(
            presentation,
            evidence,
            filtered);
    }
}
