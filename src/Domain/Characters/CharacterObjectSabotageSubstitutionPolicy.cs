using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires an eligible evidence-bound substitution of a currently unsubstituted target.</summary>
public static class CharacterObjectSabotageSubstitutionPolicy
{
    /// <summary>Returns whether the candidate can add exact substitution and evidence state.</summary>
    public static bool CanApply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot existingEvidence,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(existingEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        return !substitutions.Entries.Any(substitution =>
                substitution.Occurrence.Target.Identity.Id
                    == candidate.Substitution.Occurrence.Target.Identity.Id)
            && CharacterObjectSabotageEvidencePolicy.CanLeave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate.Evidence);
    }
}
