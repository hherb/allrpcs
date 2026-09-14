namespace Allrpcs.Domain.Characters;

/// <summary>Validates one move selection or abstention against exact consideration state.</summary>
public static class CharacterObjectSabotageMoveDecisionPolicy
{
    /// <summary>Returns whether the exact character may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveDecision decision)
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

        CharacterObjectSabotageMoveCandidate? selected = request.Candidates.FirstOrDefault(candidate =>
            ReferenceEquals(candidate.Evidence.Evidence.EvidenceId, decision.SelectedEvidenceId));

        return selected is not null
            && CharacterObjectSabotageMovePolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Placements,
                request.Evidence,
                selected);
    }
}
