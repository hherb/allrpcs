using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires eligible object sabotage to define fresh target-bound evidence.</summary>
public static class CharacterObjectSabotageEvidencePolicy
{
    /// <summary>Returns whether the eligible occurrence can leave its exact evidence definition.</summary>
    public static bool CanLeave(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        DiscoveryEvidenceSnapshot existingEvidence,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(existingEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.Evidence.ObjectId == candidate.Occurrence.Target.Identity.Id
            && !existingEvidence.Entries.Any(evidence =>
                evidence.EvidenceId == candidate.Evidence.EvidenceId)
            && CharacterObjectSabotageOccurrencePolicy.CanOccur(
                character,
                physicalAccess,
                worldObjects,
                candidate.Occurrence);
    }
}
