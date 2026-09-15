using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects one exact eligible contamination candidate without applying it.</summary>
public static class CharacterObjectSabotageContaminationSelection
{
    /// <summary>Gets the maximum number of candidates accepted in one selection.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Returns the exact first eligible candidate in stable caller order, or null.</summary>
    public static CharacterObjectSabotageEvidence? SelectFirstEligible(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot existingEvidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(existingEvidence);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterObjectSabotageEvidence> retainedCandidates = [];
        HashSet<EvidenceId> evidenceIds = [];

        foreach (CharacterObjectSabotageEvidence? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Contamination candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!evidenceIds.Add(candidate.Evidence.EvidenceId))
            {
                throw new ArgumentException(
                    "Contamination candidates cannot contain duplicate evidence identifiers.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    $"Contamination selection accepts at most {MaximumCandidateCount} candidates.");
            }
        }

        return retainedCandidates.FirstOrDefault(candidate =>
            CharacterObjectSabotageContaminationPolicy.CanApply(
                character,
                physicalAccess,
                worldObjects,
                contamination,
                existingEvidence,
                candidate));
    }
}
