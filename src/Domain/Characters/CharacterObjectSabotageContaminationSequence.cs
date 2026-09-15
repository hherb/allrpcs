using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects and applies one exact eligible contamination candidate.</summary>
public static class CharacterObjectSabotageContaminationSequence
{
    /// <summary>Returns one immutable result for the exact first eligible retained candidate.</summary>
    public static CharacterObjectSabotageContaminationResult ApplyFirstEligible(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot existingEvidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates)
    {
        CharacterObjectSabotageEvidence? selected =
            CharacterObjectSabotageContaminationSelection.SelectFirstEligible(
                character,
                physicalAccess,
                worldObjects,
                contamination,
                existingEvidence,
                candidates);

        if (selected is null)
        {
            throw new InvalidOperationException(
                "No eligible contamination candidate can be applied from the supplied collection.");
        }

        return CharacterObjectSabotageContaminationTransition.Apply(
            character,
            physicalAccess,
            worldObjects,
            contamination,
            existingEvidence,
            selected);
    }
}
