using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded world state for one provider-neutral sabotage choice.</summary>
public sealed record CharacterObjectSabotageConcealmentConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or applying sabotage.</summary>
    public CharacterObjectSabotageConcealmentConsiderationRequest(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterObjectSabotageEvidence> retainedCandidates = [];
        HashSet<EvidenceId> evidenceIds = [];
        foreach (CharacterObjectSabotageEvidence? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Sabotage consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!evidenceIds.Add(candidate.Evidence.EvidenceId))
            {
                throw new ArgumentException(
                    "Sabotage consideration candidates require unique evidence identifiers.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count
                > CharacterObjectSabotageConcealmentSelection.MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Sabotage consideration exceeded the bounded candidate count.");
            }
        }

        Character = character;
        PhysicalAccess = physicalAccess;
        WorldObjects = worldObjects;
        Concealment = concealment;
        Evidence = evidence;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering sabotage.</summary>
    public Character Character { get; }

    /// <summary>Gets the exact physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; }

    /// <summary>Gets the exact authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; }

    /// <summary>Gets the exact current concealment snapshot.</summary>
    public WorldObjectConcealmentSnapshot Concealment { get; }

    /// <summary>Gets the exact current authoritative evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; }

    /// <summary>Gets the stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterObjectSabotageEvidence> Candidates { get; }
}
