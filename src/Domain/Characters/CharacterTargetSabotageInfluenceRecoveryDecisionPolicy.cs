namespace Allrpcs.Domain.Characters;

/// <summary>Validates one exact recovery selection or abstention against current request state.</summary>
public static class CharacterTargetSabotageInfluenceRecoveryDecisionPolicy
{
    /// <summary>Returns whether the exact actor may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
        CharacterTargetSabotageInfluenceRecoveryDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!ReferenceEquals(request.Actor.Identity.Id, request.Relationship.CharacterId)
            || !ReferenceEquals(decision.ActorId, request.Actor.Identity.Id))
        {
            return false;
        }

        if (decision.SelectedCandidate is null)
        {
            return true;
        }

        return request.Candidates.Any(candidate =>
                ReferenceEquals(candidate, decision.SelectedCandidate))
            && CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                request.Relationship,
                decision.SelectedCandidate);
    }
}
