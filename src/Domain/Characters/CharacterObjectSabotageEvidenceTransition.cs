using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Appends exact sabotage evidence only after complete occurrence eligibility.</summary>
public static class CharacterObjectSabotageEvidenceTransition
{
    /// <summary>Returns a fresh evidence snapshot containing the accepted candidate's evidence.</summary>
    public static CharacterObjectSabotageEvidenceResult Leave(
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

        if (!CharacterObjectSabotageEvidencePolicy.CanLeave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate))
        {
            throw new InvalidOperationException(
                "Only eligible object sabotage can append fresh target-bound evidence.");
        }

        DiscoveryEvidenceSnapshot resultingEvidence = new(
            existingEvidence.Entries.Append(candidate.Evidence));

        return new CharacterObjectSabotageEvidenceResult(candidate, resultingEvidence);
    }
}
