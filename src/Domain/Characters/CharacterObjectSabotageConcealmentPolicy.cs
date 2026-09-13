using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires an eligible evidence-bound concealment of a currently visible target.</summary>
public static class CharacterObjectSabotageConcealmentPolicy
{
    /// <summary>Returns whether the candidate can atomically conceal its target and leave evidence.</summary>
    public static bool CanApply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot existingEvidence,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(existingEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.Occurrence.Proposal.ActionKind == CharacterSabotageActionKind.Conceal
            && !concealment.ConcealedObjectIds.Any(
                objectId => objectId == candidate.Occurrence.Target.Identity.Id)
            && CharacterObjectSabotageEvidencePolicy.CanLeave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate);
    }
}
