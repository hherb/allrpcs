using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires an exact active substitution and visible authoritative evidence for recovery.</summary>
public static class CharacterObjectSabotageSubstitutionRecoveryPolicy
{
    /// <summary>Reports whether retained evidence permits recovery of the exact substitution.</summary>
    public static bool CanRecover(
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        return substitutions.Entries.Any(substitution =>
                ReferenceEquals(substitution, candidate.Substitution))
            && candidate.Evidence.Evidence.ObjectId
                == candidate.Substitution.Occurrence.Target.Identity.Id
            && evidence.Entries.Any(definition =>
                ReferenceEquals(definition, candidate.Evidence.Evidence))
            && evidenceView.EvidenceIds.Any(evidenceId =>
                evidenceId == candidate.Evidence.Evidence.EvidenceId);
    }
}
