using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded world state for one provider-neutral contamination choice.</summary>
/// <remarks>Instances use reference identity; copied collections do not define value equality.</remarks>
public sealed class CharacterObjectSabotageContaminationConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or applying contamination.</summary>
    public CharacterObjectSabotageContaminationConsiderationRequest(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterObjectSabotageEvidence> retainedCandidates = [];
        HashSet<EvidenceId> evidenceIds = [];
        foreach (CharacterObjectSabotageEvidence? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Contamination consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!evidenceIds.Add(candidate.Evidence.EvidenceId))
            {
                throw new ArgumentException(
                    "Contamination consideration candidates require unique evidence identifiers.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > CharacterObjectSabotageContaminationSelection.MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Contamination consideration exceeded the bounded candidate count.");
            }
        }

        Character = character;
        PhysicalAccess = physicalAccess;
        WorldObjects = worldObjects;
        Contamination = contamination;
        Evidence = evidence;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering contamination.</summary>
    public Character Character { get; }

    /// <summary>Gets the exact physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; }

    /// <summary>Gets the exact authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; }

    /// <summary>Gets the exact current contamination snapshot.</summary>
    public WorldObjectContaminationSnapshot Contamination { get; }

    /// <summary>Gets the exact current authoritative evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; }

    /// <summary>Gets the stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterObjectSabotageEvidence> Candidates { get; }
}
