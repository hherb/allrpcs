namespace Allrpcs.Domain.Characters;

/// <summary>Validates one contamination selection or abstention against exact consideration state.</summary>
public static class CharacterObjectSabotageContaminationDecisionPolicy
{
    /// <summary>Returns whether the exact character may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterObjectSabotageContaminationConsiderationRequest request,
        CharacterObjectSabotageContaminationDecision decision)
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
            && CharacterObjectSabotageContaminationPolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Contamination,
                request.Evidence,
                selected);
    }
}
