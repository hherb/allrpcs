using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Characters;

/// <summary>Exposes one authored evidence identifier from an eligible recovered Delay history.</summary>
public static class CharacterTargetSabotageDelayInvestigationTransition
{
    /// <summary>Returns a fresh investigator-owned evidence view or fails before publication.</summary>
    public static CharacterEvidenceViewSnapshot Investigate(
        Character investigator,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetSabotageDelayRecoveryResult currentRecovery,
        CharacterTargetSabotageDelayInvestigationCandidate currentCandidate,
        CharacterTargetSabotageDelayInvestigationCandidate candidate)
    {
        if (!CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
                investigator,
                interactionAccess,
                currentRecovery,
                currentCandidate,
                candidate))
        {
            throw new InvalidOperationException(
                "Only exact reachable recovered Delay history may expose authored evidence.");
        }

        return new CharacterEvidenceViewSnapshot(
            investigator.Identity.Id,
            [candidate.EvidenceId]);
    }
}
