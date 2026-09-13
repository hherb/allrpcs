using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects and applies one exact eligible concealment candidate.</summary>
public static class CharacterObjectSabotageConcealmentSequence
{
    /// <summary>Returns one immutable result for the exact first eligible retained candidate.</summary>
    public static CharacterObjectSabotageConcealmentResult ApplyFirstEligible(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot existingEvidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates)
    {
        CharacterObjectSabotageEvidence? selected =
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                character,
                physicalAccess,
                worldObjects,
                concealment,
                existingEvidence,
                candidates);

        if (selected is null)
        {
            throw new InvalidOperationException(
                "No eligible concealment candidate can be applied from the supplied collection.");
        }

        return CharacterObjectSabotageConcealmentTransition.Apply(
            character,
            physicalAccess,
            worldObjects,
            concealment,
            existingEvidence,
            selected);
    }
}
