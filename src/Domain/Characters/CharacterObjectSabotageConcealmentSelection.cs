using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects one exact eligible concealment candidate without applying it.</summary>
public static class CharacterObjectSabotageConcealmentSelection
{
    /// <summary>Gets the maximum number of candidates accepted in one selection.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Returns the exact first eligible candidate in stable caller order, or null.</summary>
    public static CharacterObjectSabotageEvidence? SelectFirstEligible(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot existingEvidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(existingEvidence);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterObjectSabotageEvidence> retainedCandidates = [];
        HashSet<EvidenceId> evidenceIds = [];

        foreach (CharacterObjectSabotageEvidence? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Concealment candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!evidenceIds.Add(candidate.Evidence.EvidenceId))
            {
                throw new ArgumentException(
                    "Concealment candidates cannot contain duplicate evidence identifiers.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    $"Concealment selection accepts at most {MaximumCandidateCount} candidates.");
            }
        }

        return retainedCandidates.FirstOrDefault(candidate =>
            CharacterObjectSabotageConcealmentPolicy.CanApply(
                character,
                physicalAccess,
                worldObjects,
                concealment,
                existingEvidence,
                candidate));
    }
}
