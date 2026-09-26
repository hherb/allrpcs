using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Discovery;

/// <summary>Binds exact shrine access to its retained reinterpretation candidate and trigger.</summary>
public sealed class OpeningRegionLayeredInscriptionCandidateBinding
{
    /// <summary>Creates one inert candidate binding without resolving or consuming it.</summary>
    public OpeningRegionLayeredInscriptionCandidateBinding(
        OpeningRegionLayeredInscriptionAccess access,
        ObservationReinterpretationCandidate candidate,
        KnowledgeId triggerKnowledgeId)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(triggerKnowledgeId);

        DiscoveryScenario scenario = access.InscriptionBinding.DiscoveryScenario;
        if (!scenario.ReinterpretationCandidates.Entries.Any(entry =>
                ReferenceEquals(entry, candidate))
            || !ReferenceEquals(candidate.ObserverCharacterId, access.SeekerCharacterId)
            || !ReferenceEquals(
                candidate.TargetObjectId,
                access.InscriptionBinding.WorldObject.Identity.Id))
        {
            throw new ArgumentException(
                "The layered inscription requires its exact retained Seeker candidate.",
                nameof(candidate));
        }

        if (!ReferenceEquals(candidate.TriggerKnowledgeId, triggerKnowledgeId)
            || !scenario.AvailableKnowledge.KnowledgeReferences.Any(reference =>
                ReferenceEquals(reference, triggerKnowledgeId)))
        {
            throw new ArgumentException(
                "The layered inscription candidate requires its exact available trigger knowledge.",
                nameof(triggerKnowledgeId));
        }

        Access = access;
        Candidate = candidate;
        TriggerKnowledgeId = triggerKnowledgeId;
    }

    /// <summary>Gets the exact shrine-local access.</summary>
    public OpeningRegionLayeredInscriptionAccess Access { get; }

    /// <summary>Gets the exact retained reinterpretation candidate.</summary>
    public ObservationReinterpretationCandidate Candidate { get; }

    /// <summary>Gets the exact retained trigger-knowledge identifier.</summary>
    public KnowledgeId TriggerKnowledgeId { get; }
}
