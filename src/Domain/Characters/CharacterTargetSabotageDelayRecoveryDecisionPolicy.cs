namespace Allrpcs.Domain.Characters;

/// <summary>Validates one exact delay-recovery selection or abstention against current request state.</summary>
public static class CharacterTargetSabotageDelayRecoveryDecisionPolicy
{
    /// <summary>Returns whether the exact actor may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        CharacterTargetSabotageDelayRecoveryDecision decision)
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

        CharacterTargetSabotageDelayRecoveryCandidate candidate = decision.SelectedCandidate;

        return request.Candidates.Any(requestCandidate =>
                ReferenceEquals(requestCandidate, candidate))
            && ReferenceEquals(
                candidate.DelayResult.Candidate.Occurrence.Target.Identity.Id,
                request.Actor.Identity.Id)
            && CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                request.WorldEvents,
                request.EventLog,
                request.WorldTime,
                candidate);
    }
}
