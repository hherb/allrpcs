using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Atomically returns fresh substitution and evidence state for eligible sabotage.</summary>
public static class CharacterObjectSabotageSubstitutionTransition
{
    /// <summary>Returns fresh snapshots containing the exact substitution and its exact evidence.</summary>
    public static CharacterObjectSabotageSubstitutionResult Apply(
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

        if (!CharacterObjectSabotageSubstitutionPolicy.CanApply(
                character,
                physicalAccess,
                worldObjects,
                substitutions,
                existingEvidence,
                candidate))
        {
            throw new InvalidOperationException(
                "Only eligible substitution can atomically add substitution and evidence state.");
        }

        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate.Evidence);
        CharacterObjectSabotageSubstitutionSnapshot resultingSubstitutions = new(
            substitutions.Entries.Append(candidate.Substitution));

        return new CharacterObjectSabotageSubstitutionResult(
            resultingSubstitutions,
            evidenceResult);
    }
}
