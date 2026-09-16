namespace Allrpcs.Domain.Characters;

/// <summary>Validates one exact influence selection or abstention against current request state.</summary>
public static class CharacterTargetSabotageInfluenceDecisionPolicy
{
    /// <summary>Returns whether the exact actor may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        CharacterTargetSabotageInfluenceDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!ReferenceEquals(decision.ActorId, request.Actor.Identity.Id))
        {
            return false;
        }

        if (decision.SelectedCandidate is null)
        {
            return true;
        }

        return request.Candidates.Any(candidate =>
                ReferenceEquals(candidate, decision.SelectedCandidate))
            && CharacterTargetSabotageInfluencePolicy.CanApply(
                request.Actor,
                request.InteractionAccess,
                request.Relationship,
                decision.SelectedCandidate);
    }
}
