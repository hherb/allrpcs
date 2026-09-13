namespace Allrpcs.Domain.Characters;

/// <summary>Validates one sabotage selection or abstention against exact consideration state.</summary>
public static class CharacterObjectSabotageConcealmentDecisionPolicy
{
    /// <summary>Returns whether the exact character may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        CharacterObjectSabotageConcealmentDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!ReferenceEquals(decision.CharacterId, request.Character.Identity.Id))
        {
            return false;
        }

        if (decision.SelectedEvidenceId is null)
        {
            return true;
        }

        CharacterObjectSabotageEvidence? selected = request.Candidates.FirstOrDefault(candidate =>
            ReferenceEquals(candidate.Evidence.EvidenceId, decision.SelectedEvidenceId));

        return selected is not null
            && CharacterObjectSabotageConcealmentPolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Concealment,
                request.Evidence,
                selected);
    }
}
