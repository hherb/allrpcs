namespace Allrpcs.Domain.Characters;

/// <summary>Validates one substitution selection or abstention against exact current state.</summary>
public static class CharacterObjectSabotageSubstitutionDecisionPolicy
{
    /// <summary>Returns whether the exact character may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        CharacterObjectSabotageSubstitutionDecision decision)
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

        CharacterObjectSabotageSubstitutionCandidate? selected =
            request.Candidates.FirstOrDefault(candidate =>
                ReferenceEquals(
                    candidate.Evidence.Evidence.EvidenceId,
                    decision.SelectedEvidenceId));

        return selected is not null
            && CharacterObjectSabotageSubstitutionPolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Substitutions,
                request.Evidence,
                selected);
    }
}
