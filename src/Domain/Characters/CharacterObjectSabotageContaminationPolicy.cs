using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires eligible evidence-bound contamination of a currently uncontaminated target.</summary>
public static class CharacterObjectSabotageContaminationPolicy
{
    /// <summary>Returns whether the candidate can add exact contamination and evidence state.</summary>
    public static bool CanApply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot existingEvidence,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(existingEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.Occurrence.Proposal.ActionKind == CharacterSabotageActionKind.Contaminate
            && !contamination.IsContaminated(candidate.Occurrence.Target.Identity.Id)
            && CharacterObjectSabotageEvidencePolicy.CanLeave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate);
    }
}
