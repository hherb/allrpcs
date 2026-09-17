namespace Allrpcs.Domain.Characters;

/// <summary>Validates one exact target-delay selection or abstention against current state.</summary>
public static class CharacterTargetSabotageDelayDecisionPolicy
{
    /// <summary>Returns whether the exact actor may make the supplied current decision.</summary>
    public static bool CanAccept(
        CharacterTargetSabotageDelayConsiderationRequest request,
        CharacterTargetSabotageDelayDecision decision)
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
            && CharacterTargetSabotageDelayPolicy.CanApply(
                request.Actor,
                request.InteractionAccess,
                request.WorldEvents,
                request.EventLog,
                request.WorldTime,
                decision.SelectedCandidate);
    }
}
