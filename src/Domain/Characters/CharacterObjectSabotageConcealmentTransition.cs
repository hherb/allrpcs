using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Atomically returns fresh concealment and evidence state for eligible sabotage.</summary>
public static class CharacterObjectSabotageConcealmentTransition
{
    /// <summary>Returns fresh snapshots containing the exact target and its exact evidence.</summary>
    public static CharacterObjectSabotageConcealmentResult Apply(
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

        if (!CharacterObjectSabotageConcealmentPolicy.CanApply(
                character,
                physicalAccess,
                worldObjects,
                concealment,
                existingEvidence,
                candidate))
        {
            throw new InvalidOperationException(
                "Only eligible concealment can atomically conceal its target and leave evidence.");
        }

        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate);
        WorldObjectConcealmentSnapshot resultingConcealment = new(
            concealment.ConcealedObjectIds.Append(candidate.Occurrence.Target.Identity.Id));

        return new CharacterObjectSabotageConcealmentResult(
            resultingConcealment,
            evidenceResult);
    }
}
