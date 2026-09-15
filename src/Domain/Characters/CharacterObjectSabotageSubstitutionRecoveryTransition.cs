using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Characters;

/// <summary>Returns fresh substitution state after evidence-authorized recovery.</summary>
public static class CharacterObjectSabotageSubstitutionRecoveryTransition
{
    /// <summary>Removes only the exact evidence-bound substitution without consuming evidence.</summary>
    public static CharacterObjectSabotageSubstitutionRecoveryResult Recover(
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterObjectSabotageSubstitutionRecoveryPolicy.CanRecover(
                substitutions,
                evidence,
                evidenceView,
                candidate))
        {
            throw new InvalidOperationException(
                "Only visible authoritative sabotage evidence can recover an exact substitution.");
        }

        CharacterObjectSabotageSubstitutionSnapshot resultingSubstitutions = new(
            substitutions.Entries.Where(substitution =>
                !ReferenceEquals(substitution, candidate.Substitution)));

        return new CharacterObjectSabotageSubstitutionRecoveryResult(
            resultingSubstitutions,
            evidenceView,
            candidate);
    }
}
