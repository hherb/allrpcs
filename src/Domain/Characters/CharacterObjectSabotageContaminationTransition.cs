using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Atomically returns fresh contamination and evidence state for eligible sabotage.</summary>
public static class CharacterObjectSabotageContaminationTransition
{
    /// <summary>Returns fresh snapshots containing the exact target and its exact evidence.</summary>
    public static CharacterObjectSabotageContaminationResult Apply(
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

        if (!CharacterObjectSabotageContaminationPolicy.CanApply(
                character,
                physicalAccess,
                worldObjects,
                contamination,
                existingEvidence,
                candidate))
        {
            throw new InvalidOperationException(
                "Only eligible contamination can atomically contaminate its target and leave evidence.");
        }

        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate);
        WorldObjectContaminationSnapshot resultingContamination = new(
            contamination.ContaminatedObjectIds.Append(candidate.Occurrence.Target.Identity.Id));

        return new CharacterObjectSabotageContaminationResult(
            resultingContamination,
            evidenceResult);
    }
}
