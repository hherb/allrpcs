using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Characters;

/// <summary>Exposes one authored evidence identifier from eligible recovered Influence history.</summary>
public static class CharacterTargetSabotageInfluenceInvestigationTransition
{
    /// <summary>Returns a fresh investigator-owned evidence view or fails before publication.</summary>
    public static CharacterEvidenceViewSnapshot Investigate(
        Character investigator,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetSabotageInfluenceRecoveryResult currentRecovery,
        CharacterRelationshipMemory currentRelationship,
        CharacterTargetSabotageInfluenceInvestigationCandidate currentCandidate,
        CharacterTargetSabotageInfluenceInvestigationCandidate candidate)
    {
        if (!CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
                investigator,
                interactionAccess,
                currentRecovery,
                currentRelationship,
                currentCandidate,
                candidate))
        {
            throw new InvalidOperationException(
                "Only exact reachable recovered Influence history may expose authored evidence.");
        }

        return new CharacterEvidenceViewSnapshot(
            investigator.Identity.Id,
            [candidate.EvidenceId]);
    }
}
